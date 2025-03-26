using Newtonsoft.Json;

namespace Kick.API.Events
{
    public class EventUser
    {
        [JsonProperty("id")]
        public long Id { get; internal set; }
        [JsonProperty("username")]
        public string Username { get; internal set; }
        [JsonProperty("slug")]
        public string Slug { get; internal set; }
    }

    public class VipUserEvent
    {
        [JsonProperty("vip")]
        public EventUser Vip { get; internal set; }
    }

    public class OgUserEvent
    {
        [JsonProperty("og")]
        public EventUser OG { get; internal set; }
    }

    public class ModeratorUserEvent
    {
        [JsonProperty("moderator")]
        public EventUser Moderator { get; internal set; }
    }
}
