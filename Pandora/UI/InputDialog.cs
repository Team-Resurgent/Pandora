using ImGuiNET;
using System.Numerics;

namespace Pandora.UI
{
    public class InputDialog
    {
        private bool m_show;
        private bool m_open;

        public string Title { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string Input { get; set; } = string.Empty;

        public bool Cancelled { get; set; } = false;

        public void ShowdDialog()
        {
            m_show = true;
        }

        private void CloseModal(bool cancelled)
        {
            ImGui.CloseCurrentPopup();
            Cancelled = cancelled;
            m_open = false;
        }

        public bool Render()
        {
            if (m_show)
            {
                m_show = false;
                m_open = true;
                ImGui.OpenPopup("###inputDialog");
            }

            if (!m_open)
            {
                return false;
            }

            var open = true;
            if (!ImGui.BeginPopupModal($"###inputDialog", ref open, ImGuiWindowFlags.NoResize))
            {
                CloseModal(true);
                return false;
            }

            var result = false;

            if (ImGui.IsWindowAppearing())
            {
                ImGui.SetWindowSize(new Vector2(300, 120));
            }

            ImGui.Text(Message);

            ImGui.Spacing();

            var input = Input;
            ImGui.PushItemWidth(300 - 12);
            if (ImGui.InputText("###input", ref input, 300))
            {
                Input = input;
            }
            ImGui.PopItemWidth();

            ImGui.Spacing();

            if (ImGui.Button("Cancel", new Vector2(100, 30)))
            {
                result = true;
                CloseModal(true);
            }

            ImGui.SameLine();

            if (ImGui.Button("Ok", new Vector2(100, 30)))
            {
                result = true;
                CloseModal(false);
            }
            ImGui.EndPopup();

            return result;
        }
    }
}