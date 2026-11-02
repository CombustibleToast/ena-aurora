using Robust.Shared.Prototypes;

namespace Content.Shared.Radio.Components;

public sealed partial class EncryptionKeyHolderComponent
{
    /// <summary>
    /// Contains a list of channels which should NOT be listened to
    /// </summary>
    [ViewVariables]
    [AutoNetworkedField]
    public HashSet<ProtoId<RadioChannelPrototype>> DisabledChannels = [];
}
