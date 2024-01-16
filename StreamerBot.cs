/*
    Copyright (C) 2023-2024 Sehelitar

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

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
        private static StreamerBotSettings _settings;
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

        public static StreamerBotSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    Load();
                }
                return _settings;
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
                if (e.Name == "settings.json")
                    LoadSettings();
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
            BotClient.CPH?.LogVerbose("[Kick] Loading chat commands");
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

        private static void LoadSettings()
        {
            BotClient.CPH?.LogVerbose("[Kick] Loading main cofiguration");
            var fs = new FileStream("./data/settings.json", FileMode.Open);
            var config = new StreamReader(fs).ReadToEnd();
            fs.Close();
            _settings = JsonConvert.DeserializeObject<StreamerBotSettings>(config);

            Timer timer = new Timer(1000);
            timer.Elapsed += delegate
            {
                timer.Stop();
                BotTimedActionManager.ReloadTimedActions();
                timer.Close();
            };
            timer.Start();
        }

        public static void Load()
        {
            LoadCommandsSettings();
            LoadSettings();
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

    internal class StreamerBotSettings
    {
        [JsonProperty("timedActions")]
        public TimedActionsSettings TimedActions { get; set; }
    }

    internal class TimedActionsSettings
    {
        [JsonProperty("timers")]
        public List<TimedAction> Timers { get; set; }
    }

    internal class TimedAction
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("enabled")]
        public bool Enabled { get; set; } = true;
        public bool Repeat { get; set; } = false;
        public int Interval { get; set; } = 0;
        public bool RandomInterval { get; set; } = false;
        public int UpperInterval { get; set; } = 0;
        public int Lines { get; set; } = 0;
        public int Counter { get; set; } = 0;
    }
}