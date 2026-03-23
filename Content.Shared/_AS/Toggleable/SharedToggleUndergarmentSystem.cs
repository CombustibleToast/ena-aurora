using Content.Shared.Humanoid;

namespace Content.Shared.Toggleable;

/// <summary>
/// This handles...
/// </summary>
public sealed class SharedToggleUndergarmentSystem : EntitySystem
{
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoid = default!;

    private static readonly HumanoidVisualLayers UndergarmentTop = HumanoidVisualLayers.UndergarmentTop;
    private static readonly HumanoidVisualLayers UndergarmentBottom = HumanoidVisualLayers.UndergarmentBottom;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ToggleUndergarmentComponent, ToggleUndergarmentTopActionEvent>(OnToggleTop);
        SubscribeLocalEvent<ToggleUndergarmentComponent, ToggleUndergarmentBottomActionEvent>(OnToggleBottom);
    }

    private void OnToggleTop(Entity<ToggleUndergarmentComponent> ent, ref ToggleUndergarmentTopActionEvent _)
    {
        ent.Comp.UndergarmentTopEnabled = !ent.Comp.UndergarmentTopEnabled;

        _humanoid.SetLayerVisibility(ent.Owner, UndergarmentTop, ent.Comp.UndergarmentTopEnabled);
    }

    private void OnToggleBottom(Entity<ToggleUndergarmentComponent> ent, ref ToggleUndergarmentBottomActionEvent _)
    {
        ent.Comp.UndergarmentBottomEnabled = !ent.Comp.UndergarmentBottomEnabled;

        _humanoid.SetLayerVisibility(ent.Owner, UndergarmentBottom, ent.Comp.UndergarmentTopEnabled);
    }
}
