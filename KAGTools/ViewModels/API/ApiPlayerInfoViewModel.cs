using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using KAGTools.Data;
using KAGTools.Data.API;
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
        private string _searchInput = "";
        private AsyncTaskState _searchState = AsyncTaskState.Standby;
        private AsyncTaskState _resultPlayerServerState = AsyncTaskState.Standby;
        private AsyncTaskState _avatarState = AsyncTaskState.Standby;

        private CancellationTokenSource AvatarCancellationSource { get; set; }

        private IApiService ApiService { get; }

        public ApiPlayerInfoViewModel(IApiService apiService)
        {
            ApiService = apiService;

            SearchCommand = new RelayCommand(ExecuteSearchCommand);
        }

        public string SearchInput
        {
            get => _searchInput;
            set => Set(ref _searchInput, value);
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
            if (string.IsNullOrWhiteSpace(SearchInput))
                return;
            if (SearchInput.Equals(ResultPlayer?.Info.Username, StringComparison.OrdinalIgnoreCase))
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
                ResultPlayer = await ApiService.GetPlayer(SearchInput, CancellationToken.None);
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

                ApiPlayerAvatarResults results = await ApiService.GetPlayerAvatarInfo(SearchInput, AvatarCancellationSource.Token);

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
