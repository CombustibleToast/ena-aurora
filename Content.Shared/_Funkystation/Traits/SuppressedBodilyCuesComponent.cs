using Robust.Shared.GameStates;

namespace Content.Shared._Funkystation.Traits;

/// <summary>
/// Suppresses minor hunger and thirst alerts for the Suppressed Bodily Cues trait
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SuppressedBodilyCuesComponent : Component;
