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
    public class KicksGiftedEvent
    {
        [JsonProperty("message")]
        public string Message { get; internal set; }
        [JsonProperty("sender")]
        public KicksGiftingUser Sender { get; internal set; }
        [JsonProperty("gift")]
        public KicksGift Gift { get; internal set; }
    }

    public class KicksGiftingUser
    {
        [JsonProperty("id")]
        public string Id { get; internal set; }
        [JsonProperty("username")]
        public string Username { get; internal set; }
        [JsonProperty("username_color")]
        public string Color { get; internal set; }
    }

    public class KicksGift
    {
        [JsonProperty("gift_id")]
        public string GiftId { get; internal set; }
        [JsonProperty("name")]
        public string Name { get; internal set; }
        [JsonProperty("amount")]
        public int Amount { get; internal set; }
        [JsonProperty("type")]
        public string Type { get; internal set; }
        [JsonProperty("tier")]
        public string Tier { get; internal set; }
        [JsonProperty("character_limit")]
        public int CharacterLimit { get; internal set; }
        [JsonProperty("pinned_time")]
        public int PinnedTime { get; internal set; }
    }
}
