using Serilog;
using Squirrel;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace KAGTools.Helpers
{
    public static class UpdateHelper
    {
        public static async Task<bool> AutoUpdate(string updateUrl, Func<ReleaseEntry, bool> acceptNewUpdates, bool usePreReleases = false)
        {
            Log.Information("AutoUpdate: Attempting auto-update from: {UpdateUrl}", updateUrl);
            try
            {
                using (var updateManager = await UpdateManager.GitHubUpdateManager(updateUrl, prerelease: usePreReleases))
                {
                    UpdateInfo updateInfo;

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

                    if (!acceptNewUpdates(updateInfo.FutureReleaseEntry))
                    {
                        Log.Information("AutoUpdate: Updates declined by user");
                        return false;
                    }

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
