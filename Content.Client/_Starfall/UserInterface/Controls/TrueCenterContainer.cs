using System.Numerics;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._Starfall.UserInterface.Controls;

/// <summary>
/// What if <see cref="CenterContainer"/> wasn't complete ass.
/// properly centers children and doesn't shrink to the size of the largest child.
/// </summary>
public sealed class TrueCenterContainer : Container
{
    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        var desiredSize = Vector2.Zero;

        foreach (var child in Children)
        {
            child.Measure(availableSize);
            desiredSize = Vector2.Max(desiredSize, child.DesiredSize);
        }

        return desiredSize;
    }

    protected override Vector2 ArrangeOverride(Vector2 finalSize)
    {
        foreach (var child in Children)
        {
            var childSize = Vector2.Min(child.DesiredSize, finalSize);
            var childPosition = Vector2.Max((finalSize - childSize) / 2f, Vector2.Zero);
            child.Arrange(UIBox2.FromDimensions(childPosition, childSize));
        }

        return finalSize;
    }
}
