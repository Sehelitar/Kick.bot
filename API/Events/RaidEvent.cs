using Newtonsoft.Json;

namespace Kick.API.Events
{
    public class RaidEvent : KickBaseEvent
    {
        [JsonProperty("host")]
        public RaidHost Host { get; internal set; }
    }

    public class RaidHost
    {
        [JsonProperty("viewers_count")]
        public int ViewersCount { get; internal set; }

        [JsonProperty("user")]
        public EventUser User { get; internal set; }
    }
}
