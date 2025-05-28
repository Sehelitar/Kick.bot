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

using System;
using Newtonsoft.Json;

namespace Kick.API.Models
{
    public class Redemption
    {
        [JsonProperty("channel_id")]
        public long ChannelId { get; set; }
        
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("reward_id")]
        public string RewardId { get; set; }
        
        [JsonProperty("reward_title")]
        public string RewardTitle { get; set; }
        
        [JsonProperty("status")]
        public string Status { get; set; }
        
        [JsonProperty("transaction_id")]
        public string TransactionId { get; set; }
        
        [JsonProperty("user_id")]
        public long UserId { get; set; }
        
        [JsonProperty("username")]
        public string Username { get; set; }
        
        [JsonProperty("username_color")]
        public string UsernameColor { get; set; }
    }

    public class RedemptionList
    {
        [JsonProperty("next_page_token")]
        public string NextPageToken { get; set; }
        
        [JsonProperty("redemptions")]
        public Redemption[] Redemptions { get; set; }
    }
    
    public class FailedRedemptionsList
    {
        [JsonProperty("failed_redemption_ids")]
        public string[] FailedRedemptionIds { get; set; }
    }
}