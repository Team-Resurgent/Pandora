using FluentFTP.Helpers;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Pandora.RemoteContextDialog;

namespace Pandora.UI
{
    public class SplashDialog
    {
        private bool m_show = false;
        private IntPtr m_splashTexture;

        public void ShowdDialog(IntPtr splashTexture)
        {
            var timer = new System.Timers.Timer(3000);
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            timer.Enabled = true;

            m_splashTexture = splashTexture;
            m_show = true;
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            ImGui.CloseCurrentPopup();
        }

        public bool Render()
        {
            if (m_show)
            {
                m_show = false;
                ImGui.OpenPopup("splashDialog");
            }

            bool open = true;
            if (!ImGui.BeginPopupModal("splashDialog", ref open, ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground))
            {
                return false;
            }

            ImGui.Image(m_splashTexture, new Vector2(640, 480));

            return false;
        }
    }
}
