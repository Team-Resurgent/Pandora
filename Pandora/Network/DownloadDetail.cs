using FluentFTP.Helpers;

namespace Pandora.Network
{
    public class DownloadDetail
    {
        public string Progress { get; set; }
        public string RemoteRelativePath { get; set; }
        public string RemotePath { get; set; }
        public string RemoteFile { get; set; }
        public string LocalPath { get; set; }
        public long FileSize { get; set; }

        public string Key
        {
            get
            {
                return $"{RemotePath}|{RemoteFile}|{LocalPath}";
            }
        }

        public DownloadDetail(string remoteRelativePath, string remotePath, string remoteFile, string localPath, long fileSize)
        {            
            Progress = string.Empty;
            RemoteRelativePath = remoteRelativePath;
            RemotePath = remotePath;
            RemoteFile = remoteFile;
            LocalPath = localPath;
            FileSize = fileSize;
        }
    }
}
