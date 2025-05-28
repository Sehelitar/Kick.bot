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
    public class Poll
    {
        [JsonProperty("duration")]
        public int Duration { get; internal set; }
        [JsonProperty("remaining")]
        public int Remaining { get; internal set; }
        [JsonProperty("result_display_duration")]
        public int ResultDisplayDuration { get; internal set; }
        [JsonProperty("title")]
        public string Title {  get; internal set; }
        [JsonProperty("has_voted")]
        public bool HasVoted { get; internal set; }
        [JsonProperty("options")]
        public PollOption[] Options { get; internal set; }
    }

    public class PollOption
    {
        [JsonProperty("id")]
        public int Id { get; internal set; }
        [JsonProperty("label")]
        public string Label { get; internal set; }
        [JsonProperty("votes")]
        public int Votes { get; internal set; }
    }
}
