using ImGuiNET;

namespace Pandora
{
    public class RemoteContextDialog
    {
        public enum RemoteContextAction
        {
            None,
            Download
        }

        public string RemotePath { get; set; } = string.Empty;
        public string LocalPath { get; set; } = string.Empty;
        public long FileSize { get; set; } = 0;

        private bool m_show = false;

        public void ShowdDialog()
        {
            m_show = true;
        }

        public RemoteContextAction Render()
        {
            if (m_show)
            {
                m_show = false;
                ImGui.OpenPopup("###remoteContextDialog");
            }

            if (!ImGui.BeginPopup("###remoteContextDialog"))
            {
                return RemoteContextAction.None;
            }

            if (ImGui.Selectable($"Download '{RemotePath}'", false, ImGuiSelectableFlags.None))
            {
                return RemoteContextAction.Download;
            }

            return RemoteContextAction.None;
        }
    }
}