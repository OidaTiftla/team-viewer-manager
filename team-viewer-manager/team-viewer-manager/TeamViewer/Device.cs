using System;

namespace team_viewer_manager.TeamViewer {

    public enum OnlineState {
        Online,
        Offline,
    }

    [Flags]
    public enum Feature {
        None = 0,
        Chat = 1,
        RemoteControl = 2,
    }

    public class Device {

        /// <summary>
        /// The ID that is unique for this entry of the computers & contacts list. Values are always prefixed with a 'd'.
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// The ID that is unique to this device and can be used to start a remote control session.
        /// </summary>
        public string RemoteControlId { get; set; }

        /// <summary>
        /// The ID of the group that this device is a member of.
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// The alias that the current user has given to this device.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// The description that the current user has entered for this device.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The current online state of the device. Possible values are: Online, Offline.
        /// </summary>
        public OnlineState OnlineState { get; set; }

        /// <summary>
        /// The features supported by the device. Possible values are: Chat, RemoteControl.
        /// </summary>
        public Feature SupportedFeatures { get; set; }

        /// <summary>
        /// Indicates whether the device is assigned to the current user.
        /// </summary>
        public bool IsAssignedToCurrentUser { get; set; }
    }
}