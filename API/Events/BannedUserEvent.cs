using Newtonsoft.Json;
using System;

namespace Kick.API.Events
{
    public class BannedUserEvent : KickBaseEvent
    {
        [JsonProperty("ban")]
        public UserBan Ban { get; internal set; }
        [JsonProperty("banned")]
        public EventUser Banned { get; internal set; }
        public bool IsBanned => Ban != null;
    }

    public class UserBan
    {
        [JsonProperty("reason")]
        public string Reason { get; internal set; } = string.Empty;
        [JsonProperty("created_at")]
        public DateTime BannedSince { get; internal set; } = DateTime.Now;
        [JsonProperty("expires_at")]
        public DateTime? BannedUntil { get; internal set; }
        [JsonProperty("type")]
        public string Type { get; internal set; } = "complete";
    }

    public class UserBanResult : UserBan
    {
        [JsonProperty("id")]
        public string Id { get; internal set; }
        [JsonProperty("permanent")]
        public bool Permanent { get; internal set; }
        [JsonProperty("banner_id")]
        public string BannerId { get; internal set; }
        [JsonProperty("banned_id")]
        public string BannedId { get; internal set; }
        [JsonProperty("chat_id")]
        public string ChatId { get; internal set; }
    }
}
