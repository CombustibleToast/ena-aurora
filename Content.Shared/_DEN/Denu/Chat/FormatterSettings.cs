using Robust.Shared.Serialization;

namespace Content.Shared._DEN.Denu.Chat;

[Serializable, NetSerializable]
public sealed class FormatterSettings : IScopedItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public List<int> ProfileIds { get; set; } = new();
    public bool Enabled { get; set; } = true;
    public bool RemoveAsterisks { get; set; }
    public string DialogueColor { get; set; } = "#FFFFFF";
    public string EmoteColor { get; set; } = "#FF13FF";
}
