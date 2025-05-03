/*
    Copyright (C) 2023 Sehelitar

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
using Models.API;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        public async Task ClearChat(Channel channel)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            await SendChatCommand(channel, new ChatCommand("clear", null));
        }

        public async Task SetChannelTitle(Channel channel, string title)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            await SendChatCommand(channel, new ChatCommand("title", title));
        }

        private List<ParentCategory> _cacheCategories;

        public async Task SetChannelCategory(Channel channel, string category)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            if (_cacheCategories == null)
            {
                _cacheCategories = await ApiGet<List<ParentCategory>>($"/api/v1/listsubcategories") ?? throw new Exception("Unable to retrieve categories list.");
            }

            var categoryLower = category.ToLower();
            long categoryId = 0;
            var bestScore = int.MaxValue;
            _cacheCategories.ForEach(mainCat => {
                var score = ComputeLevenshtein(categoryLower, mainCat.Name.ToLower());
                if (score >= bestScore) return;
                bestScore = score;
                categoryId = mainCat.Id;
            });
            
            var updateRequest = await ApiJsonPost<Dictionary<string, object>>($"/stream/{channel.LiveStream.Slug}/update", new Dictionary<string, long>() { { "subcategoryId", categoryId } });
            if (updateRequest == null)
            {
                throw new Exception("Unable to set category.");
            }
        }

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

        public async Task<Clip> MakeClip(Channel channel, int duration, string title = null, int? startTime = null)
        {
            if (!IsAuthenticated)
                throw new UnauthenticatedException();

            if (channel.LiveStream == null)
                throw new Exception("This channel must be live to create a clip!");

            if (duration <= 0 || duration > 90)
                throw new Exception("Clip duration must be an integer between 1 and 90.");

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

        private static int ComputeLevenshtein(string source, string target)
        {
            int sourceLength = source.Length;
            int targetLength = target.Length;

            int[,] matrix = new int[sourceLength + 1, targetLength + 1];

            for (int i = 0; i <= sourceLength; i++)
                matrix[i, 0] = i;

            for (int j = 0; j <= targetLength; j++)
                matrix[0, j] = j;

            for (int i = 1; i <= sourceLength; i++)
            {
                for (int j = 1; j <= targetLength; j++)
                {
                    int cost = (source[i - 1] == target[j - 1]) ? 0 : 1;

                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost
                    );
                }
            }

            return matrix[sourceLength, targetLength];
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
    }
}