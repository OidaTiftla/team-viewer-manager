using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using team_viewer_manager.TeamViewer;

namespace team_viewer_manager {

    internal class Program {

        private static async Task Main(string[] args) {
            if (!tryGetAuthorizationToken(out var token)) {
                Console.Write("Please type the authorization token: ");
                token = Console.ReadLine().Trim();
                Console.Write("Remember this authorization token? (y/N): ");
                var answer = Console.ReadLine().Trim();
                if (answer == "y"
                    || answer == "Y") {
                    saveAuthorizationToken(token);
                }
            }

            try {
                var tvClient = new TeamViewerApiClient();
                if (!await tvClient.Authorize(token)) {
                    throw new Exception("Login failed");
                }
                ConsoleWriteLineSuccess("Login successfully");

                Console.WriteLine("What do you want to do?");
                Console.WriteLine("  1: export devices and groups (export.json)");
                Console.WriteLine("  2: import devices and groups (import.json or export.json)");
                Console.WriteLine("  3: delete all devices");
                Console.WriteLine("  4: delete all contacts");
                Console.WriteLine("  5: delete all groups (will also delete all devices and contacts)");
                Console.WriteLine("  else: quit and exit the program");
                var answer = Console.ReadLine().Trim();
                switch (answer) {
                    case "1":
                        await export(tvClient);
                        break;

                    case "2":
                        await import(tvClient);
                        break;

                    case "3":
                        await deleteAllDevices(tvClient);
                        break;

                    case "4":
                        await deleteAllContacts(tvClient);
                        break;

                    case "5":
                        await deleteAllGroups(tvClient);
                        break;
                }
            } catch (Exception ex) {
                ConsoleWriteLineError("Exception occurred:");
                ConsoleWriteLineError(ex.ToString());
            }
        }

        private static bool tryGetAuthorizationToken(out string token) {
            if (File.Exists("authorization.token")) {
                token = File.ReadAllText("authorization.token");
                return !string.IsNullOrWhiteSpace(token);
            }
            token = null;
            return false;
        }

        private static void saveAuthorizationToken(string token) {
            File.WriteAllText("authorization.token", token);
        }

        private static async Task export(TeamViewerApiClient tvClient) {
            Console.WriteLine("Get devices ...");
            var devices = await tvClient.GetDevices();
            ConsoleWriteLineSuccess("Get devices successfully");
            print(devices);

            Console.WriteLine("Get contacts ...");
            var contacts = await tvClient.GetContacts();
            ConsoleWriteLineSuccess("Get contacts successfully");
            print(contacts);

            Console.WriteLine("Get groups ...");
            var groups = await tvClient.GetGroups();
            ConsoleWriteLineSuccess("Get groups successfully");
            print(groups);

            Console.WriteLine("Export ...");
            exportToFile(new FileInfo("export.json"), groups, contacts, devices);
            ConsoleWriteLineSuccess("Export successfully");
        }

        private static void exportToFile(FileInfo file, List<Group> groups, List<Contact> contacts, List<Device> devices) {
            var o = new {
                groups = groups.Select(group => new {
                    group.GroupId,
                    group.Name,
                    group.SharedWith,
                    group.Owner,
                    group.Permissions,
                    contacts = contacts.Where(contact => contact.GroupId == group.GroupId).ToList(),
                    devices = devices.Where(device => device.GroupId == group.GroupId).ToList(),
                }),
            };
            var settings = new JsonSerializerSettings() { };
            settings.Converters.Add(new StringEnumConverter());
            File.WriteAllText(file.FullName, JsonConvert.SerializeObject(o, Formatting.Indented, settings));
        }

        private static async Task import(TeamViewerApiClient tvClient) {
            Console.WriteLine("Get existing devices ...");
            var existingDevices = await tvClient.GetDevices();
            ConsoleWriteLineSuccess("Get existing devices successfully");
            print(existingDevices);

            Console.WriteLine("Get existing contacts ...");
            var existingContacts = await tvClient.GetContacts();
            ConsoleWriteLineSuccess("Get existing contacts successfully");
            print(existingContacts);

            Console.WriteLine("Get existing groups ...");
            var existingGroups = await tvClient.GetGroups();
            ConsoleWriteLineSuccess("Get existing groups successfully");
            print(existingGroups);

            Console.WriteLine("Import ...");
            FileInfo file;
            if (File.Exists("import.json")) {
                file = new FileInfo("import.json");
            } else if (File.Exists("export.json")) {
                file = new FileInfo("export.json");
            } else {
                throw new Exception("No file found to import (import.json or export.json). It must be in the working directory (by default it is the same as the *.exe file).");
            }
            importFromFile(file, out var importGroups, out var importContacts, out var importDevices);
            ConsoleWriteLineSuccess("Read import file successfully");

            Console.WriteLine("Import groups ...");
            var alreadyExistingGroups = importGroups.Where(x => existingGroups.Any(y => y.GroupId == x.GroupId || y.Name == x.Name)).ToList();
            var newGroups = importGroups.Except(alreadyExistingGroups).ToList();
            foreach (var group in newGroups) {
                Console.WriteLine($"Import group {group.Name} ...");
                print(group);
                var newGroup = await tvClient.AddGroup(group.Name);
                existingGroups.Add(newGroup);
                ConsoleWriteLineSuccess($"Import group {group.Name} successfully");
                print(newGroup);
            }
            ConsoleWriteLineSuccess("Import groups successfully");

            Console.WriteLine("Import devices ...");
            var alreadyExistingDevices = importDevices.Where(x => existingDevices.Any(y => y.DeviceId == x.DeviceId || y.RemoteControlId == x.RemoteControlId)).ToList();
            var newDevices = importDevices.Except(alreadyExistingDevices).ToList();
            foreach (var device in newDevices) {
                Console.WriteLine($"Import device {device.RemoteControlId} ...");
                print(device);
                var groupName = importGroups.First(x => x.GroupId == device.GroupId).Name;
                var newGroupId = existingGroups.First(x => x.Name == groupName).GroupId;
                var newDevice = await tvClient.AddDevice(
                    remoteControlId: device.RemoteControlId,
                    groupId: newGroupId,
                    description: device.Description,
                    alias: device.Alias);
                existingDevices.Add(newDevice);
                ConsoleWriteLineSuccess($"Import device {device.RemoteControlId} successfully");
                print(newDevice);
            }
            ConsoleWriteLineSuccess("Import devices successfully");

            if (importContacts.Any()) {
                ConsoleWriteLineWarning("Import contacts is not supported. Skipping them.");
                print(importContacts);
                Console.Write("Press ENTER to continue ...");
                Console.ReadLine();
            }

            ConsoleWriteLineSuccess("Import successfully");
        }

        private static void importFromFile(FileInfo file, out List<Group> groups, out List<Contact> contacts, out List<Device> devices) {
            groups = new List<Group>();
            contacts = new List<Contact>();
            devices = new List<Device>();

            var settings = new JsonSerializerSettings() { };
            settings.Converters.Add(new StringEnumConverter());
            dynamic o = JsonConvert.DeserializeObject(File.ReadAllText(file.FullName), settings);
            List<dynamic> groupsJson = o.groups.ToObject<List<dynamic>>();
            foreach (var group in groupsJson) {
                groups.Add(new Group() {
                    GroupId = group.GroupId,
                    Name = group.Name,
                    SharedWith = group.SharedWith.ToObject<List<GroupShare>>(),
                    Owner = group.Owner.ToObject<GroupOwner>(),
                    Permissions = group.Permissions.ToObject<Permission>(),
                });
                List<Contact> contactsJson = group.contacts.ToObject<List<Contact>>();
                contacts.AddRange(contactsJson);
                List<Device> devicesJson = group.devices.ToObject<List<Device>>();
                devices.AddRange(devicesJson);
            }
        }

        private static async Task<bool> deleteAllDevices(TeamViewerApiClient tvClient) {
            Console.WriteLine("Get existing devices ...");
            var existingDevices = await tvClient.GetDevices();
            ConsoleWriteLineSuccess("Get existing devices successfully");
            print(existingDevices);

            Console.Write("Are you sure to delete all devices? (y/N): ");
            var answer = Console.ReadLine().Trim();
            if (answer == "y"
                || answer == "Y") {
                foreach (var device in existingDevices) {
                    print(device);
                    await tvClient.DeleteDevice(device.DeviceId);
                }
                ConsoleWriteLineSuccess("Delete all devices successfully");
                return true;
            }
            Console.Write("The user aborted.");
            return false;
        }

        private static async Task<bool> deleteAllContacts(TeamViewerApiClient tvClient) {
            Console.WriteLine("Get existing contacts ...");
            var existingContacts = await tvClient.GetContacts();
            ConsoleWriteLineSuccess("Get existing contacts successfully");
            print(existingContacts);

            Console.Write("Are you sure to delete all contacts? (y/N): ");
            var answer = Console.ReadLine().Trim();
            if (answer == "y"
                || answer == "Y") {
                foreach (var contact in existingContacts) {
                    print(contact);
                    await tvClient.DeleteContact(contact.ContactId);
                }
                ConsoleWriteLineSuccess("Delete all contacts successfully");
                return true;
            }
            Console.Write("The user aborted.");
            return false;
        }

        private static async Task<bool> deleteAllGroups(TeamViewerApiClient tvClient) {
            if (!await deleteAllDevices(tvClient)) {
                return false;
            }
            if (!await deleteAllContacts(tvClient)) {
                return false;
            }

            Console.WriteLine("Get existing groups ...");
            var existingGroups = await tvClient.GetGroups();
            ConsoleWriteLineSuccess("Get existing groups successfully");
            print(existingGroups);

            Console.Write("Are you sure to delete all devices? (y/N): ");
            var answer = Console.ReadLine().Trim();
            if (answer == "y"
                || answer == "Y") {
                foreach (var group in existingGroups) {
                    //if (group.Name == "Meine Computer"
                    //    || group.Name == "My computers") {
                    //    ConsoleWriteLineWarning("Skip default group.");
                    //    continue;
                    //}
                    print(group);
                    await tvClient.DeleteGroup(group.GroupId);
                }
                ConsoleWriteLineSuccess("Delete all groups successfully");
                return true;
            }
            Console.Write("The user aborted.");
            return false;
        }

        #region console helpers

        private static void ConsoleWriteLineSuccess(string text) {
            ConsoleWriteLineColor(text, ConsoleColor.DarkGreen);
        }

        private static void ConsoleWriteLineWarning(string text) {
            ConsoleWriteLineColor(text, ConsoleColor.DarkYellow);
        }

        private static void ConsoleWriteLineError(string text) {
            ConsoleWriteLineColor(text, ConsoleColor.Red);
        }

        private static void ConsoleWriteLineColor(string text, ConsoleColor color) {
            var oldColor = Console.ForegroundColor;
            try {
                Console.ForegroundColor = color;
                Console.WriteLine(text);
            } finally {
                Console.ForegroundColor = oldColor;
            }
        }

        private static void print(List<Device> devices) {
            foreach (var device in devices) {
                print(device);
            }
        }

        private static void print(Device device) {
            Console.WriteLine($"----DeviceId: {device.DeviceId}");
            Console.WriteLine($"    RemoteControlId: {device.RemoteControlId}");
            Console.WriteLine($"    GroupId: {device.GroupId}");
            Console.WriteLine($"    Alias: {device.Alias}");
            Console.WriteLine($"    Description: {device.Description}");
            Console.WriteLine($"    OnlineState: {device.OnlineState}");
            Console.WriteLine($"    SupportedFeatures: {device.SupportedFeatures}");
            Console.WriteLine($"    IsAssignedToCurrentUser: {device.IsAssignedToCurrentUser}");
        }

        private static void print(List<Contact> contacts) {
            foreach (var contact in contacts) {
                print(contact);
            }
        }

        private static void print(Contact contact) {
            Console.WriteLine($"----ContactId: {contact.ContactId}");
            Console.WriteLine($"    UserId: {contact.UserId}");
            Console.WriteLine($"    Name: {contact.Name}");
            Console.WriteLine($"    GroupId: {contact.GroupId}");
            Console.WriteLine($"    Description: {contact.Description}");
            Console.WriteLine($"    OnlineState: {contact.OnlineState}");
            Console.WriteLine($"    ProfilePictureUrl: {contact.ProfilePictureUrl}");
            Console.WriteLine($"    SupportedFeatures: {contact.SupportedFeatures}");
        }

        private static void print(List<Group> groups) {
            foreach (var group in groups) {
                print(group);
            }
        }

        private static void print(Group group) {
            Console.WriteLine($"----GroupId: {group.GroupId}");
            Console.WriteLine($"    Name: {group.Name}");
            if (group.SharedWith is null) {
                Console.WriteLine($"    SharedWith: <null>");
            } else {
                Console.WriteLine($"    SharedWith:");
                foreach (var share in group.SharedWith) {
                    Console.WriteLine($"    ----UserId: {share.UserId}");
                    Console.WriteLine($"        Name: {share.Name}");
                    Console.WriteLine($"        Permissions: {share.Permissions}");
                    Console.WriteLine($"        IsPending: {share.IsPending}");
                }
            }
            if (group.Owner is null) {
                Console.WriteLine($"    Owner: <null>");
            } else {
                Console.WriteLine($"    Owner:");
                Console.WriteLine($"    ----UserId: {group.Owner.UserId}");
                Console.WriteLine($"        Name: {group.Owner.Name}");
            }
            Console.WriteLine($"    Permissions: {group.Permissions}");
        }

        #endregion console helpers
    }
}