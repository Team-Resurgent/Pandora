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

        public string DownloadPath { get; set; } = string.Empty;

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
                ImGui.OpenPopup("remoteContextDialog");
            }

            if (!ImGui.BeginPopup("remoteContextDialog"))
            {
                return RemoteContextAction.None;
            }

            if (ImGui.Selectable($"Download '{DownloadPath}'", false, ImGuiSelectableFlags.None))
            {
                return RemoteContextAction.Download;
            }

            return RemoteContextAction.None;
        }
    }
}