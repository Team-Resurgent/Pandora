using ImGuiNET;
using Pandora.Helpers;
using Pandora.Network;
using Pandora.UI;
using System.Diagnostics;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Pandora
{
    //https://stackoverflow.com/questions/74900568/how-to-change-the-background-color-of-the-title-bar-color-c-sdl2
    //https://github.com/lite-xl/lite-xl/pull/514/commits/42aeaf682b68b0f70b019466eee20e417aeb7802

    public class PandoraUI 
    {
        private class LogDetail
        {
            public string LogType { get; set; }
            public string Message { get; set; }

            public LogDetail(string logType, string message)
            {
                LogType = logType;
                Message = message;
            }
        }

        private Sdl2Window? m_window;
        private GraphicsDevice? m_graphicsDevice;
        private CommandList? m_commandList;
        private ImGuiController? m_controller;
        private RemoteContextDialog m_remoteContextDialog = new();
        private Client? m_client;
        private ClientFileInfo[]? m_cachedRemoteFileInfo;

        private List<LogDetail> m_logDetails = new();
        private bool m_logDetailsChanged = false; 
        private bool m_busy = false;

        public string LocalSelectedFolder { get; set; } = Utility.GetApplicationPath() ?? string.Empty;

        public string RemoteSelectedFolder { get; set; } = "/";
        
        public void Start(string version)
        {
            var admin = Utility.IsAdmin() ? " ADMIN" : string.Empty;
            VeldridStartup.CreateWindowAndGraphicsDevice(new WindowCreateInfo(50, 50, 1280, 720, WindowState.Normal, $"Pandora - {version}{admin}"), new GraphicsDeviceOptions(true, null, true, ResourceBindingModel.Improved, true, true), VeldridStartup.GetPlatformDefaultBackend(), out m_window, out m_graphicsDevice);

            m_controller = new ImGuiController(m_graphicsDevice, m_graphicsDevice.MainSwapchain.Framebuffer.OutputDescription, m_window.Width, m_window.Height);

            UIControls.SetXboxTheme();

            m_window.Resized += () =>
            {
                m_graphicsDevice.MainSwapchain.Resize((uint)m_window.Width, (uint)m_window.Height);
                m_controller.WindowResized(m_window.Width, m_window.Height);
            };

            m_commandList = m_graphicsDevice.ResourceFactory.CreateCommandList();

            while (m_window.Exists)
            {
                InputSnapshot snapshot = m_window.PumpEvents();
                if (!m_window.Exists)
                {
                    break;
                }
                m_controller.Update(1f / 60f, snapshot);

                RenderUI();

                m_commandList.Begin();
                m_commandList.SetFramebuffer(m_graphicsDevice.MainSwapchain.Framebuffer);
                m_commandList.ClearColorTarget(0, new RgbaFloat(0.0f, 0.0f, 0.0f, 1f));
                m_controller.Render(m_graphicsDevice, m_commandList);
                m_commandList.End();
                m_graphicsDevice.SubmitCommands(m_commandList);
                m_graphicsDevice.SwapBuffers(m_graphicsDevice.MainSwapchain);
            }

            m_graphicsDevice.WaitForIdle();
            m_controller.Dispose();
            m_commandList.Dispose();
            m_graphicsDevice.Dispose();
            m_client?.Dispose();
        }

        private void LogMessage(string logType, string message)
        {
            m_logDetails.Add(new LogDetail(logType, message));
            m_logDetailsChanged = true;
        }

        private void LocalProcessChildFolders(string path, int width)
        {
            var lineHeight = ImGui.GetTextLineHeight() + 4;

            var fses = Directory.EnumerateFileSystemEntries(path).OrderBy(o => o).ToArray();
            foreach (var fse in fses)
            {
                var name = Path.GetFileName(fse);

                var attributes = File.GetAttributes(fse);
                var isHidden = (attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
                if (isHidden)
                {
                    continue;
                }

                var isDirectory = (attributes & FileAttributes.Directory) == FileAttributes.Directory;
                if (!isDirectory)
                {
                    continue;
                }

                var selected = false;
                if (UIControls.Selectable(ref selected, name, new Vector2(width, lineHeight), UIControls.SelectableIcon.Directory) == UIControls.SelectableState.Clicked)
                {
                    LocalSelectedFolder = fse;
                }
            }
        }

        private void LocalProcessChildFiles(string path, int width)
        {
            var lineHeight = ImGui.GetTextLineHeight() + 4;

            var fses = Directory.EnumerateFileSystemEntries(path).OrderBy(o => o).ToArray();
            foreach (var fse in fses)
            {
                var name = Path.GetFileName(fse);

                var attributes = File.GetAttributes(fse);
                var isHidden = (attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
                if (isHidden)
                {
                    continue;
                }

                var isDirectory = (attributes & FileAttributes.Directory) == FileAttributes.Directory;
                if (isDirectory)
                {
                    continue;
                }

                var selected = false;
                UIControls.Selectable(ref selected, name, new Vector2(width, lineHeight), UIControls.SelectableIcon.File);
            }
        }

        private void RenderUI()
        {
            if (m_window == null)
            {
                return;
            }

            if (m_remoteContextDialog.Render() == RemoteContextDialog.RemoteContextAction.Download)
            {
                var remotePath = m_remoteContextDialog.RemotePath;
                var localPath = m_remoteContextDialog.LocalPath;

                new Thread(() =>
                {
                    if (m_client == null)
                    {
                        return;
                    }
                    m_client.AddFilesToDownloadStore(RemoteSelectedFolder, remotePath, localPath);                                        
                }).Start();
            }

            ImGui.Begin("Main", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize);
            ImGui.SetWindowSize(new Vector2(m_window.Width, m_window.Height));
            ImGui.SetWindowPos(new Vector2(0, 0), ImGuiCond.Always);

            ImGui.Text("Log:");

            ImGuiTableFlags flags = ImGuiTableFlags.Resizable | ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.RowBg;
            if (ImGui.BeginTable("tableLog", 2, flags, new Vector2(m_window.Width - 16, 170), 0.0f))
            {
                ImGui.TableSetupColumn("Log Type", ImGuiTableColumnFlags.WidthFixed, 75.0f, 0);
                ImGui.TableSetupColumn("Message", ImGuiTableColumnFlags.WidthStretch, 300.0f, 1);
                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableHeadersRow();

                for (var i = 0; i < m_logDetails.Count; i++)
                {
                    ImGui.PushID(i);
                    ImGui.TableNextRow(ImGuiTableRowFlags.None, 22);

                    ImGui.TableNextColumn();
                    var logTypeWidth = ImGui.GetColumnWidth();
                    ImGui.PushItemWidth(logTypeWidth);
                    ImGui.PushStyleColor(ImGuiCol.FrameBg, 0);
                    string logType = m_logDetails[i].LogType;
                    ImGui.InputText($"###logType{i}", ref logType, 0, ImGuiInputTextFlags.ReadOnly);
                    ImGui.PopStyleColor();
                    ImGui.PopItemWidth();

                    ImGui.TableNextColumn();
                    var messageWidth = ImGui.GetColumnWidth();
                    ImGui.PushItemWidth(messageWidth);
                    ImGui.PushStyleColor(ImGuiCol.FrameBg, 0);
                    string message = m_logDetails[i].Message;
                    ImGui.InputText($"###message{i}", ref message, 0, ImGuiInputTextFlags.ReadOnly);
                    ImGui.PopStyleColor();
                    ImGui.PopItemWidth();

                    ImGui.PopID();
                }

                if (m_logDetailsChanged)
                {
                    m_logDetailsChanged = false;
                    ImGui.SetScrollHereY();
                }
                ImGui.EndTable();
            }

            ImGui.Spacing();

            var halfWidth = m_window.Width / 2;

            ImGui.Text("Local:");
            ImGui.SameLine();
            ImGui.SetCursorPosX(halfWidth);
            ImGui.Text("Remote:");

            ImGui.Spacing();

            ImGui.PushItemWidth(halfWidth - 14);

            var localSelectedFolder = LocalSelectedFolder;
            ImGui.InputText("###localSelectedFolder", ref localSelectedFolder, 300, ImGuiInputTextFlags.ReadOnly);
            ImGui.SameLine();
            var remoteSelectedFolder = RemoteSelectedFolder;
            ImGui.InputText("###remoteSelectedFolder", ref remoteSelectedFolder, 300, ImGuiInputTextFlags.ReadOnly);

            ImGui.Spacing();

            var lineHeight = ImGui.GetTextLineHeight() + 4;

            if (ImGui.BeginChildFrame(1, new Vector2(200, m_window.Height - (358 + 78)), ImGuiWindowFlags.None))
            {
                var specialFolders = Utility.GetSpecialFolders();
                foreach (var specialFolder in specialFolders)
                {
                    var parts = specialFolder.Split('|');
                    var selected = false;
                    if (UIControls.Selectable(ref selected, parts[0], new Vector2(200 - 8, lineHeight), UIControls.SelectableIcon.Directory) == UIControls.SelectableState.Clicked)
                    {
                        LocalSelectedFolder = parts[1];
                    }
                }
                ImGui.EndChildFrame();
            }

            ImGui.SameLine();

            if (ImGui.BeginChildFrame(2, new Vector2(halfWidth - 222, m_window.Height - (358 + 78)), ImGuiWindowFlags.None))
            {
                var directoryInfo = new DirectoryInfo(LocalSelectedFolder);
                if (directoryInfo.Parent != null)
                {
                    var selected = false;
                    if (UIControls.Selectable(ref selected, "..", new Vector2(halfWidth - 230, lineHeight), UIControls.SelectableIcon.Directory) == UIControls.SelectableState.Clicked)
                    {
                        LocalSelectedFolder = directoryInfo.Parent.FullName;
                    }
                }
                try
                {
                    LocalProcessChildFolders(directoryInfo.FullName, halfWidth - 230);
                    LocalProcessChildFiles(directoryInfo.FullName, halfWidth - 230);
                }
                catch
                {
                    Debug.Print($"Unable to process path '{directoryInfo.FullName}'.");
                }
                ImGui.EndChildFrame();
            }

            ImGui.SameLine();

            bool disabled = m_busy;
            if (disabled)
            {
                ImGui.BeginDisabled();
            }
            if (ImGui.BeginChildFrame(3, new Vector2(halfWidth - 14, m_window.Height - (358 + 78)), ImGuiWindowFlags.None))
            {
                if (m_client != null)
                {
                    if (m_cachedRemoteFileInfo == null)
                    {
                        if (m_client.CanGetFiles() && !m_busy)
                        {
                            m_busy = true;
                            new Thread(() =>
                            {
                                try
                                {
                                    m_cachedRemoteFileInfo = m_client.GetFiles(RemoteSelectedFolder);                                    
                                } 
                                catch (Exception ex)
                                {
                                    Debug.Print($"Ignored: exception {ex.Message}.");
                                }
                                m_busy = false;
                            }).Start();
                        }
                    }

                    var filesToProcess = m_cachedRemoteFileInfo;
                    if (filesToProcess != null)
                    {
                        foreach (var clientFileInfo in filesToProcess)
                        {
                            if (clientFileInfo.FileType == FileType.File)
                            {
                                continue;
                            }

                            var selected = false;
                            var state = UIControls.Selectable(ref selected, clientFileInfo.Name, new Vector2(halfWidth - 28, lineHeight), UIControls.SelectableIcon.Directory);
                            if (state == UIControls.SelectableState.Clicked)
                            {
                                if (clientFileInfo.Name == "..")
                                {
                                    var position = RemoteSelectedFolder.LastIndexOf("/", RemoteSelectedFolder.Length - 2);
                                    if (position != -1)
                                    {
                                        RemoteSelectedFolder = RemoteSelectedFolder.Substring(0, position + 1);
                                    }
                                }
                                else
                                {
                                    RemoteSelectedFolder = clientFileInfo.Path + clientFileInfo.Name + "/";                                    
                                }
                                m_cachedRemoteFileInfo = null;
                            }
                            else if (state == UIControls.SelectableState.ShowContext)
                            {
                                m_remoteContextDialog.RemotePath = clientFileInfo.Path + clientFileInfo.Name + "/";
                                m_remoteContextDialog.LocalPath = Path.Combine(localSelectedFolder, clientFileInfo.Name); 
                                m_remoteContextDialog.ShowdDialog();
                            }
                        }

                        foreach (var clientFileInfo in filesToProcess)
                        {
                            if (clientFileInfo.FileType == FileType.Directory)
                            {
                                continue;
                            }

                            var selected = false;
                            var state = UIControls.Selectable(ref selected, clientFileInfo.Name, new Vector2(halfWidth - 28, lineHeight), UIControls.SelectableIcon.Directory);
                            if (state == UIControls.SelectableState.ShowContext) 
                            {
                                m_remoteContextDialog.RemotePath = clientFileInfo.Path + clientFileInfo.Name;
                                m_remoteContextDialog.LocalPath = localSelectedFolder;
                                m_remoteContextDialog.ShowdDialog();
                            }
                        }
                    }
                    
                }
                ImGui.EndChildFrame();
            }
            if (disabled)
            {
                ImGui.EndDisabled();
            }

            ImGui.Spacing();

            ImGui.Text("Downloads:");

            if (ImGui.BeginTable("tableDownloads", 4, flags, new Vector2(m_window.Width - 16, 100), 0.0f))
            {
                ImGui.TableSetupColumn("Progress", ImGuiTableColumnFlags.WidthFixed, 75.0f, 0);
                ImGui.TableSetupColumn("Remote Path", ImGuiTableColumnFlags.WidthFixed, 300.0f, 1);
                ImGui.TableSetupColumn("Remote File", ImGuiTableColumnFlags.WidthFixed, 75.0f, 2);
                ImGui.TableSetupColumn("Local Path", ImGuiTableColumnFlags.WidthStretch, 300.0f, 3);
                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableHeadersRow();

                var downloadDetails = DownloadDetailStore.GetDownloadDetails();
                for (var i = 0; i < downloadDetails.Length; i++)
                {
                    ImGui.PushID(i);
                    ImGui.TableNextRow(ImGuiTableRowFlags.None, 22);

                    ImGui.TableNextColumn();
                    var progressWidth = ImGui.GetColumnWidth();
                    ImGui.PushItemWidth(progressWidth);
                    ImGui.PushStyleColor(ImGuiCol.FrameBg, 0);
                    string progress = downloadDetails[i].Progress;
                    ImGui.InputText($"###progress{i}", ref progress, 0, ImGuiInputTextFlags.ReadOnly);
                    ImGui.PopStyleColor();
                    ImGui.PopItemWidth();

                    ImGui.TableNextColumn();
                    var remotePathWidth = ImGui.GetColumnWidth();
                    ImGui.PushItemWidth(remotePathWidth);
                    ImGui.PushStyleColor(ImGuiCol.FrameBg, 0);
                    string remotePath = downloadDetails[i].RemotePath;
                    ImGui.InputText($"###remotePath{i}", ref remotePath, 0, ImGuiInputTextFlags.ReadOnly);
                    ImGui.PopStyleColor();
                    ImGui.PopItemWidth();

                    ImGui.TableNextColumn();
                    var remoteFileWidth = ImGui.GetColumnWidth();
                    ImGui.PushItemWidth(remoteFileWidth);
                    ImGui.PushStyleColor(ImGuiCol.FrameBg, 0);
                    string remoteFile = downloadDetails[i].RemoteFile;
                    ImGui.InputText($"###remoteFile{i}", ref remoteFile, 0, ImGuiInputTextFlags.ReadOnly);
                    ImGui.PopStyleColor();
                    ImGui.PopItemWidth();

                    ImGui.TableNextColumn();
                    var localPathWidth = ImGui.GetColumnWidth();
                    ImGui.PushItemWidth(localPathWidth);
                    ImGui.PushStyleColor(ImGuiCol.FrameBg, 0);
                    string localPath = downloadDetails[i].LocalPath;
                    ImGui.InputText($"###localPath{i}", ref localPath, 0, ImGuiInputTextFlags.ReadOnly);
                    ImGui.PopStyleColor();
                    ImGui.PopItemWidth();

                    ImGui.PopID();
                }                
                ImGui.EndTable();
            }

            ImGui.SetCursorPosY(m_window.Height - 40);

            if (m_client != null)
            {
                if (ImGui.Button("Disconnect", new Vector2(100, 30)))
                {
                    m_client?.Dispose();
                    m_client = null;
                }
            }
            else if (ImGui.Button("Connect", new Vector2(100, 30)))
            {
                m_logDetails.Clear();
                m_logDetailsChanged = true;

                RemoteSelectedFolder = "/";
                m_cachedRemoteFileInfo = null;

                m_client?.Dispose();
                m_client = new Client();
                m_client.OnConnected += OnConnected;
                m_client.OnDisconnected += OnDisconnected;
                m_client.OnMessageSent += OnMessageSent;
                m_client.OnMessageRecieved += OnMessageRecieved;
                m_client.OnError += OnError;

                new Thread(() =>
                {
                    var host = "irc.choopa.net";

                    LogMessage("Connecting...", $"Trying to connect to host '{host}'.");

                    var connected = m_client.Connect(host, 6667);
                    if (!connected)
                    {
                        Debug.Print("retry sever");
                    }
                }).Start();                
            }

            ImGui.SameLine();

            if (ImGui.Button("Clear Queue", new Vector2(100, 30)))
            {
                DownloadDetailStore.ClearDownloadDeatails();
            }

            ImGui.SameLine();

            if (ImGui.Button("Clear Completed", new Vector2(100, 30)))
            {
                DownloadDetailStore.ClearCompletedDownloadDeatails();
            }

            ImGui.SameLine();

            if (ImGui.Button("Retry Failed", new Vector2(100, 30)))
            {
                DownloadDetailStore.RetryDownloadDeatails();
            }

            ImGui.End();
        }

        private void OnError(object sender, string error)
        {
            LogMessage("Error", error);
        }

        private void OnMessageRecieved(object sender, string message)
        {
            LogMessage("Recieved", message);
        }

        private void OnMessageSent(object sender, string message)
        {
            LogMessage("Sent", message);
        }

        private void OnDisconnected(object sender)
        {
            LogMessage("Disconnected", "Bye!");
            m_client?.Dispose();
            m_client = null;
        }

        private void OnConnected(object sender)
        {
            m_cachedRemoteFileInfo = null;
        }
    }
}
