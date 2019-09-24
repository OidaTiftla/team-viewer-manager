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

        #endregion Devices

        #region Contacts

        public async Task<List<Contact>> GetContacts() {
            var response = await this.client_.GetAsync("api/v1/contacts");
            if (response.StatusCode != System.Net.HttpStatusCode.OK) {
                throw new Exception($"Get contacts failed with status code {response.StatusCode} ({(int)response.StatusCode})");
            }
            var responseJson = await response.Content.ReadAsStringAsync();
            dynamic responseJsonObject = JsonConvert.DeserializeObject(responseJson);
            List<dynamic> contacts = responseJsonObject.contacts.ToObject<List<dynamic>>();
            if (contacts is null) {
                throw new Exception($"Get contacts failed.");
            }
            var result = new List<Contact>();
            foreach (var contact in contacts) {
                string onlineState = contact.online_state;
                string supportedFeatures = contact.supported_features;
                result.Add(new Contact() {
                    ContactId = contact.contact_id,
                    UserId = contact.user_id,
                    Name = contact.name,
                    GroupId = contact.groupid,
                    Description = contact.description,
                    OnlineState = this.convertToOnlineState_(onlineState),
                    ProfilePictureUrl = contact.profilepicture_url,
                    SupportedFeatures = this.convertToSupportedFeatures_(supportedFeatures),
                });
            }
            return result;
        }

        #endregion Contacts

        #region Groups

        public async Task<List<Group>> GetGroups() {
            var response = await this.client_.GetAsync("api/v1/groups");
            if (response.StatusCode != System.Net.HttpStatusCode.OK) {
                throw new Exception($"Get groups failed with status code {response.StatusCode} ({(int)response.StatusCode})");
            }
            var responseJson = await response.Content.ReadAsStringAsync();
            dynamic responseJsonObject = JsonConvert.DeserializeObject(responseJson);
            List<dynamic> groups = responseJsonObject.groups.ToObject<List<dynamic>>();
            if (groups is null) {
                throw new Exception($"Get groups failed.");
            }
            var result = new List<Group>();
            foreach (var group in groups) {
                var resultShares = new List<GroupShare>();
                if (group.shared_with != null) {
                    List<dynamic> shares = group.shared_with.ToObject<List<dynamic>>();
                    if (shares != null) {
                        foreach (var share in shares) {
                            string sharePermissions = share.permissions;
                            resultShares.Add(new GroupShare() {
                                UserId = share.userid,
                                Name = share.name,
                                Permissions = this.convertToPermission_(sharePermissions),
                                IsPending = share.pending,
                            });
                        }
                    }
                }
                dynamic owner = group.owner;
                GroupOwner resultOwner = null;
                if (owner != null) {
                    resultOwner = new GroupOwner() {
                        UserId = owner.userid,
                        Name = owner.name,
                    };
                }
                string permissions = group.permissions;
                result.Add(new Group() {
                    GroupId = group.id,
                    Name = group.name,
                    SharedWith = resultShares,
                    Owner = resultOwner,
                    Permissions = this.convertToPermission_(permissions),
                });
            }
            return result;
        }

        #endregion Groups

        #region helpers

        private OnlineState convertToOnlineState_(string value) {
            if (value?.ToLower()?.Trim() == "online") {
                return OnlineState.Online;
            } else if (value?.ToLower()?.Trim() == "busy") {
                return OnlineState.Busy;
            } else if (value?.ToLower()?.Trim() == "away") {
                return OnlineState.Away;
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
            if (value.Contains("meeting")) {
                result |= Feature.Meeting;
            }
            if (value.Contains("videocall")) {
                result |= Feature.VideoCall;
            }
            return result;
        }

        private Permission convertToPermission_(string value) {
            if (value?.ToLower()?.Trim() == "read") {
                return Permission.Read;
            } else if (value?.ToLower()?.Trim() == "read-write"
                || value?.ToLower()?.Trim() == "readwrite") {
                return Permission.ReadWrite;
            } else if (value?.ToLower()?.Trim() == "owned") {
                return Permission.Owned;
            } else {
                return Permission.Read;
            }
        }

        #endregion helpers
    }
}