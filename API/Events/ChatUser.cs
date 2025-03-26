using Newtonsoft.Json;
using System.Linq;

namespace Kick.API.Events
{
    public class ChatUser
    {
        [JsonProperty("id")]
        public long Id { get; internal set; }
        [JsonProperty("username")]
        public string Username { get; internal set; }
        [JsonProperty("slug")]
        public string Slug { get; internal set; }
        [JsonProperty("identity")]
        public ChatUserIdentity Identity { get; internal set; }

        [JsonIgnore]
        public bool IsBroadcaster => HasBadgeType("broadcaster");

        [JsonIgnore]
        public bool IsModerator => HasBadgeType("moderator");

        [JsonIgnore]
        public bool IsVip => HasBadgeType("vip");

        [JsonIgnore]
        public bool IsOG => HasBadgeType("og");

        [JsonIgnore]
        public bool IsSubscriber => HasBadgeType("subscriber");

        [JsonIgnore]
        public bool IsFounder => HasBadgeType("founder");

        [JsonIgnore]
        public int SubscribedFor
        {
            get
            {
                if (Identity?.Badges?.Length > 0)
                {
                    var subBadge = Identity.Badges.FirstOrDefault(badge => badge.Type == "subscriber");
                    if (subBadge != null)
                    {
                        return subBadge.Count;
                    }
                }
                return 0;
            }
        }

        public bool HasBadgeType(string badgeType)
        {
            if (!(Identity?.Badges?.Length > 0)) return false;
            return Identity.Badges.FirstOrDefault(badge => badge.Type == badgeType && badge.Active) != null;
        }
    }

    public class ChatUserIdentity
    {
        [JsonProperty("color")]
        public string Color { get; internal set; } = "#000000";
        [JsonProperty("badges")]
        public ChatUserBadges[] Badges { get; internal set; } = { };
    }

    public class ChatUserBadges
    {
        [JsonProperty("active")]
        public bool Active { get; internal set; } = true;
        [JsonProperty("type")]
        public string Type { get; internal set; }
        [JsonProperty("text")]
        public string Text { get; internal set; } = string.Empty;
        [JsonProperty("count")]
        public int Count { get; internal set; }
    }
}
