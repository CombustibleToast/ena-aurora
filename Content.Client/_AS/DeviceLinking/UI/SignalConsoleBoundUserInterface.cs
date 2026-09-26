using Content.Shared._AS.DeviceLinking;
using Robust.Client.UserInterface;

namespace Content.Client._AS.DeviceLinking.UI;

public sealed class SignalConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private SignalConsoleWindow? _window;

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<SignalConsoleWindow>();
        _window.OnToggle += channel => SendMessage(new SignalConsoleToggleMessage(channel));
        _window.OnPulse += channel => SendMessage(new SignalConsolePulseMessage(channel));
        _window.OnRename += (channel, label) => SendMessage(new SignalConsoleRenameMessage(channel, label));
        _window.OnModeChanged += (channel, mode) => SendMessage(new SignalConsoleSetModeMessage(channel, mode));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (_window != null && state is SignalConsoleBoundUserInterfaceState consoleState)
            _window.UpdateState(consoleState);
    }
}
