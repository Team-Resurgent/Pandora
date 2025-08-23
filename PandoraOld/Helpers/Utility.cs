using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Pandora.Helpers
{
    public static class Utility
    {
        public static bool IsAdmin()
        {
            if (!OperatingSystem.IsWindows())
            {
                return false;
            }
            bool isAdmin;
            try
            {
                var user = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                isAdmin = false;
            }
            return isAdmin;
        }

        public static string? GetApplicationPath()
        {
            var exePath = AppDomain.CurrentDomain.BaseDirectory;
            if (exePath == null)
            {
                return null;
            }

            var result = Path.GetDirectoryName(exePath);
            return result;
        }

        public static IEnumerable<string> GetSpecialFolders()
        {
            var specialFolders = new List<string>();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                specialFolders.Add($"/|/");
                specialFolders.Add($"User|{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}");
                specialFolders.Add($"Desktop|{Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}");
                specialFolders.Add($"Documents|{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Documents");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                specialFolders.Add($"User|{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}");
                specialFolders.Add($"Desktop|{Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}");
                specialFolders.Add($"Documents|{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}");
            }

            var logicalDrives = Directory.GetLogicalDrives();
            foreach (var logicalDrive in logicalDrives)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    if (logicalDrive.StartsWith("/Volume", StringComparison.CurrentCultureIgnoreCase))
                    {
                        specialFolders.Add($"{Path.GetFileName(logicalDrive)}|{logicalDrive}");
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    specialFolders.Add($"{logicalDrive.Substring(0, 2)}|{logicalDrive}");
                }
            }

            return specialFolders.ToArray();
        }
    }
}
