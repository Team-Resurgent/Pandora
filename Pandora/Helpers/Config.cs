﻿using ManagedBass;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace Pandora.Helpers
{
    public class Config
    {
        public string FTPHost { get; set; }

        public int FTPPort { get; set; }

        public string FTPUser { get; set; }

        public string FTPPassword { get; set; }

        public bool  HasFTPDetails => !string.IsNullOrEmpty(FTPUser) && !string.IsNullOrEmpty(FTPPassword);

        public Config()
        {
            FTPHost = "distribution.xbins.org";
            FTPPort = 21;
            FTPUser = string.Empty;
            FTPPassword = string.Empty;
        }

//irc.mzima.net
//irc.servercentral.net
//irc.colosolutions.net
//irc.Prison.NET
//irc.underworld.no
//efnet.port80.se
//irc.homelien.no
//irc.du.se
//irc.choopa.net
//efnet.deic.eu
//irc.efnet.nl
//irc.swepipe.se
//efnet.portlane.se
//irc.efnet.fr

        public static Config LoadConfig(string path)
        {
            if (File.Exists(path)) 
            { 
                var configJson = File.ReadAllText(path);
                var result = JsonSerializer.Deserialize<Config>(configJson);
                return result ?? new Config();
            }
            return new Config();
        }

        public static Config LoadConfig()
        {
            var applicationPath = Utility.GetApplicationPath();
            if (applicationPath == null)
            {
                return new Config();
            }

            var configPath = Path.Combine(applicationPath, "config.json");
            return LoadConfig(configPath);
        }

        public static void SaveConfig(string path, Config? config)
        {
            if (config == null)
            {
                return;
            }

            var result = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, result);
        }

        public static void SaveConfig(Config? config)
        {
            var applicationPath = Utility.GetApplicationPath();
            if (applicationPath == null)
            {
                return;
            }

            var configPath = Path.Combine(applicationPath, "config.json");
            SaveConfig(configPath, config);
        }

    }
}
