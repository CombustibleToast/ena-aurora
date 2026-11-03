using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;

namespace Content.Shared.Chat;

public sealed partial class SharedSuicideSystem
{
    [Dependency] private SharedStaminaSystem _stamina = default!;

    /// <summary>
    /// Tops up stamina damage to the target's stamina crit threshold, bypassing resistance.
    /// </summary>
    public void ApplyStaminaCritDamage(Entity<StaminaComponent> target, EntityUid? source = null, EntityUid? with = null)
    {
        if (target.Comp.Critical || _stamina.UniversalStaminaDamageModifier <= 0f)
            return;

        var remaining = MathF.Max(0f, target.Comp.CritThreshold - target.Comp.StaminaDamage);
        _stamina.TakeStaminaDamage(target.Owner,
            MathF.Ceiling(remaining / _stamina.UniversalStaminaDamageModifier),
            target.Comp,
            source: source,
            with: with,
            ignoreResist: true);
    }
}
