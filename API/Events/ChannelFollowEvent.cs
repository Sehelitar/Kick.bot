using Newtonsoft.Json;

namespace Kick.API.Events
{
    public class ChannelFollowEvent : KickBaseEvent
    {
        [JsonProperty("followers_count")]
        public long FollowersCount { get; internal set; }

        public bool IsFollowing { get; internal set; } = true;
    }
}
