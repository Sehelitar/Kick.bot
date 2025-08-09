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

using Kick.API;
using Kick.API.Events;
using Kick.API.Models;
using Streamer.bot.Plugin.Interface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using LiteDB;
using System.Text.RegularExpressions;
using System.Text;
using System.Windows.Forms;
using Kick.Properties;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;

namespace Kick.Bot
{
    public sealed class BotClient
    {
        // ReSharper disable once InconsistentNaming
        public static IInlineInvokeProxy CPH;
        
        internal static PluginUi GlobalPluginUi;

        private static KickClient Client => GlobalPluginUi.BroadcasterKClient;
        private static KickClient AltClient => GlobalPluginUi.BotKClient;

        public User AuthenticatedUser { get; private set; }
        public User AuthenticatedBot { get; private set; }
        
        public BotEventListener BroadcasterListener { get; private set; }

        public BotClient() {
            CPH.LogDebug("[Kick] Extension loaded. Starting...");
            
            CPH.RegisterCustomTrigger("[Kick] Chat Message", BotEventListener.BotEventType.Message, new[] { "Kick", "Chat" });
            CPH.RegisterCustomTrigger("[Kick] Chat config updated", BotEventListener.BotEventType.ChatUpdated, new[] { "Kick", "Chat" });

            CPH.RegisterCustomTrigger("[Kick] Chat Command (Any)", BotEventListener.BotEventType.ChatCommand, new[] { "Kick", "Commands" });
            CPH.RegisterCustomTrigger("[Kick] Chat Command Cooldown (Any)", BotEventListener.BotEventType.ChatCommandCooldown, new[] { "Kick", "Commands Cooldown" });

            CPH.RegisterCustomTrigger("[Kick] Message Pinned", BotEventListener.BotEventType.MessagePinned, new[] { "Kick", "Chat" });
            CPH.RegisterCustomTrigger("[Kick] Message Unpinned", BotEventListener.BotEventType.MessageUnpinned, new[] { "Kick", "Chat" });

            CPH.RegisterCustomTrigger("[Kick] Follow", BotEventListener.BotEventType.Follow, new[] { "Kick", "Channel" });

            CPH.RegisterCustomTrigger("[Kick] Subscription", BotEventListener.BotEventType.Subscription, new[] { "Kick", "Subscriptions" });
            CPH.RegisterCustomTrigger("[Kick] Sub Gift (x1)", BotEventListener.BotEventType.SubGift, new[] { "Kick", "Subscriptions" });
            CPH.RegisterCustomTrigger("[Kick] Sub Gifts (multiple)", BotEventListener.BotEventType.SubGifts, new[] { "Kick", "Subscriptions" });

            CPH.RegisterCustomTrigger("[Kick] Chat Message Deleted", BotEventListener.BotEventType.MessageDeleted, new[] { "Kick", "Moderation" });
            CPH.RegisterCustomTrigger("[Kick] Timeout", BotEventListener.BotEventType.Timeout, new[] { "Kick", "Moderation" });
            CPH.RegisterCustomTrigger("[Kick] User Ban", BotEventListener.BotEventType.UserBanned, new[] { "Kick", "Moderation" });
            CPH.RegisterCustomTrigger("[Kick] User Unban", BotEventListener.BotEventType.UserUnbanned, new[] { "Kick", "Moderation" });

            CPH.RegisterCustomTrigger("[Kick] Poll Created", BotEventListener.BotEventType.PollCreated, new[] { "Kick", "Polls" });
            CPH.RegisterCustomTrigger("[Kick] Poll Updated", BotEventListener.BotEventType.PollUpdated, new[] { "Kick", "Polls" });
            CPH.RegisterCustomTrigger("[Kick] Poll Completed", BotEventListener.BotEventType.PollCompleted, new[] { "Kick", "Polls" });
            CPH.RegisterCustomTrigger("[Kick] Poll Cancelled", BotEventListener.BotEventType.PollCancelled, new[] { "Kick", "Polls" });

            CPH.RegisterCustomTrigger("[Kick] Stream Started", BotEventListener.BotEventType.StreamStarted, new[] { "Kick", "Stream" });
            CPH.RegisterCustomTrigger("[Kick] Stream Ended", BotEventListener.BotEventType.StreamEnded, new[] { "Kick", "Stream" });
            CPH.RegisterCustomTrigger("[Kick] Title/Category Changed", BotEventListener.BotEventType.TitleChanged, new[] { "Kick", "Stream" });

            CPH.RegisterCustomTrigger("[Kick] Raid", BotEventListener.BotEventType.Raid, new[] { "Kick" });
            
            CPH.RegisterCustomTrigger("[Kick] Reward Redeemed (Any)", BotEventListener.BotEventType.RewardRedeemed, new[] { "Kick", "Rewards" });
            
            CPH.RegisterCustomTrigger("[Kick] Prediction Created", BotEventListener.BotEventType.PredictionCreated, new[] { "Kick", "Predictions" });
            CPH.RegisterCustomTrigger("[Kick] Prediction Updated", BotEventListener.BotEventType.PredictionUpdated, new[] { "Kick", "Predictions" });
            CPH.RegisterCustomTrigger("[Kick] Prediction Locked", BotEventListener.BotEventType.PredictionLocked, new[] { "Kick", "Predictions" });
            CPH.RegisterCustomTrigger("[Kick] Prediction Resolved", BotEventListener.BotEventType.PredictionResolved, new[] { "Kick", "Predictions" });
            CPH.RegisterCustomTrigger("[Kick] Prediction Cancelled", BotEventListener.BotEventType.PredictionCancelled, new[] { "Kick", "Predictions" });
            
            CPH.LogDebug("[Kick] Basic triggers registered.");
            
            CPH.LogDebug("[Kick] Starting UI thread...");
            GlobalPluginUi = new PluginUi();
            
            // Broadcaster
            CPH.LogDebug("[Kick] Client initialization...");
            Client.Browser.OnAuthenticated += Authenticated;
            
            // Bot
            CPH.LogDebug("[Kick] Bot initialization...");
            AltClient.Browser.OnAuthenticated += BotAuthenticated;
            
            CommandCounter.PruneVolatile();
            CPH.LogDebug("[Kick] Init completed.");
        }

        ~BotClient()
        {
            CPH.LogDebug("[Kick] Extension is shuting down");
        }
        
        public static bool CheckCompatibility()
        {
            CPH.LogError("[Kick.bot] Running environment testing");
            try
            {
                CheckSBAssemblies();
            }
            catch(Exception e)
            {
                CPH.LogError("[Kick.bot] SB assemblies version mismatch");
                CPH.LogError(e.Message);
                var result = MessageBox.Show($"This version of Kick.bot ({Assembly.GetExecutingAssembly().GetName().Version}) is not compatible with the running version of Streamer.bot ({CPH.GetVersion()}). Kick.bot must be updated and won't run in its current state.\r\n\r\nDo you want to check for updates on the project page?", "Kick.bot - Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                if(result == DialogResult.Yes)
                    System.Diagnostics.Process.Start("https://github.com/Sehelitar/Kick.bot/releases/latest");
                return false;
            }
            
            try
            {
                CheckExternalAssemblies();
            }
            catch(Exception e)
            {
                CPH.LogError("[Kick.bot] External dependencies assemblies version mismatch");
                CPH.LogError(e.Message);
                var file = "Unknown";
                if (e.GetType() == typeof(FileNotFoundException))
                {
                    file = (e as FileNotFoundException)?.FileName;
                    if (file != null && file.Contains(","))
                        file = file.Split(',').FirstOrDefault();
                    if (file == null)
                        file = "Unknown";
                    else
                        file += ".dll";
                }
                MessageBox.Show($"Kick.bot cannot run because at least one dependency is missing. Please check that all .dll files from the archive are present in the dlls folder of your Streamer.bot installation directory, and try again.\r\nIf the problem persists, please open an issue on Github.\r\n\r\nMissing dependency : {file}", "Kick.bot - Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return false;
            }

            var updateAvailable = CheckUpdates();
            if (updateAvailable != null)
            {
                var currentRev = Assembly.GetExecutingAssembly().GetName().Version;
                MessageBox.Show($"A new update for Kick.bot ({updateAvailable.Revision}) is available!\r\nDownload and install it from the project page: https://github.com/Sehelitar/Kick.bot", "Kick.bot - Update available", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                System.Diagnostics.Process.Start(updateAvailable.File);
            }
            
            return true;
        }

        private static void CheckSBAssemblies()
        {
            _ = typeof(WebView2).Assembly.FullName;
            _ = typeof(LiteDatabase).Assembly.FullName;
        }
        
        private static void CheckExternalAssemblies()
        {
            _ = typeof(Imazen.WebP.SimpleDecoder).Assembly.FullName;
            _ = typeof(PusherClient.Pusher).Assembly.FullName;
            _ = typeof(System.Resources.Extensions.DeserializingResourceReader).Assembly.FullName;
            _ = typeof(WebSocket4Net.WebSocket).Assembly.FullName;
            _ = typeof(SuperSocket.ClientEngine.ClientSession).Assembly.FullName;
            _ = typeof(NaCl.Curve25519).Assembly.FullName;
        }

        private static UpdateRelease CheckUpdates()
        {
            var currentRev = Assembly.GetExecutingAssembly().GetName().Version;
            var currentVersion = Version.Parse(CPH.GetVersion());
            
            try
            {
                using (var client = new WebClient())
                {
                    CPH.LogDebug($"[Kick.bot] Checking for updates... (SB:{currentVersion}, KB:{currentRev})");
                    var releasesJson = client.DownloadString("https://raw.githubusercontent.com/Sehelitar/Kick.bot/main/version");
                    var releases = JsonConvert.DeserializeObject<UpdateMeta>(releasesJson);

                    var matchingRelease = releases.Releases.FirstOrDefault(x => currentVersion >= x.Version);
                    if (matchingRelease != null && matchingRelease.Revision > currentRev)
                    {
                        CPH.LogDebug($"[Kick.bot] Update available! (SB:{matchingRelease.Version}, KB:{matchingRelease.Revision})");
                        return matchingRelease;
                    }

                    CPH.LogDebug($"[Kick.bot] No update available.");
                }
            } catch (Exception e)
            {
                CPH.LogError("[Kick.bot] Unable to check for updates");
                CPH.LogError(e.Message);
            }
            
            return null;
        }

        public static bool OpenConfig()
        {
            GlobalPluginUi.OpenConfig();
            return true;
        }

        // @deprecated
        public void BeginBroadcasterAuthentication()
        {
            CPH.LogDebug("[Kick] Begin broadcaster authentication...");
            Client.Browser.BeginAuthentication();
        }
        
        // @deprecated
        public void BeginBotAuthentication()
        {
            CPH.LogDebug("[Kick] Begin bot authentication...");
            AltClient.Browser.BeginAuthentication();
        }

        private void Authenticated(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                AuthenticatedUser = await Client.GetCurrentUserInfos();
                CPH.LogDebug($"[Kick] Connected as {AuthenticatedUser.Username}");
                CPH.SetGlobalVar("KickNotFirstLaunch", true);

                var logoPath = Path.Combine(Path.GetTempPath(), "kick_pp.png");
                try
                {
                    Bitmap logo;
                    var decoder = new Imazen.WebP.SimpleDecoder();
                    using (var stream = WebRequest
                               .CreateHttp(AuthenticatedUser.ProfilePicAlt)
                               .GetResponse().GetResponseStream())
                    using (var memoryStream = new MemoryStream())
                    {
                        if (stream != null)
                        {
                            await stream.CopyToAsync(memoryStream);
                            var pictureBuffer = memoryStream.ToArray();
                            logo = decoder.DecodeFromBytes(pictureBuffer, pictureBuffer.Length);
                        }
                        else
                        {
                            logo = Resources.KickLogo;
                        }
                    }
                    logo.Save(logoPath, ImageFormat.Png);
                }
                catch (Exception ex)
                {
                    CPH.LogDebug($"[Kick] An error occured when trying to fetch user's profile image :");
                    CPH.LogError($"{ex}");
                }
                
                CPH.ShowToastNotification(
                    "kick.bot.config",
                    "Kick.bot", 
                    $"Connected as {AuthenticatedUser.Username}",
                    "Click to open Kick.bot config window",
                    logoPath
                );

                try
                {
                    BroadcasterListener = await StartListeningToSelf();
                    GlobalPluginUi.RefreshEventListenerStatus();
                    CPH.LogDebug($"[Kick] Listener active.");
                }
                catch (Exception ex)
                {
                    CPH.LogError($"[Kick] An error occurred while starting listener : {ex}");
                    return;
                }

                ReloadRewards();
            });
        }
        
        private void BotAuthenticated(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                AuthenticatedBot = await AltClient.GetCurrentUserInfos();
                CPH.LogDebug($"[Kick] Bot connected as {AuthenticatedBot.Username}");
                
                var logoPath = Path.Combine(Path.GetTempPath(), "kick_b_pp.png");
                try
                {
                    Bitmap logo;
                    var decoder = new Imazen.WebP.SimpleDecoder();
                    using (var stream = WebRequest
                               .CreateHttp(AuthenticatedBot.ProfilePicAlt)
                               .GetResponse().GetResponseStream())
                    using (var memoryStream = new MemoryStream())
                    {
                        if (stream != null)
                        {
                            await stream.CopyToAsync(memoryStream);
                            var pictureBuffer = memoryStream.ToArray();
                            logo = decoder.DecodeFromBytes(pictureBuffer, pictureBuffer.Length);
                        }
                        else
                        {
                            logo = Resources.KickLogo;
                        }
                    }
                    logo.Save(logoPath, ImageFormat.Png);
                }
                catch (Exception ex)
                {
                    CPH.LogDebug($"[Kick] An error occured when trying to fetch user's profile image :");
                    CPH.LogError($"{ex}");
                }
                
                CPH.ShowToastNotification(
                    "kick.bot.config",
                    "Kick.bot",
                    $"Bot also connected as {AuthenticatedBot.Username}",
                    "Click to open Kick.bot config window",
                    logoPath
                );
            });
        }

        public async Task<BotEventListener> StartListeningTo(string channelName)
        {
            if (AuthenticatedUser == null)
                throw new Exception("authentication required");

            CPH.LogDebug($"[Kick] Listening events for channel {channelName}");
            var channel = await Client.GetChannelInfos(channelName);
            return new BotEventListener(Client.GetEventListener(), channel);
        }

        public async Task<BotEventListener> StartListeningToSelf()
        {
            if (AuthenticatedUser == null)
                throw new Exception("Authentication required");

            CPH.LogDebug($"[Kick] Listening events for channel {AuthenticatedUser.StreamerChannel.Slug}");
            var channel = await Client.GetChannelInfos(AuthenticatedUser.StreamerChannel.Slug);
            return new BotEventListener(Client.GetEventListener(), channel);
        }

        public bool ReloadRewards(Channel channel = null)
        {
            try
            {
                if (channel == null)
                    channel = BroadcasterListener.Channel;
                var rewards = new List<Reward>(Client.GetRewardsList(channel, true).Result);
                rewards.Sort((x, y) => string.Compare(x.Title, y.Title, StringComparison.InvariantCultureIgnoreCase));
                foreach (var reward in rewards)
                {
                    CPH.RegisterCustomTrigger($"[Kick/Reward] {reward.Title}", $"{BotEventListener.BotEventType.RewardRedeemed}.{reward.Id}", new[] { "Kick", "Rewards" });
                }

                return true;
            }
            catch(Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while reloading rewards : {ex}");
                return false;
            }
        }
        
        public bool SendMessage(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                if (args.TryGetValue("chatroomId", out var chatroomId))
                    chatroomId = Convert.ToInt64(chatroomId);
                else if (channel != null)
                    chatroomId = channel.Chatroom.Id;
                else
                    throw new Exception("missing argument, chatroomId required");
                
                if (!args.TryGetValue("message", out var message))
                    throw new Exception("missing argument, message required");
                
                if (args.TryGetValue("useBotProfile", out var useBotProfile))
                    useBotProfile = Convert.ToBoolean(useBotProfile);
                else
                    useBotProfile = true;

                Task<ChatMessageEvent> result;
                if(AltClient.Browser.IsAuthenticated && useBotProfile)
                    result = AltClient.SendMessageToChatroom(chatroomId, Convert.ToString(message));
                else
                    result = Client.SendMessageToChatroom(chatroomId, Convert.ToString(message));

                result.Wait();
                if (result.Status != TaskStatus.RanToCompletion) return false;
                
                CPH.SetArgument("msgId", result.Result.Id);
                CPH.SetArgument("pinnableMessage", JsonConvert.SerializeObject(result.Result));
                return true;
            }
            catch(Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while sending a message to chatroom : {ex}");
                return false;
            }
        }

        public bool DeleteMessage(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                if (args.TryGetValue("chatroomId", out var chatroomId))
                    chatroomId = Convert.ToInt64(chatroomId);
                else if (channel != null)
                    chatroomId = channel.Chatroom.Id;
                else
                    throw new Exception("missing argument, chatroomId required");

                if (!args.TryGetValue("msgId", out var msgId))
                    throw new Exception("missing argument, msgId required");

                var result = Client.DeleteMessage(chatroomId, msgId);
                result.Wait();
                return result.Status != TaskStatus.RanToCompletion ? false : result.Result;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while deleting a message from chatroom : {ex}");
                return false;
            }
        }

        public bool SendReply(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;
                
                if (args.TryGetValue("chatroomId", out var chatroomId))
                    chatroomId = Convert.ToInt64(chatroomId);
                else if (channel != null)
                    chatroomId = channel.Chatroom.Id;
                else
                    throw new Exception("missing argument, chatroomId required");
                
                if (!args.TryGetValue("message", out var message))
                    throw new Exception("missing argument, message required");
                
                if (!args.TryGetValue("msgId", out var msgId))
                    throw new Exception("missing argument, msgId required");
                
                if (!args.TryGetValue("reply", out var reply))
                    throw new Exception("missing argument, reply required");
                
                if (!args.TryGetValue("user", out var user))
                    throw new Exception("missing argument, user required");
                
                if (!args.TryGetValue("userId", out var userId))
                    throw new Exception("missing argument, userId required");
                
                if (args.TryGetValue("useBotProfile", out var useBotProfile))
                    useBotProfile = Convert.ToBoolean(useBotProfile);
                else
                    useBotProfile = true;
                
                Task<ChatMessageEvent> result;
                if(AltClient.Browser.IsAuthenticated && useBotProfile)
                    result = AltClient.SendReplyToChatroom(
                        chatroomId,
                        Convert.ToString(reply),
                        Convert.ToString(msgId),
                        Convert.ToString(message),
                        Convert.ToInt64(userId),
                        Convert.ToString(user)
                    );
                else
                    result = Client.SendReplyToChatroom(
                        chatroomId,
                        Convert.ToString(reply),
                        Convert.ToString(msgId),
                        Convert.ToString(message),
                        Convert.ToInt64(userId),
                        Convert.ToString(user)
                    );

                result.Wait();
                if (result.Status != TaskStatus.RanToCompletion) return false;
                
                CPH.SetArgument("msgId", result.Id);
                CPH.SetArgument("pinnableMessage", JsonConvert.SerializeObject(result.Result));
                return true;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while sending a reply : {ex}");
                return false;
            }
        }
        
        public bool ClearChat(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                var result = Client.ClearChat(channel);
                result.Wait();
                return result.Status == TaskStatus.RanToCompletion;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while clearing chat : {ex}");
                return false;
            }
        }

        public bool GetUserInfos(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if(channel == null)
                    channel = BroadcasterListener.Channel;
                
                if (args.TryGetValue("targetUserName", out var userName) && userName != null && userName != String.Empty)
                    return GetKickChannelInfos(args, channel, Convert.ToString(userName), true);
                if (args.TryGetValue("targetUser", out var user) && user != null && user != String.Empty)
                    return GetKickChannelInfos(args, channel, Convert.ToString(user));
                if (args.TryGetValue("userName", out userName) && userName != null && userName != String.Empty)
                    return GetKickChannelInfos(args, channel, Convert.ToString(userName) , true);
                if (args.TryGetValue("user", out user) && user != null && user != String.Empty)
                    return GetKickChannelInfos(args, channel, Convert.ToString(user));
                return GetKickChannelInfos(args, channel, channel.Slug, true);
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while fetching user infos : {ex}");
                return false;
            }
        }

        public bool GetBroadcasterInfos(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if(channel == null)
                    channel = BroadcasterListener.Channel;
                return GetKickChannelInfos(args, channel, channel.Slug, true);
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while fetching broadcaster infos : {ex}");
                return false;
            }
        }

        private bool GetKickChannelInfos(Dictionary<string, dynamic> _, Channel channel, string username, bool isSlug = false)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                if(username.StartsWith("@"))
                    username = username.Substring(1);

                Channel channelInfos;
                ChannelUser userInfos;

                if(isSlug)
                {
                    channelInfos = Client.GetChannelInfos(username).Result;
                    userInfos = Client.GetChannelUserInfos(channel.Slug, channelInfos.User.Username).Result;
                }
                else
                {
                    userInfos = Client.GetChannelUserInfos(channel.Slug, username).Result;
                    channelInfos = Client.GetChannelInfos(userInfos.Slug).Result;
                }

                if (channelInfos == null || userInfos == null)
                    return false;

                UserActivity extraUser = null;
                try
                {
                    var basePath = Path.GetDirectoryName(typeof(CPHInlineBase).Assembly.Location) ?? "./";
                    using (var database = new LiteDatabase(Path.Combine(basePath, @"data\kick-ext.db")))
                    {
                        var dbCollection = database.GetCollection<UserActivity>("users");
                        var activityQuery = from activityObject in dbCollection.Query()
                                            where activityObject.UserId == userInfos.Id
                                            select activityObject;
                        extraUser = activityQuery.FirstOrDefault();
                    }
                }
                catch (Exception e)
                {
                    CPH.LogError($"[Kick] A database error occured (l:GetKickChannelInfos) : {e}");
                }

                var profilePic = channelInfos.User.ProfilePic ?? "https://dbxmjjzl5pc1g.cloudfront.net/49e68df9-6ede-4a97-b593-340a400cb57b/images/user-profile-pic.png";

                CPH.SetArgument("targetUser", channelInfos.User.Username);
                CPH.SetArgument("targetUserName", channelInfos.Slug);
                CPH.SetArgument("targetUserId", channelInfos.User.Id);
                CPH.SetArgument("targetDescription", channelInfos.User.Bio);
                CPH.SetArgument("targetDescriptionEscaped", WebUtility.UrlEncode(channelInfos.User.Bio));
                CPH.SetArgument("targetUserProfileImageUrl", profilePic);
                CPH.SetArgument("targetUserProfileImageUrlEscaped", WebUtility.UrlEncode(profilePic));
                CPH.SetArgument("targetUserProfileImageEscaped", WebUtility.UrlEncode(profilePic));
                CPH.SetArgument("targetUserType", channelInfos.IsVerified ? "partner" : (channelInfos.IsAffiliate ? "affiliate" : String.Empty));
                CPH.SetArgument("targetIsAffiliate", channelInfos.IsAffiliate);
                CPH.SetArgument("targetIsPartner", channelInfos.IsVerified);
                CPH.SetArgument("targetFollowers", channelInfos.FollowersCount);
                CPH.SetArgument("targetLastActive", extraUser != null ? extraUser.LastActivity : DateTime.Now);
                CPH.SetArgument("targetPreviousActive", extraUser != null ? extraUser.LastActivity : DateTime.Now);
                CPH.SetArgument("targetIsSubscribed", userInfos.IsSubscriber);
                CPH.SetArgument("targetSubscriptionTier", (userInfos.IsSubscriber && userInfos.SubscribedFor > 0 ? 1000 : 0));
                CPH.SetArgument("targetIsModerator", userInfos.IsModerator);
                CPH.SetArgument("targetIsVip", userInfos.IsVip);
                CPH.SetArgument("targetIsFollowing", userInfos.IsFollowing);
                CPH.SetArgument("targetChannelTitle", channelInfos.LiveStream?.SessionTitle ?? String.Empty);
                CPH.SetArgument("game", channelInfos.LiveStream?.Categories.First()?.Name);
                CPH.SetArgument("gameId", channelInfos.LiveStream?.Categories.First()?.Id);
                CPH.SetArgument("createdAt", channelInfos.Chatroom.CreatedAt);
                CPH.SetArgument("accountAge", Convert.ToInt64((DateTime.Now - channelInfos.Chatroom.CreatedAt).TotalSeconds));
                CPH.SetArgument("tagCount", 0);
                CPH.SetArgument("tags", new List<string>());
                CPH.SetArgument("tagsDelimited", String.Empty);
                CPH.SetArgument("targetIsLive", false);

                if (channelInfos.LiveStream != null)
                {
                    CPH.SetArgument("targetIsLive", true);
                    CPH.SetArgument("targetLiveThumbnail", channelInfos.LiveStream.Thumbnail.Url);
                    CPH.SetArgument("targetLiveViewers", channelInfos.LiveStream.Viewers);
                    CPH.SetArgument("targetLiveStartedAt", channelInfos.LiveStream.StartDate);
                    CPH.SetArgument("targetLiveDuration", Convert.ToInt64((DateTime.Now - channelInfos.LiveStream.StartDate).TotalSeconds));
                    CPH.SetArgument("targetLiveMature", channelInfos.LiveStream.IsMature);

                    CPH.SetArgument("tagCount", channelInfos.LiveStream.Tags.Count);
                    CPH.SetArgument("tags", channelInfos.LiveStream.Tags);
                    var tagId = 0;
                    channelInfos.LiveStream.Tags.ForEach((tag) => {
                        CPH.SetArgument("tag" + (tagId++), tag);
                    });
                    CPH.SetArgument("tagsDelimited", string.Join(", ", channelInfos.LiveStream.Tags.ToArray()));
                }
                else if (channelInfos.PreviousLiveStreams.Count > 0)
                {
                    CPH.SetArgument("targetChannelTitle", channelInfos.PreviousLiveStreams[0].SessionTitle);
                    var categories = channelInfos.PreviousLiveStreams[0].Categories;

                    if (categories != null && categories.Count > 0)
                    {
                        var latestCategory = categories.First();
                        CPH.SetArgument("game", latestCategory.Name);
                        CPH.SetArgument("gameId", latestCategory.Id);
                    }

                    CPH.SetArgument("tagCount", channelInfos.PreviousLiveStreams[0].Tags.Count);
                    CPH.SetArgument("tags", channelInfos.PreviousLiveStreams[0].Tags);
                    int tagId = 0;
                    channelInfos.PreviousLiveStreams[0].Tags.ForEach((tag) => {
                        CPH.SetArgument("tag" + (tagId++), tag);
                    });
                    CPH.SetArgument("tagsDelimited", String.Join(", ", channelInfos.PreviousLiveStreams[0].Tags.ToArray()));
                }
                else if (channelInfos.RecentCategories != null && channelInfos.RecentCategories.Count > 0)
                {
                    var latestCategory = channelInfos.RecentCategories.First();
                    CPH.SetArgument("game", latestCategory.Name);
                    CPH.SetArgument("gameId", latestCategory.Id);
                }

                return true;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while fetching channel infos : {ex}");
                return false;
            }
        }

        #region Channel Roles
        public bool AddChannelVip(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                if (!args.TryGetValue("user", out var username))
                    throw new Exception("missing argument, user required");

                var result = Client.AddChannelVip(channel, username);
                result.Wait();
                return result.Status == TaskStatus.RanToCompletion;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while adding a new VIP : {ex}");
                return false;
            }
        }

        public bool RemoveChannelVip(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                if (!args.TryGetValue("user", out var username))
                    throw new Exception("missing argument, user required");

                var result = Client.RemoveChannelVip(channel, username);
                result.Wait();
                return result.Status == TaskStatus.RanToCompletion;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while removing a VIP : {ex}");
                return false;
            }
        }

        public bool AddChannelOG(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                if (!args.TryGetValue("user", out var username))
                    throw new Exception("missing argument, user required");

                var result = Client.AddChannelOG(channel, username);
                result.Wait();
                return result.Status == TaskStatus.RanToCompletion;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while adding a new OG : {ex}");
                return false;
            }
        }

        public bool RemoveChannelOG(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                if (!args.TryGetValue("user", out var username))
                    throw new Exception("missing argument, user required");

                var result = Client.RemoveChannelOG(channel, username);
                result.Wait();
                return result.Status == TaskStatus.RanToCompletion;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while removing an OG : {ex}");
                return false;
            }
        }

        public bool AddChannelModerator(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                if (!args.TryGetValue("user", out var username))
                    throw new Exception("missing argument, user required");

                var result = Client.AddChannelModerator(channel, username);
                result.Wait();
                return result.Status == TaskStatus.RanToCompletion;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while adding a moderator : {ex}");
                return false;
            }
        }

        public bool RemoveChannelModerator(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                if (!args.TryGetValue("user", out var username))
                    throw new Exception("missing argument, user required");

                var result = Client.RemoveChannelModerator(channel, username);
                result.Wait();
                return result.Status == TaskStatus.RanToCompletion;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while removing a moderator : {ex}");
                return false;
            }
        }
        #endregion

        #region Ban / Timeout
        public bool BanUser(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                if (!args.TryGetValue("user", out var username))
                    throw new Exception("missing argument, user required");

                if (!args.TryGetValue("banReason", out var banReason))
                    banReason = "";

                var result = Client.BanUser(channel, (string)username, (string)banReason);
                result.Wait();
                return result.Status == TaskStatus.RanToCompletion;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while banning a user : {ex}");
                return false;
            }
        }

        public bool TimeoutUser(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                if (!args.TryGetValue("user", out var username))
                    throw new Exception("missing argument, user required");

                if (!args.TryGetValue("banDuration", out var banDuration))
                    throw new Exception("missing argument, banDuration required");

                if (!args.TryGetValue("banReason", out var banReason))
                    banReason = "";

                var result = Client.TimeoutUser(channel, (string)username, Convert.ToInt64(banDuration),
                    (string)banReason);
                result.Wait();
                return result.Status == TaskStatus.RanToCompletion;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while temporarily banning a user : {ex}");
                return false;
            }
        }

        public bool UnbanUser(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                if (!args.TryGetValue("user", out var username))
                    throw new Exception("missing argument, user required");

                var result = Client.UnbanUser(channel, (string)username);
                result.Wait();
                return result.Status == TaskStatus.RanToCompletion;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while unbanning a user : {ex}");
                return false;
            }
        }
        #endregion
        
        #region Polls
        public bool StartPoll(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                if (!args.TryGetValue("pollTitle", out var title))
                    throw new Exception("missing argument, pollTitle required");

                if (!args.TryGetValue("pollDuration", out var duration))
                    throw new Exception("missing argument, pollDuration required");
                 
                if (!args.TryGetValue("pollDisplayTime", out var displayTime))
                    displayTime = 30;

                if (!args.TryGetValue("pollChoices", out var choices))
                    throw new Exception("missing argument, pollChoices required");

                var result = Client.StartPoll(channel, (string)title, ((string)choices).Split('|'), Convert.ToInt32(duration), Convert.ToInt32(displayTime));
                result.Wait();
                return result.Status == TaskStatus.RanToCompletion;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while starting a new poll : {ex}");
                return false;
            }
        }
        #endregion
        
        #region Live Settings
        public bool ChangeStreamInfo(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                var streamInfo = Client.GetStreamInfo(channel).Result;

                if (args.TryGetValue("title", out var title))
                    streamInfo.StreamTitle = (string)title;
                if (args.TryGetValue("category", out var category))
                {
                    var bestCategory = Client.FindClosestStreamCategory(category).Result as StreamCategory;
                    streamInfo.Category = bestCategory;
                }
                if (args.TryGetValue("isMature", out var isMature))
                    streamInfo.IsMature = Convert.ToBoolean(isMature);

                var result = Client.SetStreamInfo(channel, streamInfo);
                result.Wait();
                return result.Status == TaskStatus.RanToCompletion;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while changing stream title : {ex}");
                return false;
            }
        }
        #endregion
        
        #region Clips
        public bool MakeClip(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                args.TryGetValue("title", out var title);

                if (args.TryGetValue("duration", out var duration))
                {
                    duration = Convert.ToInt32(duration);
                } else
                {
                    duration = 30;
                }
                    

                if (args.TryGetValue("skip", out var skip))
                {
                    skip = Convert.ToInt32(skip);
                } else
                {
                    skip = 0;
                }
                
                var maxDuration = 90 - Convert.ToInt32(skip);
                duration = Math.Min(duration, maxDuration);
                var startTime = maxDuration - duration;

                // Update channel infos to have fresh livestream data
                if (channel == null) return false;
                channel = Client.GetChannelInfos(channel.Slug).Result;
                
                try
                {
                    var result = Client.MakeClip(channel, Convert.ToInt32(duration), (string)title, startTime);
                    result.Wait();
                    if (result.Status == TaskStatus.RanToCompletion)
                    {
                        var clip = result.Result;
                        CPH.SetArgument("createClipSuccess", true);
                        CPH.SetArgument("createClipId", clip.Id);
                        CPH.SetArgument("createClipCreatedAt", DateTime.Now);
                        CPH.SetArgument("createClipUrl", $"https://kick.com/{channel.Slug}?clip={clip.Id}");
                        return true;
                    }
                }
                catch
                {
                    CPH.SetArgument("createClipSuccess", false);
                }
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to create to clip : {ex}");
            }

            return false;
        }
        
        public bool GetClips(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                if(args.TryGetValue("channel", out var customChannel))
                {
                    var userInfos = Client.GetChannelUserInfos(channel.Slug, customChannel).Result;
                    channel = Client.GetChannelInfos(userInfos.Slug).Result;
                }

                int count = 20;
                if (args.TryGetValue("count", out var rawCount))
                {
                    count = Convert.ToInt32(rawCount);
                }

                string orderBy = "date";
                if (args.TryGetValue("orderBy", out var rawOrderBy))
                {
                    orderBy = rawOrderBy;
                }

                string timeRange = "all";
                if (args.TryGetValue("timeRange", out var rawTimeRange))
                {
                    timeRange = rawTimeRange;
                }

                var result = Client.GetLatestClips(channel, count, orderBy, timeRange);
                result.Wait();
                if (result.Status == TaskStatus.RanToCompletion)
                {
                    var clips = result.Result;

                    for (int i = 0; i < clips.Count; i++)
                    {
                        var clip = clips[i];
                        CPH.SetArgument($"clip{i}.id", clip.Id);
                        CPH.SetArgument($"clip{i}.title", clip.Title);
                        CPH.SetArgument($"clip{i}.preview", clip.ThumbnailUrl);
                        CPH.SetArgument($"clip{i}.video", clip.ClipUrl);
                        CPH.SetArgument($"clip{i}.link", $"https://kick.com/{channel.Slug}?clip={clip.Id}");
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to change chat mode : {ex}");
            }

            return false;
        }

        public bool GetClipVideoUrl(Dictionary<string, dynamic> args)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");

                if (!args.TryGetValue("clipId", out var clipId))
                {
                    throw new Exception("authentication required");
                }

                var result = Client.GetClipMp4Url(clipId);
                result.Wait();
                if (result.Status == TaskStatus.RanToCompletion)
                {
                    CPH.SetArgument($"clipLink", result.Result);
                    return true;
                }
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to fetch clip URL : {ex}");
            }

            return false;
        }
        #endregion

        #region Chatroom Modes
        public bool ChatEmotesOnly(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                if (!args.TryGetValue("enable", out var enable))
                {
                    if (channel != null)
                    {
                        channel = Client.GetChannelInfos(channel.Slug).Result;
                        enable = !channel.Chatroom.IsEmotesOnly;
                    }
                }

                var result = Client.SetChannelChatroomEmotesOnly(channel, enable);
                result.Wait();
                if (result.Status == TaskStatus.RanToCompletion)
                {
                    var updated = result.Result;
                    CPH.SetArgument("chatEmotesOnly", updated.EmotesMode.Enabled);
                    CPH.SetArgument("chatFollowersOnly", updated.FollowersMode.Enabled);
                    CPH.SetArgument("chatFollowersSince", updated.FollowersMode.MinDuration);
                    CPH.SetArgument("chatSlowMode", updated.SlowMode.Enabled);
                    CPH.SetArgument("chatSlowModeInterval", updated.SlowMode.MessageInterval);
                    CPH.SetArgument("chatSubsOnly", updated.SubscribersMode.Enabled);
                    CPH.SetArgument("chatBotProtection", updated.AdvancedBotProtection.Enabled);
                    CPH.SetArgument("chatBotProtectionRemaining", updated.AdvancedBotProtection.RemainingTime);
                    CPH.SetArgument("chatBotAccountAgeSince", updated.AccountAge.MinDuration);
                    CPH.SetArgument("chatBotAccountAgeRequired", updated.AccountAge.Enabled);
                    return true;
                }
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to change chat mode : {ex}");
            }
            return false;
        }

        public bool ChatSubsOnly(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                if (!args.TryGetValue("enable", out var enable))
                {
                    if (channel != null)
                    {
                        channel = Client.GetChannelInfos(channel.Slug).Result;
                        enable = !channel.Chatroom.IsSubOnly;
                    }
                }

                var result = Client.SetChannelChatroomSubscribersOnly(channel, enable);
                result.Wait();
                if (result.Status == TaskStatus.RanToCompletion)
                {
                    var updated = result.Result;
                    CPH.SetArgument("chatEmotesOnly", updated.EmotesMode.Enabled);
                    CPH.SetArgument("chatFollowersOnly", updated.FollowersMode.Enabled);
                    CPH.SetArgument("chatFollowersSince", updated.FollowersMode.MinDuration);
                    CPH.SetArgument("chatSlowMode", updated.SlowMode.Enabled);
                    CPH.SetArgument("chatSlowModeInterval", updated.SlowMode.MessageInterval);
                    CPH.SetArgument("chatSubsOnly", updated.SubscribersMode.Enabled);
                    CPH.SetArgument("chatBotProtection", updated.AdvancedBotProtection.Enabled);
                    CPH.SetArgument("chatBotProtectionRemaining", updated.AdvancedBotProtection.RemainingTime);
                    CPH.SetArgument("chatBotAccountAgeSince", updated.AccountAge.MinDuration);
                    CPH.SetArgument("chatBotAccountAgeRequired", updated.AccountAge.Enabled);
                    return true;
                }
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to change chat mode : {ex}");
            }
            return false;
        }

        public bool ChatBotProtection(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                if (!args.TryGetValue("enable", out var enable))
                {
                    enable = true;
                }

                var result = Client.SetChannelChatroomBotProtection(channel, enable);
                result.Wait();
                if (result.Status == TaskStatus.RanToCompletion)
                {
                    var updated = result.Result;
                    CPH.SetArgument("chatEmotesOnly", updated.EmotesMode.Enabled);
                    CPH.SetArgument("chatFollowersOnly", updated.FollowersMode.Enabled);
                    CPH.SetArgument("chatFollowersSince", updated.FollowersMode.MinDuration);
                    CPH.SetArgument("chatSlowMode", updated.SlowMode.Enabled);
                    CPH.SetArgument("chatSlowModeInterval", updated.SlowMode.MessageInterval);
                    CPH.SetArgument("chatSubsOnly", updated.SubscribersMode.Enabled);
                    CPH.SetArgument("chatBotProtection", updated.AdvancedBotProtection.Enabled);
                    CPH.SetArgument("chatBotProtectionRemaining", updated.AdvancedBotProtection.RemainingTime);
                    CPH.SetArgument("chatBotAccountAgeSince", updated.AccountAge.MinDuration);
                    CPH.SetArgument("chatBotAccountAgeRequired", updated.AccountAge.Enabled);
                    return true;
                }
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to change chat protection : {ex}");
            }
            return false;
        }

        public bool ChatFollowersOnly(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                int? duration = null;
                if (args.TryGetValue("duration", out var rawDuration))
                {
                    duration = Math.Min(Convert.ToInt32(rawDuration), 525960); // Kick has a 1-year limit
                }

                var result = Client.SetChannelChatroomFollowersMode(channel, duration);
                result.Wait();
                if(result.Status == TaskStatus.RanToCompletion) {
                    var updated = result.Result;
                    CPH.SetArgument("chatEmotesOnly", updated.EmotesMode.Enabled);
                    CPH.SetArgument("chatFollowersOnly", updated.FollowersMode.Enabled);
                    CPH.SetArgument("chatFollowersSince", updated.FollowersMode.MinDuration);
                    CPH.SetArgument("chatSlowMode", updated.SlowMode.Enabled);
                    CPH.SetArgument("chatSlowModeInterval", updated.SlowMode.MessageInterval);
                    CPH.SetArgument("chatSubsOnly", updated.SubscribersMode.Enabled);
                    CPH.SetArgument("chatBotProtection", updated.AdvancedBotProtection.Enabled);
                    CPH.SetArgument("chatBotProtectionRemaining", updated.AdvancedBotProtection.RemainingTime);
                    CPH.SetArgument("chatBotAccountAgeSince", updated.AccountAge.MinDuration);
                    CPH.SetArgument("chatBotAccountAgeRequired", updated.AccountAge.Enabled);
                    return true;
                }
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to change chat mode : {ex}");
            }
            return false;
        }

        public bool ChatSlowMode(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if (channel == null)
                    channel = BroadcasterListener.Channel;

                int? interval = null;
                if (args.TryGetValue("interval", out var rawInterval))
                {
                    interval = Math.Min(Convert.ToInt32(rawInterval), 300); // Kick has a 5-minutes limit
                }

                var result = Client.SetChannelChatroomSlowMode(channel, interval);
                result.Wait();
                if (result.Status == TaskStatus.RanToCompletion)
                {
                    var updated = result.Result;
                    CPH.SetArgument("chatEmotesOnly", updated.EmotesMode.Enabled);
                    CPH.SetArgument("chatFollowersOnly", updated.FollowersMode.Enabled);
                    CPH.SetArgument("chatFollowersSince", updated.FollowersMode.MinDuration);
                    CPH.SetArgument("chatSlowMode", updated.SlowMode.Enabled);
                    CPH.SetArgument("chatSlowModeInterval", updated.SlowMode.MessageInterval);
                    CPH.SetArgument("chatSubsOnly", updated.SubscribersMode.Enabled);
                    CPH.SetArgument("chatBotProtection", updated.AdvancedBotProtection.Enabled);
                    CPH.SetArgument("chatBotProtectionRemaining", updated.AdvancedBotProtection.RemainingTime);
                    CPH.SetArgument("chatBotAccountAgeSince", updated.AccountAge.MinDuration);
                    CPH.SetArgument("chatBotAccountAgeRequired", updated.AccountAge.Enabled);
                    return true;
                }
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to change chat mode : {ex}");
            }
            return false;
        }

        public bool ChatAccountAge(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                int? duration = null;
                if (args.TryGetValue("duration", out var rawDuration))
                {
                    duration = Convert.ToInt32(rawDuration);
                }

                var result = Client.SetChannelChatroomAccountAge(channel, duration);
                result.Wait();
                if(result.Status == TaskStatus.RanToCompletion) {
                    var updated = result.Result;
                    CPH.SetArgument("chatEmotesOnly", updated.EmotesMode.Enabled);
                    CPH.SetArgument("chatFollowersOnly", updated.FollowersMode.Enabled);
                    CPH.SetArgument("chatFollowersSince", updated.FollowersMode.MinDuration);
                    CPH.SetArgument("chatSlowMode", updated.SlowMode.Enabled);
                    CPH.SetArgument("chatSlowModeInterval", updated.SlowMode.MessageInterval);
                    CPH.SetArgument("chatSubsOnly", updated.SubscribersMode.Enabled);
                    CPH.SetArgument("chatBotProtection", updated.AdvancedBotProtection.Enabled);
                    CPH.SetArgument("chatBotProtectionRemaining", updated.AdvancedBotProtection.RemainingTime);
                    CPH.SetArgument("chatBotAccountAgeSince", updated.AccountAge.MinDuration);
                    CPH.SetArgument("chatBotAccountAgeRequired", updated.AccountAge.Enabled);
                    return true;
                }
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to change chat mode : {ex}");
            }
            return false;
        }
        #endregion

        #region Pinned Messages
        public bool PinMessage(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                if (!args.TryGetValue("pinnableMessage", out var message))
                    throw new Exception("missing argument, message required");

                if (!args.TryGetValue("pinDuration", out var duration))
                    duration = 120;

                var originalMessage = JsonConvert.DeserializeObject<ChatMessageEvent>(message);
                var result = Client.PinMessage(channel, originalMessage, duration);
                result.Wait();
                return result.Status == TaskStatus.RanToCompletion;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to change chat mode : {ex}");
                return false;
            }
        }

        public bool UnpinMessage(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                var result = Client.UnpinMessage(channel);
                result.Wait();
                return result.Status == TaskStatus.RanToCompletion;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to unpin a message : {ex}");
                return false;
            }
        }

        public bool GetPinnedMessage(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                var result = Client.GetPinnedMessage(channel);
                result.Wait();
                if (result.Status == TaskStatus.RanToCompletion)
                {
                    var pinnedMessage = result.Result;

                    var emoteRe = new Regex(@"\[emote:(?<emoteId>\d+):(?<emoteText>\w+)\]");
                    var messageStripped = emoteRe.Replace(pinnedMessage.Message.Content, "");
                    var emotes = emoteRe.Matches(pinnedMessage.Message.Content);
                    var emotesList = new List<string>();
                    for (var i = 0; i < emotes.Count; ++i)
                    {
                        emotesList.Add(emotes[i].Value);
                    }

                    var role = 1;
                    if (pinnedMessage.Message.Sender.IsVip)
                        role = 2;
                    if (pinnedMessage.Message.Sender.IsModerator)
                        role = 3;
                    if (pinnedMessage.Message.Sender.IsBroadcaster)
                        role = 4;

                    CPH.SetArgument("user", pinnedMessage.Message.Sender.Username);
                    CPH.SetArgument("userName", pinnedMessage.Message.Sender.Slug);
                    CPH.SetArgument("userId", pinnedMessage.Message.Sender.Id);
                    CPH.SetArgument("userType", "kick");
                    CPH.SetArgument("isSubscribed", pinnedMessage.Message.Sender.IsSubscriber);
                    CPH.SetArgument("isModerator", pinnedMessage.Message.Sender.IsModerator);
                    CPH.SetArgument("isVip", pinnedMessage.Message.Sender.IsVip);

                    CPH.SetArgument("msgId", pinnedMessage.Message.Id);
                    CPH.SetArgument("chatroomId", pinnedMessage.Message.ChatroomId);
                    CPH.SetArgument("role", role);
                    CPH.SetArgument("color", pinnedMessage.Message.Sender.Identity.Color);
                    CPH.SetArgument("message", pinnedMessage.Message.Content);
                    CPH.SetArgument("emoteCount", emotes.Count);
                    CPH.SetArgument("emotes", string.Join(",", emotesList));
                    CPH.SetArgument("messageStripped", messageStripped);
                    CPH.SetArgument("messageCheermotesStripped", messageStripped);
                    CPH.SetArgument("isHighlight", false);
                    CPH.SetArgument("bits", 0);
                    CPH.SetArgument("isAction", false);
                    CPH.SetArgument("isReply", pinnedMessage.Message.IsReply);
                    return true;
                }
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to fetch pinned message : {ex}");
            }
            return false;
        }
        #endregion
        
        public bool GetUserStats(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                string username;
                var isSlug = false;

                {
                    if (args.TryGetValue("targetUserName", out var userName) && userName != null && userName != string.Empty)
                    {
                        username = Convert.ToString(userName);
                        isSlug = true;
                    }
                    else if (args.TryGetValue("targetUser", out var user) && user != null && user != string.Empty)
                    {
                        username = Convert.ToString(user);
                    }
                    else if (args.TryGetValue("userName", out userName) && userName != null && userName != string.Empty)
                    {
                        username = Convert.ToString(userName);
                        isSlug = true;
                    }
                    else if (args.TryGetValue("user", out user) && user != null && user != string.Empty)
                    {
                        username = Convert.ToString(user);
                    }
                    else
                    {
                        username = channel.Slug;
                        isSlug = true;
                    }
                }

                if (username.StartsWith("@"))
                    username = username.Substring(1);

                ChannelUser userInfos;

                if (isSlug)
                {
                    var channelInfos = Client.GetChannelInfos(username).Result;
                    userInfos = Client.GetChannelUserInfos(channel.Slug, channelInfos.User.Username).Result;
                }
                else
                {
                    userInfos = Client.GetChannelUserInfos(channel.Slug, username).Result;
                }

                if(!userInfos.FollowingSince.HasValue)
                {
                    // No follow date => Do nothing
                    CPH.SetArgument("isFollowing", false);
                    return true;
                }

                var timeDiff = DateTime.Now - userInfos.FollowingSince;
                string timeLong = "", timeShort = "";

                if(timeDiff.HasValue)
                {
                    StringBuilder tlong = new StringBuilder();
                    StringBuilder tshort = new StringBuilder();

                    var days = timeDiff.Value.Days;

                    var totalYears = days / 365.25;
                    if (totalYears > 0) {
                        var nYears = Convert.ToInt32(Math.Floor(totalYears));
                        tlong.Append(nYears + " year" + (nYears > 1 ? "s" : "") + ", ");
                        tshort.Append(nYears + "y, ");

                        days -= Convert.ToInt32(Math.Floor(nYears * 365.25));
                    }

                    var totalMonths = timeDiff.Value.TotalDays / 30.4375;
                    if (totalMonths > 0)
                    {
                        var nMonths = Convert.ToInt32(Math.Floor(totalMonths));
                        tlong.Append(nMonths + " month" + (nMonths > 1 ? "s" : "") + ", ");
                        tshort.Append(nMonths + "m, ");

                        days -= Convert.ToInt32(Math.Floor(nMonths * 30.4375));
                    }

                    timeLong = tlong
                        .Append(days    + " day"    + (days > 1    ? "s, " : ", "))
                        .Append(timeDiff.Value.Hours   + " hour"   + (timeDiff.Value.Hours > 1   ? "s, " : ", "))
                        .Append(timeDiff.Value.Minutes + " minute" + (timeDiff.Value.Minutes > 1 ? "s" : ""))
                        .ToString();

                    timeShort = tshort
                        .Append(days + "d, ")
                        .Append(timeDiff.Value.Hours   + "h, ")
                        .Append(timeDiff.Value.Minutes + "m")
                        .ToString();
                }

                CPH.SetArgument("isFollowing", true);
                CPH.SetArgument("followDate", userInfos.FollowingSince);
                CPH.SetArgument("followAgeLong", timeLong);
                CPH.SetArgument("followAgeShort", timeShort);
                CPH.SetArgument("followAgeDays", timeDiff.HasValue ? Convert.ToInt32(Math.Floor(timeDiff.Value.TotalDays)) : 0);
                CPH.SetArgument("followAgeMinutes", timeDiff.HasValue ? Convert.ToInt32(Math.Floor(timeDiff.Value.TotalMinutes)) : 0);
                CPH.SetArgument("followAgeSeconds", timeDiff.HasValue ? Convert.ToInt32(Math.Floor(timeDiff.Value.TotalSeconds)) : 0);
                CPH.SetArgument("followUser", userInfos.Username);
                CPH.SetArgument("followUserName", userInfos.Slug);
                CPH.SetArgument("followUserId", userInfos.Id);
                return true;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to fetch follower infos : {ex}");
                return false;
            }
        }

        public bool PickRandomActiveUser(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                var interval = 600;
                if (args.TryGetValue("interval", out var intervalValue))
                {
                    interval = Convert.ToInt32(intervalValue);
                }

                UserActivity pickedUser = null;
                var basePath = Path.GetDirectoryName(typeof(CPHInlineBase).Assembly.Location) ?? "./";
                using (var database = new LiteDatabase(Path.Combine(basePath, @"data\kick-ext.db")))
                {
                    var dbCollection = database.GetCollection<UserActivity>("users");
                    var notBefore = DateTime.Now.Subtract(TimeSpan.FromSeconds(interval));
                    var activityQuery = from activityObject in dbCollection.Query()
                                        where activityObject.LastActivity.HasValue &&
                                            activityObject.LastActivity.Value >= notBefore
                                        select activityObject;

                    if (activityQuery.Count() > 0)
                    {
                        var pickId = new Random().Next(0, activityQuery.Count());
                        pickedUser = activityQuery.Offset(pickId).FirstOrDefault();
                    }
                }
                
                if (pickedUser != null)
                {
                    GetKickChannelInfos(args, channel, pickedUser.Username);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to fetch a random active user : {ex}");
                return false;
            }
        }

        public bool GetChannelCounters(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                var result = Client.GetChannelInfos(channel.Slug);
                result.Wait();
                if (result.Status != TaskStatus.RanToCompletion) return false;
                
                var channelInfos = result.Result;
                CPH.SetArgument("followerCount", channelInfos.FollowersCount);
                if (channelInfos.LiveStream != null)
                    CPH.SetArgument("viewerCount", channelInfos.LiveStream.ViewerCount);
                return true;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to fetch channel counters : {ex}");
                return false;
            }
        }
        
        #region Rewards
        public bool GetRewardsList(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                if (!args.TryGetValue("rewardEnabledOnly", out var enabledOnly))
                    enabledOnly = false;
                
                Task<Reward[]> result = Client.GetRewardsList(channel, enabledOnly);
                result.Wait();
                var success = result.Status == TaskStatus.RanToCompletion;
                if (!success) return false;
                
                for (var i = 0; i < result.Result.Length; ++i)
                {
                    CPH.SetArgument($"reward{i}", result.Result[i].Id);
                }
                return true;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while unbanning a user : {ex}");
                return false;
            }
        }
        
        public bool GetReward(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                if (!args.TryGetValue("rewardId", out var hold))
                    throw new Exception("missing argument, rewardId required");
                
                Task<Reward> result = Client.GetReward(channel, hold);
                result.Wait();
                var success = result.Status == TaskStatus.RanToCompletion;
                if (!success) return false;
                
                var reward = result.Result;
                CPH.SetArgument("rewardTitle", reward.Title);
                CPH.SetArgument("rewardDescription", reward.Description);
                CPH.SetArgument("rewardBackgroundColor", reward.BackgroundColor);
                CPH.SetArgument("rewardRedemptionSkipQueue", reward.ShouldRedemptionsSkipRequestQueue);
                CPH.SetArgument("rewardCost", reward.Cost);
                CPH.SetArgument("rewardPrompt", reward.Prompt);
                CPH.SetArgument("rewardUserInputRequired", reward.IsUserInputRequired);
                CPH.SetArgument("rewardPaused", reward.IsPaused);
                CPH.SetArgument("rewardEnabled", reward.IsEnabled);
                return true;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while unbanning a user : {ex}");
                return false;
            }
        }
        
        public bool CreateReward(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                var reward = new Reward();

                if (!args.TryGetValue("rewardTitle", out var hold))
                    throw new Exception("missing argument, rewardTitle required");
                reward.Title = hold.ToString();
                
                if (!args.TryGetValue("rewardDescription", out hold))
                    throw new Exception("missing argument, rewardDescription required");
                reward.Description = hold.ToString();
                
                if (!args.TryGetValue("rewardBackgroundColor", out hold))
                    throw new Exception("missing argument, rewardBackgroundColor required");
                reward.BackgroundColor = hold.ToString();
                
                if (!args.TryGetValue("rewardCost", out hold))
                    throw new Exception("missing argument, rewardCost required");
                reward.Cost = Convert.ToInt64(hold);
                
                reward.IsEnabled = !args.TryGetValue("rewardEnabled", out hold) || (bool)Convert.ToBoolean(hold);
                reward.IsPaused = args.TryGetValue("rewardPaused", out hold) && (bool)Convert.ToBoolean(hold);
                reward.IsUserInputRequired = args.TryGetValue("rewardUserInputRequired", out hold) && (bool)Convert.ToBoolean(hold);

                if (reward.IsUserInputRequired)
                {
                    if (!args.TryGetValue("rewardPrompt", out hold))
                        throw new Exception("missing argument, rewardPrompt required");
                    reward.Prompt = hold.ToString();
                }
                else
                {
                    reward.Prompt = "";
                }

                reward.ShouldRedemptionsSkipRequestQueue = args.TryGetValue("rewardRedemptionSkipQueue", out hold) && (bool)Convert.ToBoolean(hold);

                var result = Client.CreateReward(channel, reward);
                result.Wait();
                var success = result.Status == TaskStatus.RanToCompletion;
                if (!success) return false;
                
                CPH.SetArgument("rewardId", result.Result.Id);
                Task.Run(() => ReloadRewards());
                return true;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to fetch channel counters : {ex}");
                return false;
            }
        }
        
        public bool UpdateReward(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                if (!args.TryGetValue("rewardId", out var hold))
                    throw new Exception("missing argument, rewardTitle required");
                
                var reward = Client.GetReward(channel, hold).Result;
                
                if (args.TryGetValue("rewardTitle", out hold))
                    reward.Title = hold.ToString();
                
                if (args.TryGetValue("rewardDescription", out hold))
                    reward.Description = hold.ToString();
                
                if (args.TryGetValue("rewardBackgroundColor", out hold))
                    reward.BackgroundColor = hold.ToString();
                
                if (args.TryGetValue("rewardCost", out hold))
                    reward.Cost = Convert.ToInt64(hold);
                
                if (args.TryGetValue("rewardEnabled", out hold))
                    reward.IsEnabled = Convert.ToBoolean(hold);
                
                if (args.TryGetValue("rewardPaused", out hold))
                    reward.IsPaused = Convert.ToBoolean(hold);
                
                if (args.TryGetValue("rewardUserInputRequired", out hold))
                    reward.IsUserInputRequired = Convert.ToBoolean(hold);

                if (reward.IsUserInputRequired)
                {
                    if (args.TryGetValue("rewardPrompt", out hold))
                        reward.Prompt = hold.ToString();
                }

                if (args.TryGetValue("rewardRedemptionSkipQueue", out hold))
                    reward.ShouldRedemptionsSkipRequestQueue = Convert.ToBoolean(hold);
                
                var result = Client.UpdateReward(channel, reward);
                result.Wait();
                return result.Status == TaskStatus.RanToCompletion;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to fetch channel counters : {ex}");
                return false;
            }
        }
        
        public bool DeleteReward(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                if (!args.TryGetValue("rewardId", out var hold))
                    throw new Exception("missing argument, rewardId required");
                
                var result = Client.DeleteReward(channel, hold);
                result.Wait();
                return result.Status == TaskStatus.RanToCompletion;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while unbanning a user : {ex}");
                return false;
            }
        }
        
        public bool GetRedemptionsList(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                args.TryGetValue("rewardId", out var hold);

                var result = Client.GetRedemptionsList(channel, hold);
                result.Wait();
                var success = result.Status == TaskStatus.RanToCompletion;
                if (!success) return false;
                
                CPH.SetArgument("redemptionsCount", result.Result.Length);
                //CPH.SetArgument("redemptions", result.Result);
                Redemption[] redemptions = result.Result;
                for (var i = 0; i < redemptions.Length; ++i)
                {
                    var redemption = redemptions[i];
                    CPH.SetArgument($"redemption{i}Id", redemption.Id);
                    CPH.SetArgument($"redemption{i}RewardId", redemption.RewardId);
                    CPH.SetArgument($"redemption{i}RewardTitle", redemption.RewardTitle);
                    CPH.SetArgument($"redemption{i}TransactionId", redemption.TransactionId);
                    CPH.SetArgument($"redemption{i}UserId", redemption.UserId);
                    CPH.SetArgument($"redemption{i}Username", redemption.Username);
                    CPH.SetArgument($"redemption{i}UsernameColor", redemption.UsernameColor);
                    CPH.SetArgument($"redemption{i}Status", redemption.Status);
                    CPH.SetArgument($"redemption{i}ChannelId", redemption.ChannelId);
                }
                return true;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] APIError : {ex}");
                return false;
            }
        }
        
        public bool AcceptRedemption(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                if (!args.TryGetValue("redemptionId", out var hold))
                    throw new Exception("missing argument, redemptionId required");
                
                var result = Client.AcceptRedemptions(channel, new string[] { Convert.ToString(hold) });
                result.Wait();
                return result.Status == TaskStatus.RanToCompletion;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] APIError : {ex}");
                return false;
            }
        }
        
        public bool RejectRedemption(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                if (!args.TryGetValue("redemptionId", out var hold))
                    throw new Exception("missing argument, redemptionId required");
                
                var result = Client.RejectRedemptions(channel, new string[] { Convert.ToString(hold) });
                result.Wait();
                return result.Status == TaskStatus.RanToCompletion;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] APIError : {ex}");
                return false;
            }
        }
        #endregion
        
        #region Predictions
        public bool GetLatestPrediction(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                var result = Client.GetLatestPrediction(channel);
                result.Wait();
                var success = result.Status == TaskStatus.RanToCompletion;
                if (success)
                {
                    var predi = result.Result;
                    CPH.SetArgument("predictionId", predi.Id);
                    CPH.SetArgument("predictionTitle", predi.Title);
                    CPH.SetArgument("predictionState", predi.State);
                    CPH.SetArgument("predictionDuration", predi.Duration);
                    CPH.SetArgument("predictionCreatedAt", predi.CreatedAt);
                    CPH.SetArgument("predictionUpdatedAt", predi.UpdatedAt);
                    CPH.SetArgument("predictionLockedAt", predi.LockedAt);
                    CPH.SetArgument("predictionOutcomesCount", predi.Outcomes.Length);
                    
                    for (var i = 0; i < predi.Outcomes.Length; ++i)
                    {
                        CPH.SetArgument($"predictionOutcome{i}Id", predi.Outcomes[i].Id);
                        CPH.SetArgument($"predictionOutcome{i}Title", predi.Outcomes[i].Title);
                        CPH.SetArgument($"predictionOutcome{i}TotalVoteAmount", predi.Outcomes[i].TotalVoteAmount);
                        CPH.SetArgument($"predictionOutcome{i}VoteCount", predi.Outcomes[i].VoteCount);
                        CPH.SetArgument($"predictionOutcome{i}ReturnRate", predi.Outcomes[i].ReturnRate);
                    }

                    if (predi.State == Prediction.StateResolved)
                    {
                        CPH.SetArgument($"predictionWinningOutcomeId", predi.WinningOutcomeId);
                        CPH.SetArgument($"predictionWinningOutcomeIndex", predi.Outcomes.ToList().FindIndex(x => x.Id == predi.WinningOutcomeId));
                    }
                }
                return success;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to create a prediction : {ex}");
                return false;
            }
        }
        
        public bool GetRecentPredictions(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                var result = Client.GetRecentPredictions(channel);
                result.Wait();
                var success = result.Status == TaskStatus.RanToCompletion;
                if (success)
                {
                    for (var i = 0; i < result.Result.Length; ++i)
                    {
                        var predi = result.Result[i];
                        CPH.SetArgument($"prediction{i}Id", predi.Id);
                        CPH.SetArgument($"prediction{i}Title", predi.Title);
                        CPH.SetArgument($"prediction{i}Duration", predi.Duration);
                        CPH.SetArgument($"prediction{i}OutcomesCount", predi.Outcomes.Length);
                        
                        for (var j = 0; j < predi.Outcomes.Length; ++j)
                        {
                            CPH.SetArgument($"prediction{i}Outcome{j}Title", predi.Outcomes[i].Title);
                        }
                    }
                    CPH.SetArgument($"predictionsCount", result.Result.Length);
                }
                return success;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to create a prediction : {ex}");
                return false;
            }
        }
        
        public bool CreatePrediction(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                var prediction = new Prediction();
                dynamic hold, hold2;
                
                if (!args.TryGetValue("predictionTitle", out hold))
                    throw new Exception("missing argument, predictionTitle required");
                prediction.Title = hold.ToString();
                
                if (!args.TryGetValue("predictionDuration", out hold))
                    throw new Exception("missing argument, predictionDuration required");
                prediction.Duration = Convert.ToInt64(hold);
                
                if (!args.TryGetValue("predictionOutcome0", out hold))
                    throw new Exception("missing argument, predictionOutcome0 required");
                if (!args.TryGetValue("predictionOutcome1", out hold2))
                    throw new Exception("missing argument, predictionOutcome1 required");
                
                prediction.Outcomes = new[]
                {
                    new Outcome() { Title = hold.ToString() },
                    new Outcome() { Title = hold2.ToString() }
                };

                var result = Client.CreatePrediction(channel, prediction);
                result.Wait();
                var success = result.Status == TaskStatus.RanToCompletion;
                if (success)
                {
                    CPH.SetArgument("predictionId", result.Result.Id);
                }
                return success;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to create a prediction : {ex}");
                return false;
            }
        }
        
        public bool CancelPrediction(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                if (!args.TryGetValue("predictionId", out var predictionId))
                    throw new Exception("missing argument, predictionId required");
                
                var result = Client.CancelPrediction(channel, predictionId.ToString());
                result.Wait();
                return result.Status == TaskStatus.RanToCompletion;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to cancel the prediction : {ex}");
                return false;
            }
        }
        
        public bool LockPrediction(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                if (!args.TryGetValue("predictionId", out var predictionId))
                    throw new Exception("missing argument, predictionId required");
                
                var result = Client.LockPrediction(channel, predictionId.ToString());
                result.Wait();
                return result.Status == TaskStatus.RanToCompletion;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to lock the prediction : {ex}");
                return false;
            }
        }
        
        public bool ResolvePrediction(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;

                if (!args.TryGetValue("predictionId", out var predictionId))
                    throw new Exception("missing argument, predictionId required");
                if (!args.TryGetValue("outcomeId", out var outcomeId))
                    throw new Exception("missing argument, outcomeId required");
                
                var result = Client.ResolvePrediction(channel, predictionId.ToString(), outcomeId.ToString());
                result.Wait();
                return result.Status == TaskStatus.RanToCompletion;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to resolve the prediction : {ex}");
                return false;
            }
        }
        #endregion
        
        #region Partners
        public bool EnableMultistream(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;
                
                var result = Client.EnableMultistream(channel);
                result.Wait();
                return result.Status == TaskStatus.RanToCompletion;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to enable multistreaming : {ex}");
                return false;
            }
        }
        
        public bool DisableMultistream(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");
                if(channel == null)
                    channel = BroadcasterListener.Channel;
                
                var result = Client.DisableMultistream(channel);
                result.Wait();
                return result.Status == TaskStatus.RanToCompletion;
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to disable multistreaming : {ex}");
                return false;
            }
        }
        #endregion
    }
}
