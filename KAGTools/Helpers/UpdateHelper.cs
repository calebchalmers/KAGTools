using Squirrel;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace KAGTools.Helpers
{
    public static class UpdateHelper
    {
        public static async Task<bool> UpdateApp()
        {
            try
            {
                using (UpdateManager mgr = await UpdateManager.GitHubUpdateManager(ConfigurationManager.AppSettings["UpdateUrl"], prerelease: App.Settings.UsePreReleases))
                {
                    UpdateInfo updates = await mgr.CheckForUpdate();
                    if (updates.ReleasesToApply.Count > 0)
                    {
                        ReleaseEntry lastVersion = updates.ReleasesToApply.OrderBy(x => x.Version).Last();
                        if (MessageBox.Show(
                            string.Format("An update for {0} is available (v{1}).{2}Would you like to install it?", AssemblyHelper.AppName, lastVersion.Version, Environment.NewLine),
                            "Update",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question,
                            MessageBoxResult.Yes
                            ) == MessageBoxResult.Yes)
                        {
                            await mgr.DownloadReleases(new[] { lastVersion });
                            await mgr.ApplyReleases(updates);
                            await UpdateManager.RestartAppWhenExited();
                            Application.Current.Shutdown(0);
                        }

                        return true;
                    }
                }
            }
            catch (InvalidOperationException e)
            {
                Debug.WriteLine(e.Message);
            }

            return false;
        }
    }
}
