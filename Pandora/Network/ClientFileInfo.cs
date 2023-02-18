namespace Pandora.Network
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
        public long Size { get; set; } = 0;
    }
}
