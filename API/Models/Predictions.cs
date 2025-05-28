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
    public class Prediction
    {
        public const string StateActive = "ACTIVE";
        public const string StateLocked = "LOCKED";
        public const string StateResolved = "RESOLVED";
        public const string StateCancelled = "CANCELLED";
        
        [JsonProperty("id")]
        public string Id { get; internal set; }
        [JsonProperty("channel_id")]
        public long ChannelId { get; internal set; }
        [JsonProperty("title")]
        public string Title { get; internal set; }
        [JsonProperty("state")]
        public string State { get; internal set; }
        [JsonProperty("outcomes")]
        public Outcome[] Outcomes { get; internal set; }
        [JsonProperty("duration")]
        public long Duration { get; internal set; }
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; internal set; }
        [JsonProperty("updated_at")]
        public DateTime? UpdatedAt { get; internal set; }
        [JsonProperty("locked_at")]
        public DateTime? LockedAt { get; internal set; }
        [JsonProperty("winning_outcome_id")]
        public string WinningOutcomeId { get; internal set; }
    }

    public class Outcome
    {
        [JsonProperty("id")]
        public string Id { get; internal set; }
        [JsonProperty("title")]
        public string Title { get; internal set; }
        [JsonProperty("total_vote_amount")]
        public long TotalVoteAmount { get; internal set; }
        [JsonProperty("vote_count")]
        public long VoteCount { get; internal set; }
        [JsonProperty("return_rate")]
        public double ReturnRate { get; internal set; }
        [JsonProperty("top_users")]
        public UserChannel[] TopUsers { get; internal set; }
    }

    public class PredictionResponse
    {
        [JsonProperty("prediction")]
        public Prediction Prediction { get; internal set; }
    }

    public class PredictionsList
    {
        [JsonProperty("predictions")]
        public Prediction[] Predictions { get; internal set; }
    }
}