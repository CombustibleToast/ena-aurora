using Robust.Shared.Prototypes;

namespace Content.Shared.Radio.Components;

public sealed partial class IntrinsicRadioTransmitterComponent
{
    /// <summary>
    ///     A list of radio channels that are ReadOnly, you must still include the channel in the channels list.
    /// </summary>
    [ViewVariables]
    public HashSet<ProtoId<RadioChannelPrototype>> ReadOnlyChannels = new();
}
