using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._AS.Traits;

/// <summary>
/// Schedules recurring sleep and migraine episodes while hunger or thirst is critically low for the Metabolic Crashes trait.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
[Access(typeof(MetabolicCrashesSchedulerSystem))]
public sealed partial class MetabolicCrashesSchedulerComponent : Component
{
    [DataField, AutoNetworkedField]
    public float HungerCrashThreshold = 0f;

    [DataField, AutoNetworkedField]
    public float ThirstCrashThreshold = 0f;

    [DataField, AutoNetworkedField]
    public TimeSpan MinTimeBetweenCrashes = TimeSpan.FromMinutes(5);

    [DataField, AutoNetworkedField]
    public TimeSpan MaxTimeBetweenCrashes = TimeSpan.FromMinutes(30);

    [DataField, AutoNetworkedField]
    public TimeSpan MinSleepDuration = TimeSpan.FromSeconds(10);

    [DataField, AutoNetworkedField]
    public TimeSpan MaxSleepDuration = TimeSpan.FromSeconds(20);

    [DataField, AutoNetworkedField]
    public TimeSpan MinMigraineDuration = TimeSpan.FromSeconds(10);

    [DataField, AutoNetworkedField]
    public TimeSpan MaxMigraineDuration = TimeSpan.FromSeconds(20);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan NextHungerCrash;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan NextThirstCrash;
}
