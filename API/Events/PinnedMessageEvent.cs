using Newtonsoft.Json;
using System;

namespace Kick.API.Events
{
    public class PinnedMessageEvent
    {
        [JsonProperty("duration")]
        public long Duration { get; internal set; }

        [JsonProperty("finish_at")]
        public DateTime? FinishAt { get; internal set; }

        [JsonProperty("message")]
        public ChatMessageEvent Message { get; internal set; }
    }
}
