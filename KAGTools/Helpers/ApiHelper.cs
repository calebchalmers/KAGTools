using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KAGTools.Data;
using Newtonsoft.Json;

namespace KAGTools.Helpers
{
    public static class ApiHelper
    {
        private const string UrlPlayer = "https://api.kag2d.com/v1/player/{0}";
        private const string UrlServers = "https://api.kag2d.com/v1/game/thd/kag/servers";

        private static readonly HttpClient httpClient;

        static ApiHelper()
        {
            httpClient = new HttpClient();
        }

        public static async Task<ApiPlayerResults> GetPlayer(string username)
        {
            return await HttpGetApiResult<ApiPlayerResults>(string.Format(UrlPlayer, username));
        }

        public static async Task<ApiServerResults> GetServers(params ApiFilter[] filters)
        {
            string filterJson = "?filters=" + JsonConvert.SerializeObject(filters);
            return await HttpGetApiResult<ApiServerResults>(UrlServers + filterJson);
        }

        private static async Task<T> HttpGetApiResult<T>(string requestUri) where T : class
        {
            try
            {
                string data = await httpClient.GetStringAsync(requestUri);
                return data != null ? JsonConvert.DeserializeObject<T>(data) : null;
            }
            catch (HttpRequestException e)
            {
                MessageBox.Show(string.Format("{0}{2}{2}Request URL: {1}", e.Message, requestUri, Environment.NewLine), "HTTP Request Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return null;
        }
    }
}
