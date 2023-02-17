namespace Pandora.Network
{
    public class DownloadDetail
    {
        public string Progress { get; set; }
        public string RemoteRelativePath { get; set; }
        public string RemotePath { get; set; }
        public string RemoteFile { get; set; }
        public string LocalPath { get; set; }

        public string Key
        {
            get
            {
                return $"{RemotePath}|{RemoteFile}|{LocalPath}";
            }
        }

        public DownloadDetail(string remoteRelativePath, string remotePath, string remoteFile, string localPath)
        {            
            Progress = string.Empty;
            RemoteRelativePath = remoteRelativePath;
            RemotePath = remotePath;
            RemoteFile = remoteFile;
            LocalPath = localPath;
        }
    }
}
