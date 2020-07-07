using KAGTools.Data;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace KAGTools.Services
{
    public class TestService
    {
        private readonly string DEFAULT_RCONPASSWORD = "test";

        // TCPR port testing options
        private readonly int MAX_RETRIES = 100;
        private readonly TimeSpan RETRY_INTERVAL = TimeSpan.FromSeconds(1);

        public FileLocations FileLocations { get; set; }

        public TestService(FileLocations fileLocations)
        {
            FileLocations = fileLocations;
        }

        public bool TestSolo()
        {
            Log.Information("Starting solo test");

            try
            {
                return StartKagProcess(FileLocations.SoloAutoStartScriptPath) != null;
            }
            catch (Win32Exception ex)
            {
                Log.Error(ex, "Failed to start solo test process");
                return false;
            }
        }

        public async Task<bool> TestMultiplayerAsync(ConfigService configService)
        {
            Log.Information("Starting multiplayer test");

            int port = TryForceMultiplayerAutoConfigProperties(configService);

            // Make sure that sv_tcpr is enabled and sv_rconpassword is set (otherwise we can't test the TCPR port)
            if (port == -1)
            {
                return false;
            }

            // Start the server process
            Process serverProcess;

            try
            {
                Log.Information("TestMultiplayer: Starting test server process");
                serverProcess = StartKagProcess(FileLocations.ServerAutoStartScriptPath);
            }
            catch (Win32Exception ex)
            {
                Log.Error(ex, "TestMultiplayer: Failed to start test server process");
                return false;
            }

            if (serverProcess == null)
            {
                return false;
            }

            // Wait until we can connect to the server's TCPR port and start the client process
            bool connected = await TryConnectToServerTcprPortAsync(serverProcess, port);

            if (connected)
            {
                Log.Information("TestMultiplayer: Starting test client process");
                var clientProcess = StartKagProcess(FileLocations.ClientAutoStartScriptPath);

                if (clientProcess != null)
                {
                    // If client or server exits, close the other
                    clientProcess.EnableRaisingEvents = true;
                    serverProcess.EnableRaisingEvents = true;
                    clientProcess.Exited += (s, e) => CloseIfStillRunning(serverProcess);
                    serverProcess.Exited += (s, e) => CloseIfStillRunning(clientProcess);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (serverProcess.HasExited)
                {
                    Log.Warning("TestMultiplayer: Test server closed before we could connect");
                }
                else
                {
                    Log.Warning("TestMultiplayer: Failed all tries to connect to the test server");
                }
                return false;
            }
        }

        private int TryForceMultiplayerAutoConfigProperties(ConfigService configService)
        {
            var portProperty = new IntConfigProperty("sv_port", -1);
            var tcprProperty = new BoolConfigProperty("sv_tcpr", false);
            var passwordProperty = new StringConfigProperty("sv_rconpassword", "");

            bool readSuccess = configService.ReadConfigProperties(FileLocations.AutoConfigPath,
                tcprProperty,
                portProperty,
                passwordProperty
            );

            if (!readSuccess)
            {
                return -1;
            }

            if (tcprProperty.Value == false || passwordProperty.Value == "")
            {
                tcprProperty.Value = true;

                if (passwordProperty.Value == "")
                {
                    passwordProperty.Value = DEFAULT_RCONPASSWORD;
                }

                bool writeSuccess = configService.WriteConfigProperties(FileLocations.AutoConfigPath,
                    tcprProperty,
                    passwordProperty
                );

                if (!writeSuccess)
                {
                    return -1;
                }
            }

            return portProperty.Value;
        }

        private async Task<bool> TryConnectToServerTcprPortAsync(Process serverProcess, int port)
        {
            TcpClient tcpClient = new TcpClient(AddressFamily.InterNetwork);

            for (int i = 0; i < MAX_RETRIES; i++)
            {
                if (serverProcess.HasExited) break;

                await Task.Delay(RETRY_INTERVAL);

                try
                {
                    Log.Information("TryConnectToServerTcprPort: Attempting to connect to test server ({Tries})", i + 1);
                    await tcpClient.ConnectAsync("localhost", port);

                    if (tcpClient.Connected)
                    {
                        Log.Information("TryConnectToServerTcprPort: Sucessfully connected to test server");
                        break;
                    }
                }
                catch (SocketException) { }
            }

            bool connected = tcpClient.Connected;
            tcpClient.Close();

            return connected;
        }

        private void CloseIfStillRunning(Process process)
        {
            if (process != null && !process.HasExited)
            {
                process.CloseMainWindow();
            }
        }

        private Process StartKagProcess(string autostart)
        {
            return Process.Start(new ProcessStartInfo()
            {
                FileName = FileLocations.KagExecutablePath,
                Arguments = $"noautoupdate nolauncher autostart \"{autostart}\"",
                WorkingDirectory = FileLocations.KagDirectory
            });
        }
    }
}
