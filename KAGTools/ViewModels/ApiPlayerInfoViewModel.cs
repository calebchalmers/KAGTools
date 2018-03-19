using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using KAGTools.Data.API;
using KAGTools.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace KAGTools.ViewModels
{
    public class ApiPlayerInfoViewModel : ViewModelBase
    {
        private ApiPlayerResults _resultPlayer;
        private ApiServer _resultPlayerServer;
        private BitmapImage _avatarBitmap;
        private string _searchFilter = "";

        private CancellationTokenSource AvatarCancellationSource { get; set; } = new CancellationTokenSource();

        public ApiPlayerInfoViewModel() :
            base()
        {
            SearchCommand = new RelayCommand(ExecuteSearchCommand);
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

        public BitmapImage AvatarBitmap
        {
            get { return _avatarBitmap; }
            set
            {
                if (_avatarBitmap != value)
                {
                    _avatarBitmap = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ApiServer ResultPlayerServer
        {
            get { return _resultPlayerServer; }
            set
            {
                if (_resultPlayerServer != value)
                {
                    _resultPlayerServer = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ICommand SearchCommand { get; private set; }

        private async void ExecuteSearchCommand()
        {
            if (string.IsNullOrWhiteSpace(SearchFilter))
                return;

            await Task.WhenAll(FindResultPlayer(), FindAvatarBitmap());
        }

        private async Task FindResultPlayer()
        {
            try
            {
                ResultPlayer = await ApiHelper.GetPlayer(SearchFilter, CancellationToken.None);
            }
            catch (System.Net.Http.HttpRequestException)
            {
                ResultPlayer = null;
            }

            ResultPlayerServer = null;

            try
            {
                var playerServer = ResultPlayer?.Status.Server;
                if (playerServer == null) return;
                ResultPlayerServer = await ApiHelper.GetServer(playerServer.IPv4Address, playerServer.Port, CancellationToken.None);
            }
            catch (System.Net.Http.HttpRequestException) { }
        }

        private async Task FindAvatarBitmap()
        {
            try
            {
                AvatarCancellationSource.Cancel();
                AvatarCancellationSource = new CancellationTokenSource();

                ApiPlayerAvatarResults results = await ApiHelper.GetPlayerAvatar(SearchFilter, AvatarCancellationSource.Token);

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bitmap.CacheOption = BitmapCacheOption.Default;
                bitmap.UriSource = new Uri(results.LargeUrl);
                bitmap.EndInit();

                AvatarBitmap = bitmap;
            }
            catch (System.Net.Http.HttpRequestException)
            {
                AvatarBitmap = null;
            }
        }
    }
}
