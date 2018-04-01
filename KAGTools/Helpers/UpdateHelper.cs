using Squirrel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Reflection;
using KAGTools.Properties;
using System.Windows;

namespace KAGTools.Helpers
{
    public static class UpdateHelper
    {
        private static readonly string BackupUserSettingsFilePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\..\user.config.backup";
        
        public static async Task<bool> UpdateApp(bool notify = false)
        {
            try
            {
                using (UpdateManager mgr = await UpdateManager.GitHubUpdateManager(Settings.Default.UpdateUrl, prerelease: Settings.Default.UsePreReleases))
                {
                    UpdateInfo updates = await mgr.CheckForUpdate();
                    if (updates.ReleasesToApply.Count > 0)
                    {
                        ReleaseEntry lastVersion = updates.ReleasesToApply.OrderBy(x => x.Version).Last();
                        if (MessageBox.Show(
                            string.Format("An update is available (v{0}).{1}Would you like to install it?", lastVersion.Version, Environment.NewLine),
                            "Update",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question,
                            MessageBoxResult.Yes
                            ) == MessageBoxResult.Yes)
                        {
                            await mgr.DownloadReleases(new[] { lastVersion });
                            await mgr.ApplyReleases(updates);
                            BackupSettings();
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
        
        // Backup settings before applying update or else they get reset
        public static void BackupSettings()
        {
            string settingsFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            File.Copy(settingsFile, BackupUserSettingsFilePath, true);
        }

        // Restore settings after update
        public static bool RestoreSettings()
        {
            string destFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            string sourceFile = BackupUserSettingsFilePath;
            
            if (!File.Exists(sourceFile))
            {
                return true;
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destFile)); // Create directory if doesn't exist
                File.Copy(sourceFile, destFile, true);
            }
            catch (IOException)
            {
                return false;
            }

            try
            {
                File.Delete(sourceFile);
            }
            catch (IOException)
            {

            }

            return true;
        }
    }
}
