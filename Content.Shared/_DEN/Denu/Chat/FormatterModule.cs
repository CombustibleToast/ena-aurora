using Robust.Shared.Serialization;

namespace Content.Shared._DEN.Denu.Chat;

[Serializable, NetSerializable]
public sealed class FormatterModule
{
    public List<FormatterSettings> Items { get; set; } = new();
}
