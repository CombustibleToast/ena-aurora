using Content.Client.Chat.TypingIndicator;
using Content.Shared.Chat.TypingIndicator;
using Content.Shared.DeltaV.AACTablet;
using Content.Shared.DeltaV.QuickPhrase;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client.DeltaV.AACTablet.UI;

public sealed partial class AACBoundUserInterface : BoundUserInterface // starcup: made partial
{
    [ViewVariables]
    private AACWindow? _window;

    private static readonly ProtoId<TypingIndicatorPrototype> AACTypingIndicator = "aac";

    private TypingIndicatorSystem? _typing;

    public AACBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _window?.Close();
        _window = this.CreateWindow<AACWindow>();
        _window.PhraseButtonPressed += OnPhraseButtonPressed;
        _window.Typing += OnTyping;
        _window.SubmitPressed += OnSubmit;
    }

    private void OnPhraseButtonPressed(List<ProtoId<QuickPhrasePrototype>> phraseId, string prefix)
    {
        SendMessage(new AACTabletSendPhraseMessage(phraseId, prefix)); // starcup: prefix parameter
    }

    private void OnTyping()
    {
        _typing ??= EntMan.System<TypingIndicatorSystem>();
        _typing?.ClientAlternateTyping(AACTypingIndicator);
    }

    private void OnSubmit()
    {
        _typing ??= EntMan.System<TypingIndicatorSystem>();
        _typing?.ClientSubmittedChatText();
    }
}
