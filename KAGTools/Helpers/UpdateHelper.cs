using Serilog;
using Squirrel;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace KAGTools.Helpers
{
    public static class UpdateHelper
    {
        public static async Task AutoUpdate(string updateUrl, Func<ReleaseEntry, bool> acceptNewUpdates, bool usePreReleases = false)
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
                        return;
                    }

                    if (updateInfo?.ReleasesToApply.Count == 0)
                    {
                        Log.Information("AutoUpdate: No new updates found");
                        return;
                    }

                    Log.Information("AutoUpdate: New updates found: {ReleasesToApply}", updateInfo.ReleasesToApply.Select(r => r.Version));

                    if (!acceptNewUpdates(updateInfo.FutureReleaseEntry))
                    {
                        Log.Information("AutoUpdate: Updates declined by user");
                        return;
                    }

                    try
                    {
                        Log.Information("AutoUpdate: Downloading new updates");
                        await updateManager.DownloadReleases(updateInfo.ReleasesToApply);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "AutoUpdate: Failed to download updates");
                        return;
                    }

                    try
                    {
                        Log.Information("AutoUpdate: Applying new updates");
                        await updateManager.ApplyReleases(updateInfo);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "AutoUpdate: Failed to apply updates");
                        return;
                    }
                }

                Log.Information("AutoUpdate: New updates installed. Restarting");
                await UpdateManager.RestartAppWhenExited();
                Application.Current.Shutdown(0);
            }
            catch (HttpRequestException ex)
            {
                Log.Warning(ex, "AutoUpdate: There was a problem connecting to GitHub");
                return;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AutoUpdate: Failed to auto-update");
                return;
            }
        }
    }
}
