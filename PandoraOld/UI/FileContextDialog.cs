using ImGuiNET;

namespace Pandora
{
    public class FileContextDialog
    {
        public enum FileContextAction
        {
            None,
            CreateFolder,
            DeleteFolder,
            DeleteFile
        }

        public string LocalPath { get; set; } = string.Empty;

        private bool m_show = false;
        private FileAttributes m_fileAttributes;

        public void ShowdDialog()
        {
            m_show = true;
            m_fileAttributes = File.GetAttributes(LocalPath);
        }

        public FileContextAction Render()
        {
            if (m_show)
            {
                m_show = false;
                ImGui.OpenPopup("fileContextDialog");
            }

            if (!ImGui.BeginPopup("fileContextDialog"))
            {
                return FileContextAction.None;
            }

            if ((m_fileAttributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                if (ImGui.Selectable($"Create Folder", false, ImGuiSelectableFlags.None))
                {
                    return FileContextAction.CreateFolder;
                }

                ImGui.Spacing();

                if (ImGui.Selectable($"Delete Folder", false, ImGuiSelectableFlags.None))
                {
                    return FileContextAction.DeleteFolder;
                }
            }
            else
            {
                if (ImGui.Selectable($"Create Folder", false, ImGuiSelectableFlags.None))
                {
                    return FileContextAction.CreateFolder;
                }

                ImGui.Spacing();

                if (ImGui.Selectable($"Delete File", false, ImGuiSelectableFlags.None))
                {
                    return FileContextAction.DeleteFile;
                }
            }

            return FileContextAction.None;
        }
    }
}