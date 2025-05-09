using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kick.Bot;

public class CPHInline
{
	/*
		!! REFERENCES TO ADD !!
		I have no way to add relative paths for references, so you have to add them.
			
			* Kick.bot.dll
	*/
	
	/// DON'T CHANGE ANYTHING BELOW ///
	
	private BotClient Client = null;
	
    public void Init()
    {
    	BotClient.CPH = CPH;
    	Client = new BotClient();
    }

    public bool Execute()
    {
    	BotClient.OpenConfig();
    	return true;
    }
    
    public bool SendMessage() {
    	return Client.SendMessage(args);
    }
    
    public bool SendReply() {
    	return Client.SendReply(args);
    }
    
    public bool DeleteMessage() {
	    return Client.DeleteMessage(args);
    }
    
    public bool ClearChat() {
	    return Client.ClearChat(args);
    }
    
    public bool PinMessage() {
	    return Client.PinMessage(args);
    }
    
    public bool UnpinMessage() {
	    return Client.UnpinMessage(args);
    }
    
    public bool GetPinnedMessage() {
	    return Client.GetPinnedMessage(args);
    }
    
    public bool ChatAccountAge() {
	    return Client.ChatBotProtection(args);
    }
    
    public bool ChatBotProtection() {
	    return Client.ChatBotProtection(args);
    }
    
    public bool ChatEmotesOnly() {
	    return Client.ChatEmotesOnly(args);
    }
    
    public bool ChatFollowersOnly() {
	    return Client.ChatFollowersOnly(args);
    }
    
    public bool ChatSlowMode() {
	    return Client.ChatSlowMode(args);
    }
    
    public bool ChatSubsOnly() {
	    return Client.ChatSubsOnly(args);
    }
    
    public bool AddVip() {
    	return Client.AddChannelVip(args);
    }
    
    public bool RemoveVip() {
    	return Client.RemoveChannelVip(args);
    }
    
    public bool AddOG() {
    	return Client.AddChannelOG(args);
    }
    
    public bool RemoveOG() {
    	return Client.RemoveChannelOG(args);
    }
    
    public bool AddModerator() {
    	return Client.AddChannelModerator(args);
    }
    
    public bool RemoveModerator() {
    	return Client.RemoveChannelModerator(args);
    }
    
    public bool BanUser() {
    	return Client.BanUser(args);
    }
    
    public bool TimeoutUser() {
    	return Client.TimeoutUser(args);
    }
    
    public bool UnbanUser() {
    	return Client.UnbanUser(args);
    }
    
    public bool StartPoll() {
    	return Client.StartPoll(args);
    }
    
    public bool ChangeStreamInfo() {
    	return Client.ChangeStreamInfo(args);
    }
    
    public bool MakeClip() {
    	return Client.MakeClip(args);
    }

    public bool GetClips()
    {
        return Client.GetClips(args);
    }

    public bool GetClipVideoUrl()
    {
        return Client.GetClipVideoUrl(args);
    }

    public bool CreateReward()
    {
	    return Client.CreateReward(args);
    }
    
    public bool UpdateReward()
    {
	    return Client.UpdateReward(args);
    }
    
    public bool DeleteReward()
    {
	    return Client.DeleteReward(args);
    }
    
    public bool ReloadRewards()
    {
    	return Client.ReloadRewards();
	}
    
    public bool GetRedemptionsList()
    {
	    return Client.GetRedemptionsList(args);
    }
    
    public bool AcceptRedemption()
    {
	    return Client.AcceptRedemption(args);
    }
    
    public bool RejectRedemption()
    {
	    return Client.RejectRedemption(args);
    }

    public bool GetFollowAgeInfo()
    {
        return Client.GetUserStats(args);
    }

    public bool PickRandomActiveUser()
    {
        return Client.PickRandomActiveUser(args);
    }

    public bool GetChannelCounters()
    {
        return Client.GetChannelCounters(args);
    }
    
    public bool GetUserInfos() {
	    return Client.GetUserInfos(args);
    }
    
    public bool GetBroadcasterInfos() {
	    return Client.GetBroadcasterInfos(args);
    }
}