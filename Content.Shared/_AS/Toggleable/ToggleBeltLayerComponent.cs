using Robust.Shared.GameStates;

namespace Content.Shared.Toggleable;

/// <summary>
/// Holds the state and action entity for toggling belt visuals above or below outer clothing.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
public sealed partial class ToggleBeltLayerComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool BeltOverOuterClothing = true;

    [DataField]
    public EntityUid? ToggleActionEntity;
}
