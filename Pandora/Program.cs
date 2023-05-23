using Pandora;
using System.Runtime.InteropServices;

[DllImport("kernel32.dll")]
static extern IntPtr GetConsoleWindow();

[DllImport("user32.dll", CharSet = CharSet.Auto)]
static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

const int WM_CLOSE = 0x0010;

try
{
    if (OperatingSystem.IsWindows())
    {
        var handle = GetConsoleWindow();
        SendMessage(handle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
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