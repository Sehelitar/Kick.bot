using Kick.API.Events;
using Newtonsoft.Json;

namespace Kick.API.Models
{
    public class Messages
    {
        [JsonProperty("cursor")]
        public string Cursor { get; internal set; }

        [JsonProperty("pinned_message")]
        public PinnedMessageEvent PinnedMessage { get; internal set; }
    }
}
