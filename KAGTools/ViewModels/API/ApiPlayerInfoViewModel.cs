using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using KAGTools.Data;
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

namespace KAGTools.ViewModels.API
{
    public class ApiPlayerInfoViewModel : ViewModelBase
    {
        private ApiPlayerResults _resultPlayer;
        private ApiServer _resultPlayerServer;
        private BitmapImage _avatarBitmap;
        private string _searchFilter = "";
        private AsyncTaskState _searchState = AsyncTaskState.Standby;
        private AsyncTaskState _resultPlayerServerState = AsyncTaskState.Standby;
        private AsyncTaskState _avatarState = AsyncTaskState.Standby;

        private CancellationTokenSource AvatarCancellationSource { get; set; } = new CancellationTokenSource();

        public ApiPlayerInfoViewModel()
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

        public AsyncTaskState SearchState
        {
            get { return _searchState; }
            set
            {
                if (_searchState != value)
                {
                    _searchState = value;
                    RaisePropertyChanged();
                }
            }
        }

        public AsyncTaskState ResultPlayerServerState
        {
            get { return _resultPlayerServerState; }
            set
            {
                if (_resultPlayerServerState != value)
                {
                    _resultPlayerServerState = value;
                    RaisePropertyChanged();
                }
            }
        }

        public AsyncTaskState AvatarState
        {
            get { return _avatarState; }
            set
            {
                if (_avatarState != value)
                {
                    _avatarState = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ICommand SearchCommand { get; private set; }

        private async void ExecuteSearchCommand()
        {
            if (string.IsNullOrWhiteSpace(SearchFilter))
                return;

            await FindResultPlayer();

            if (ResultPlayer != null)
            {
                await Task.WhenAll(FindResultPlayerServer(), FindAvatarBitmap());
            }
        }

        private async Task FindResultPlayer()
        {
            ResultPlayer = null;

            try
            {
                SearchState = AsyncTaskState.Running;
                ResultPlayer = await ApiHelper.GetPlayer(SearchFilter, CancellationToken.None);
                SearchState = AsyncTaskState.Finished;
            }
            catch (System.Net.Http.HttpRequestException)
            {
                SearchState = AsyncTaskState.Failed;
            }
        }

        private async Task FindResultPlayerServer()
        {
            ResultPlayerServer = null;

            try
            {
                var playerServer = ResultPlayer?.Status.Server;

                if (playerServer == null)
                {
                    ResultPlayerServerState = AsyncTaskState.Failed;
                    return;
                }

                ResultPlayerServerState = AsyncTaskState.Running;
                ResultPlayerServer = await ApiHelper.GetServer(playerServer.IPv4Address, playerServer.Port, CancellationToken.None);
                ResultPlayerServerState = AsyncTaskState.Finished;
            }
            catch (System.Net.Http.HttpRequestException)
            {
                ResultPlayerServerState = AsyncTaskState.Failed;
            }
        }

        private async Task FindAvatarBitmap()
        {
            AvatarBitmap = null;

            try
            {
                AvatarCancellationSource.Cancel();
                AvatarCancellationSource = new CancellationTokenSource();
                AvatarState = AsyncTaskState.Running;

                ApiPlayerAvatarResults results = await ApiHelper.GetPlayerAvatar(SearchFilter, AvatarCancellationSource.Token);

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bitmap.CacheOption = BitmapCacheOption.Default;
                bitmap.UriSource = new Uri(results.LargeUrl);
                bitmap.EndInit();

                AvatarBitmap = bitmap;
                AvatarState = AsyncTaskState.Finished;
            }
            catch (System.Net.Http.HttpRequestException)
            {
                AvatarState = AsyncTaskState.Failed;
            }
        }
    }
}
