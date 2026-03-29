namespace Content.Shared.Toggleable;

/// <summary>
/// Holds the state for whether the undergarment (top and bottom) are hidden.
/// </summary>
[RegisterComponent]
public sealed partial class ToggleUndergarmentComponent : Component
{
    [DataField]
    public bool UndergarmentTopEnabled = true;

    [DataField]
    public bool UndergarmentBottomEnabled = true;

    [DataField]
    public EntityUid? ToggleTopActionEntity;

    [DataField]
    public EntityUid? ToggleBottomActionEntity;
}
