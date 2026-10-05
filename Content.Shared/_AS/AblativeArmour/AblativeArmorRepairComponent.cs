using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._AS.Armor;

/// <summary>
/// A component applied to an entity wearing Ablative armour that can be repaired externally that contains a hashset of tracked entities to relay repair attempts to.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AblativeArmorRepairComponent : Component
{
    [DataField, AutoNetworkedField]
    public HashSet<EntityUid> Recipients = new();
}
