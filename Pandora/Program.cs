using Pandora;
using System.Runtime.InteropServices;

[DllImport("kernel32.dll")]
static extern IntPtr GetConsoleWindow();

[DllImport("user32.dll")]
static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

const int SW_HIDE = 0;

try
{
    if (OperatingSystem.IsWindows())
    {
        var handle = GetConsoleWindow();
        ShowWindow(handle, SW_HIDE);
    }
    var version = "V1.0.4";
    var application = new ApplicationUI();
    application.Start(version);
}
catch (Exception ex)
{
    var now = DateTime.Now.ToString("MMddyyyyHHmmss");
    File.WriteAllText($"Crashlog-{now}.txt", ex.ToString());
    Console.WriteLine($"Error: {ex}");
}