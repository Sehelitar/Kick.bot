namespace Kick.API.Events
{
    public class ChatModeChangedEvent : KickBaseEvent
    {
        public ChatMode ChatMode { get; internal set; }
    }

    public enum ChatMode
    {
        SlowModeEnabled,
        SlowModeDisabled,
        EmotesOnlyEnabled,
        EmotesOnlyDisabled,
        FollowersOnlyEnabled,
        FollowersOnlyDisabled,
        SubsOnlyEnabled,
        SubsOnlyDisabled,
        AllowLinksActivated,
        AllowLinksDeactivated
    }
}
