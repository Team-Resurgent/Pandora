using Pandora;

public static class Program
{ 
    const string version = "v1.0.2";

    static void Main(string[] args)
    {
        var PandoraUI = new PandoraUI();
        PandoraUI.Start(version);
    }
}
