using Robust.Shared.Serialization;

namespace Content.Shared._DEN.Denu.Chat;

[Serializable, NetSerializable]
public sealed class EarmuffRangeSettings : IScopedItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public List<int> ProfileIds { get; set; } = new();
    public float Value { get; set; } = 10.0f;
}
