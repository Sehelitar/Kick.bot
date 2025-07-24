using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kick.Bot;

/// ========================================================== ///
///                       KICK.BOT 1.0                         ///
///                                                            ///
///   Created by | Sehelitar                                   ///
/// Project page | https://github.com/Sehelitar/Kick.bot       ///
///         Wiki | https://github.com/Sehelitar/Kick.bot/wiki  ///
///                                                            ///
///       YOU DON'T HAVE TO CHANGE ANYTHING IN THIS FILE       ///
///           PLEASE READ THE WIKI  IF YOU NEED HELP           ///
/// ========================================================== ///

public class CPHInline
{
	private BotClient Client = null;
	
    public void Init()
    {
	    BotClient.CPH = CPH;
	    if (!BotClient.CheckCompatibility())
		    return;
    	Client = new BotClient();
    }

    public bool Execute()
    {
    	if(Client != null)
            BotClient.OpenConfig();
    	return true;
    }
    
    public bool AcceptRedemption()
    {
	    var result = Client?.AcceptRedemption(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool AddModerator() {
	    var result = Client?.AddChannelModerator(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool AddOG() {
	    var result = Client?.AddChannelOG(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool AddVip() {
	    var result = Client?.AddChannelVip(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool BanUser() {
	    var result = Client?.BanUser(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool CancelPrediction()
    {
	    var result = Client?.CancelPrediction(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool ChangeStreamInfo() {
	    var result = Client?.ChangeStreamInfo(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool ChatAccountAge() {
	    var result = Client?.ChatBotProtection(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool ChatBotProtection() {
	    var result = Client?.ChatBotProtection(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool ChatEmotesOnly() {
	    var result = Client?.ChatEmotesOnly(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool ChatFollowersOnly() {
	    var result = Client?.ChatFollowersOnly(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool ChatSlowMode() {
	    var result = Client?.ChatSlowMode(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool ChatSubsOnly() {
	    var result = Client?.ChatSubsOnly(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }

    public bool ClearChat() {
	    var result = Client?.ClearChat(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool CreatePrediction()
    {
	    var result = Client?.CreatePrediction(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool CreateReward()
    {
	    var result = Client?.CreateReward(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }

    public bool DeleteMessage() {
	    var result = Client?.DeleteMessage(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool DeleteReward()
    {
	    var result = Client?.DeleteReward(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool DisableMultistream() {
	    var result = Client?.DisableMultistream(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool EnableMultistream() {
	    var result = Client?.EnableMultistream(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool GetBroadcasterInfos() {
	    var result = Client?.GetBroadcasterInfos(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }

    public bool GetChannelCounters()
    {
	    var result = Client?.GetChannelCounters(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool GetClips()
    {
	    var result = Client?.GetClips(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool GetClipVideoUrl()
    {
	    var result = Client?.GetClipVideoUrl(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool GetFollowAgeInfo()
    {
	    var result = Client?.GetUserStats(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool GetLatestPrediction()
    {
	    var result = Client?.GetLatestPrediction(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool GetPinnedMessage() {
	    var result = Client?.GetPinnedMessage(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool GetRecentPredictions()
    {
	    var result = Client?.GetRecentPredictions(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool GetRedemptionsList()
    {
	    var result = Client?.GetRedemptionsList(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool GetReward()
    {
	    var result = Client?.GetReward(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool GetRewardsList()
    {
	    var result = Client?.GetRewardsList(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool GetUserInfos() {
	    var result = Client?.GetUserInfos(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool LockPrediction()
    {
	    var result = Client?.LockPrediction(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool MakeClip() {
	    var result = Client?.MakeClip(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool PickRandomActiveUser()
    {
	    var result = Client?.PickRandomActiveUser(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }

    public bool PinMessage() {
	    var result = Client?.PinMessage(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }

    public bool RejectRedemption()
    {
	    var result = Client?.RejectRedemption(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool ReloadRewards()
    {
	    var result = Client?.ReloadRewards();
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool RemoveModerator() {
	    var result = Client?.RemoveChannelModerator(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool RemoveOG() {
	    var result = Client?.RemoveChannelOG(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool RemoveVip() {
	    var result = Client?.RemoveChannelVip(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool ResolvePrediction()
    {
	    var result = Client?.ResolvePrediction(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }

    public bool SendMessage() {
    	var result = Client?.SendMessage(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }

    public bool SendReply() {
    	var result = Client?.SendReply(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool StartPoll() {
	    var result = Client?.StartPoll(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool TimeoutUser() {
	    var result = Client?.TimeoutUser(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool UnbanUser() {
	    var result = Client?.UnbanUser(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }

    public bool UnpinMessage() {
	    var result = Client?.UnpinMessage(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
    
    public bool UpdateReward()
    {
	    var result = Client?.UpdateReward(args);
	    if(!result.HasValue)
		    return false;
	    return result.Value;
    }
}