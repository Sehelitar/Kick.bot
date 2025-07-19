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

using Kick.API.Models;
using Kick.API.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Kick.API
{
    public sealed partial class KickClient
    {
        /// <summary>Fetch channel infos from a channel identified by its slug name.</summary>
        /// <param name="channelName">Slug name of the channel to fetch infos from.</param>
        /// <returns>A channel object or null if the channel doesn't exist.</returns>
        public async Task<Channel> GetChannelInfos(string channelName)
        {
            return await ApiGet<Channel>("/api/v1/channels/" + channelName);
        }

        /// <summary>Fetch user infos from a channel. Both names have to be slugs.</summary>
        /// <param name="channelName">Slug name of the channel to fetch infos from.</param>
        /// <param name="username">Slug name of the user to fetch infos from.</param>
        /// <returns>A ChatroomUser object or null if the channel doesn't exist.</returns>
        public async Task<ChannelUser> GetChannelUserInfos(string channelName, string username)
        {
            return await ApiGet<ChannelUser>("/api/v2/channels/" + channelName + "/users/" + username);
        }

        /// <summary>Fetch user infos about the currently authenticated user.</summary>
        /// <returns>A User object representing the authenticated user, or null if still unauthenticated.</returns>
        public async Task<User> GetCurrentUserInfos()
        {
            return await ApiGet<User>("/api/v1/user");
        }

        #region Chat / Utils
        /// <summary>
        /// Send a chat message to a chatroom.
        /// </summary>
        /// <param name="chatroom">ID of the target chatroom. You can find it by fetching channel's infos, as Chatroom.Id.</param>
        /// <param name="message">Message to send to the channel.</param>
        /// <returns></returns>
        /// <exception cref="UnauthenticatedException">Thrown if user is unauthenticated.</exception>
        /// <exception cref="Exception">Thrown if remote API replied with an error message.</exception>
        public async Task<ChatMessageEvent> SendMessageToChatroom(long chatroom, string message)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            Dictionary<string, dynamic> data = new Dictionary<string, dynamic>() {
                { "content", message },
                { "type", "message" }
            };
            var response = await ApiJsonPost<KickApiOperationResponse<ChatMessageEvent>>("/api/v2/messages/send/" + chatroom, data);
            if(response.Status.Error)
            {
                throw new Exception(response.Status.Message);
            }
            return response.Data;
        }

        /// <summary>
        /// Send a reply to message received from a chatroom.
        /// </summary>
        /// <param name="chatroom">Id of the target chatroom. You can find it by fetching channel's infos, as Chatroom.Id.</param>
        /// <param name="message">Reply to send to the channel.</param>
        /// <param name="originalMessageId">ID of the message you are replying to.</param>
        /// <param name="originalMessage">Content of the message you are replying to.</param>
        /// <param name="originalSenderId">ID of the user you are replying to.</param>
        /// <param name="originalSender">Display name of the user you are replying to.</param>
        /// <returns></returns>
        /// <exception cref="UnauthenticatedException">Thrown if user is unauthenticated.</exception>
        /// <exception cref="Exception">Thrown if remote API replied with an error message.</exception>
        public async Task<ChatMessageEvent> SendReplyToChatroom(long chatroom, string message, string originalMessageId, string originalMessage, long originalSenderId, string originalSender)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            Dictionary<string, dynamic> data = new Dictionary<string, dynamic>() {
                { "content", message },
                { "type", "reply" },
                {
                    "metadata",
                    new Dictionary<string, dynamic>()
                    {
                        { "original_message", new Dictionary<string, dynamic>() { { "id", originalMessageId }, { "content", originalMessage } } },
                        { "original_sender", new Dictionary<string, dynamic>() { { "id", originalSenderId }, { "username", originalSender } } }
                    }
                }
            };
            var response = await ApiJsonPost<KickApiOperationResponse<ChatMessageEvent>>("/api/v2/messages/send/" + chatroom, data);
            if (response.Status.Error)
            {
                throw new Exception(response.Status.Message);
            }
            return response.Data;
        }

        public async Task<bool> DeleteMessage(long chatroom, string message)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            var response = await ApiDelete<KickApiOperationResponse<string>>("/api/v2/chatrooms/" + chatroom + "/messages/" + message);
            if (response.Status.Error)
            {
                throw new Exception(response.Status.Message);
            }
            return true;
        }

        private async Task<ChatCommandResult> SendChatCommand(Channel channel, ChatCommand command)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            var response = await ApiJsonPost<ChatCommandResult>($"/api/v2/channels/{channel.Slug}/chat-commands", command);
            if (response == null)
            {
                throw new Exception("Chat command failed");
            }
            return response;
        }
        
        public async Task ClearChat(Channel channel)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            await SendChatCommand(channel, new ChatCommand("clear", null));
        }
        #endregion
        
        #region Chatroom Modes
        public async Task<ChatUpdatedEvent> SetChannelChatroomEmotesOnly(Channel channel, bool enable)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            var data = new Dictionary<string, dynamic>() { { "emotes_mode", enable } };
            var response = await ApiJsonPut<ChatUpdatedEvent>($"/api/v2/channels/{channel.Slug}/chatroom", data);
            if (response == null)
            {
                throw new Exception("Cannot fetch data from API.");
            }
            return response;
        }

        public async Task<ChatUpdatedEvent> SetChannelChatroomSubscribersOnly(Channel channel, bool enable)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            var data = new Dictionary<string, dynamic>() { { "subscribers_mode", enable } };
            var response = await ApiJsonPut<ChatUpdatedEvent>($"/api/v2/channels/{channel.Slug}/chatroom", data);
            if (response == null)
            {
                throw new Exception("Cannot fetch data from API.");
            }
            return response;
        }

        public async Task<ChatUpdatedEvent> SetChannelChatroomSlowMode(Channel channel, int? messageInterval)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            var data = new Dictionary<string, dynamic>() { { "slow_mode", messageInterval.HasValue }, { "message_interval", messageInterval.GetValueOrDefault(1) } };
            var response = await ApiJsonPut<ChatUpdatedEvent>($"/api/v2/channels/{channel.Slug}/chatroom", data);
            if (response == null)
            {
                throw new Exception("Cannot fetch data from API.");
            }
            return response;
        }

        public async Task<ChatUpdatedEvent> SetChannelChatroomFollowersMode(Channel channel, int? minDuration)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            var data = new Dictionary<string, dynamic>() { { "followers_mode", minDuration.HasValue }, { "following_min_duration", minDuration.GetValueOrDefault(0) } };
            var response = await ApiJsonPut<ChatUpdatedEvent>($"/api/v2/channels/{channel.Slug}/chatroom", data);
            if (response == null)
            {
                throw new Exception("Cannot fetch data from API.");
            }
            return response;
        }

        public async Task<ChatUpdatedEvent> SetChannelChatroomBotProtection(Channel channel, bool enable)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            var data = new Dictionary<string, dynamic>() { { "advanced_bot_protection", enable } };
            var response = await ApiJsonPut<ChatUpdatedEvent>($"/api/v2/channels/{channel.Slug}/chatroom", data);
            if (response == null)
            {
                throw new Exception("Cannot fetch data from API.");
            }
            return response;
        }
        
        public async Task<ChatUpdatedEvent> SetChannelChatroomAccountAge(Channel channel, int? minDuration)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            var data = new Dictionary<string, dynamic>() { { "account_age_mode", minDuration.HasValue }, { "account_age_min_duration", minDuration.GetValueOrDefault(0) } };
            var response = await ApiJsonPut<ChatUpdatedEvent>($"/api/v2/channels/{channel.Slug}/chatroom", data);
            if (response == null)
            {
                throw new Exception("Cannot fetch data from API.");
            }
            return response;
        }
        #endregion
        
        #region Channel Roles
        public async Task<VipUserEvent> AddChannelVip(Channel channel, string username)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            var data = new Dictionary<string, dynamic>() { { "username", username } };
            var response = await ApiJsonPost<KickApiOperationResponse<VipUserEvent>>($"/api/internal/v1/channels/{channel.Slug}/community/vips", data);
            if (response.Status.Error)
            {
                throw new Exception(response.Status.Message);
            }
            return response.Data;
        }

        public async Task RemoveChannelVip(Channel channel, string username)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            var response = await ApiDelete<KickApiOperationResponse<VipUserEvent>>($"/api/internal/v1/channels/{channel.Slug}/community/vips/{username}");
            if (response.Status.Error)
            {
                throw new Exception(response.Status.Message);
            }
        }

        public async Task<OgUserEvent> AddChannelOG(Channel channel, string username)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            var data = new Dictionary<string, dynamic>() { { "username", username } };
            var response = await ApiJsonPost<KickApiOperationResponse<OgUserEvent>>($"/api/internal/v1/channels/{channel.Slug}/community/ogs", data);
            if (response.Status.Error)
            {
                throw new Exception(response.Status.Message);
            }
            return response.Data;
        }

        public async Task RemoveChannelOG(Channel channel, string username)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            var response = await ApiDelete<KickApiOperationResponse<VipUserEvent>>($"/api/internal/v1/channels/{channel.Slug}/community/ogs/{username}");
            if (response.Status.Error)
            {
                throw new Exception(response.Status.Message);
            }
        }

        public async Task<ModeratorUserEvent> AddChannelModerator(Channel channel, string username)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            Dictionary<string, dynamic> data = new Dictionary<string, dynamic>() { { "username", username } };
            var response = await ApiJsonPost<KickApiOperationResponse<ModeratorUserEvent>>($"/api/internal/v1/channels/{channel.Slug}/community/moderators", data);
            if (response.Status.Error)
            {
                throw new Exception(response.Status.Message);
            }
            return response.Data;
        }

        public async Task RemoveChannelModerator(Channel channel, string username)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            var response = await ApiDelete<KickApiOperationResponse<VipUserEvent>>($"/api/internal/v1/channels/{channel.Slug}/community/moderators/{username}");
            if (response.Status.Error)
            {
                throw new Exception(response.Status.Message);
            }
        }
        #endregion

        #region Ban/Timeout
        public async Task<UserBanResult> BanUser(Channel channel, string username, string reason = "")
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            var data = new Dictionary<string, dynamic>() { { "banned_username", username }, { "permanent", true }, { "reason", reason } };
            var response = await ApiJsonPost<KickApiOperationResponse<UserBanResult>>($"/api/v2/channels/{channel.Slug}/bans", data);
            if (response.Status.Error)
            {
                throw new Exception(response.Status.Message);
            }
            return response.Data;
        }

        public async Task<UserBanResult> TimeoutUser(Channel channel, string username, long duration, string reason = "")
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            Dictionary<string, dynamic> data = new Dictionary<string, dynamic>() { { "banned_username", username }, { "permanent", false }, { "reason", reason }, { "duration", duration } };
            var response = await ApiJsonPost<KickApiOperationResponse<UserBanResult>>($"/api/v2/channels/{channel.Slug}/bans", data);
            if (response.Status.Error)
            {
                throw new Exception(response.Status.Message);
            }
            return response.Data;
        }

        public async Task UnbanUser(Channel channel, string username)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            var response = await ApiDelete<KickApiOperationSimpleResponse>($"/api/v2/channels/{channel.Slug}/bans/{username}");
            if (!response.Status)
            {
                throw new Exception(response.Message);
            }
        }
        #endregion
        
        #region Live Settings
        public async Task<StreamInfo> GetStreamInfo(Channel channel)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();
            
            return await ApiGet<StreamInfo>($"/api/v2/channels/{channel.Slug}/stream-info");
        }

        public async Task<StreamInfo> SetStreamInfo(Channel channel, StreamInfo streamInfo)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();
            
            var data = new Dictionary<string, dynamic>()
            {
                { "category_id", streamInfo.Category.Id },
                { "is_mature", streamInfo.IsMature },
                { "language", streamInfo.Language },
                { "stream_title", streamInfo.StreamTitle }
            };
            return await ApiJsonPatch<StreamInfo>($"/api/v2/channels/{channel.Slug}/stream-info", data);
        }

        public async Task<StreamCategory> FindClosestStreamCategory(string categoryName)
        {
            var query = new Dictionary<string, dynamic>() { { "q", categoryName }, { "query_by", "name" } };
            var result = await ApiSearch<StreamCategory>(SearchCollections.Categories, query);
            
            if (result.Found <= 0) throw new Exception("No category found with that name.");
            
            var bestHit = result.Hits[0];
            return bestHit.Document;
        }
        #endregion

        #region Poll
        public async Task<Poll> StartPoll(Channel channel, string question, string[] answers, int duration, int resultDuration)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            Dictionary<string, dynamic> data = new Dictionary<string, dynamic>() {
                { "duration", duration },
                { "title", question },
                { "options", answers },
                { "result_display_duration", resultDuration }
            };
            var response = await ApiJsonPost<KickApiOperationResponse<PollUpdateEvent>>($"/api/v2/channels/{channel.Slug}/polls", data);
            if (response.Status.Error)
            {
                throw new Exception(response.Status.Message);
            }
            return response.Data.Poll;
        }
        #endregion

        #region Clips
        public async Task<Clip> MakeClip(Channel channel, int duration, string title = null, int? startTime = null)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();
            
            channel = GetChannelInfos(channel.Slug).Result;

            if (channel.LiveStream == null)
                throw new Exception("This channel must be live to create a clip!");

            if (duration <= 0 || duration > 60)
                throw new Exception("Clip duration must be an integer between 1 and 60.");

            if (title == null)
                title = DateTime.Now.ToString(CultureInfo.CurrentCulture);

            if (startTime.HasValue)
            {
                if (startTime.Value <= 0 || startTime.Value > 90)
                    throw new Exception("Clip start time must be an integer between 1 and 90.");
            } else
            {
                startTime = 90 - duration; // Get latest X seconds
            }

            // Adjust duration time if necessary
            if(startTime.Value + duration > 90)
            {
                duration = 90 - startTime.Value;
            }

            var createClipResponse = await ApiPost<CreateClipResponse>($"/api/internal/v1/livestreams/{channel.LiveStream.Slug}/clips");
            if (createClipResponse == null)
            {
                throw new Exception("An unknown error occured while trying to create a clip.");
            }

            Dictionary<string, dynamic> data = new Dictionary<string, dynamic>() {
                { "duration", duration },
                { "title", title },
                { "start_time", startTime }
            };
            var response = await ApiJsonPost<Clip>($"/api/internal/v1/livestreams/{channel.LiveStream.Slug}/clips/{createClipResponse.Id}/finalize", data);
            if (response == null)
            {
                throw new Exception("Clip finalization failed!");
            }
            return response;
        }

        public async Task<List<Clip>> GetLatestClips(Channel channel, int count = 20, string orderBy = "date", string timeRange = "all")
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            var clips = new List<Clip>();
            string cursor = "0";
            while (clips.Count < count)
            {
                var response = await ApiGet<ClipsResponse>($"/api/v2/channels/{channel.Slug}/clips?cursor={cursor}&sort={orderBy}&time={timeRange}");
                clips.AddRange(response.Clips);
                if (response.NextCursor == null)
                    break;
                cursor = response.NextCursor;
            }
            if(clips.Count > count)
                clips.RemoveRange(count, clips.Count - count);
            return clips;
        }

        public async Task<string> GetClipMp4Url(string clipId)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            var response = await ApiGet<Dictionary<string, string>>($"/api/v2/clips/{clipId}/download");
            if(response != null && response.TryGetValue("url", out var url))
            {
                return url;
            }
            return null;
        }
        #endregion

        #region Pinned Messages
        public async Task PinMessage(Channel channel, ChatMessageEvent message, int duration = 120)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            Dictionary<string, dynamic> data = new Dictionary<string, dynamic>()
            {
                { "duration", duration },
                { "message", message }
            };
            var response = await ApiJsonPost<KickApiOperationResponse<object>>($"/api/v2/channels/{channel.Slug}/pinned-message", data);
            if (response.Status.Error)
            {
                throw new Exception(response.Status.Message);
            }
        }

        public async Task UnpinMessage(Channel channel)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            var response = await ApiDelete<KickApiOperationResponse<object>>($"/api/v2/channels/{channel.Slug}/pinned-message");
            if (response.Status.Error)
            {
                throw new Exception(response.Status.Message);
            }
        }
        
        public async Task<PinnedMessageEvent> GetPinnedMessage(Channel channel)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            var response = await ApiGet<KickApiOperationResponse<Messages>>($"/api/v2/channels/{channel.Id}/messages");
            if (response.Status.Error)
            {
                throw new Exception(response.Status.Message);
            }
            return response.Data.PinnedMessage;
        }
        #endregion
        
        #region Rewards
        public async Task<Reward[]> GetRewardsList(Channel channel, bool enabledOnly = false)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();
            
            var param = enabledOnly ? "?is_enabled=true" : string.Empty;
            var response = await ApiGet<KickApiMessageOperationResponse<Reward[]>>($"/api/v2/channels/{channel.Slug}/rewards{param}");
            if (response.Message != "OK")
            {
                throw new Exception(response.Message);
            }
            return response.Data;
        }
        
        public async Task<Reward> GetReward(Channel channel, string rewardId)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();
            
            var response = await ApiGet<KickApiMessageOperationResponse<Reward>>($"/api/v2/channels/{channel.Slug}/rewards/{rewardId}");
            if (response.Message != "OK")
            {
                throw new Exception(response.Message);
            }
            return response.Data;
        }
        
        public async Task<Reward> CreateReward(Channel channel, Reward reward)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();
            
            var data = new Dictionary<string, dynamic>()
            {
                { "background_color", reward.BackgroundColor },
                { "cost", reward.Cost },
                { "description", reward.Description },
                { "is_enabled", reward.IsEnabled },
                { "is_paused", reward.IsPaused },
                { "is_user_input_required", reward.IsUserInputRequired },
                { "should_redemptions_skip_request_queue", reward.ShouldRedemptionsSkipRequestQueue },
                { "title", reward.Title },
            };
            if(reward.IsUserInputRequired)
                data.Add("prompt", reward.Prompt);
            var response = await ApiJsonPost<KickApiMessageOperationResponse<Reward>>($"/api/v2/channels/{channel.Slug}/rewards", data);
            if (response.Message != "Created")
            {
                throw new Exception(response.Message);
            }
            return response.Data;
        }
        
        public async Task<Reward> UpdateReward(Channel channel, Reward reward)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();
            
            var data = new Dictionary<string, dynamic>()
            {
                { "background_color", reward.BackgroundColor },
                { "cost", reward.Cost },
                { "description", reward.Description },
                { "is_enabled", reward.IsEnabled },
                { "is_paused", reward.IsPaused },
                { "is_user_input_required", reward.IsUserInputRequired },
                { "should_redemptions_skip_request_queue", reward.ShouldRedemptionsSkipRequestQueue },
                { "title", reward.Title },
            };
            if(reward.IsUserInputRequired)
                data.Add("prompt", reward.Prompt);
            var response = await ApiJsonPatch<KickApiMessageOperationResponse<Reward>>($"/api/v2/channels/{channel.Slug}/rewards/{reward.Id}", data);
            if (response.Message != "OK")
            {
                throw new Exception(response.Message);
            }
            return response.Data;
        }
        
        public async Task DeleteReward(Channel channel, string rewardId)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();
            
            var response = await ApiDelete<KickApiMessageOperationResponse<dynamic>>($"/api/v2/channels/{channel.Slug}/rewards/{rewardId}");
            if (response.Message != "OK")
            {
                throw new Exception(response.Message);
            }
        }
        
        public async Task<Redemption[]> GetRedemptionsList(Channel channel, string rewardId = null)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();
            
            var param = rewardId != null ? "?reward_id=" + rewardId : string.Empty;
            var response = await ApiGet<KickApiMessageOperationResponse<RedemptionList>>($"/api/v2/channels/{channel.Slug}/redemptions{param}");
            if (response.Message != "OK")
            {
                throw new Exception(response.Message);
            }
            return response.Data.Redemptions;
        }
        
        public async Task<string[]> AcceptRedemptions(Channel channel, string[] redemptionIds)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();
            
            var data = new Dictionary<string, dynamic>()
            {
                { "redemption_ids", redemptionIds }
            };
            var response = await ApiJsonPost<KickApiMessageOperationResponse<FailedRedemptionsList>>($"/api/v2/channels/{channel.Slug}/redemptions/accept", data);
            if (response.Message != "OK")
            {
                throw new Exception(response.Message);
            }
            return response.Data.FailedRedemptionIds;
        }
        
        public async Task<string[]> RejectRedemptions(Channel channel, string[] redemptionIds)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();
            
            var data = new Dictionary<string, dynamic>()
            {
                { "redemption_ids", redemptionIds }
            };
            var response = await ApiJsonPost<KickApiMessageOperationResponse<FailedRedemptionsList>>($"/api/v2/channels/{channel.Slug}/redemptions/reject", data);
            if (response.Message != "OK")
            {
                throw new Exception(response.Message);
            }
            return response.Data.FailedRedemptionIds;
        }
        #endregion
        
        #region Predictions
        public async Task<Prediction> CreatePrediction(Channel channel, Prediction prediction)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();
            
            var data = new Dictionary<string, dynamic>()
            {
                { "title", prediction.Title },
                { "duration", prediction.Duration },
                { "outcomes", prediction.Outcomes.Select(p => p.Title).ToArray() }
            };
            var response = await ApiJsonPost<KickApiMessageOperationResponse<Prediction>>($"/api/v2/channels/{channel.Slug}/predictions", data);
            if (response.Message != "Created")
            {
                throw new Exception(response.Message);
            }
            return response.Data;
        }
        
        public async Task<Prediction> GetLatestPrediction(Channel channel)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();
            
            var response = await ApiGet<KickApiMessageOperationResponse<PredictionResponse>>($"/api/v2/channels/{channel.Slug}/predictions/latest");
            if (response.Message != "Success")
            {
                throw new Exception(response.Message);
            }
            return response.Data.Prediction;
        }
        
        public async Task<Prediction[]> GetRecentPredictions(Channel channel)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();
            
            var response = await ApiGet<KickApiMessageOperationResponse<PredictionsList>>($"/api/v2/channels/{channel.Slug}/predictions/recent");
            if (response.Message != "Success")
            {
                throw new Exception(response.Message);
            }
            return response.Data.Predictions;
        }
        
        public async Task<Prediction> LockPrediction(Channel channel, string predictionId)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();
            
            var data = new Dictionary<string, dynamic>()
            {
                { "state", Prediction.StateLocked }
            };
            var response = await ApiJsonPatch<KickApiMessageOperationResponse<Prediction>>($"/api/v2/channels/{channel.Slug}/predictions/{predictionId}", data);
            if (response.Message != "Success")
            {
                throw new Exception(response.Message);
            }
            return response.Data;
        }
        
        public async Task<Prediction> CancelPrediction(Channel channel, string predictionId)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();
            
            var data = new Dictionary<string, dynamic>()
            {
                { "state", Prediction.StateCancelled }
            };
            var response = await ApiJsonPatch<KickApiMessageOperationResponse<Prediction>>($"/api/v2/channels/{channel.Slug}/predictions/{predictionId}", data);
            if (response.Message != "Success")
            {
                throw new Exception(response.Message);
            }
            return response.Data;
        }
        
        public async Task<Prediction> ResolvePrediction(Channel channel, string predictionId, string winningOutcomeId)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();
            
            var data = new Dictionary<string, dynamic>()
            {
                { "state", Prediction.StateResolved },
                { "winning_outcome_id", winningOutcomeId }
            };
            var response = await ApiJsonPatch<KickApiMessageOperationResponse<Prediction>>($"/api/v2/channels/{channel.Slug}/predictions/{predictionId}", data);
            if (response.Message != "Success")
            {
                throw new Exception(response.Message);
            }
            return response.Data;
        }
        #endregion
        
        #region Partners
        public async Task EnableMultistream(Channel channel)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            if (channel.LiveStream == null)
            {
                channel = GetChannelInfos(channel.Slug).Result;
            }
            if (channel.LiveStream == null)
            {
                throw new Exception("This channel must be live to enable multistreaming!");
            }
            
            var response = await ApiPost<KickApiMessageOperationResponse<Dictionary<string, dynamic>>>($"/api/v2/livestreams/{channel.LiveStream.Slug}/multistream/enable");
            if (response.Message != "Success" && response.Message != "OK")
            {
                throw new Exception(response.Message);
            }
        }
        
        public async Task DisableMultistream(Channel channel)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();
            
            if (channel.LiveStream == null)
            {
                channel = GetChannelInfos(channel.Slug).Result;
            }
            if (channel.LiveStream == null)
            {
                throw new Exception("This channel must be live to disable multistreaming!");
            }
            
            var response = await ApiPost<KickApiMessageOperationResponse<Dictionary<string, dynamic>>>($"/api/v2/livestreams/{channel.LiveStream.Slug}/multistream/disable");
            if (response.Message != "Success" && response.Message != "OK")
            {
                throw new Exception(response.Message);
            }
        }
        #endregion
    }
}