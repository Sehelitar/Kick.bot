using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;

namespace Kick.Bot
{
    internal class StreamerBotAppSettings
    {
        private static StreamerBotCommands _commands;
        private static FileSystemWatcher _configWatcher;

        public static List<StreamerBotCommand> Commands
        {
            get
            {
                if (_commands == null)
                {
                    Load();
                }
                return _commands.Commands;
            }
        }

        public static void StartWatcher()
        {
            if (_configWatcher != null)
                return;

            _configWatcher = new FileSystemWatcher("./data/", "*.json")
            {
                NotifyFilter = NotifyFilters.Attributes
                    | NotifyFilters.CreationTime
                    | NotifyFilters.DirectoryName
                    | NotifyFilters.FileName
                    | NotifyFilters.LastAccess
                    | NotifyFilters.LastWrite
                    | NotifyFilters.Security
                    | NotifyFilters.Size,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };
            _configWatcher.Changed += delegate (object sender, FileSystemEventArgs e)
            {
                if (e.Name == "commands.json")
                    LoadCommandsSettings();
            };
        }

        public static void StopWatcher()
        {
            if (_configWatcher != null)
            {
                _configWatcher.Dispose();
                _configWatcher = null;
            }
        }

        private static void LoadCommandsSettings()
        {
            BotClient.CPH?.LogVerbose("[Kick] Chargement des commandes de chat");
            var fs = new FileStream("./data/commands.json", FileMode.Open);
            var config = new StreamReader(fs).ReadToEnd();
            fs.Close();
            _commands = JsonConvert.DeserializeObject<StreamerBotCommands>(config);

            Timer timer = new Timer(1000);
            timer.Elapsed += delegate
            {
                timer.Stop();
                BotChatCommander.ReloadCommands();
                timer.Close();
            };
            timer.Start();
        }

        public static void Load()
        {
            LoadCommandsSettings();
            StartWatcher();
        }
    }

    internal class StreamerBotCommands
    {
        [JsonProperty("commands")]
        public List<StreamerBotCommand> Commands { get; set; }
    }

    internal class StreamerBotCommand
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("sources")]
        public List<long> Sources { get; set; }
        [JsonProperty("permittedUsers")]
        public List<string> PermittedUsers { get; set; }
        [JsonProperty("permittedGroups")]
        public List<string> PermittedGroups { get; set; }
        [JsonProperty("enabled")]
        public bool Enabled { get; set; } = true;
        [JsonProperty("include")]
        public bool Include { get; set; } = true;
        [JsonProperty("mode")]
        public int Mode { get; set; } = 0;
        [JsonProperty("command")]
        public string Command { get; set; }
        [JsonProperty("location")]
        public long Location { get; set; } = 0;
        [JsonProperty("persistCounter")]
        public bool PersistCounter { get; set; } = false;
        [JsonProperty("persistUserCounter")]
        public bool PersistUserCounter { get; set; } = false;
        [JsonProperty("caseSensitive")]
        public bool CaseSensitive { get; set; } = false;
        [JsonProperty("globalCooldown")]
        public long GlobalCooldown { get; set; } = 0;
        [JsonProperty("userCooldown")]
        public long UserCooldown { get; set; } = 0;
        [JsonProperty("group")]
        public string Group { get; set; } = String.Empty;
        [JsonProperty("grantType")]
        public int GrantType { get; set; } = 0;
    }
}