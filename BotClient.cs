using Kick;
using Kick.Models.Events;
using Kick.Models.API;
using Newtonsoft.Json;
using Streamer.bot.Plugin.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.CodeDom;
using static Kick.Bot.BotClient;
using System.Runtime.Remoting.Channels;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Drawing.Imaging;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Linq;

namespace Kick.Bot
{
    public sealed class BotClient
    {
        public static IInlineInvokeProxy CPH;
        
        internal KickClient Client { get; private set; }

        public User AuthenticatedUser { get; private set; }

        public BotClient() {
            CPH.RegisterCustomTrigger("[Kick] Chat Message", BotEventListener.BotEventType.Message, new string[] { "Kick", "Chat" });

            CPH.RegisterCustomTrigger("[Kick] Chat Command (Any)", BotEventListener.BotEventType.ChatCommand, new string[] { "Kick", "Commands" });
            CPH.RegisterCustomTrigger("[Kick] Chat Command Cooldown (Any)", BotEventListener.BotEventType.ChatCommand, new string[] { "Kick", "Commands" });

            CPH.RegisterCustomTrigger("[Kick] Follow", BotEventListener.BotEventType.Follow, new string[] { "Kick", "Channel" });

            CPH.RegisterCustomTrigger("[Kick] Subscription", BotEventListener.BotEventType.Subscription, new string[] { "Kick", "Subscriptions" });
            CPH.RegisterCustomTrigger("[Kick] Sub Gift (x1)", BotEventListener.BotEventType.SubGift, new string[] { "Kick", "Subscriptions" });
            CPH.RegisterCustomTrigger("[Kick] Sub Gifts (multiple)", BotEventListener.BotEventType.SubGifts, new string[] { "Kick", "Subscriptions" });

            CPH.RegisterCustomTrigger("[Kick] Chat Message Deleted", BotEventListener.BotEventType.MessageDeleted, new string[] { "Kick", "Moderation" });
            CPH.RegisterCustomTrigger("[Kick] Timeout", BotEventListener.BotEventType.Timeout, new string[] { "Kick", "Moderation" });
            CPH.RegisterCustomTrigger("[Kick] User Ban", BotEventListener.BotEventType.UserBanned, new string[] { "Kick", "Moderation" });

            CPH.RegisterCustomTrigger("[Kick] Poll Created", BotEventListener.BotEventType.PollCreated, new string[] { "Kick", "Polls" });
            CPH.RegisterCustomTrigger("[Kick] Poll Updated", BotEventListener.BotEventType.PollUpdated, new string[] { "Kick", "Polls" });
            CPH.RegisterCustomTrigger("[Kick] Poll Completed", BotEventListener.BotEventType.PollCompleted, new string[] { "Kick", "Polls" });
            CPH.RegisterCustomTrigger("[Kick] Poll Cancelled", BotEventListener.BotEventType.PollCancelled, new string[] { "Kick", "Polls" });

            CPH.RegisterCustomTrigger("[Kick] Stream Started", BotEventListener.BotEventType.StreamStarted, new string[] { "Kick", "Stream" });
            CPH.RegisterCustomTrigger("[Kick] Stream Ended", BotEventListener.BotEventType.StreamEnded, new string[] { "Kick", "Stream" });
            CPH.RegisterCustomTrigger("[Kick] Title/Category Changed", BotEventListener.BotEventType.TitleChanged, new string[] { "Kick", "Stream" });

            CPH.RegisterCustomTrigger("[Kick] Raid", BotEventListener.BotEventType.Raid, new string[] { "Kick" });

            var target = Path.GetTempPath() + "KickLogo.png";
            Properties.Resources.KickLogo.Save(target, ImageFormat.Png);

            CPH.LogDebug("[Kick] Démarrage du bot Kick");
            Client = new KickClient();
        }

        ~BotClient()
        {
            CPH.LogDebug("[Kick] Arrêt du bot Kick");
        }

        public async Task Authenticate()
        {
            CPH.LogDebug("[Kick] Authentification...");
            AuthenticatedUser = await Client.Authenticate();
            CPH.LogDebug($"[Kick] Connecté en tant que {AuthenticatedUser.Username}");

            var target = Path.GetTempPath() + "KickLogo.png";
            //CPH.ShowToastNotification("Kick", $"Successfuly connected as {AuthenticatedUser.UserName}", "via Kick.bot", target);
            new ToastContentBuilder()
                .AddText("Kick")
                .AddText($"Successfuly connected as {AuthenticatedUser.Username}")
                .AddAppLogoOverride(new Uri(target), ToastGenericAppLogoCrop.None)
                .SetToastDuration(ToastDuration.Short)
                .Show();
        }

        public async Task<BotEventListener> StartListeningTo(string channelName)
        {
            if (AuthenticatedUser == null)
                throw new Exception("Authentification requise");

            CPH.LogDebug($"[Kick] Mise en place de l'écoute des évènements pour la chaine de {channelName}");
            var channel = await Client.GetChannelInfos(channelName);
            return new BotEventListener(Client.GetEventListener(), channel);
        }

        public async Task<BotEventListener> StartListeningToSelf()
        {
            if (AuthenticatedUser == null)
                throw new Exception("Authentification requise");

            CPH.LogDebug($"[Kick] Mise en place de l'écoute des évènements pour la chaine de {AuthenticatedUser.StreamerChannel.Slug}");
            var channel = await Client.GetChannelInfos(AuthenticatedUser.StreamerChannel.Slug);
            return new BotEventListener(Client.GetEventListener(), channel);
        }

        public void SendMessage(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try {
                if (AuthenticatedUser == null)
                    throw new Exception("authentification requise");

                if (args.TryGetValue("chatroomId", out var chatroomId))
                    chatroomId = Convert.ToInt64(chatroomId);
                else if (channel != null)
                    chatroomId = channel.Chatroom.Id;
                else
                    throw new Exception("argument chatroomId manquant.");

                Client.SendMessageToChatroom(chatroomId, Convert.ToString(args["message"])).Wait();
            }
            catch(Exception ex)
            {
                CPH.LogDebug($"[Kick] Une erreur s'est produite lors de l'envoi du message : {ex}");
            }
        }

        public void SendReply(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentification requise");

                if (args.TryGetValue("chatroomId", out var chatroomId))
                    chatroomId = Convert.ToInt64(chatroomId);
                else if (channel != null)
                    chatroomId = channel.Chatroom.Id;
                else
                    throw new Exception("argument chatroomId manquant.");
                string message = String.Empty;
                if (args.TryGetValue("rawInput", out var rawInput))
                    message = Convert.ToString(rawInput);
                else
                    message = args["message"];
                Client.SendReplyToChatroom(
                    chatroomId,
                    Convert.ToString(args["reply"]),
                    Convert.ToString(args["msgId"]),
                    Convert.ToString(message),
                    Convert.ToInt64(args["userId"]),
                    Convert.ToString(args["user"])
                ).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] Une erreur s'est produite lors de l'envoi de la réponse : {ex}");
            }
        }

        public void GetUserInfos(Dictionary<string, dynamic> args, Channel channel)
        {
            try
            {
                if (args.TryGetValue("targetUserName", out var userName) && userName != null && userName != String.Empty)
                    GetKickChannelInfos(args, channel, Convert.ToString(userName), true);
                else if (args.TryGetValue("targetUser", out var user) && user != null && user != String.Empty)
                    GetKickChannelInfos(args, channel, Convert.ToString(user));
                else if (args.TryGetValue("userName", out userName) && userName != null && userName != String.Empty)
                    GetKickChannelInfos(args, channel, Convert.ToString(userName) , true);
                else if (args.TryGetValue("user", out user) && user != null && user != String.Empty)
                    GetKickChannelInfos(args, channel, Convert.ToString(user));
                else
                    GetKickChannelInfos(args, channel, channel.Slug, true);
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] Une erreur s'est produite lors de l'envoi de la réponse : {ex}");
            }
        }

        public void GetBroadcasterInfos(Dictionary<string, dynamic> args, Channel channel)
        {
            try
            {
                GetKickChannelInfos(args, channel, channel.Slug, true);
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] Une erreur s'est produite lors de l'envoi de la réponse : {ex}");
            }
        }

        private void GetKickChannelInfos(Dictionary<string, dynamic> args, Channel channel, string username, bool isSlug = false)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentification requise");

                if(username.StartsWith("@"))
                    username = username.Substring(1);


                Channel channelInfos = null;
                ChannelUser userInfos = null;

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

                CPH.SetArgument("targetUser", channelInfos.User.Username);
                CPH.SetArgument("targetUserName", channelInfos.Slug);
                CPH.SetArgument("targetUserId", channelInfos.User.Id);
                CPH.SetArgument("targetDescription", channelInfos.User.Bio);
                CPH.SetArgument("targetDescriptionEscaped", WebUtility.UrlEncode(channelInfos.User.Bio));
                CPH.SetArgument("targetUserProfileImageUrl", channelInfos.User.ProfilePic);
                CPH.SetArgument("targetUserProfileImageUrlEscaped", WebUtility.UrlEncode(channelInfos.User.ProfilePic));
                CPH.SetArgument("targetUserProfileImageEscaped", WebUtility.UrlEncode(channelInfos.User.ProfilePic));
                CPH.SetArgument("targetUserType", channelInfos.IsVerified ? "partner" : (channelInfos.IsAffiliate ? "affiliate" : String.Empty));
                CPH.SetArgument("targetIsAffiliate", channelInfos.IsAffiliate);
                CPH.SetArgument("targetIsPartner", channelInfos.IsVerified);
                CPH.SetArgument("targetLastActive", DateTime.Now);
                CPH.SetArgument("targetPreviousActive", DateTime.Now);
                CPH.SetArgument("targetIsSubscribed", userInfos.IsSubscriber);
                CPH.SetArgument("targetSubscriptionTier", (userInfos.IsSubscriber && userInfos.SubscribedFor > 0 ? 1000 : 0));
                CPH.SetArgument("targetIsModerator", userInfos.IsModerator);
                CPH.SetArgument("targetIsVip", userInfos.IsVIP);
                CPH.SetArgument("targetIsFollowing", userInfos.IsFollowing);
                CPH.SetArgument("targetChannelTitle", channelInfos.LiveStream?.SessionTitle ?? String.Empty);
                CPH.SetArgument("game", channelInfos.LiveStream?.Categories.First()?.Name);
                CPH.SetArgument("gameId", channelInfos.LiveStream?.Categories.First()?.Id);
                CPH.SetArgument("createdAt", DateTime.Now);
                CPH.SetArgument("accountAge", Convert.ToInt64((DateTime.Now - channelInfos.Chatroom.CreatedAt).TotalSeconds));
                CPH.SetArgument("tagCount", 0);
                CPH.SetArgument("tags", new List<string>());
                CPH.SetArgument("tagsDelimited", String.Empty);

                if (channelInfos.LiveStream != null)
                {
                    CPH.SetArgument("tagCount", channelInfos.LiveStream.Tags.Count);
                    CPH.SetArgument("tags", channelInfos.LiveStream.Tags);
                    int tagId = 0;
                    channelInfos.LiveStream.Tags.ForEach((tag) => {
                        CPH.SetArgument("tag" + (tagId++), tag);
                    });
                    CPH.SetArgument("tagsDelimited", String.Join(", ", channelInfos.LiveStream.Tags.ToArray()));
                    ;
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
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] Une erreur s'est produite lors de l'envoi de la réponse : {ex}");
            }
        }

        public void AddChannelVip(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentification requise");

                if (!args.TryGetValue("user", out var username))
                    throw new Exception("nom d'utilisateur manquant.");

                Client.AddChannelVip(channel, username).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] Une erreur s'est produite lors de l'ajout du VIP : {ex}");
            }
        }

        public void RemoveChannelVip(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentification requise");

                if (!args.TryGetValue("user", out var username))
                    throw new Exception("nom d'utilisateur manquant.");

                Client.RemoveChannelVip(channel, username).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] Une erreur s'est produite lors du retrait du VIP : {ex}");
            }
        }

        public void AddChannelOG(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentification requise");

                if (!args.TryGetValue("user", out var username))
                    throw new Exception("nom d'utilisateur manquant.");

                Client.AddChannelOG(channel, username).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] Une erreur s'est produite lors de l'ajout du OG : {ex}");
            }
        }

        public void RemoveChannelOG(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentification requise");

                if (!args.TryGetValue("user", out var username))
                    throw new Exception("nom d'utilisateur manquant.");

                Client.RemoveChannelOG(channel, username).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] Une erreur s'est produite lors du retrait du OG : {ex}");
            }
        }

        public void AddChannelModerator(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentification requise");

                if (!args.TryGetValue("user", out var username))
                    throw new Exception("nom d'utilisateur manquant.");

                Client.AddChannelModerator(channel, username).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] Une erreur s'est produite lors de l'ajout du modérateur : {ex}");
            }
        }

        public void RemoveChannelModerator(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentification requise");

                if (!args.TryGetValue("user", out var username))
                    throw new Exception("nom d'utilisateur manquant.");

                Client.RemoveChannelModerator(channel, username).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] Une erreur s'est produite lors du retrait du modérateur : {ex}");
            }
        }

        public void BanUser(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentification requise");

                if (!args.TryGetValue("user", out var username))
                    throw new Exception("nom d'utilisateur manquant.");

                if (!args.TryGetValue("banReason", out var banReason))
                    banReason = "";

                Client.BanUser(channel, (string)username, (string)banReason).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] Une erreur s'est produite lors du banissement : {ex}");
            }
        }

        public void TimeoutUser(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentification requise");

                if (!args.TryGetValue("user", out var username))
                    throw new Exception("nom d'utilisateur manquant.");

                if (!args.TryGetValue("banDuration", out var banDuration))
                    throw new Exception("durée manquante.");

                if (!args.TryGetValue("banReason", out var banReason))
                    banReason = "";

                Client.TimeoutUser(channel, (string)username, Convert.ToInt64(banDuration), (string)banReason).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] Une erreur s'est produite lors du timeout : {ex}");
            }
        }

        public void UnbanUser(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentification requise");

                if (!args.TryGetValue("user", out var username))
                    throw new Exception("nom d'utilisateur manquant.");

                Client.UnbanUser(channel, (string)username).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] Une erreur s'est produite lors du timeout : {ex}");
            }
        }

        public void StartPoll(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentification requise");

                if (!args.TryGetValue("pollTitle", out var title))
                    throw new Exception("titre manquant.");

                if (!args.TryGetValue("pollDuration", out var duration))
                    throw new Exception("durée manquante.");
                 
                if (!args.TryGetValue("pollDisplayTime", out var displayTime))
                    displayTime = 30;

                if (!args.TryGetValue("pollChoices", out var choices))
                    throw new Exception("options manquantes.");

                Client.StartPoll(channel, (string)title, ((string)choices).Split('|'), Convert.ToInt32(duration), Convert.ToInt32(displayTime)).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] Une erreur s'est produite lors du démarrage du sondage : {ex}");
            }
        }

        public void ChangeTitle(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentification requise");

                if (!args.TryGetValue("title", out var title))
                    throw new Exception("titre manquant.");

                Client.SetChannelTitle(channel, (string)title).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] Une erreur s'est produite lors du changement du titre : {ex}");
            }
        }

        public void ChangeCategory(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentification requise");

                if (!args.TryGetValue("category", out var category))
                    throw new Exception("catégorie manquante.");

                Client.SetChannelCategory(channel, (string)category).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] Une erreur s'est produite lors du changement de catégorie : {ex}");
            }
        }

        public void ClearChat(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentification requise");

                Client.ClearChat(channel).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] Une erreur s'est produite lors du changement du titre : {ex}");
            }
        }

        public void MakeClip(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentification requise");

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
                channel = Client.GetChannelInfos(channel.Slug).Result;
                try
                {
                    var clip = Client.MakeClip(channel, Convert.ToInt32(duration), (string)title, startTime).Result;
                    CPH.SetArgument("createClipSuccess", true);
                    CPH.SetArgument("createClipId", clip.Id);
                    CPH.SetArgument("createClipCreatedAt", DateTime.Now);
                    CPH.SetArgument("createClipUrl", $"https://kick.com/{channel.Slug}?clip={clip.Id}");
                }
                catch
                {
                    CPH.SetArgument("createClipSuccess", false);
                }
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] Une erreur s'est produite lors du changement du titre : {ex}");
            }
        }
    }
}
