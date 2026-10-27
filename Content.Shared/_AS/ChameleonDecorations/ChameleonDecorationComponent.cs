using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._AS.ChameleonDecorations;

/// <summary>
/// Allows a decoration to copy the name, description, and sprite of another decoration.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class ChameleonDecorationComponent : Component
{
    [DataField(required: true)]
    public ProtoId<ChameleonDecorationGroupPrototype> Group;

    [DataField(required: true), AutoNetworkedField]
    public EntProtoId Selected;
}

[Serializable, NetSerializable]
public enum ChameleonDecorationUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class ChameleonDecorationUiState(
    ProtoId<ChameleonDecorationGroupPrototype> group,
    EntProtoId selected) : BoundUserInterfaceState
{
    public readonly ProtoId<ChameleonDecorationGroupPrototype> Group = group;
    public readonly EntProtoId Selected = selected;
}

[Serializable, NetSerializable]
public sealed class ChameleonDecorationSelectMessage(EntProtoId selected) : BoundUserInterfaceMessage
{
    public readonly EntProtoId Selected = selected;
}
