using System.Net.Sockets;
using System.Text;
using FluentFTP;
using System.Net;
using System.Diagnostics;
using Pandora.Helpers;
using ManagedBass;
using FluentFTP.Helpers;

namespace Pandora.Network
{
    public class Client : IDisposable
    {
        private TcpClient? m_tcpClient;
        
        private FtpClient? m_ftpClient;

        public delegate void ErrorEventHandler(object sender, string error);
        public delegate void ConnectedHandler(object sender);
        public delegate void DisconnectedHandler(object sender);
        public delegate void MessageSentHandler(object sender, string message);
        public delegate void MessageRecievedHandler(object sender, string message);
        public delegate void ConnectingHandler(object sender, string message);

        public event ErrorEventHandler? OnError;
        public event ConnectedHandler? OnConnected;
        public event DisconnectedHandler? OnDisconnected;
        public event MessageSentHandler? OnMessageSent;
        public event MessageRecievedHandler? OnMessageRecieved;
        public event ConnectingHandler? OnConnecting;

        private bool m_disposed = false;
        private bool m_disconnected = false;
        private bool m_disconnectRequested = false;
        private bool m_ftpRady = false;
        private bool m_ftpDownloading = false;

        private string GenerateWord(int length)
        {
            string consonants = "bcdfghjklmnpqrstvwxyz";
            string vowels = "aeiou";

            string word = string.Empty;

            var random = new Random(Guid.NewGuid().GetHashCode());

            if (random.Next() % 2 == 0)
            {
                word += consonants[random.Next(0, consonants.Length)];
            }
            else
            {
                word += vowels[random.Next(0, vowels.Length)];
            }

            for (int i = 1; i < length; i += 2)
            {
                char c = consonants[random.Next(0, consonants.Length)];
                char v = vowels[random.Next(0, vowels.Length)];
                if (c == 'q')
                {
                    word += "qu";
                }
                else
                {
                    word += $"{c}{v}";
                }
            }

            if (word.Length < length)
            {
                word += consonants[random.Next(0, consonants.Length)];
            }

            return word;
        }

        private void SendMessage(string message)
        {
            if (m_tcpClient == null)
            {
                return;
            }
            try
            {
                var stream = m_tcpClient.GetStream();
                var buffer = Encoding.UTF8.GetBytes($"{message}\r\n");
                stream.Write(buffer, 0, buffer.Length);
                OnMessageSent?.Invoke(this, message);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, $"SendMessage: Exception occured, disconnecting. '{ex.Message}'.");
                m_disconnectRequested = true;
            }
        }

        private string ReadResponse()
        {
            if (m_tcpClient == null)
            {
                return string.Empty;
            }
            try
            {
                var stream = m_tcpClient.GetStream();
                var response = new StringBuilder();
                while (stream.DataAvailable)
                {
                    var value = (char)stream.ReadByte();
                    if (value == '\r')
                    {
                        continue;
                    }
                    if (value == '\n')
                    {
                        break;
                    }
                    response.Append(value);
                }
                var message = response.ToString();
                OnMessageRecieved?.Invoke(this, message);
                return message;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, $"ReadResponse: Exception occured, disconnecting. '{ex.Message}'.");
                m_disconnectRequested = true;
                return string.Empty;
            }
        }

        private void ConnectFTP(string host, int port, string user, string pass)
        {
            try
            {
                m_ftpClient = new FtpClient
                {
                    Credentials = new NetworkCredential(user, pass),
                    Host = host,
                    Port = port
                };
                m_ftpClient.Config.StaleDataCheck = false;

                try
                {
                    m_ftpClient.Connect();
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, $"ConnectFTP: Unable to connect to FTP, disconnecting '{ex.Message}");
                    m_disconnectRequested = true;
                    return;
                }

                m_ftpRady = true;
                OnConnected?.Invoke(this);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, $"ProcessPrivMsg: Exception occured, disconnecting. '{ex.Message}'.");
                m_disconnectRequested = true;
            }
        }

        private void ProcessPrivMsg(string argument)
        {
            if (!argument.Contains("FTP ADDRESS"))
            {
                return;
            }
            var parts = argument.Split('', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length != 10)
            {
                return;
            }
            var host = parts[1];
            if (!int.TryParse(parts[3], out var port))
            {
                return;
            }
            var user = parts[5];
            var pass = parts[7];

            ConnectFTP(host, port, user, pass);
        }

        public bool CanGetFiles()
        {
            if (m_ftpClient == null || m_ftpRady == false || m_disconnectRequested == true)
            {
                return false;
            }
            return true;
        }

        public ClientFileInfo[]? GetFiles(string path)
        {
            try
            {
                var files = new List<ClientFileInfo>();

                if (m_ftpClient == null)
                {
                    return null;
                }

                var ftpListing = m_ftpClient.GetListing(path, FtpListOption.AllFiles);

                foreach (var item in ftpListing)
                {
                    var clientFileInfo = new ClientFileInfo();

                    if (item.Type == FtpObjectType.File)
                    {
                        clientFileInfo.FileType = FileType.File;
                    }
                    else if (item.Type == FtpObjectType.Directory)
                    {
                        clientFileInfo.FileType = FileType.Directory;
                    }
                    else
                    {
                        continue;
                    }

                    clientFileInfo.Path = path;
                    clientFileInfo.Name = item.Name;
                    clientFileInfo.Size = item.Size;
                    files.Add(clientFileInfo);
                }

                files = files.OrderBy(o => o.Name).ToList();
                if (path != "/")
                {
                    files.Insert(0, new ClientFileInfo { FileType = FileType.Directory, Name = "..", Path = path });
                }
                return files.ToArray();
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, $"GetFiles: Exception occured, disconnecting. '{ex.Message}'.");
                m_disconnectRequested = true;
                return null;
            }
        }

        public void AddFileToDownloadStore(string remoteRelativePath, string remotePath, string localPath, long fileSize)
        {
            if (remotePath.EndsWith("/"))
            {
                var files = GetFiles(remotePath);
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        if (file.Name.Equals(".."))
                        {
                            continue;
                        }
                        if (file.FileType == FileType.File)
                        {
                            var downloadDetail = new DownloadDetail(remoteRelativePath, file.Path, file.Name, localPath, file.Size);
                            DownloadDetailStore.AddDownloadDetail(downloadDetail);
                        }
                        else if (file.FileType == FileType.Directory)
                        {
                            var downloadDetail = new DownloadDetail(remoteRelativePath, file.Path + file.Name + "/", string.Empty, Path.Combine(localPath, file.Name), file.Size);
                            DownloadDetailStore.AddDownloadDetail(downloadDetail);
                        }
                    }
                }
            }
            else
            {
                var position = remotePath.LastIndexOf("/", remoteRelativePath.Length - 1);
                if (position != -1)
                {
                    var remoteFile = remotePath.Substring(position + 1);
                    remotePath = remotePath.Substring(0, position + 1);
                    var downloadDetail = new DownloadDetail(remoteRelativePath, remotePath, remoteFile, localPath, fileSize);
                    DownloadDetailStore.AddDownloadDetail(downloadDetail);
                }
            }
        }

        private bool TryDownloadFile(string remotePath, string localPath, long fileSize, Action<float> progress)
        {
            if (m_ftpClient == null)
            {
                return false;
            }
            try
            {
                m_ftpClient.DownloadFile(localPath, remotePath, FtpLocalExists.Overwrite, FtpVerify.None, p =>
                {
                    progress(Math.Max((((float)p.TransferredBytes / fileSize) * 100), 0f));
                });

                progress(100);
            }
            catch (Exception ex)
            {
                Debug.Print($"Download exception '{ex}'.");
                return false;
            }
            return true;
        }

        private void ProcessResponse(string response)
        {
            if (response.Length == 0)
            {
                return;
            }

            var prefix = string.Empty;

            var startOffset = 0;
            if (response[0] == ':')
            {
                startOffset = 1;
                while (startOffset < response.Length)
                {
                    char value = response[startOffset];
                    if (value == ' ')
                    {
                        startOffset++;
                        break;
                    }
                    prefix += value;
                    startOffset++;
                }
            }

            var position = response.IndexOf(':', startOffset);
            if (position < 0)
            {
                return;
            }

            var commandParts = response.Substring(startOffset, position - startOffset).Trim().Split(' ');
            var argument = response.Substring(position + 1).Trim();
            switch (commandParts[0])
            {
                case "001":
                    SendMessage($"JOIN #xbins");
                    break;
                case "332":
                    SendMessage($"PRIVMSG #xbins :!list");
                    break;
                case "PRIVMSG":
                    ProcessPrivMsg(argument);
                    break;
                case "ERROR":
                    Disconnect();
                    break;
                case "PING":
                    SendMessage($"PONG :{argument}");
                    break;
            }
        }

        private void ProcessDownloads()
        {
            if (m_ftpRady == true && m_ftpDownloading == false)
            {
                var downloadDetails = DownloadDetailStore.GetReadyDownloadDetails();
                if (downloadDetails.Length > 0)
                {
                    m_ftpDownloading = true;
                    var downloadDetail = downloadDetails[0];

                    new Thread(() =>
                    {
                        if (downloadDetail.RemoteFile == string.Empty)
                        {
                            try
                            {
                                AddFileToDownloadStore(downloadDetail.RemoteRelativePath, downloadDetail.RemotePath, downloadDetail.LocalPath, downloadDetail.FileSize);
                                Directory.CreateDirectory(downloadDetail.LocalPath);
                                downloadDetail.Progress = "100%";
                            }
                            catch
                            {
                                downloadDetail.Progress = $"Error: Failed to get folder.";
                            }
                        }
                        else
                        {
                            if (TryDownloadFile(downloadDetail.RemotePath + downloadDetail.RemoteFile, Path.Combine(downloadDetail.LocalPath, downloadDetail.RemoteFile), downloadDetail.FileSize, p =>
                            {
                                downloadDetail.Progress = $"{(int)p}%";
                            }) == false)
                            {
                                downloadDetail.Progress = $"Error: Failed to download.";
                            }
                        }
                        m_ftpDownloading = false;

                    }).Start();
                }
            }
        }

        public void ConnectFTP()
        {
            var config = Config.LoadConfig();

            new Thread(() =>
            {
                m_disconnectRequested = false;
                m_disconnected = false;
                m_ftpRady = false;

                try
                {
                    OnConnecting?.Invoke(this, $"Tring to connect to '{config.FTPHost}'.");
                    ConnectFTP(config.FTPHost, config.FTPPort, config.FTPUser, config.FTPPassword);
                    while (!m_disconnectRequested)
                    {
                        ProcessDownloads();
                        Thread.Sleep(100);
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, $"Disconnecting as exception occured '{ex.Message}'.");
                }

                m_disconnected = true;
                m_ftpRady = false;
                m_ftpDownloading = false;

                m_ftpClient?.Disconnect();

                m_ftpClient?.Dispose();

                OnDisconnected?.Invoke(this);

            }).Start();
        }

        public void ConnectIRC()
        {
            new Thread(() =>
            {
                m_disconnectRequested = false;
                m_disconnected = false;
                m_ftpRady = false;

                var config = Config.LoadConfig();

                var connected = false;
                var servers = config.EffnetServers;
                for (int i = 0; i < servers.Length; i++)
                {
                    if (m_disconnectRequested == true)
                    {
                        break;
                    }
                    OnConnecting?.Invoke(this, $"Tring to connect to '{servers[i]}'.");
                    var tcpClient = new TcpClient();
                    var result = tcpClient.BeginConnect(servers[i], 6667, null, null);
                    var waitHandle = result.AsyncWaitHandle;
                    try
                    {
                        if (!result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(30), false))
                        {
                            tcpClient.Close();
                            tcpClient.Dispose();
                        }
                        else
                        {
                            tcpClient.EndConnect(result);
                            m_tcpClient = tcpClient;
                            connected = true;
                            break;
                        }
                    }
                    catch
                    {
                        // do nothing
                    }
                    finally
                    {
                        waitHandle.Close();
                    }
                }

                if (!connected || m_disconnectRequested == true || m_tcpClient == null)
                {
                    m_disconnected = true;
                    OnDisconnected?.Invoke(this);
                    return;
                }

                SendMessage($"NICK {GenerateWord(10)}");
                SendMessage($"USER {GenerateWord(10)} . . {GenerateWord(10)}");

                try
                {
                    var stream = m_tcpClient.GetStream();
    
                    while (!m_disconnectRequested)
                    {
                        ProcessDownloads();
                        if (stream.DataAvailable)
                        {
                            var resonse = ReadResponse();
                            ProcessResponse(resonse);
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                    SendMessage("QUIT");
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, $"Disconnecting as exception occured '{ex.Message}'.");
                }            

                m_disconnected = true;
                m_ftpRady = false;
                m_ftpDownloading = false;

                m_ftpClient?.Disconnect();

                m_ftpClient?.Dispose();
                m_tcpClient?.Dispose();

                OnDisconnected?.Invoke(this);

            }).Start();
        }

        public void Disconnect()
        {
            m_disconnectRequested = true;
        }

        public void WaitDisconenct()
        {
            while (!m_disconnected)
            {
                Thread.Sleep(100);
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    Disconnect();
                    WaitDisconenct();
                }
                m_disposed = true;
            }
        }

        ~Client()
        {
            Dispose(disposing: false);
        }
    }
}
