using Newtonsoft.Json;

namespace Kick.API.Models
{
    public class ChatCommand
    {
        [JsonProperty("command")]
        public string Command { get; internal set; }
        [JsonProperty("parameter")]
        public string Parameter { get; internal set; }

        internal ChatCommand() { }

        public ChatCommand(string command, string parameter) {
            Command = command;
            Parameter = parameter;
        }
    }

    public class ChatCommandResult : ChatCommand
    {
        [JsonProperty("success")]
        public bool Success { get; internal set; }
    }
}
