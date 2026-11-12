using Content.Shared.CartridgeLoader;
using Robust.Shared.Serialization;

namespace Content.Shared._WF.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class CriticalImplantTrackerUiState : BoundUserInterfaceState
{
    public List<CriticalPatientData> Patients { get; }
    public bool Muted { get; } // Aurora's Song

    public CriticalImplantTrackerUiState(List<CriticalPatientData> patients, bool muted) // Aurora's Song - Add muted
    {
        Patients = patients;
        Muted = muted; // Aurora's Song
    }
}

[Serializable, NetSerializable]
public sealed class CriticalPatientData
{
    public string Name { get; }
    public string Coordinates { get; }
    public string Species { get; }
    public string TimeSinceCrit { get; }
    public bool IsDead { get; }
    public bool IsSpaceSleepDisorder { get; }

    public CriticalPatientData(string name, string coordinates, string species, string timeSinceCrit, bool isDead, bool isSpaceSleepDisorder)
    {
        Name = name;
        Coordinates = coordinates;
        Species = species;
        TimeSinceCrit = timeSinceCrit;
        IsDead = isDead;
        IsSpaceSleepDisorder = isSpaceSleepDisorder;
    }
}

[Serializable, NetSerializable]
public sealed class CriticalImplantTrackerRefreshMessage : CartridgeMessageEvent
{
}
// Aurora's Song Start
[Serializable, NetSerializable]
public sealed class CriticalImplantTrackerMuteMessage : CartridgeMessageEvent
{
}
// Aurora's Song End
