namespace Content.Shared.Toggleable;

/// <summary>
/// This is used for...
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
