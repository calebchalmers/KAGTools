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
            get => _searchFilter;
            set => this.SetProperty(ref _searchFilter, value);
        }

        public ApiPlayerResults ResultPlayer
        {
            get => _resultPlayer;
            set => this.SetProperty(ref _resultPlayer, value);
        }

        public BitmapImage AvatarBitmap
        {
            get => _avatarBitmap;
            set => this.SetProperty(ref _avatarBitmap, value);
        }

        public ApiServer ResultPlayerServer
        {
            get => _resultPlayerServer;
            set => this.SetProperty(ref _resultPlayerServer, value);
        }

        public AsyncTaskState SearchState
        {
            get => _searchState;
            set => this.SetProperty(ref _searchState, value);
        }

        public AsyncTaskState ResultPlayerServerState
        {
            get => _resultPlayerServerState;
            set => this.SetProperty(ref _resultPlayerServerState, value);
        }

        public AsyncTaskState AvatarState
        {
            get => _avatarState;
            set => this.SetProperty(ref _avatarState, value);
        }

        public ICommand SearchCommand { get; private set; }

        private async void ExecuteSearchCommand()
        {
            if (string.IsNullOrWhiteSpace(SearchFilter))
                return;
            if (SearchFilter.Equals(ResultPlayer?.Info.Username, StringComparison.OrdinalIgnoreCase))
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
