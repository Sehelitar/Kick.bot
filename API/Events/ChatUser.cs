/*
    Copyright (C) 2023-2025 Sehelitar

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

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
