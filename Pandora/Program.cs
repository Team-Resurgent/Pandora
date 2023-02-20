using Pandora;

public static class Program
{ 
    const string version = "v1.0.3";

    static void Main(string[] args)
    {
        var PandoraUI = new PandoraUI();
        PandoraUI.Start(version);
    }
}
