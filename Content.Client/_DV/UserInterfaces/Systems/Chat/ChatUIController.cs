using Content.Client.CharacterInfo;
using Content.Shared.Chat;
using Content.Shared.Chat.TypingIndicator;
using Robust.Shared.Prototypes;
using Robust.Client.UserInterface.Controllers;

namespace Content.Client.UserInterface.Systems.Chat;

public sealed partial class ChatUIController : IOnSystemChanged<CharacterInfoSystem>
{
    public ChatSelectChannel CurrentChannel = ChatSelectChannel.None;
    private static readonly ProtoId<TypingIndicatorPrototype> WhisperID = "whisper";
    private static readonly ProtoId<TypingIndicatorPrototype> EmoteID = "emote";
    private static readonly ProtoId<TypingIndicatorPrototype> OocID = "ooc";
    private static readonly ProtoId<TypingIndicatorPrototype> RadioID = "radio";

    /// <summary>
    ///     Notifies and sets what type of typing indicator should be put.
    /// </summary>
    public void NotifySpecificChatTextChange(ChatSelectChannel selectedChannel)
    {
        var channel = CurrentChannel;
        if (CurrentChannel == ChatSelectChannel.None)
            channel = selectedChannel;

        switch (channel)
        {
            case ChatSelectChannel.Whisper:
            case ChatSelectChannel.Subtle: // Aurora's Song
            case ChatSelectChannel.SubtleOOC: // Aurora's Song
                _typingIndicator?.ClientAlternateTyping(TypingIndicatorState.Typing, WhisperID); // Aurora's Song - feed state for starcup changes
                break;

            case ChatSelectChannel.Radio:
                _typingIndicator?.ClientAlternateTyping(TypingIndicatorState.Typing, RadioID); // Aurora's Song - feed state for starcup changes
                break;

            case ChatSelectChannel.Emotes:
                _typingIndicator?.ClientAlternateTyping(TypingIndicatorState.Typing, EmoteID); // Aurora's Song - feed state for starcup changes
                break;

            case ChatSelectChannel.LOOC:
            case ChatSelectChannel.OOC:
                _typingIndicator?.ClientAlternateTyping(TypingIndicatorState.Typing, OocID); // Aurora's Song - feed state for starcup changes
                break;

            default:
                _typingIndicator?.ClientChangedChatText();
                break;
        }
    }
}
