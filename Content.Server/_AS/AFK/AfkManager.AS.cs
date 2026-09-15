using Robust.Shared.Player;

// This is intentional to extend the existing AfkManager without changing upstream files
namespace Content.Server.Afk;

public sealed partial class AfkManager
{
    /// <summary>
    /// Checks the existing server-observed activity timestamp against a caller-provided duration.
    /// </summary>
    public bool IsInactiveFor(ICommonSession player, TimeSpan duration)
    {
        return _lastActionTimes.TryGetValue(player, out var lastAction) &&
               _gameTiming.RealTime - lastAction >= duration;
    }
}
