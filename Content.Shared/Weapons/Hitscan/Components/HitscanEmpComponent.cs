using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Hitscan.Components;

/// <summary>
/// Hitscan entities that have this component will cause an EMP detonation to the target.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HitscanEmpComponent : Component
{
    /// <summary>
    /// EMP range.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Range = 1.0f;

    /// <summary>
    /// How much energy (in Joules) will be consumed per battery in range.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float EnergyConsumption;

    /// <summary>
    /// How long it disables targets.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan DisableDuration = TimeSpan.FromSeconds(60);
}
