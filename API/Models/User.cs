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

namespace Kick.API.Models
{
    public class User
    {
        [JsonProperty("id")]
        public int Id { get; internal set; } = -1;
        [JsonProperty("username")]
        public string Username { get; internal set; }
        [JsonProperty("agreed_to_terms")]
        public bool AgreedToTerms { get; internal set; } = true;
        [JsonProperty("apple_id")]
        public string AppleId { get; internal set; }
        [JsonProperty("bio")]
        public string Bio { get; internal set; } = string.Empty;
        [JsonProperty("channel_can_be_updated")]
        public bool ChannelCanBeUpdated { get; internal set; } = true;
        [JsonProperty("city")]
        public string City { get; internal set; } = string.Empty;
        [JsonProperty("country")]
        public string Country { get; internal set; } = string.Empty;
        [JsonProperty("discord")]
        public string Discord { get; internal set; } = string.Empty;
        [JsonProperty("email")]
        public string Email { get; internal set; } = string.Empty;
        [JsonProperty("enable_live_notifications")]
        public bool EnableLiveNotifications { get; internal set; }
        [JsonProperty("enable_onscreen_live_notifications")]
        public bool EnableOnScreenLiveNotifications { get; internal set; }
        [JsonProperty("enable_sms_promo")]
        public bool EnableSMSPromo { get; internal set; }
        [JsonProperty("enable_sms_security")]
        public bool EnableSMSSecurity { get; internal set; }
        [JsonProperty("facebook")]
        public string Facebook { get; internal set; } = string.Empty;
        [JsonProperty("google_id")]
        public string GoogleId { get; internal set; } = null;
        [JsonProperty("instagram")]
        public string Instagram { get; internal set; } = string.Empty;
        [JsonProperty("is_2fa_internal setup")]
        public bool Is2FASetup { get; internal set; }
        [JsonProperty("is_live")]
        public bool IsLive { get; internal set; }
        [JsonProperty("newsletter_subscribed")]
        public bool NewsletterSubscribed { get; internal set; }
        [JsonProperty("phone")]
        public string Phone { get; internal set; }
        [JsonProperty("profile_pic")]
        public string ProfilePic { get; internal set; }
        [JsonProperty("profilepic")]
        public string ProfilePicAlt { get; internal set; }
        [JsonProperty("redirect")]
        public string Redirect { get; internal set; } = string.Empty;
        [JsonProperty("state")]
        public string State { get; internal set; } = string.Empty;
        [JsonProperty("streamer_channel")]
        public UserChannel StreamerChannel { get; internal set; }
        [JsonProperty("tiktok")]
        public string Tiktok { get; internal set; } = string.Empty;
        [JsonProperty("twitter")]
        public string Twitter { get; internal set; } = string.Empty;
        [JsonProperty("youtube")]
        public string Youtube { get; internal set; } = string.Empty;
    }

    public class UserChannel
    {
        [JsonProperty("can_host")]
        public bool CanHost { get; internal set; } = true;
        [JsonProperty("id")]
        public long Id { get; internal set; }
        [JsonProperty("is_banned")]
        public bool IsBanned { get; internal set; }
        [JsonProperty("playback_url")]
        public string PlaybackUrl { get; internal set; } = string.Empty;
        [JsonProperty("slug")]
        public string Slug { get; internal set; }
        [JsonProperty("subscription_enabled")]
        public bool SubscriptionEnabled { get; internal set; }
        [JsonProperty("user_id")]
        public long UserId { get; internal set; }
        [JsonProperty("vod_enabled")]
        public bool VodEnabled { get; internal set; } = true;
    }
}
