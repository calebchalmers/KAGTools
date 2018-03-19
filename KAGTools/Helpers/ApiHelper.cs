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
            return await GetApiResultObject<ApiPlayerResults>(string.Format(UrlPlayer, username), cancellationToken);
        }

        public static async Task<ApiServer[]> GetServers(ApiFilter[] filters, CancellationToken cancellationToken)
        {
            string filterJson = "?filters=" + JsonConvert.SerializeObject(filters);
            var results = await GetApiResultObject<ApiServerResults>(UrlServers + filterJson, cancellationToken);
            return results.Servers;
        }

        public static async Task<BitmapImage> GetServerMinimap(string ip, int port, CancellationToken cancellationToken)
        {
            string requestUri = string.Format(UrlServerMinimap, ip, port);
            var stream = await HttpGetStream(requestUri, cancellationToken);

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            bitmap.CacheOption = BitmapCacheOption.Default;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            return bitmap;
        }

        public static async Task<T> GetApiResultObject<T>(string requestUri, CancellationToken cancellationToken) where T : class
        {
            return JsonConvert.DeserializeObject<T>(await HttpGetString(requestUri, cancellationToken));
        }

        private static async Task<string> HttpGetString(string requestUri, CancellationToken cancellationToken)
        {
            return await (await HttpGetContent(requestUri, cancellationToken)).ReadAsStringAsync();
        }

        private static async Task<System.IO.Stream> HttpGetStream(string requestUri, CancellationToken cancellationToken)
        {
            return await (await HttpGetContent(requestUri, cancellationToken)).ReadAsStreamAsync();
        }

        private static async Task<byte[]> HttpGetByteArray(string requestUri, CancellationToken cancellationToken)
        {
            return await (await HttpGetContent(requestUri, cancellationToken)).ReadAsByteArrayAsync();
        }

        private static async Task<HttpContent> HttpGetContent(string requestUri, CancellationToken cancellationToken)
        {
            HttpResponseMessage result = null;

            try
            {
                result = await httpClient.GetAsync(requestUri, cancellationToken);
                return result.EnsureSuccessStatusCode().Content;
            }
            catch (HttpRequestException e) when (result?.StatusCode == HttpStatusCode.InternalServerError)
            {
                var message = string.Format("{0}{2}{2}Request URL: {1}", e.Message, requestUri, Environment.NewLine);
                MessageBox.Show(message, "HTTP Request Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }
    }
}
