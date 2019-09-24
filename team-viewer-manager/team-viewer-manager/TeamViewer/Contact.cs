namespace team_viewer_manager.TeamViewer {

    public class Contact {

        /// <summary>
        /// The ID that is unique for this entry of the computers & contacts list. Values are always prefixed with a 'c'.
        /// </summary>
        public string ContactId { get; set; }

        /// <summary>
        /// The User ID of the contact. Prefixed with a 'u'.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The name of the contact.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The ID of the group that this contact is a member of.
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// The description that the current user has entered for this contact.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The current online state of the contact. Possible values are: Online, Busy, Away, Offline.
        /// </summary>
        public OnlineState OnlineState { get; set; }

        /// <summary>
        /// (optional) The profile picture of the contact. Contains the URL at which the profile picture
        /// can be found. The URL contains the string "[size]" as placeholder for the size of the picture,
        /// which needs to be replaced by an integer to retrieve the picture of that size.
        /// Valid sizes are 16, 32, 64, 128 and 256. Omitted if a contact has no profile picture set.
        /// </summary>
        public string ProfilePictureUrl { get; set; }

        /// <summary>
        /// The features supported by the contact. Possible values are: Chat, RemoteControl, Meeting, VideoCall.
        /// </summary>
        public Feature SupportedFeatures { get; set; }
    }
}