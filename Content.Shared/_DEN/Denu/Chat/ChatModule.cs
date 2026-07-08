using Robust.Shared.Serialization;

namespace Content.Shared._DEN.Denu.Chat;

[Serializable, NetSerializable]
public sealed class ChatModule
{
    public FormatterModule Formatter { get; set; } = new();
    public EarmuffRangeModule EarmuffRange { get; set; } = new();
}
