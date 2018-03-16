using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using KAGTools.Data.API;
using Newtonsoft.Json;

namespace KAGTools.Helpers
{
    public static class ApiHelper
    {
        private const string UrlPlayer = "https://api.kag2d.com/v1/player/{0}";
        private const string UrlServers = "https://api.kag2d.com/v1/game/thd/kag/servers";
        private const string UrlServerMinimap = "https://api.kag2d.com/v1/game/thd/kag/server/{0}/{1}/minimap";

        private static readonly HttpClient httpClient;

        static ApiHelper()
        {
            httpClient = new HttpClient();
        }

        public static async Task<ApiPlayerResults> GetPlayer(string username, CancellationToken cancellationToken)
        {
            return await HttpGetApiResult<ApiPlayerResults>(string.Format(UrlPlayer, username), cancellationToken);
        }

        public static async Task<ApiServer[]> GetServers(ApiFilter[] filters, CancellationToken cancellationToken)
        {
            string filterJson = "?filters=" + JsonConvert.SerializeObject(filters);
            var results = await HttpGetApiResult<ApiServerResults>(UrlServers + filterJson, cancellationToken);
            return results?.Servers ?? Array.Empty<ApiServer>();
        }

        public static async Task<BitmapImage> GetServerMinimap(string ip, int port, CancellationToken cancellationToken)
        {
            string requestUri = string.Format(UrlServerMinimap, ip, port);
            Task<System.IO.Stream> task = (await HttpGetContent(requestUri, cancellationToken))?.ReadAsStreamAsync();
            if (task == null) return null;
            System.IO.Stream stream = await task;
            if (stream == null) return null;

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            bitmap.CacheOption = BitmapCacheOption.Default;
            bitmap.UriSource = null;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            return bitmap;
        }

        private static async Task<T> HttpGetApiResult<T>(string requestUri, CancellationToken cancellationToken) where T : class
        {
            Task<string> task = (await HttpGetContent(requestUri, cancellationToken))?.ReadAsStringAsync();
            if (task == null) return null;
            return JsonConvert.DeserializeObject<T>(await task);
        }

        private static async Task<HttpContent> HttpGetContent(string requestUri, CancellationToken cancellationToken)
        {
            try
            {
                var result = await httpClient.GetAsync(requestUri, cancellationToken);

                if(result.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                result.EnsureSuccessStatusCode();
                return result.Content;
            }
            catch (HttpRequestException e)
            {
                var message = string.Format("{0}{2}{2}Request URL: {1}", e.Message, requestUri, Environment.NewLine);
                MessageBox.Show(message, "HTTP Request Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TaskCanceledException)
            {
                return null;
            }

            return null;
        }
    }
}
