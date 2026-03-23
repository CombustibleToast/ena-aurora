using Content.Server.Actions;
using Content.Shared.Toggleable;
using Robust.Shared.Prototypes;

namespace Content.Server._AS.Togglable;

/// <summary>
/// This handles...
/// </summary>
public sealed class ToggleUndergarmentSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;

    private static readonly EntProtoId ToggleTopAction = "ActionToggleUndergarmentTop";
    private static readonly EntProtoId ToggleBottomAction = "ActionToggleUndergarmentBottom";

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ToggleUndergarmentComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<ToggleUndergarmentComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, ref ent.Comp.ToggleTopActionEntity, ToggleTopAction);
        _actions.AddAction(ent, ref ent.Comp.ToggleBottomActionEntity, ToggleBottomAction);
    }
}
