using System.Collections.Generic;

namespace team_viewer_manager.TeamViewer {

    public class Group {

        /// <summary>
        /// Group ID.
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// Name of the group.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of users who this group is shared with.
        /// </summary>
        public List<GroupShare> SharedWith { get; set; } = new List<GroupShare>();

        /// <summary>
        /// Owner of this group. Omitted if the owner is the current user.
        /// </summary>
        public GroupOwner Owner { get; set; }

        /// <summary>
        /// Read, ReadWrite or Owned.
        /// </summary>
        public Permission Permissions { get; set; }
    }

    public class GroupShare {

        /// <summary>
        /// User ID of the user the group is shared with.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Name of the user the group is shared with.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Access-permissions of the user on this group. Either Read or ReadWrite.
        /// </summary>
        public Permission Permissions { get; set; }

        /// <summary>
        /// true if the user hasn't accepted the shared group yet.
        /// </summary>
        public bool IsPending { get; set; }
    }

    public class GroupOwner {

        /// <summary>
        /// User ID of the owner of this group.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Name of the owner of this group.
        /// </summary>
        public string Name { get; set; }
    }
}