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