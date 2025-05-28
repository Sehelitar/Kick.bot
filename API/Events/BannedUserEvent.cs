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
