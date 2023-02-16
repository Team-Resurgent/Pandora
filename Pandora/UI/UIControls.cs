using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.UI
{
    public static class UIControls
    {
        public static void SetXboxTheme()
        {
            ImGui.StyleColorsDark();
            var style = ImGui.GetStyle();
            var colors = style.Colors;
            colors[(int)ImGuiCol.Text] = new Vector4(0.94f, 0.94f, 0.94f, 1.00f);
            colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.86f, 0.93f, 0.89f, 0.28f);
            colors[(int)ImGuiCol.WindowBg] = new Vector4(0.10f, 0.10f, 0.10f, 1.00f);
            colors[(int)ImGuiCol.ChildBg] = new Vector4(0.06f, 0.06f, 0.06f, 0.98f);
            colors[(int)ImGuiCol.PopupBg] = new Vector4(0.10f, 0.10f, 0.10f, 1.00f);
            colors[(int)ImGuiCol.Border] = new Vector4(0.11f, 0.11f, 0.11f, 0.60f);
            colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.16f, 0.16f, 0.16f, 0.00f);
            colors[(int)ImGuiCol.FrameBg] = new Vector4(0.18f, 0.18f, 0.18f, 1.00f);
            colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.30f, 0.30f, 0.30f, 1.00f);
            colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.28f, 0.71f, 0.25f, 1.00f);
            colors[(int)ImGuiCol.TitleBg] = new Vector4(0.20f, 0.51f, 0.18f, 1.00f);
            colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.26f, 0.66f, 0.23f, 1.00f);
            colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.16f, 0.16f, 0.16f, 0.75f);
            colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.14f, 0.14f, 0.14f, 0.00f);
            colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.16f, 0.16f, 0.16f, 0.00f);
            colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.30f, 0.30f, 0.30f, 1.00f);
            colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.24f, 0.60f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.24f, 0.60f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.CheckMark] = new Vector4(0.26f, 0.66f, 0.23f, 1.00f);
            colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.90f, 0.90f, 0.90f, 1.00f);
            colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
            colors[(int)ImGuiCol.Button] = new Vector4(0.17f, 0.17f, 0.17f, 1.00f);
            colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.24f, 0.60f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.26f, 0.66f, 0.23f, 1.00f);
            colors[(int)ImGuiCol.Header] = new Vector4(0.24f, 0.60f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.24f, 0.60f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.24f, 0.60f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.Separator] = new Vector4(1.00f, 1.00f, 1.00f, 0.25f);
            colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.13f, 0.87f, 0.16f, 0.78f);
            colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.25f, 0.75f, 0.10f, 1.00f);
            colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.47f, 0.83f, 0.49f, 0.04f);
            colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.28f, 0.71f, 0.25f, 0.78f);
            colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.28f, 0.71f, 0.25f, 1.00f);
            colors[(int)ImGuiCol.Tab] = new Vector4(0.26f, 0.67f, 0.23f, 0.95f);
            colors[(int)ImGuiCol.TabHovered] = new Vector4(0.24f, 0.60f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.TabActive] = new Vector4(0.24f, 0.60f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.21f, 0.54f, 0.19f, 0.99f);
            colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.24f, 0.60f, 0.21f, 1.00f);
            colors[(int)ImGuiCol.PlotLines] = new Vector4(0.86f, 0.93f, 0.89f, 0.63f);
            colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(0.28f, 0.71f, 0.25f, 1.00f);
            colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.86f, 0.93f, 0.89f, 0.63f);
            colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(0.28f, 0.71f, 0.25f, 1.00f);
            colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.26f, 0.66f, 0.23f, 1.00f);
            colors[(int)ImGuiCol.DragDropTarget] = new Vector4(1.00f, 1.00f, 0.00f, 0.90f);
            colors[(int)ImGuiCol.NavHighlight] = new Vector4(0.28f, 0.71f, 0.25f, 1.00f);
            colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f);
            colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.20f);
            colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.16f, 0.16f, 0.16f, 0.73f);
            colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.16f, 0.16f, 0.16f, 0.73f);

            style.WindowRounding = 6;
            style.FrameRounding = 6;
            style.PopupRounding = 6;
        }

        private static void DrawToggle(bool enabled, bool hovered, Vector2 pos, Vector2 size)
        {
            var drawList = ImGui.GetWindowDrawList();

            float radius = size.Y * 0.5f;
            float rounding = size.Y * 0.25f;
            float slotHalfHeight = size.Y * 0.5f;

            var background = hovered ? ImGui.GetColorU32(enabled ? ImGuiCol.FrameBgActive : ImGuiCol.FrameBgHovered) : ImGui.GetColorU32(enabled ? ImGuiCol.CheckMark : ImGuiCol.FrameBg);

            var paddingMid = new Vector2(pos.X + radius + (enabled ? 1 : 0) * (size.X - radius * 2), pos.Y + size.Y / 2);
            var sizeMin = new Vector2(pos.X, paddingMid.Y - slotHalfHeight);
            var sizeMax = new Vector2(pos.X + size.X, paddingMid.Y + slotHalfHeight);
            drawList.AddRectFilled(sizeMin, sizeMax, background, rounding);

            var offs = new Vector2(radius * 0.8f, radius * 0.8f);
            drawList.AddRectFilled(paddingMid - offs, paddingMid + offs, ImGui.GetColorU32(ImGuiCol.SliderGrab), rounding);
        }

        public static bool Toggle(string str_id, ref bool v, Vector2 size)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetColorU32(new Vector4()));

            var style = ImGui.GetStyle();

            ImGui.PushID(str_id);
            bool status = ImGui.Button("###toggle_button", size);
            if (status)
            {
                v = !v;
            }
            ImGui.PopID();

            var maxRect = ImGui.GetItemRectMax();
            var toggleSize = new Vector2(size.X - 8, size.Y - 8);
            var togglePos = new Vector2(maxRect.X - toggleSize.X - style.FramePadding.X, maxRect.Y - toggleSize.Y - style.FramePadding.Y);
            DrawToggle(v, ImGui.IsItemHovered(), togglePos, toggleSize);

            ImGui.PopStyleColor();

            return status;
        }

        public enum SelectableState
        {
            None,
            Clicked,
            ShowContext
        }

        public enum SelectableIcon
        {
            None,
            File,
            Directory
        }

        public static SelectableState Selectable(ref bool selected, string text, Vector2 size, SelectableIcon selectableIcon)
        {
            var background = selected ? ImGui.GetColorU32(ImGuiCol.ButtonHovered) : ImGui.GetColorU32(new Vector4());            
            var status = SelectableState.None;

            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0);
            ImGui.PushStyleColor(ImGuiCol.Button, background);

            var origPos = ImGui.GetCursorPos();

            ImGui.Button("###selectable", size);

            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            {
                selected = true;
                status = SelectableState.Clicked;
            }
            else if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                status = SelectableState.ShowContext;
            }

            var newPos = ImGui.GetCursorPos();
            var textSize = ImGui.CalcTextSize(text);

            ImGui.SetCursorPos(origPos + new Vector2(8 + size.Y, ((size.Y - textSize.Y) / 2)));
            ImGui.Text(text);
            
            var iconPadding = 4;
            var iconSize = size.Y - iconPadding;
            var iconPosition = ImGui.GetWindowPos() + origPos;
            iconPosition.Y -= ImGui.GetScrollY() + 2;

            if (selectableIcon == SelectableIcon.File)
            {
                GenerateFileIcon(iconPosition + new Vector2(iconPadding, iconPadding), iconSize);
            }
            else if (selectableIcon == SelectableIcon.Directory)
            {
                GenerateFolderIcon(iconPosition + new Vector2(iconPadding, iconPadding), iconSize);
            }

            ImGui.SetCursorPos(newPos);

            ImGui.PopStyleColor();
            ImGui.PopStyleVar();

            return status;
        }

        public static void DrawLines(IReadOnlyList<Vector2> points, Vector2 location, float size)
        {
            var iconColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1));
            var drawList = ImGui.GetWindowDrawList();
            for (var i = 0; i < points.Count; i += 2)
            {
                var vector1 = points[i] / 100 * size;
                var vector2 = points[i + 1] / 100 * size;
                drawList.AddLine(location + vector1, location + vector2, iconColor);
            }
        }

        public static void GenerateFolderIcon(Vector2 location, float size)
        {
            var points = new[] {
                new Vector2(0.0f,0.0f), new Vector2(45.0f, 0.0f),
                new Vector2(45.0f,0.0f), new Vector2(55.0f, 22.5f),
                new Vector2(55.0f,22.5f), new Vector2(100.0f, 22.5f),
                new Vector2(100.0f,22.5f), new Vector2(100.0f, 87.5f),
                new Vector2(100.0f,87.5f), new Vector2(0.0f, 87.5f),
                new Vector2(0.0f,87.5f), new Vector2(0.0f, 0.0f)
            };
            DrawLines(points, location, size);
        }

        public static void GenerateFileIcon(Vector2 location, float size)
        {
            var points = new[] {
                new Vector2(12.5f,0.0f), new Vector2(62.5f, 0.0f),
                new Vector2(62.5f,0.0f), new Vector2(87.5f, 50.0f),
                new Vector2(87.5f,50.0f), new Vector2(87.5f, 100.0f),
                new Vector2(87.5f,100.0f), new Vector2(12.5f, 100.0f),
                new Vector2(12.5f,100.0f), new Vector2(12.5f, 0.0f),
                new Vector2(62.5f,0.0f), new Vector2(62.5f, 50.0f),
                new Vector2(62.5f,50.0f), new Vector2(87.5f, 50.0f)
            };
            DrawLines(points, location, size);
        }
    }
}
