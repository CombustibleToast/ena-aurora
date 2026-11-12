using Robust.Shared.Prototypes;

namespace Content.Shared.Radio.Components;

public sealed partial class ActiveRadioComponent
{
    /// <summary>
    /// Contains a list of channels which should NOT be listened to
    /// </summary>
    [ViewVariables]
    public HashSet<ProtoId<RadioChannelPrototype>> DisabledChannels = [];
}
