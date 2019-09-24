using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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

        public async Task<Device> AddDevice(string remoteControlId, string groupId, string description = null, string alias = null, string password = null) {
            var o = new Dictionary<string, object>() {
                { "remotecontrol_id", remoteControlId },
                { "groupid" , groupId },
            };
            if (description != null) {
                o.Add("description", description);
            }
            if (alias != null) {
                o.Add("alias", alias);
            }
            if (password != null) {
                o.Add("password", password);
            }
            var response = await this.client_.PostAsync(
                "api/v1/devices",
                new StringContent(
                    JsonConvert.SerializeObject(o),
                    Encoding.UTF8,
                    "application/json"));
            if (response.StatusCode != System.Net.HttpStatusCode.OK) {
                throw new Exception($"Add device failed with status code {response.StatusCode} ({(int)response.StatusCode})");
            }
            var responseJson = await response.Content.ReadAsStringAsync();
            dynamic responseJsonObject = JsonConvert.DeserializeObject(responseJson);
            string onlineState = responseJsonObject.online_state;
            string supportedFeatures = responseJsonObject.supported_features;
            return new Device() {
                DeviceId = responseJsonObject.device_id,
                RemoteControlId = responseJsonObject.remotecontrol_id,
                GroupId = responseJsonObject.groupid,
                Alias = responseJsonObject.alias,
                Description = responseJsonObject.description,
                OnlineState = this.convertToOnlineState_(onlineState),
                SupportedFeatures = this.convertToSupportedFeatures_(supportedFeatures),
                IsAssignedToCurrentUser = responseJsonObject.assigned_to,
            };
        }

        public async Task<bool> UpdateDevice(string deviceId, string alias = null, string description = null, string password = null, string groupId = null) {
            var o = new Dictionary<string, object>() { };
            if (alias != null) {
                o.Add("alias", alias);
            }
            if (description != null) {
                o.Add("description", description);
            }
            if (password != null) {
                o.Add("password", password);
            }
            if (groupId != null) {
                o.Add("groupid", groupId);
            }
            if (!o.Any()) {
                // nothing to update
                return true;
            }
            var response = await this.client_.PutAsync(
                $"api/v1/devices/{deviceId}",
                new StringContent(
                    JsonConvert.SerializeObject(o),
                    Encoding.UTF8,
                    "application/json"));
            if (response.StatusCode != System.Net.HttpStatusCode.OK
               && response.StatusCode != System.Net.HttpStatusCode.NoContent) {
                throw new Exception($"Update device failed with status code {response.StatusCode} ({(int)response.StatusCode})");
            }
            return true;
        }

        public async Task<bool> DeleteDevice(string deviceId) {
            var response = await this.client_.DeleteAsync($"api/v1/devices/{deviceId}");
            if (response.StatusCode != System.Net.HttpStatusCode.OK
               && response.StatusCode != System.Net.HttpStatusCode.NoContent) {
                throw new Exception($"Update device failed with status code {response.StatusCode} ({(int)response.StatusCode})");
            }
            return true;
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

        public async Task<Group> AddGroup(string groupName) {
            var response = await this.client_.PostAsync(
                "api/v1/groups",
                new StringContent(
                    JsonConvert.SerializeObject(new { name = groupName }),
                    Encoding.UTF8,
                    "application/json"));
            if (response.StatusCode != System.Net.HttpStatusCode.OK) {
                throw new Exception($"Add group failed with status code {response.StatusCode} ({(int)response.StatusCode})");
            }
            var responseJson = await response.Content.ReadAsStringAsync();
            dynamic responseJsonObject = JsonConvert.DeserializeObject(responseJson);
            string permissions = responseJsonObject.permissions;
            return new Group() {
                GroupId = responseJsonObject.id,
                Name = responseJsonObject.name,
                Permissions = this.convertToPermission_(permissions),
            };
        }

        public async Task<bool> RenameGroup(string groupId, string newName) {
            var response = await this.client_.PutAsync(
                $"api/v1/groups/{groupId}",
                new StringContent(
                    JsonConvert.SerializeObject(new { name = newName }),
                    Encoding.UTF8,
                    "application/json"));
            if (response.StatusCode != System.Net.HttpStatusCode.OK
               && response.StatusCode != System.Net.HttpStatusCode.NoContent) {
                throw new Exception($"Rename group failed with status code {response.StatusCode} ({(int)response.StatusCode})");
            }
            return true;
        }

        public async Task<bool> DeleteGroup(string groupId) {
            var response = await this.client_.DeleteAsync($"api/v1/groups/{groupId}");
            if (response.StatusCode != System.Net.HttpStatusCode.OK
               && response.StatusCode != System.Net.HttpStatusCode.NoContent) {
                throw new Exception($"Delete group failed with status code {response.StatusCode} ({(int)response.StatusCode})");
            }
            return true;
        }

        public async Task<bool> ShareGroup(string groupId, IDictionary<string, Permission> userIdsAndPermissions) {
            var o = new {
                users = userIdsAndPermissions
                    .Select(user =>
                        new {
                            userid = user.Key,
                            permissions = convertPermissionToString_(user.Value, allowOwned: false)
                        })
                    .ToList(),
            };
            var response = await this.client_.PostAsync(
                $"api/v1/groups/{groupId}/share_group",
                new StringContent(
                    JsonConvert.SerializeObject(o),
                    Encoding.UTF8,
                    "application/json"));
            if (response.StatusCode != System.Net.HttpStatusCode.OK
               && response.StatusCode != System.Net.HttpStatusCode.NoContent) {
                throw new Exception($"Share group failed with status code {response.StatusCode} ({(int)response.StatusCode})");
            }
            return true;
        }

        public async Task<bool> UnshareGroup(string groupId, IEnumerable<string> userIds) {
            var o = new {
                users = userIds.ToList(),
            };
            var response = await this.client_.PostAsync(
                $"api/v1/groups/{groupId}/unshare_group",
                new StringContent(
                    JsonConvert.SerializeObject(o),
                    Encoding.UTF8,
                    "application/json"));
            if (response.StatusCode != System.Net.HttpStatusCode.OK
               && response.StatusCode != System.Net.HttpStatusCode.NoContent) {
                throw new Exception($"Unshare group failed with status code {response.StatusCode} ({(int)response.StatusCode})");
            }
            return true;
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

        private string convertPermissionToString_(Permission value, bool allowOwned = true) {
            switch (value) {
                case Permission.Read:
                    return "read";

                case Permission.ReadWrite:
                    return "readwrite";

                case Permission.Owned:
                    if (!allowOwned) {
                        throw new Exception($"{nameof(Permission.Owned)} is not allowed.");
                    }
                    return "owned";

                default:
                    throw new Exception($"Unknown value '{value}',");
            }
        }

        #endregion helpers
    }
}