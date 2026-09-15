using Robust.Shared.Serialization;

namespace Content.Shared._DEN.Denu.Chat;

[Serializable, NetSerializable]
public sealed class EarmuffRangeModule
{
    public List<EarmuffRangeSettings> Items { get; set; } = new();
}
