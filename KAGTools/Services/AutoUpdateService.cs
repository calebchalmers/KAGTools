using Serilog;
using Squirrel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KAGTools.Services
{
    public class AutoUpdateService
    {
        public string UpdateUrl { get; set; }
        public bool UsePreReleases { get; set; }

        public AutoUpdateService(string updateUrl, bool usePreReleases)
        {
            UpdateUrl = updateUrl;
            UsePreReleases = usePreReleases;
        }

        public async Task<bool> AutoUpdate(Func<ReleaseEntry, bool> acceptNewUpdates)
        {
            Log.Information("AutoUpdate: Attempting auto-update from: {UpdateUrl}", UpdateUrl);
            try
            {
                using (var updateManager = await UpdateManager.GitHubUpdateManager(UpdateUrl, prerelease: UsePreReleases))
                {
                    UpdateInfo updateInfo;

                    // Check for updates
                    try
                    {
                        Log.Information("AutoUpdate: Checking for updates");
                        updateInfo = await updateManager.CheckForUpdate();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "AutoUpdate: Failed to check for updates");
                        return false;
                    }

                    if (updateInfo?.ReleasesToApply.Count == 0)
                    {
                        Log.Information("AutoUpdate: No new updates found");
                        return false;
                    }

                    Log.Information("AutoUpdate: New updates found: {ReleasesToApply}", updateInfo.ReleasesToApply.Select(r => r.Version));

                    // Ask user if they want to accept the updates
                    if (!acceptNewUpdates(updateInfo.FutureReleaseEntry))
                    {
                        Log.Information("AutoUpdate: Updates declined by user");
                        return false;
                    }

                    // Download releases
                    try
                    {
                        Log.Information("AutoUpdate: Downloading new updates");
                        await updateManager.DownloadReleases(updateInfo.ReleasesToApply);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "AutoUpdate: Failed to download updates");
                        return false;
                    }

                    // Apply releases
                    try
                    {
                        Log.Information("AutoUpdate: Applying new updates");
                        await updateManager.ApplyReleases(updateInfo);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "AutoUpdate: Failed to apply updates");
                        return false;
                    }
                }

                Log.Information("AutoUpdate: New updates installed. Restarting");
                await UpdateManager.RestartAppWhenExited();
                return true;
            }
            catch (HttpRequestException ex)
            {
                Log.Warning(ex, "AutoUpdate: There was a problem connecting to GitHub");
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AutoUpdate: Failed to auto-update");
                return false;
            }
        }
    }
}
