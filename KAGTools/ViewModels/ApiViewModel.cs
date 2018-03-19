using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using KAGTools.Data.API;
using KAGTools.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace KAGTools.ViewModels
{
    public class ApiViewModel : FilterListViewModelBase<ApiServer>
    {
        private BitmapImage _serverMinimapBitmap;
        private ApiPlayerResults _resultPlayer;
        private string _serverSearchFilter = "";
        private string _playerSearchFilter = "";

        private CancellationTokenSource MinimapCancellationSource { get; set; } = new CancellationTokenSource();
        private bool RefreshingServers { get; set; } = false;

        public ApiViewModel() :
            base()
        {
            RefreshServersCommand = new RelayCommand(ExecuteRefreshServersCommand);
            SearchPlayerCommand = new RelayCommand(ExecuteSearchPlayerCommand);

            SortDescriptions.Add(new SortDescription("PlayerCount", ListSortDirection.Descending));

            PropertyChanged += async (s, e) =>
            {
                if(e.PropertyName == "Selected" && Selected != null)
                {
                    await UpdateMinimapAsync();
                }
            };

            var tmp = RefreshServersAsync();
        }

        protected override bool FilterItem(ApiServer item)
        {
            return item.Name.IndexOf(ServerSearchFilter, StringComparison.OrdinalIgnoreCase) >= 0;
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

        public string ServerSearchFilter
        {
            get { return _serverSearchFilter; }
            set
            {
                if (_serverSearchFilter != value)
                {
                    _serverSearchFilter = value;
                    RaisePropertyChanged();
                    RefreshFilters();
                }
            }
        }

        public string PlayerSearchFilter
        {
            get { return _playerSearchFilter; }
            set
            {
                if (_playerSearchFilter != value)
                {
                    _playerSearchFilter = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int ServerCount
        {
            get { return Items.Count; }
        }

        public int PlayerCount
        {
            get {
                int total = 0;
                foreach(int count in Items.Select(s => s.PlayerCount))
                {
                    total += count;
                }
                return total;
            }
        }

        public ApiPlayerResults ResultPlayer
        {
            get { return _resultPlayer; }
            set
            {
                if (_resultPlayer != value)
                {
                    _resultPlayer = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ICommand RefreshServersCommand { get; private set; }
        public ICommand SearchPlayerCommand { get; private set; }

        private async void ExecuteRefreshServersCommand()
        {
            await RefreshServersAsync();
        }

        private async void ExecuteSearchPlayerCommand()
        {
            if (string.IsNullOrWhiteSpace(PlayerSearchFilter))
                return;

            string username = PlayerSearchFilter;

            try
            {
                ResultPlayer = await ApiHelper.GetPlayer(username, CancellationToken.None);
            }
            catch (System.Net.Http.HttpRequestException)
            {
                ResultPlayer = null;
            }
        }

        private async Task RefreshServersAsync()
        {
            if (RefreshingServers) return;
            RefreshingServers = true;

            Items.Clear();

            try
            {
                ApiServer[] results = await ApiHelper.GetServers(
                    new ApiFilter[] {
                    new ApiFilter("current", true)
                    },
                    CancellationToken.None
                    );
                Items = new ObservableCollection<ApiServer>(results);
            }
            catch (System.Net.Http.HttpRequestException) { }

            RefreshingServers = false;

            RaisePropertyChanged("ServerCount");
            RaisePropertyChanged("PlayerCount");
        }

        private async Task UpdateMinimapAsync()
        {
            MinimapCancellationSource.Cancel();
            MinimapCancellationSource = new CancellationTokenSource();

            ServerMinimapBitmap = null;

            try
            {
                var bitmap = await ApiHelper.GetServerMinimap(Selected.IPv4Address, Selected.Port, MinimapCancellationSource.Token);
                ServerMinimapBitmap = bitmap;
            }
            catch (TaskCanceledException) { }
            catch (System.Net.Http.HttpRequestException) { }
        }
    }
}
