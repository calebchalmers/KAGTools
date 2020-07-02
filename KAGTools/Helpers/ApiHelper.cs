using KAGTools.Data.API;
using Newtonsoft.Json;
using Serilog;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace KAGTools.Helpers
{
    public static class ApiHelper
    {
        private static readonly string UrlPlayer = ConfigurationManager.AppSettings["ApiPlayerUrl"];
        private static readonly string UrlPlayerAvatar = ConfigurationManager.AppSettings["ApiPlayerAvatarUrl"];
        private static readonly string UrlServers = ConfigurationManager.AppSettings["ApiServersUrl"];
        private static readonly string UrlServer = ConfigurationManager.AppSettings["ApiServerUrl"];
        private static readonly string UrlServerMinimap = ConfigurationManager.AppSettings["ApiServerMinimapUrl"];

        private static readonly HttpClient httpClient;

        static ApiHelper()
        {
            httpClient = new HttpClient();
        }

        public static async Task<ApiPlayerResults> GetPlayer(string username, CancellationToken cancellationToken)
        {
            Log.Information("API: Requesting player info for {Username}", username);

            return await GetApiResultObject<ApiPlayerResults>(string.Format(UrlPlayer, username), cancellationToken);
        }

        public static async Task<ApiPlayerAvatarResults> GetPlayerAvatarInfo(string username, CancellationToken cancellationToken)
        {
            Log.Information("API: Requesting avatar info of player {Username}", username);
            
            return await GetApiResultObject<ApiPlayerAvatarResults>(string.Format(UrlPlayerAvatar, username), cancellationToken);
        }

        public static async Task<ApiServer[]> GetServerList(ApiFilter[] filters, CancellationToken cancellationToken)
        {
            Log.Information("API: Requesting server list with filters: {@Filters}", filters);

            string filterJson = "?filters=" + JsonConvert.SerializeObject(filters);
            var results = await GetApiResultObject<ApiServerResults>(UrlServers + filterJson, cancellationToken);
            return results.Servers;
        }

        public static async Task<ApiServer> GetServer(string ip, string port, CancellationToken cancellationToken)
        {
            Log.Information("API: Requesting server info for {Ip:l}:{Port:l}", ip, port);

            string requestUri = string.Format(UrlServer, ip, port);
            var results = await GetApiResultObject<ApiServerResults>(requestUri, cancellationToken);
            return results.Server;
        }

        public static async Task<Stream> GetServerMinimapStream(string ip, string port, CancellationToken cancellationToken)
        {
            Log.Information("API: Requesting minimap stream from server at {Ip:l}:{Port:l}", ip, port);

            string requestUri = string.Format(UrlServerMinimap, ip, port);
            return await HttpGetStream(requestUri, cancellationToken);
        }

        private static async Task<T> GetApiResultObject<T>(string requestUri, CancellationToken cancellationToken) where T : class
        {
            string json = await HttpGetString(requestUri, cancellationToken);
            return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            });
        }

        private static async Task<string> HttpGetString(string requestUri, CancellationToken cancellationToken)
        {
            return await (await HttpGetContent(requestUri, cancellationToken)).ReadAsStringAsync();
        }

        private static async Task<Stream> HttpGetStream(string requestUri, CancellationToken cancellationToken)
        {
            var content = await HttpGetContent(requestUri, cancellationToken);
            return await content.ReadAsStreamAsync();
        }

        // NOTE: This is the only method that actually makes HTTP requests
        private static async Task<HttpContent> HttpGetContent(string requestUri, CancellationToken cancellationToken)
        {
            try
            {
                var result = await httpClient.GetAsync(requestUri, cancellationToken);
                return result.EnsureSuccessStatusCode().Content;
            }
            catch (HttpRequestException ex)
            {
                Log.Error(ex, "API: Failed HTTP GET request to {RequestUri}", requestUri);
                throw;
            }
        }
    }
}
