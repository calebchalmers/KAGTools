using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using KAGTools.Data.API;
using KAGTools.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace KAGTools.ViewModels
{
    public class ApiViewModel : ViewModelBase
    {
        private ObservableCollection<ApiServer> _servers = null;
        private ApiServer _selectedServer;
        private BitmapImage _serverMinimapBitmap;
        private string _searchFilter = "";

        public ApiViewModel()
        {
            RefreshServersCommand = new RelayCommand(ExecuteRefreshServersCommand);
            RefreshServersAsync();
        }

        public ObservableCollection<ApiServer> Servers
        {
            get { return _servers; }
            set
            {
                if(_servers != value)
                {
                    _servers = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged("ServerCount");
                }
            }
        }

        public ApiServer SelectedServer
        {
            get { return _selectedServer; }
            set
            {
                if (_selectedServer != value)
                {
                    _selectedServer = value;
                    RaisePropertyChanged();

                    UpdateMinimap();
                }
            }
        }

        public BitmapImage ServerMinimapBitmap
        {
            get { return _serverMinimapBitmap; }
            set
            {
                if (_serverMinimapBitmap != value)
                {
                    _serverMinimapBitmap = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string SearchFilter
        {
            get { return _searchFilter; }
            set
            {
                if (_searchFilter != value)
                {
                    _searchFilter = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int ServerCount
        {
            get { return Servers?.Count ?? 0; }
        }

        public ICommand RefreshServersCommand { get; private set; }

        private async void ExecuteRefreshServersCommand()
        {
            await RefreshServersAsync();
        }

        private async Task RefreshServersAsync()
        {
            Servers = null;
            ApiServerResults results = await ApiHelper.GetServers(
                new ApiFilter("current", true)
                );
            if(results != null)
            {
                Servers = new ObservableCollection<ApiServer>(results.Servers.Where(s => s.Name.ToLower().Contains(SearchFilter.ToLower())).OrderByDescending(s => s.PlayerCount));
            }
        }

        private async void UpdateMinimap()
        {
            ServerMinimapBitmap = null;
            ServerMinimapBitmap = await ApiHelper.GetServerMinimap(SelectedServer.IPv4Address, SelectedServer.Port);
        }
    }
}
