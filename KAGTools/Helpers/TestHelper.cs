using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using KAGTools.Data;

namespace KAGTools.Helpers
{
    public static class TestHelper
    {
        private static readonly string DEFAULT_RCONPASSWORD = "test";

        public static void TestSolo()
        {
            StartKagProcess(FileHelper.SoloAutoStartScriptPath);
        }

        public static async Task TestMultiplayer()
        {
            var tcprProperty = new BoolConfigProperty("sv_tcpr", false);
            var portProperty = new IntConfigProperty("sv_port", -1);
            var passwordProperty = new StringConfigProperty("sv_rconpassword", "");
            FileHelper.ReadConfigProperties(FileHelper.AutoConfigPath, 
                tcprProperty,
                portProperty,
                passwordProperty
            );
            
            if(tcprProperty.Value == false || passwordProperty.Value == "")
            {
                tcprProperty.Value = true;
                passwordProperty.Value = DEFAULT_RCONPASSWORD; // default password
                FileHelper.WriteConfigProperties(FileHelper.AutoConfigPath, tcprProperty, passwordProperty);
            }

            var serverProcess = StartKagProcess(FileHelper.ServerAutoStartScriptPath); // Start server process

            TcpClient tcpClient = new TcpClient(AddressFamily.InterNetwork);
            
            for(int i = 0; i < 100; i++)
            {
                if (serverProcess.HasExited) break;

                await Task.Delay(TimeSpan.FromSeconds(1));

                try
                {
                    Debug.WriteLine("Attempting to connect to server #{0}...", i);
                    await tcpClient.ConnectAsync("localhost", portProperty.Value);

                    if (tcpClient.Connected)
                    {
                        Debug.WriteLine("Sucessfully connected to server");
                        StartKagProcess(FileHelper.ClientAutoStartScriptPath); // Start client process
                        break;
                    }
                }
                catch (SocketException)
                {

                }
            }

            tcpClient.Close();
        }

        private static Process StartKagProcess(string autostart)
        {
            return Process.Start(new ProcessStartInfo()
            {
                FileName = FileHelper.KagExecutablePath,
                Arguments = $"noautoupdate nolauncher autostart \"{autostart}\"",
                WorkingDirectory = FileHelper.KagDir
            });
        }
    }
}
