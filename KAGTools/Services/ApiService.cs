using KAGTools.Data.API;
using Newtonsoft.Json;
using Serilog;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace KAGTools.Services
{
    public class ApiService
    {
        public string PlayerUrlTemplate { get; set; }
        public string PlayerAvatarUrlTemplate { get; set; }
        public string ServerListUrlTemplate { get; set; }
        public string ServerUrlTemplate { get; set; }
        public string ServerMinimapUrlTemplate { get; set; }

        private HttpClient HttpClient { get; }

        public ApiService(string playerUrlTemplate, string playerAvatarUrlTemplate, string serverListUrlTemplate, string serverUrlTemplate, string serverMinimapUrlTemplate)
        {
            PlayerUrlTemplate = playerUrlTemplate;
            PlayerAvatarUrlTemplate = playerAvatarUrlTemplate;
            ServerListUrlTemplate = serverListUrlTemplate;
            ServerUrlTemplate = serverUrlTemplate;
            ServerMinimapUrlTemplate = serverMinimapUrlTemplate;

            // Keep this for the duration of the app's life
            HttpClient = new HttpClient();
        }

        public async Task<ApiPlayerResults> GetPlayer(string username, CancellationToken cancellationToken)
        {
            Log.Information("API: Requesting player info for {Username}", username);

            return await GetApiResultObject<ApiPlayerResults>(string.Format(PlayerUrlTemplate, username), cancellationToken);
        }

        public async Task<ApiPlayerAvatarResults> GetPlayerAvatarInfo(string username, CancellationToken cancellationToken)
        {
            Log.Information("API: Requesting avatar info of player {Username}", username);

            return await GetApiResultObject<ApiPlayerAvatarResults>(string.Format(PlayerAvatarUrlTemplate, username), cancellationToken);
        }

        public async Task<ApiServer[]> GetServerList(ApiFilter[] filters, CancellationToken cancellationToken)
        {
            Log.Information("API: Requesting server list with filters: {@Filters}", filters);

            string filtersJson = JsonConvert.SerializeObject(filters);
            var results = await GetApiResultObject<ApiServerResults>(string.Format(ServerListUrlTemplate, filtersJson), cancellationToken);
            return results.Servers;
        }

        public async Task<ApiServer> GetServer(string ip, string port, CancellationToken cancellationToken)
        {
            Log.Information("API: Requesting server info for {Ip:l}:{Port:l}", ip, port);

            string requestUri = string.Format(ServerUrlTemplate, ip, port);
            var results = await GetApiResultObject<ApiServerResults>(requestUri, cancellationToken);
            return results.Server;
        }

        public async Task<Stream> GetServerMinimapStream(string ip, string port, CancellationToken cancellationToken)
        {
            Log.Information("API: Requesting minimap stream from server at {Ip:l}:{Port:l}", ip, port);

            string requestUri = string.Format(ServerMinimapUrlTemplate, ip, port);
            return await HttpGetStream(requestUri, cancellationToken);
        }

        private async Task<T> GetApiResultObject<T>(string requestUri, CancellationToken cancellationToken) where T : class
        {
            string json = await HttpGetString(requestUri, cancellationToken);
            return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            });
        }

        private async Task<string> HttpGetString(string requestUri, CancellationToken cancellationToken)
        {
            return await (await HttpGetContent(requestUri, cancellationToken)).ReadAsStringAsync();
        }

        private async Task<Stream> HttpGetStream(string requestUri, CancellationToken cancellationToken)
        {
            var content = await HttpGetContent(requestUri, cancellationToken);
            return await content.ReadAsStreamAsync();
        }

        // NOTE: This is the only method that actually makes HTTP requests
        private async Task<HttpContent> HttpGetContent(string requestUri, CancellationToken cancellationToken)
        {
            try
            {
                var result = await HttpClient.GetAsync(requestUri, cancellationToken);
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
