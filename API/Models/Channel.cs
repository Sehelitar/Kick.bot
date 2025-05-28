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
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kick.API.Models
{
    public class Channel
    {
        [JsonProperty("banner_image")]
        public ChannelBanner Banner { get; internal set; }
        [JsonProperty("can_host")]
        public bool CanHost { get; internal set; } = true;
        [JsonProperty("chatroom")]
        public ChannelChatroom Chatroom { get; internal set; }
        [JsonProperty("followersCount")]
        public long FollowersCount { get; internal set; }
        [JsonProperty("following")]
        public bool Following { get; internal set; }
        [JsonProperty("id")]
        public long Id { get; internal set; }
        [JsonProperty("is_banned")]
        public bool IsBanned { get; internal set; }
        [JsonProperty("livestream")]
        public LiveStream LiveStream { get; internal set; }
        [JsonProperty("muted")]
        public bool Muted { get; internal set; }
        [JsonProperty("offline_banner_image")]
        public ChannelBanner OfflineBannerImage { get; internal set; }
        [JsonProperty("playback_url")]
        public string PlaybackUrl { get; internal set; }
        [JsonProperty("previous_livestreams")]
        public List<PreviousStream> PreviousLiveStreams { get; internal set; }
        [JsonProperty("recent_categories")]
        public List<Category> RecentCategories { get; internal set; }
        [JsonProperty("role")]
        public string Role { get; internal set; }
        [JsonProperty("slug")]
        public string Slug { get; internal set; }
        [JsonProperty("user")]
        public User User { get; internal set; }
        [JsonProperty("user_id")]
        public long UserId { get; internal set; }
        [JsonProperty("subscription_enabled")]
        public bool SubscriptionEnabled { get; internal set; }
        [JsonProperty("verified")]
        public ChannelVerified Verified { get; internal set; }
        [JsonProperty("vod_enabled")]
        public bool VodEnabled { get; internal set; } = true;

        public bool IsAffiliate => SubscriptionEnabled;

        public bool IsVerified => Verified != null;
    }

    public class ChannelBanner
    {
        [JsonProperty("responsive")]
        public string Responsive { get; internal set; } = string.Empty;
        [JsonProperty("url")]
        public string Url { get; internal set; } = string.Empty;
    }

    public class ChannelChatroom
    {
        [JsonProperty("channel_id")]
        public long ChannelId { get; internal set; }
        [JsonProperty("chat_mode")]
        public string ChatMode { get; internal set; } = "public";
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; internal set; }
        [JsonProperty("emotes_mode")]
        public bool IsEmotesOnly { get; internal set; }
        [JsonProperty("followers_mode")]
        public bool IsFollowersOnly { get; internal set; }
        [JsonProperty("following_min_duration")]
        public long MinFollowDuration { get; internal set; }
        [JsonProperty("id")]
        public long Id { get; internal set; }
        [JsonProperty("message_interval")]
        public long MessageInterval { get; internal set; }
        [JsonProperty("slow_mode")]
        public bool IsSlowModeEnabled { get; internal set; }
        [JsonProperty("subscribers_mode")]
        public bool IsSubOnly { get; internal set; }
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; internal set; }
    }

    public class ChannelVerified
    {
        [JsonProperty("channel_id")]
        public long ChannelId { get; internal set; }
        [JsonProperty("id")]
        public long Id { get; internal set; }
        [JsonProperty("created_at")]
        public DateTime Since { get; internal set; }
    }

    public class ChannelUser
    {
        [JsonProperty("badges")]
        public List<ChannelUserBadge> Badges { get; internal set; }
        [JsonProperty("banned")]
        public ChannelUserBan Banned { get; internal set; }
        [JsonProperty("following_since")]
        public DateTime? FollowingSince { get; internal set; }
        [JsonProperty("id")]
        public long Id { get; internal set; }
        [JsonProperty("is_channel_owner")]
        public bool IsChannelOwner { get; internal set; }
        [JsonProperty("is_moderator")]
        public bool IsModerator { get; internal set; }
        [JsonProperty("is_staff")]
        public bool IsStaff { get; internal set; }
        [JsonProperty("profile_pic")]
        public string ProfilePic { get; internal set; } = String.Empty;
        [JsonProperty("slug")]
        public string Slug { get; internal set; }
        [JsonProperty("subscribed_for")]
        public int SubscribedFor { get; internal set; }
        [JsonProperty("username")]
        public string Username { get; internal set; }

        public bool IsBanned => Banned != null;

        public bool IsBroadcaster => IsChannelOwner;

        public bool IsVip => HasBadgeType("vip");

        public bool IsOG => HasBadgeType("og");

        public bool IsSubscriber => HasBadgeType("subscriber");

        public bool IsFounder => HasBadgeType("founder");

        public bool IsFollowing => FollowingSince.HasValue;

        public bool HasBadgeType(string badgeType)
        {
            if (Badges?.Count > 0)
            {
                if (Badges.FirstOrDefault(badge => badge.Type == badgeType) != null)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class ChannelUserBadge
    {
        [JsonProperty("type")]
        public string Type { get; internal set; }
        [JsonProperty("text")]
        public string Text { get; internal set; } = string.Empty;
    }

    public class ChannelUserBan
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
}
