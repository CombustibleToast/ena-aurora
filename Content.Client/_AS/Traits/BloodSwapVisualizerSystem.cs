using Content.Client.Damage;
using Content.Shared._AS.Traits;
using Content.Shared.Chemistry.Reagent;
using Robust.Client.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Client._AS.Traits;

/// <summary>
/// Colors brute damage visuals to match the blood reagent selected by a blood-swap trait.
/// </summary>
public sealed partial class BloodSwapVisualizerSystem : EntitySystem
{
    private const string BruteDamageGroup = "Brute";

    [Dependency] private IPrototypeManager _prototypeManager = null!;
    [Dependency] private SpriteSystem _sprite = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodSwapComponent, ComponentStartup>(OnBloodSwapStartup);
        SubscribeLocalEvent<BloodSwapComponent, AfterAutoHandleStateEvent>(OnBloodSwapHandleState);
    }

    private void OnBloodSwapStartup(Entity<BloodSwapComponent> ent, ref ComponentStartup args)
    {
        UpdateBruteOverlayColor(ent);
    }

    private void OnBloodSwapHandleState(Entity<BloodSwapComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        UpdateBruteOverlayColor(ent);
    }

    private void UpdateBruteOverlayColor(Entity<BloodSwapComponent> ent)
    {
        if (ent.Comp.BloodReagent == null ||
            !_prototypeManager.TryIndex<ReagentPrototype>(ent.Comp.BloodReagent, out var reagent) ||
            !TryComp<SpriteComponent>(ent, out var sprite) ||
            !TryComp<DamageVisualsComponent>(ent, out var damageVisuals) ||
            damageVisuals.DamageOverlayGroups?.ContainsKey(BruteDamageGroup) != true)
            return;

        if (damageVisuals.TargetLayerMapKeys.Count == 0)
        {
            SetLayerColor((ent, sprite), $"DamageOverlay{BruteDamageGroup}", reagent.SubstanceColor);
            return;
        }

        foreach (var targetLayer in damageVisuals.TargetLayerMapKeys)
        {
            SetLayerColor((ent, sprite), $"{targetLayer}{BruteDamageGroup}", reagent.SubstanceColor);
        }
    }

    private void SetLayerColor(Entity<SpriteComponent> ent, string layerKey, Color color)
    {
        if (_sprite.LayerMapTryGet(ent.AsNullable(), layerKey, out var layer, false))
            _sprite.LayerSetColor(ent.AsNullable(), layer, color);
    }
}
