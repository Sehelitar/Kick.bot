using Newtonsoft.Json;
using System;

namespace Kick.API.Events
{
    public class PinnedMessageEvent
    {
        [JsonProperty("duration")]
        public string Duration { get; internal set; }

        [JsonProperty("message")]
        public ChatMessageEvent Message { get; internal set; }
        
        [JsonProperty("pinnedBy")]
        public ChatUser PinnedBy { get; internal set; }
    }
}
