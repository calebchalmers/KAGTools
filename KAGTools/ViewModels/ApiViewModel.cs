using GalaSoft.MvvmLight;
using KAGTools.ViewModels.API;

namespace KAGTools.ViewModels
{
    public class ApiViewModel : ViewModelBase
    {
        private IApiService ApiService { get; }

        public ApiViewModel(IApiService apiService)
        {
            ApiService = apiService;

            ApiServerBrowserViewModel = new ApiServerBrowserViewModel(apiService);
            ApiPlayerInfoViewModel = new ApiPlayerInfoViewModel(apiService);
        }

        public ApiServerBrowserViewModel ApiServerBrowserViewModel { get; }
        public ApiPlayerInfoViewModel ApiPlayerInfoViewModel { get; }
    }
}
