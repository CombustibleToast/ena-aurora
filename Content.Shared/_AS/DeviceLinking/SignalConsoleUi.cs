using Robust.Shared.Serialization;

namespace Content.Shared._AS.DeviceLinking;

// have i ever mentioned how much i hate setting ui keys? because i do.
[Serializable, NetSerializable]
public enum SignalConsoleUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public enum SignalConsoleChannelMode : byte
{
    Pulse,
    Toggle,
}

[Serializable, NetSerializable]
public sealed class SignalConsoleBoundUserInterfaceState(
    List<string> labels,
    List<bool> states,
    List<SignalConsoleChannelMode> modes,
    List<bool> linked) : BoundUserInterfaceState
{
    public List<string> Labels { get; } = labels;
    public List<bool> States { get; } = states;
    public List<SignalConsoleChannelMode> Modes { get; } = modes;
    public List<bool> Linked { get; } = linked;
}

[Serializable, NetSerializable]
public sealed class SignalConsoleToggleMessage(int channel) : BoundUserInterfaceMessage
{
    public int Channel { get; } = channel;
}

[Serializable, NetSerializable]
public sealed class SignalConsolePulseMessage(int channel) : BoundUserInterfaceMessage
{
    public int Channel { get; } = channel;
}

[Serializable, NetSerializable]
public sealed class SignalConsoleRenameMessage(int channel, string label) : BoundUserInterfaceMessage
{
    public int Channel { get; } = channel;
    public string Label { get; } = label;
}

[Serializable, NetSerializable]
public sealed class SignalConsoleSetModeMessage(int channel, SignalConsoleChannelMode mode) : BoundUserInterfaceMessage
{
    public int Channel { get; } = channel;
    public SignalConsoleChannelMode Mode { get; } = mode;
}
