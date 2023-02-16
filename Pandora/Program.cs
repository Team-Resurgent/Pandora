using Pandora;

public static class Program
{ 
    const string version = "v1.0.0 Beta";

    static void Main(string[] args)
    {
        var PandoraUI = new PandoraUI();
        PandoraUI.Start(version);
    }
}