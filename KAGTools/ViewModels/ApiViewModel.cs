using GalaSoft.MvvmLight;
using KAGTools.Services;
using KAGTools.ViewModels.API;

namespace KAGTools.ViewModels
{
    public class ApiViewModel : ViewModelBase
    {
        private ApiService ApiService { get; }

        public ApiViewModel(ApiService apiService)
        {
            ApiService = apiService;

            ApiServerBrowserViewModel = new ApiServerBrowserViewModel(apiService);
            ApiPlayerInfoViewModel = new ApiPlayerInfoViewModel(apiService);
        }

        public ApiServerBrowserViewModel ApiServerBrowserViewModel { get; }
        public ApiPlayerInfoViewModel ApiPlayerInfoViewModel { get; }
    }
}
