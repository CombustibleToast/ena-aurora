using Robust.Shared.Serialization;

namespace Content.Shared._DEN.Denu;

[Serializable, NetSerializable]
public sealed class DenuModules
{
    public Chat.ChatModule Chat { get; set; } = new();
}
