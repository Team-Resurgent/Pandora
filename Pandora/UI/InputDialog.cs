using ImGuiNET;
using System.Numerics;

namespace Pandora
{
    public class InputDialog
    {
        private bool _showModal;
        private bool _open;

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string Input { get; set; } = string.Empty;

        public bool Cancelled { get; set; } = false;

        public void ShowModal()
        {
            _showModal = true;
        }

        private void CloseModal(bool cancelled)
        {
            ImGui.CloseCurrentPopup();
            Cancelled = cancelled;
            _open = false;
        }

        public bool Render()
        {
            if (_showModal)
            {
                _showModal = false;
                _open = true;
                ImGui.OpenPopup(Title);
            }

            if (!_open)
            {
                return false;
            }

            var open = true;
            if (!ImGui.BeginPopupModal(Title, ref open, ImGuiWindowFlags.NoResize))
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