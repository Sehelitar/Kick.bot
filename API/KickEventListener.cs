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

using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PusherClient;
using Kick.API.Events;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Kick.API
{
    public sealed class KickEventListener
    {
        private const string KickAppId = "32cbd69e4b950bf97679";
        private const string KickCluster = "us2";

        // [CHAT] Nouveau Message
        public delegate void OnChatMessageHandler(ChatMessageEvent message);
        public event OnChatMessageHandler OnChatMessage;


        // [Chat] Message supprimé
        public delegate void OnChatMessageDeletedHandler(ChatMessageDeletedEvent message);
        public event OnChatMessageDeletedHandler OnChatMessageDeleted;

        // [PUB] Stream Started
        public delegate void OnStreamStartedHandler(LivestreamStartedEvent kickEvent);
        public event OnStreamStartedHandler OnStreamStarted;

        // [PUB] Stream Ended
        public delegate void OnStreamEndedHandler(LivestreamStoppedEvent kickEvent);
        public event OnStreamEndedHandler OnStreamEnded;

        // [PUB] Nouveau Follower
        public delegate void OnNewFollowerHandler(ChannelFollowEvent followEvent);
        public event OnNewFollowerHandler OnViewerFollow;

        // [PUB] Nouveau Sub
        public delegate void OnNewSubscriberHandler(SubscriptionEvent subEvent);
        public event OnNewSubscriberHandler OnSubscription;

        // [PUB] Nouveau.x Gift.s
        public delegate void OnSubGiftedHandler(GiftedSubscriptionEvent giftEvent);
        public event OnSubGiftedHandler OnSubGift;

        // [PUB] Mode de chat modifié
        public delegate void OnChatUpdatedHandler(ChatUpdatedEvent chatUpdateEvent);
        public event OnChatUpdatedHandler OnChatUpdated;

        // [PUB] Sondage créé
        public delegate void OnPollCreatedHandler(PollUpdateEvent pollUpdateEvent);
        public event OnPollCreatedHandler OnPollCreated;

        // [PUB] Sondage mis à jour
        public delegate void OnPollUpdatedHandler(PollUpdateEvent pollUpdateEvent);
        public event OnPollUpdatedHandler OnPollUpdated;

        // [PUB] Sondage terminé
        public delegate void OnPollCompletedHandler(PollUpdateEvent pollUpdateEvent);
        public event OnPollCompletedHandler OnPollCompleted;

        // [PUB] Sondage annulé
        public delegate void OnPollCancelledHandler(PollUpdateEvent pollUpdateEvent);
        public event OnPollCancelledHandler OnPollCancelled;

        // [PUB] Message épinglé
        public delegate void OnMessagePinnedHandler(PinnedMessageEvent pinnedMessageEvent);
        public event OnMessagePinnedHandler OnMessagePinned;

        // [PUB] Message détaché
        public delegate void OnMessageUnpinnedHandler();
        public event OnMessageUnpinnedHandler OnMessageUnpinned;
        
        // [PUB] Récompense achetée
        public delegate void OnRewardRedeemedHandler(RewardRedeemedEvent rewardRedeemedEvent);
        public event OnRewardRedeemedHandler OnRewardRedeemed;

        // [MOD] Termes bloqués
        public delegate void OnBannedWordChangedHandler(BannedWordEvent bannedWordEvent);
        public event OnBannedWordChangedHandler OnBannedWordChanged;

        // [MOD] Bannissement
        public delegate void OnUserBannedHandler(BannedUserEvent bannedUserEvent);
        public event OnUserBannedHandler OnUserBanned;

        // [MOD] Changement de mode du chat
        public delegate void OnChatModeChangedHandler(ChatModeChangedEvent chatModeEvent);
        public event OnChatModeChangedHandler OnChatModeChanged;

        // [MOD] Modification de la liste de termes bannis
        public delegate void OnWordBannedHandler(BannedWordEvent bannedWordEvent);
        public event OnWordBannedHandler OnWordBanned;

        // [MOD] Raid entrant
        public delegate void OnRaidHandler(RaidEvent raidEvent);
        public event OnRaidHandler OnRaid;

        // [MOD] Stream Infos Updated
        public delegate void OnStreamUpdatedHandler(LivestreamUpdatedEvent livestreamEvent);
        public event OnStreamUpdatedHandler OnStreamUpdated;

        private readonly Pusher _pusherClient;

        private readonly Dictionary<long, API.Models.Channel>
            _channels = new Dictionary<long, API.Models.Channel>();

        public KickClient Client { get; }
        public bool IsConnected => _pusherClient.State == ConnectionState.Connected;

        private readonly Dictionary<long, PollUpdateEvent> _currentPolls = new Dictionary<long, PollUpdateEvent>();

        internal KickEventListener(KickClient client, IAuthorizer authorizer)
        {
            Client = client;
            Console.WriteLine($@"Pusher init, AppID: {KickAppId} (Kick.com), Cluster: {KickCluster}");
            _pusherClient = new Pusher(KickAppId, new PusherOptions() { Cluster = KickCluster, Authorizer = authorizer });
        }

        ~KickEventListener()
        {
            DisconnectAsync().RunSynchronously();
        }
        
        public async Task JoinAsync(API.Models.Channel channel, bool asMod = true)
        {
            if (_channels.ContainsKey(channel.Id))
                return;
            
            _channels.Add(channel.Id, channel);
            Console.WriteLine($@"Connecting to chatroom {channel.Slug} ... (Chatroom: {channel.Chatroom.Id}, Channel: {channel.Id})");

            if (_pusherClient.State != ConnectionState.Connected)
            {
                await _pusherClient.ConnectAsync();
            }

            Func<Task> streamListen = null;

            // [PUB] Chatroom
            try
            {
                var publicChatroomId = $"chatrooms.{channel.Chatroom.Id}.v2";
                var chatroom = await _pusherClient.SubscribeAsync(publicChatroomId);
                if (chatroom != null)
                {
                    chatroom.BindAll(delegate (string eventType, PusherEvent eventData)
                    {
                        switch (eventType)
                        {
                            case "App\\Events\\ChatMessageEvent":
                                var message = JsonConvert.DeserializeObject<ChatMessageEvent>(eventData.Data);
                                OnChatMessage?.Invoke(message);
                                return;
                            case "App\\Events\\MessageDeletedEvent":
                            case "App\\Events\\ChatroomClearEvent":
                                var deletedMessage = JsonConvert.DeserializeObject<ChatMessageDeletedEvent>(eventData.Data);
                                OnChatMessageDeleted?.Invoke(deletedMessage);
                                return;
                            case "App\\Events\\ChatroomUpdatedEvent":
                                var updatedChat = JsonConvert.DeserializeObject<ChatUpdatedEvent>(eventData.Data);
                                OnChatUpdated?.Invoke(updatedChat);
                                return;
                            case "App\\Events\\SubscriptionEvent":
                                // TODO
                                // "{\"chatroom_id\":<id>,\"username\":\"<name>\",\"months\":<duration>}"
                                return;
                            case "App\\Events\\PollUpdateEvent":
                                var pollUpdate = JsonConvert.DeserializeObject<PollUpdateEvent>(eventData.Data);
                                pollUpdate.Channel = channel;
                                PollUpdate(channel, pollUpdate);
                                return;
                            case "App\\Events\\PollDeleteEvent":
                                PollUpdate(channel, null);
                                return;
                            case "App\\Events\\PinnedMessageCreatedEvent":
                                var pinnedMessage = JsonConvert.DeserializeObject<PinnedMessageEvent>(eventData.Data);
                                OnMessagePinned?.Invoke(pinnedMessage);
                                return;
                            case "App\\Events\\PinnedMessageDeletedEvent":
                                OnMessageUnpinned?.Invoke();
                                return;
                        }

                        try
                        {
                            Console.WriteLine($@"[PUB-CHAT] {eventType} : {eventData}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($@"[PUB-CHAT] {eventType}, exception raised : {ex.Message}");
                        }
                    });
                    Console.WriteLine($@"Connected to chatroom. Welcome!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            // [PUB] Evènements de la chaine
            try
            {
                var publicChannelId = $"channel.{channel.Id}";
                var publicChannel = await _pusherClient.SubscribeAsync(publicChannelId);
                if (publicChannel != null)
                {
                    publicChannel.BindAll(delegate (string eventType, PusherEvent eventData)
                    {
                        switch (eventType)
                        {
                            case "App\\Events\\ChannelSubscriptionEvent":
                                // TODO
                                // "{\"user_ids\":[<id>],\"username\":\"<user>\",\"channel_id\":<channelId>}"
                                return;
                            case "App\\Events\\StreamerIsLive":
                                var startEvent = JsonConvert.DeserializeObject<LivestreamStartedEvent>(eventData.Data);
                                OnStreamStarted?.Invoke(startEvent);

                                var cinfo = Client.GetChannelInfos(channel.Slug);
                                cinfo.RunSynchronously();
                                channel = cinfo.Result;
                                if(channel.LiveStream != null && streamListen != null)
                                    streamListen().RunSynchronously();

                                return;
                            case "App\\Events\\StopStreamBroadcast":
                                var endEvent = JsonConvert.DeserializeObject<LivestreamStoppedEvent>(eventData.Data);
                                OnStreamEnded?.Invoke(endEvent);
                                return;
                            default:
                                try
                                {
                                    Console.WriteLine($@"[PUB-EVNT] {eventType} : {eventData.Data}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($@"[PUB-EVNT] {eventType}, exception raised : {ex.Message}");
                                }
                                return;
                        }
                    });
                    Console.WriteLine($@"Connected to channels public events. Passive listening enabled.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            if (asMod && (channel.Role == "Channel Host" || channel.Role == "Moderator"))
            {
                // [MOD] Chatroom
                try
                {
                    var privateChatroomId = $"private-chatroom_{channel.Chatroom.Id}";
                    var privChatroom = await _pusherClient.SubscribeAsync(privateChatroomId);
                    if (privChatroom != null)
                    {
                        privChatroom.BindAll(delegate (string eventType, PusherEvent eventData)
                        {
                            ChatMode? modeChanged = null;

                            switch (eventType)
                            {
                                case "BannedWordAdded":
                                    var bwAdd = JsonConvert.DeserializeObject<BannedWordEvent>(eventData.Data);
                                    OnWordBanned?.Invoke(bwAdd);
                                    return;
                                case "BannedWordDeleted":
                                    var bwDel = JsonConvert.DeserializeObject<BannedWordEvent>(eventData.Data);
                                    bwDel.IsWordBanned = false;
                                    OnWordBanned?.Invoke(bwDel);
                                    return;
                                case "BannedUserAdded":
                                case "BannedUserDeleted":
                                case "UserTimeouted":
                                    var message = JsonConvert.DeserializeObject<BannedUserEvent>(eventData.Data);
                                    OnUserBanned?.Invoke(message);
                                    return;
                                case "MessageDeleted":
                                    // TODO - Utilité ? Remonte déjà dans le flux public
                                    break;
                                case "SlowModeActivated":
                                    modeChanged = ChatMode.SlowModeEnabled;
                                    break;
                                case "SlowModeDeactivated":
                                    modeChanged = ChatMode.SlowModeDisabled;
                                    break;
                                case "EmotesModeActivated":
                                    modeChanged = ChatMode.EmotesOnlyEnabled;
                                    break;
                                case "EmotesModeDeactivated":
                                    modeChanged = ChatMode.EmotesOnlyDisabled;
                                    break;
                                case "FollowersModeActivated":
                                    modeChanged = ChatMode.FollowersOnlyEnabled;
                                    break;
                                case "FollowersModeDeactivated":
                                    modeChanged = ChatMode.FollowersOnlyDisabled;
                                    break;
                                case "SubscribersModeActivated":
                                    modeChanged = ChatMode.SubsOnlyEnabled;
                                    break;
                                case "SubscribersModeDeactivated":
                                    modeChanged = ChatMode.SubsOnlyDisabled;
                                    break;
                                case "AllowLinksActivated":
                                    modeChanged = ChatMode.AllowLinksActivated;
                                    break;
                                case "AllowLinksDeactivated":
                                    modeChanged = ChatMode.AllowLinksDeactivated;
                                    break;
                            }

                            if (modeChanged.HasValue)
                            {
                                var message = JsonConvert.DeserializeObject<ChatModeChangedEvent>(eventData.Data);
                                message.ChatMode = modeChanged.Value;
                                OnChatModeChanged?.Invoke(message);
                                return;
                            }

                            try
                            {
                                Console.WriteLine($@"[MOD-CHAT] {eventType} : {eventData.Data}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($@"[MOD-CHAT] {eventType}, exception raised : {ex.Message}");
                            }
                        });
                        Console.WriteLine($@"Connected to moderation events stream.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                // [MOD] Evènements de la chaine
                try
                {
                    var privateChannelId = $"private-channel_{channel.Id}";
                    var privChannel = await _pusherClient.SubscribeAsync(privateChannelId);
                    if (privChannel != null)
                    {
                        privChannel.BindAll(delegate (string eventType, PusherEvent eventData)
                        {
                            switch (eventType)
                            {
                                case "FollowerAdded":
                                    var followAdded = JsonConvert.DeserializeObject<ChannelFollowEvent>(eventData.Data);
                                    OnViewerFollow?.Invoke(followAdded);
                                    return;
                                case "FollowerDeleted":
                                    var followDeleted = JsonConvert.DeserializeObject<ChannelFollowEvent>(eventData.Data);
                                    followDeleted.IsFollowing = false;
                                    OnViewerFollow?.Invoke(followDeleted);
                                    return;
                                case "SubscriptionCreated":
                                case "SubscriptionRenewed":
                                    var newSub = JsonConvert.DeserializeObject<SubscriptionEvent>(eventData.Data);
                                    OnSubscription?.Invoke(newSub);
                                    return;
                                case "SubscriptionGifted":
                                    var newGift = JsonConvert.DeserializeObject<GiftedSubscriptionEvent>(eventData.Data);
                                    OnSubGift?.Invoke(newGift);
                                    return;
                                case "RedeemedReward":
                                    var newRedeem = JsonConvert.DeserializeObject<RewardRedeemedEvent>(eventData.Data);
                                    OnRewardRedeemed?.Invoke(newRedeem);
                                    return;
                            }

                            try
                            {
                                Console.WriteLine($@"[MOD-EVNT] {eventType} : {eventData.Data}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($@"[MOD-EVNT] {eventType}, exception raised : {ex.Message}");
                            }
                        });
                        Console.WriteLine($@"Connected to channels private events.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                // [MOD] Evènements de stream
                streamListen = async () =>
                {
                    try
                    {
                        var privateLivestreamId = $"private-livestream_{channel.LiveStream.Id}";
                        var privLivestream = await _pusherClient.SubscribeAsync(privateLivestreamId);
                        if (privLivestream != null)
                        {
                            privLivestream.BindAll(delegate(string eventType, PusherEvent eventData)
                            {
                                switch (eventType)
                                {
                                    case "HostReceived":
                                        var raid = JsonConvert.DeserializeObject<RaidEvent>(eventData.Data);
                                        OnRaid?.Invoke(raid);
                                        return;
                                    case "TitleChanged":
                                        return;
                                    case "CategoryChanged":
                                        return;
                                    case "MatureModeActivated":
                                        return;
                                    case "MatureModeDeactivated":
                                        return;
                                }

                                try
                                {
                                    Console.WriteLine($@"[MOD-LVSTR] {eventType} : {eventData.Data}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($@"[MOD-LVSTR] {eventType}, exception raised : {ex.Message}");
                                }
                            });
                            Console.WriteLine($@"Connected to private livestream events.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }

                    try
                    {
                        var privateLivestreamId = $"private-livestream-updated.{channel.LiveStream.Id}";
                        var privLivestream = await _pusherClient.SubscribeAsync(privateLivestreamId);
                        if (privLivestream != null)
                        {
                            privLivestream.BindAll(delegate(string eventType, PusherEvent eventData)
                            {
                                switch (eventType)
                                {
                                    case "App\\Events\\LiveStream\\UpdatedLiveStreamEvent":
                                        var liveUpdate =
                                            JsonConvert.DeserializeObject<LivestreamUpdatedEvent>(eventData.Data);
                                        channel = Client.GetChannelInfos(channel.Slug).Result;
                                        liveUpdate.Channel = channel;
                                        OnStreamUpdated?.Invoke(liveUpdate);
                                        return;
                                }

                                try
                                {
                                    Console.WriteLine($@"[MOD-LVSTR] {eventType} : {eventData.Data}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(
                                        $@"[MOD-LVSTR] {eventType}, Le traitement du contenu a provoqué une Exception : {ex.Message}");
                                }
                            });
                            Console.WriteLine($@"Connected to secondary private livestream events.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                };
                if (channel.LiveStream != null)
                    await streamListen();
            }
        }

        public async Task LeaveAsync(API.Models.Channel channel)
        {
            if (!_channels.ContainsKey(channel.Id))
                return;
            
            var publicChatroomId = $"chatrooms.{channel.Chatroom.Id}.v2";
            var publicChannelId = $"channel.{channel.Id}";
            var privateChatroomId = $"private-chatroom_{channel.Chatroom.Id}";
            var privateChannelId = $"private-channel_{channel.Id}";

            Console.WriteLine($@"Stopped listening for events from {channel.Slug}");

            await _pusherClient.UnsubscribeAsync(publicChatroomId);
            await _pusherClient.UnsubscribeAsync(publicChannelId);
            await _pusherClient.UnsubscribeAsync(privateChatroomId);
            await _pusherClient.UnsubscribeAsync(privateChannelId);

            if (channel.LiveStream != null)
            {
                var privateLivestreamId = $"private-livestream_{channel.LiveStream.Id}";
                await _pusherClient.UnsubscribeAsync(privateLivestreamId);
            }
        }

        public async Task ConnectAsync()
        {
            Console.WriteLine($@"Connecting...");
            await _pusherClient.ConnectAsync();
            if (_channels.Any())
            {
                var channels = new Dictionary<long, API.Models.Channel>(_channels);
                _channels.Clear();
                foreach (var channel in channels)
                {
                    await JoinAsync(channel.Value);
                }
            }
            Console.WriteLine($@"Connected.");
        }
        
        public async Task DisconnectAsync()
        {
            Console.WriteLine($@"Disconnecting...");
            await _pusherClient.DisconnectAsync();
            Console.WriteLine($@"Disconnected.");
        }

        private void PollUpdate(API.Models.Channel channel, PollUpdateEvent pollUpdateEvent)
        {
            if(!_currentPolls.ContainsKey(channel.Id))
            {
                _currentPolls[channel.Id] = pollUpdateEvent;
                var pollUpdater = new Thread(() =>
                {
                    // Poll Created
                    OnPollCreated?.Invoke(pollUpdateEvent);

                    // Wait for poll to end
                    var waitHandle = new AutoResetEvent(false);
                    var remaining = pollUpdateEvent.Poll.Remaining;
                    var timer = new Timer(state => {
                        if(!_currentPolls.TryGetValue(channel.Id, out var currentPoll))
                        {
                            // Not supposed to happen, thread will be stopped
                            currentPoll = pollUpdateEvent; // Restore context
                            pollUpdateEvent.State = PollState.Cancelled;
                        } else
                        {
                            pollUpdateEvent = currentPoll; // Save context
                        }

                        if(pollUpdateEvent.State == PollState.Cancelled)
                        {
                            OnPollCancelled?.Invoke(pollUpdateEvent);
                            waitHandle.Set();
                            return;
                        }

                        currentPoll.Poll.Remaining = --remaining;
                        if (currentPoll.Poll.Remaining >= 0)
                        {
                            OnPollUpdated?.Invoke(currentPoll);
                        }
                        else
                        {
                            currentPoll.Poll.Remaining = 0;
                            currentPoll.State = PollState.Completed;
                            OnPollCompleted?.Invoke(currentPoll);
                            waitHandle.Set();
                        }
                    }, null, 1000, 1000);
                    waitHandle.WaitOne();

                    // Stop Timer
                    timer.Dispose();

                    // Poll completed
                    _currentPolls.Remove(channel.Id);
                });
                pollUpdater.Start();
            }
            else {
                if (pollUpdateEvent != null)
                {
                    _currentPolls[channel.Id].Poll = pollUpdateEvent.Poll;
                }
                else
                {
                    _currentPolls[channel.Id].State = PollState.Cancelled;
                }
            }
        }
    }
}
