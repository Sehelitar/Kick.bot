using Newtonsoft.Json;

namespace Kick.API.Events
{
    public class SubscriptionEvent : KickBaseEvent
    {
        [JsonProperty("subscription")]
        public Subscription Subscription { get; internal set; }

        public bool IsNewSubscriber => Subscription != null && Subscription.Total <= 1;
    }

    public class Subscription
    {
        [JsonProperty("interval")]
        public long Interval { get; internal set; } = 1;
        [JsonProperty("tier")]
        public long Tier { get; internal set; } = 1;
        [JsonProperty("total")]
        public long Total { get; internal set; } = 1;
    }
}
