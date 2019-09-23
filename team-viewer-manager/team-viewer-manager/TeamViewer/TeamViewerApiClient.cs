using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace team_viewer_manager.TeamViewer {

    public class TeamViewerApiClient {

        #region properties

        private readonly HttpClient client_;

        #endregion properties

        #region constructors and destructor

        public TeamViewerApiClient()
            : this("https://webapi.teamviewer.com/") { }

        public TeamViewerApiClient(string baseUrl)
            : this(new Uri(baseUrl)) { }

        public TeamViewerApiClient(Uri baseUrl) {
            this.client_ = new HttpClient();
            this.client_.BaseAddress = baseUrl;
            this.client_.DefaultRequestHeaders.Accept.Clear();
            this.client_.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        #endregion constructors and destructor

        // api/v1/devices

        #region Authorization

        public async Task<bool> Authorize(string token) {
            this.client_.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await this.client_.GetAsync("api/v1/ping");
            if (response.StatusCode != System.Net.HttpStatusCode.OK) {
                throw new Exception($"Login failed with status code {response.StatusCode} ({(int)response.StatusCode})");
            }
            var responseJson = await response.Content.ReadAsStringAsync();
            dynamic responseJsonObject = JsonConvert.DeserializeObject(responseJson);
            return (bool)responseJsonObject.token_valid == true;
        }

        #endregion Authorization
    }
}