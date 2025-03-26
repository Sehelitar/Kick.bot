using Newtonsoft.Json;

namespace Kick.API.Events
{
    public class ChatUpdatedEvent
    {
        [JsonProperty("id")]
        public long Id { get; internal set; }
        [JsonProperty("slow_mode")]
        public ChatUpdatedSlowMode SlowMode { get; internal set; }
        [JsonProperty("subscribers_mode")]
        public ChatUpdatedToggle SubscribersMode { get; internal set; }
        [JsonProperty("followers_mode")]
        public ChatUpdatedFollowersMode FollowersMode { get; internal set; }
        [JsonProperty("emotes_mode")]
        public ChatUpdatedToggle EmotesMode { get; internal set; }
        [JsonProperty("advanced_bot_protection")]
        public AdvancedBotProtectionSettings AdvancedBotProtection { get; internal set; }
    }

    public class ChatUpdatedToggle
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; internal set; }
    }

    public class ChatUpdatedSlowMode : ChatUpdatedToggle
    {
        [JsonProperty("message_interval")]
        public long MessageInterval { get; internal set; }
    }

    public class ChatUpdatedFollowersMode : ChatUpdatedToggle
    {
        [JsonProperty("min_duration")]
        public long MinDuration { get; internal set; }
    }

    public class AdvancedBotProtectionSettings : ChatUpdatedToggle
    {
        [JsonProperty("remaining_time")]
        public long RemainingTime { get; internal set; }
    }
}
