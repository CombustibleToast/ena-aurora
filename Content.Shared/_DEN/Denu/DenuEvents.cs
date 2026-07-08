using Robust.Shared.Serialization;

namespace Content.Shared._DEN.Denu;

[Serializable, NetSerializable]
public sealed class DenuSettingsSyncEvent : EntityEventArgs
{
    public DenuSettingsRoot Settings { get; init; } = new();
    public int CurrentProfileId { get; init; }
}

[Serializable, NetSerializable]
public sealed class DenuSettingsSaveRequest : EntityEventArgs
{
    public DenuSettingsRoot Settings { get; init; } = new();
}

[Serializable, NetSerializable]
public sealed class DenuSettingsSaveResponse : EntityEventArgs
{
    public bool Success { get; init; }
    public DenuSettingsRoot Settings { get; init; } = new();
    public int CurrentProfileId { get; init; }
    public string? ErrorMessage { get; init; }
}
