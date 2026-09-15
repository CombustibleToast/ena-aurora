using Robust.Shared.Serialization;

namespace Content.Shared._AS.AFK;

/// <summary>
/// Notification that its local inactivity state changed. The server will double check this state and may override it if the server disagrees to prevent
/// people from spoofing their inactivity state.
/// </summary>
[Serializable, NetSerializable]
public sealed class ClientInactivityChangedEvent(bool inactive) : EntityEventArgs
{
    public bool Inactive = inactive;
}
