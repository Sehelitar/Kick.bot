using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kick;
using Kick.Bot;
using Kick.Models.API;

public class CPHInline
{
	// Name of the channel to connect to
	// Leave "null" to automatically connect to authenticated user's channel
	public static readonly string CHANNEL = null;
	
	// How to set this variable :
	// public static readonly string CHANNEL = "MyChannelName";
	
	/*
		!! REFERENCES TO ADD !!
		I have no way to add relative paths for references, so you have to add them.
			
			Kick.dll
			Kick.bot.dll
	*/
	
	/// DON'T CHANGE ANYTHING BELOW ///
	
	private BotClient Client = null;
	private BotEventListener Listener = null;
	
    public void Init()
    {
    	BotClient.CPH = CPH;
    	Authenticate();
    }
    
    public void Authenticate() {
    	Task.Run(async delegate {
    		try {
				if(Client == null)
					Client = new BotClient();
				if(Client.AuthenticatedUser == null)
					await Client.Authenticate();
				if(Listener == null)
				{
					if(CHANNEL == null)
						Listener = await Client.StartListeningToSelf();
					else
						Listener = await Client.StartListeningTo(CHANNEL);
				}
			} catch(Exception e) {
				CPH.LogDebug($"Exception : {e}");
			}
    	});
	}

    public bool Execute()
    {
    	Authenticate();
    	return true;
    }
    
    public bool SendMessage() {
    	try
    	{
    		
			Client.SendMessage(args, Listener.Channel);
			return true;
    	}
    	catch(Exception e)
    	{
    		return false;
    	}
    }
    
    public bool SendReply() {
    	try
    	{
    		
			Client.SendReply(args, Listener.Channel);
			return true;
    	}
    	catch(Exception e)
    	{
    		return false;
    	}
    }
    
    public bool GetUserInfos() {
    	try
    	{
			Client.GetUserInfos(args, Listener.Channel);
			return true;
    	}
    	catch(Exception e)
    	{
    		return false;
    	}
    }
    
    public bool GetBroadcasterInfos() {
    	try
    	{
			Client.GetBroadcasterInfos(args, Listener.Channel);
			return true;
    	}
    	catch(Exception e)
    	{
    		return false;
    	}
    }
    
    public bool AddVip() {
    	try
    	{
			Client.AddChannelVip(args, Listener.Channel);
			return true;
    	}
    	catch(Exception e)
    	{
    		return false;
    	}
    }
    
    public bool RemoveVip() {
    	try
    	{
			Client.RemoveChannelVip(args, Listener.Channel);
			return true;
    	}
    	catch(Exception e)
    	{
    		return false;
    	}
    }
    
    public bool AddOG() {
    	try
    	{
			Client.AddChannelOG(args, Listener.Channel);
			return true;
    	}
    	catch(Exception e)
    	{
    		return false;
    	}
    }
    
    public bool RemoveOG() {
    	try
    	{
			Client.RemoveChannelOG(args, Listener.Channel);
			return true;
    	}
    	catch(Exception e)
    	{
    		return false;
    	}
    }
    
    public bool AddModerator() {
    	try
    	{
			Client.AddChannelModerator(args, Listener.Channel);
			return true;
    	}
    	catch(Exception e)
    	{
    		return false;
    	}
    }
    
    public bool RemoveModerator() {
    	try
    	{
			Client.RemoveChannelModerator(args, Listener.Channel);
			return true;
    	}
    	catch(Exception e)
    	{
    		return false;
    	}
    }
    
    public bool BanUser() {
    	try
    	{
			Client.BanUser(args, Listener.Channel);
			return true;
    	}
    	catch(Exception e)
    	{
    		return false;
    	}
    }
    
    public bool TimeoutUser() {
    	try
    	{
			Client.TimeoutUser(args, Listener.Channel);
			return true;
    	}
    	catch(Exception e)
    	{
    		return false;
    	}
    }
    
    public bool UnbanUser() {
    	try
    	{
			Client.UnbanUser(args, Listener.Channel);
			return true;
    	}
    	catch(Exception e)
    	{
    		return false;
    	}
    }
    
    public bool StartPoll() {
    	try
    	{
			Client.StartPoll(args, Listener.Channel);
			return true;
    	}
    	catch(Exception e)
    	{
    		return false;
    	}
    }
    
    public bool ChangeTitle() {
    	try
    	{
			Client.ChangeTitle(args, Listener.Channel);
			return true;
    	}
    	catch(Exception e)
    	{
    		return false;
    	}
    }
    
    public bool ChangeCategory() {
    	try
    	{
			Client.ChangeCategory(args, Listener.Channel);
			return true;
    	}
    	catch(Exception e)
    	{
    		return false;
    	}
    }
    
    public bool ClearChat() {
    	try
    	{
			Client.ClearChat(args, Listener.Channel);
			return true;
    	}
    	catch(Exception e)
    	{
    		return false;
    	}
    }
    
    public bool MakeClip() {
    	try
    	{
			Client.MakeClip(args, Listener.Channel);
			return true;
    	}
    	catch(Exception e)
    	{
    		return false;
    	}
    }

    public bool GetClips()
    {
        try
        {
            Client.GetClips(args, Listener.Channel);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public bool GetClipMP4URL()
    {
        try
        {
            Client.GetClipMP4URL(args, Listener.Channel);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public bool ChatEmotesOnly() {
    	try
    	{
			Client.ChatEmotesOnly(args, Listener.Channel);
			return true;
    	}
    	catch(Exception e)
    	{
    		return false;
    	}
    }
    
    public bool ChatSubsOnly() {
    	try
    	{
			Client.ChatSubsOnly(args, Listener.Channel);
			return true;
    	}
    	catch(Exception e)
    	{
    		return false;
    	}
    }
    
    public bool ChatBotProtection() {
    	try
    	{
			Client.ChatBotProtection(args, Listener.Channel);
			return true;
    	}
    	catch(Exception e)
    	{
    		return false;
    	}
    }
    
    public bool ChatFollowersOnly() {
    	try
    	{
			Client.ChatFollowersOnly(args, Listener.Channel);
			return true;
    	}
    	catch(Exception e)
    	{
    		return false;
    	}
    }
    
    public bool ChatSlowMode() {
    	try
    	{
			Client.ChatSlowMode(args, Listener.Channel);
			return true;
    	}
    	catch(Exception e)
    	{
    		return false;
    	}
    }
    
    public bool PinMessage() {
    	try
    	{
			Client.PinMessage(args, Listener.Channel);
			return true;
    	}
    	catch(Exception e)
    	{
    		return false;
    	}
    }
    
    public bool UnpinMessage() {
    	try
    	{
			Client.UnpinMessage(args, Listener.Channel);
			return true;
    	}
    	catch(Exception e)
    	{
    		return false;
    	}
    }
    
    public bool GetPinnedMessage() {
    	try
    	{
			Client.GetPinnedMessage(args, Listener.Channel);
			return true;
    	}
    	catch(Exception e)
    	{
    		return false;
    	}
    }

    public bool GetFollowAgeInfo()
    {
        try
        {
            Client.GetUserStats(args, Listener.Channel);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public bool PickRandomActiveUser()
    {
        try
        {
            Client.PickRandomActiveUser(args, Listener.Channel);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}