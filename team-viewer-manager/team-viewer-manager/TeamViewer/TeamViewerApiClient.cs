using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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

        #region Devices

        public async Task<List<Device>> GetDevices() {
            var response = await this.client_.GetAsync("api/v1/devices");
            if (response.StatusCode != System.Net.HttpStatusCode.OK) {
                throw new Exception($"Get devices failed with status code {response.StatusCode} ({(int)response.StatusCode})");
            }
            var responseJson = await response.Content.ReadAsStringAsync();
            dynamic responseJsonObject = JsonConvert.DeserializeObject(responseJson);
            List<dynamic> devices = responseJsonObject.devices.ToObject<List<dynamic>>();
            if (devices is null) {
                throw new Exception($"Get devices failed.");
            }
            var result = new List<Device>();
            foreach (var device in devices) {
                string onlineState = device.online_state;
                string supportedFeatures = device.supported_features;
                result.Add(new Device() {
                    DeviceId = device.device_id,
                    RemoteControlId = device.remotecontrol_id,
                    GroupId = device.groupid,
                    Alias = device.alias,
                    Description = device.description,
                    OnlineState = this.convertToOnlineState_(onlineState),
                    SupportedFeatures = this.convertToSupportedFeatures_(supportedFeatures),
                    IsAssignedToCurrentUser = device.assigned_to,
                });
            }
            return result;
        }

        private OnlineState convertToOnlineState_(string value) {
            if (value?.ToLower()?.Trim() == "online") {
                return OnlineState.Online;
            } else {
                return OnlineState.Offline;
            }
        }

        private Feature convertToSupportedFeatures_(string value) {
            if (value is null) {
                return Feature.None;
            }
            var values = value
                .Split(',')
                .Select(x => x.Trim().ToLower())
                .ToList();
            var result = Feature.None;
            if (value.Contains("chat")) {
                result |= Feature.Chat;
            }
            if (value.Contains("remote_control")) {
                result |= Feature.RemoteControl;
            }
            return result;
        }

        #endregion Devices
    }
}