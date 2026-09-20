using System.Linq;
using Robust.Shared.GameStates;

namespace Content.Shared._AS.DeviceLinking.Components;

/// <summary>
/// Component for a signal console, which can control up to 12 channels of signals.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SignalConsoleComponent : Component
{
    public const int MaximumChannels = 12;
    public const int MaximumLabelLength = 32;

    // The labels for each channel, defaulting to "Channel 1", "Channel 2", etc.
    [DataField("labels")]
    public List<string> Labels =
    [
        ..Enumerable.Range(1, MaximumChannels).Select(channel => $"Channel {channel}"),
    ];

    [DataField("linked")]
    public List<bool> States = [..new bool[MaximumChannels]];

    // The mode of each channel, defaulting to Pulse.
    [DataField ("modes")]
    public List<SignalConsoleChannelMode> Modes =
    [
        ..Enumerable.Repeat(SignalConsoleChannelMode.Pulse, MaximumChannels),
    ];
}
