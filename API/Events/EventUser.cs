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
