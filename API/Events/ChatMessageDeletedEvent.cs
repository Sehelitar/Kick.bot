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
    public class ChatMessageDeletedEvent
    {
        [JsonProperty("id")]
        public string Id { get; internal set; }
        [JsonProperty("message")]
        public ChatMessageDeletedInfos Message { get; internal set; }
        [JsonProperty("aiModerated")]
        public bool AiModerated { get; internal set; }
        [JsonProperty("violatedRules")]
        public string[] ViolatedRules { get; internal set; } = {};
    }

    public class ChatMessageDeletedInfos
    {
        [JsonProperty("id")]
        public string Id { get; internal set; }
    }
}
