using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using KAGTools.Data;
using KAGTools.Data.API;
using KAGTools.Helpers;
using KAGTools.Services;
using System;
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

        private CancellationTokenSource AvatarCancellationSource { get; set; }

        private ApiService ApiService { get; }

        public ApiPlayerInfoViewModel(ApiService apiService)
        {
            ApiService = apiService;

            SearchCommand = new RelayCommand(ExecuteSearchCommand);
        }

        public string SearchFilter
        {
            get => _searchFilter;
            set => Set(ref _searchFilter, value);
        }

        public ApiPlayerResults ResultPlayer
        {
            get => _resultPlayer;
            set => Set(ref _resultPlayer, value);
        }

        public BitmapImage AvatarBitmap
        {
            get => _avatarBitmap;
            set => Set(ref _avatarBitmap, value);
        }

        public ApiServer ResultPlayerServer
        {
            get => _resultPlayerServer;
            set => Set(ref _resultPlayerServer, value);
        }

        public AsyncTaskState SearchState
        {
            get => _searchState;
            set => Set(ref _searchState, value);
        }

        public AsyncTaskState ResultPlayerServerState
        {
            get => _resultPlayerServerState;
            set => Set(ref _resultPlayerServerState, value);
        }

        public AsyncTaskState AvatarState
        {
            get => _avatarState;
            set => Set(ref _avatarState, value);
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
                ResultPlayer = await ApiService.GetPlayer(SearchFilter, CancellationToken.None);
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
                ResultPlayerServer = await ApiService.GetServer(playerServer.IPv4Address, playerServer.Port, CancellationToken.None);
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
                AvatarCancellationSource?.Cancel();
                AvatarCancellationSource = new CancellationTokenSource();
                AvatarState = AsyncTaskState.Running;

                ApiPlayerAvatarResults results = await ApiService.GetPlayerAvatarInfo(SearchFilter, AvatarCancellationSource.Token);

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
