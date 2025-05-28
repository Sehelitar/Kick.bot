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
    public class Reward
    {
        [JsonProperty("id" )]
        public string Id { get; set; }
        
        [JsonProperty("title" )]
        public string Title { get; set; }
        
        [JsonProperty("description" )]
        public string Description { get; set; }
        
        [JsonProperty("background_color" )]
        public string BackgroundColor { get; set; }
        
        [JsonProperty("cost")]
        public long Cost { get; set; }
        
        [JsonProperty("is_enabled" )]
        public bool IsEnabled { get; set; }
        
        [JsonProperty("is_paused" )]
        public bool IsPaused { get; set; }
        
        [JsonProperty("is_user_input_required" )]
        public bool IsUserInputRequired { get; set; }
        
        [JsonProperty("prompt" )]
        public string Prompt { get; set; }
        
        [JsonProperty("should_redemptions_skip_request_queue" )]
        public bool ShouldRedemptionsSkipRequestQueue { get; set; }
    }
}