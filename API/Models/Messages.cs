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

using Kick.API.Events;
using Newtonsoft.Json;

namespace Kick.API.Models
{
    public class Messages
    {
        [JsonProperty("cursor")]
        public string Cursor { get; internal set; }

        [JsonProperty("pinned_message")]
        public PinnedMessageEvent PinnedMessage { get; internal set; }
    }
}
