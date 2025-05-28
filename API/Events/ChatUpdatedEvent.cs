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
        [JsonProperty("account_age")]
        public ChatUpdatedAccountAge AccountAge { get; internal set; }
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
    
    public class ChatUpdatedAccountAge : ChatUpdatedToggle
    {
        [JsonProperty("min_duration")]
        public long MinDuration { get; internal set; }
    }
}
