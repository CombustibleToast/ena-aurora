using Robust.Shared.GameStates;

namespace Content.Shared._AS.StatusEffects;

/// <summary>
/// Marks the status effect that sustains baseline Mnemolith visuals between metabolism ticks.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MnemolithStatusEffectComponent : Component;
