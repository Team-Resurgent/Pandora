using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using FluentFTP;
using FluentFTP.Exceptions;
using System.ComponentModel;
using System;
using Veldrid.MetalBindings;

namespace Pandora.Network
{
    public class Client : IDisposable
    {
        public enum FileType
        {
            File = 0,
            Directory = 1
        }

        public class ClientFileInfo
        {
            public FileType FileType { get; set; } = FileType.File;
            public string Path { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public bool Selected { get; set; } = false;
        }

        private TcpClient? m_tcpClient;

        private FtpClient? m_ftpClient;

        public delegate void ErrorEventHandler(object sender, string error);
        public delegate void ConnectedHandler(object sender);
        public delegate void DisconnectedHandler(object sender);
        public delegate void MessageSentHandler(object sender, string message);
        public delegate void MessageRecievedHandler(object sender, string message);

        public event ErrorEventHandler? OnError;
        public event ConnectedHandler? OnConnected;
        public event DisconnectedHandler? OnDisconnected;
        public event MessageSentHandler? OnMessageSent;
        public event MessageRecievedHandler? OnMessageRecieved;

        private bool m_disposed = false;
        private bool m_disconnected = false;
        private bool m_disconnectRequested = false;
        private bool m_ftpRady = false;

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

            try
            {
                m_ftpClient = new FtpClient(host, user, pass, port);
                var profile = m_ftpClient.AutoConnect();
                if (profile == null)
                {
                    OnError?.Invoke(this, "ProcessPrivMsg: Unable to connect to FTP, disconnecting.");
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

        public bool CanGetFiles()
        {
            if (m_ftpClient == null || m_ftpRady == false)
            {
                return false;
            }
            return true;
        }

        public ClientFileInfo[]? GetFiles(string path)
        {
            if (m_ftpClient == null || m_ftpRady == false)
            {
                return null;
            }

            try
            {
                var files = new List<ClientFileInfo>();

                foreach (FtpListItem item in m_ftpClient.GetListing(path))
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

        public bool Connect(string server, int port)
        {
            var tcpClient = new TcpClient();
            var result = tcpClient.BeginConnect(server, port, null, null);
            var waitHandle = result.AsyncWaitHandle;
            try
            {
                if (!result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(30), false))
                {
                    tcpClient.Close();
                    return false;
                }
                tcpClient.EndConnect(result);
            }
            catch
            {
                return false;
            }
            finally
            {
                waitHandle.Close();
            }

            m_tcpClient = tcpClient;
            new Thread(() =>
            {
                SendMessage($"NICK {GenerateWord(10)}");
                SendMessage($"USER {GenerateWord(10)} . . {GenerateWord(10)}");

                m_disconnectRequested = false;
                m_disconnected = false;
                m_ftpRady = false;

                try
                {
                    var stream = m_tcpClient.GetStream();
                    while (!m_disconnectRequested)
                    {
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

                try
                {
                    if (m_ftpClient != null)
                    {
                        m_ftpClient?.Dispose();
                    }
                }
                catch 
                {
                    // ignore
                }

                try
                {
                    m_tcpClient?.Dispose();
                }
                catch
                {
                    // ignore
                }

                OnDisconnected?.Invoke(this);

            }).Start();

            return true;
        }

        public void Disconnect()
        {
            m_disconnectRequested = true;
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
