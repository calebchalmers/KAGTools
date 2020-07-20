using GalaSoft.MvvmLight;
using KAGTools.Data;
using KAGTools.Data.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace KAGTools
{
    public interface IWindowService
    {
        void Register<TWindow, TViewModel>(Func<TViewModel> viewModelCreator)
            where TWindow : Window
            where TViewModel : ViewModelBase;
        TViewModel OpenWindow<TViewModel>(bool modal = false, bool forceNew = false)
            where TViewModel : ViewModelBase;
        void OpenInExplorer(string path);
        void Alert(string message, string title = null, bool error = false);
    }

    public interface IConfigService
    {
        bool ReadConfigProperties(string filePath, params BaseConfigProperty[] configProperties);
        bool WriteConfigProperties(string filePath, params BaseConfigProperty[] configProperties);
        string FindGamemodeOfMod(Mod mod);
    }

    public interface IModsService
    {
        IEnumerable<Mod> EnumerateAllMods();
        IEnumerable<Mod> EnumerateActiveMods();
        bool WriteActiveMods(IEnumerable<Mod> activeMods);
        Mod CreateNewMod(string name);
    }

    public interface IManualService
    {
        ManualDocument[] ManualDocuments { get; set; }
        IEnumerable<ManualItem> EnumerateManualDocument(ManualDocument document);
    }

    public interface ITestService
    {
        bool TestSolo();
        Task<bool> TestMultiplayerAsync(int port, bool syncClientServerClosing);
        bool TryFixMultiplayerAutoConfigProperties(IConfigService configService);
    }

    public interface IApiService
    {
        Task<ApiPlayerResults> GetPlayer(string username, CancellationToken cancellationToken);
        Task<ApiPlayerAvatarResults> GetPlayerAvatarInfo(string username, CancellationToken cancellationToken);
        Task<ApiServer[]> GetServerList(ApiFilter[] filters, CancellationToken cancellationToken);
        Task<ApiServer> GetServer(string ip, string port, CancellationToken cancellationToken);
        Task<Stream> GetServerMinimapStream(string ip, string port, CancellationToken cancellationToken);
    }
}
