using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
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

        public static async Task<ApiPlayerResults> GetPlayer(string username)
        {
            return await HttpGetApiResult<ApiPlayerResults>(string.Format(UrlPlayer, username));
        }

        public static async Task<ApiServer[]> GetServers(params ApiFilter[] filters)
        {
            string filterJson = "?filters=" + JsonConvert.SerializeObject(filters);
            var results = await HttpGetApiResult<ApiServerResults>(UrlServers + filterJson);
            return results?.Servers ?? Array.Empty<ApiServer>();
        }

        public static async Task<BitmapImage> GetServerMinimap(string ip, int port)
        {
            string requestUri = string.Format(UrlServerMinimap, ip, port);
            var stream = await (await HttpGetContent(requestUri))?.ReadAsStreamAsync();
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

        private static async Task<T> HttpGetApiResult<T>(string requestUri) where T : class
        {
            string data = await (await HttpGetContent(requestUri))?.ReadAsStringAsync();
            return data != null ? JsonConvert.DeserializeObject<T>(data) : null;
        }

        private static async Task<HttpContent> HttpGetContent(string requestUri)
        {
            try
            {
                var result = await httpClient.GetAsync(requestUri);
                result.EnsureSuccessStatusCode();
                return result.Content;
            }
            catch (HttpRequestException e)
            {
                var message = string.Format("{0}{2}{2}Request URL: {1}", e.Message, requestUri, Environment.NewLine);
                MessageBox.Show(message, "HTTP Request Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return null;
        }
    }
}
