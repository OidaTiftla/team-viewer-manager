using System;

namespace team_viewer_manager.TeamViewer {

    [Flags]
    public enum Feature {
        None = 0,
        Chat = 1,
        RemoteControl = 2,
        Meeting = 4,
        VideoCall = 8,
    }
}