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
    public class SubscriptionEvent : KickBaseEvent
    {
        [JsonProperty("subscription")]
        public Subscription Subscription { get; internal set; }

        public bool IsNewSubscriber => Subscription != null && Subscription.Total <= 1;
    }

    public class Subscription
    {
        [JsonProperty("interval")]
        public long Interval { get; internal set; } = 1;
        [JsonProperty("tier")]
        public long Tier { get; internal set; } = 1;
        [JsonProperty("total")]
        public long Total { get; internal set; } = 1;
    }
}
