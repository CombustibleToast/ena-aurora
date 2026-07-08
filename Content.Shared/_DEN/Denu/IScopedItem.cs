namespace Content.Shared._DEN.Denu;

public interface IScopedItem
{
    string Id { get; }
    List<int> ProfileIds { get; }
}
