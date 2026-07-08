using System.Linq;

namespace Content.Shared._DEN.Denu;

public static class ScopeResolver
{
    public static bool IsGlobal<T>(T item) where T : IScopedItem
    {
        return item.ProfileIds.Count == 0;
    }

    public static bool Matches<T>(T item, int currentProfileId) where T : IScopedItem
    {
        return item.ProfileIds.Count == 0 || item.ProfileIds.Contains(currentProfileId);
    }

    public static T? FindGlobal<T>(IReadOnlyList<T> items) where T : IScopedItem
    {
        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (items[i].ProfileIds.Count == 0)
                return items[i];
        }

        return default;
    }

    public static T? FindExact<T>(IReadOnlyList<T> items, int profileId) where T : IScopedItem
    {
        for (int i = items.Count - 1; i >= 0; i--)
        {
            T item = items[i];

            if (item.ProfileIds.Count == 1 && item.ProfileIds[0] == profileId)
                return item;
        }

        return default;
    }

    public static T GetOrCreateGlobal<T>(List<T> items, Func<T> create) where T : class, IScopedItem
    {
        T? existing = FindGlobal(items);
        if (existing != null)
            return existing;

        T created = create();
        created.ProfileIds.Clear();
        items.Add(created);
        return created;
    }

    public static T GetOrCreateExact<T>(List<T> items, int profileId, Func<T> create) where T : class, IScopedItem
    {
        if (profileId <= 0)
            throw new ArgumentOutOfRangeException(nameof(profileId));

        T? existing = FindExact(items, profileId);
        if (existing != null)
            return existing;

        T created = create();
        created.ProfileIds.Clear();
        created.ProfileIds.Add(profileId);
        items.Add(created);
        return created;
    }

    public static void CanonicaliseProfileIds(List<int> profileIds, HashSet<int> validProfileIds)
    {
        HashSet<int> seen = new();

        for (int i = profileIds.Count - 1; i >= 0; i--)
        {
            int profileId = profileIds[i];

            if (!validProfileIds.Contains(profileId) || !seen.Add(profileId))
                profileIds.RemoveAt(i);
        }

        profileIds.Sort();
    }

    public static string ScopeKey(IReadOnlyList<int> profileIds)
    {
        if (profileIds.Count == 0)
            return string.Empty;

        return string.Join(',', profileIds);
    }

    public static T? ResolveExclusive<T>(IReadOnlyList<T> items, int currentProfileId)
        where T : IScopedItem
    {
        T? best = default;
        int bestSpecificity = int.MaxValue;
        int bestIndex = -1;

        for (int i = 0; i < items.Count; i++)
        {
            T item = items[i];

            if (item.ProfileIds.Count != 0 && !item.ProfileIds.Contains(currentProfileId))
                continue;

            int specificity = item.ProfileIds.Count == 0
                ? int.MaxValue
                : item.ProfileIds.Count;

            if (specificity < bestSpecificity || (specificity == bestSpecificity && i > bestIndex))
            {
                best = item;
                bestSpecificity = specificity;
                bestIndex = i;
            }
        }

        return best;
    }

    public static IEnumerable<T> ResolveCollection<T>(IEnumerable<T> items, int currentProfileId)
        where T : IScopedItem
    {
        return items.Where(i => Matches(i, currentProfileId));
    }
}
