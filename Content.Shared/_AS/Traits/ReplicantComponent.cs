using Robust.Shared.GameStates;

namespace Content.Shared._AS.Traits;

/// <summary>
/// Set player blood to Oxidant and chagnes their typing indicator to "Robot"
/// Used for Replicant trait.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(ReplicantSystem))]
public sealed partial class ReplicantComponent : Component
{
}