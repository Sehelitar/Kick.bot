using Newtonsoft.Json;

namespace Kick.API.Events
{
    public class GiftedSubscriptionEvent : KickBaseEvent
    {
        [JsonProperty("gifted_users")]
        public EventUser[] GiftedUsers { get; internal set; }
    }
}
