using System.Linq;
using Robust.Shared.Prototypes;

namespace Content.Shared._AS.ChameleonDecorations;

public abstract partial class SharedChameleonDecorationSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _prototypes = null!;
    [Dependency] private MetaDataSystem _metaData = null!;

    protected void ApplyAppearance(Entity<ChameleonDecorationComponent> ent)
    {
        if (!_prototypes.TryIndex(ent.Comp.Selected, out var prototype) || !IsValidTarget(prototype, ent.Comp.Group))
            return;

        var metadata = MetaData(ent.Owner);
        _metaData.SetEntityName(ent.Owner, prototype.Name, metadata);
        _metaData.SetEntityDescription(ent.Owner, prototype.Description, metadata);
        ApplySprite(ent.Owner, prototype);
    }

    public bool IsValidTarget(EntityPrototype prototype, ProtoId<ChameleonDecorationGroupPrototype> groupId)
    {
        return !prototype.Abstract && _prototypes.TryIndex(groupId, out var group) && group.Prototypes.Contains(prototype.ID);
    }

    public IEnumerable<EntProtoId> GetTargets(ProtoId<ChameleonDecorationGroupPrototype> groupId)
    {
        if (!_prototypes.TryIndex(groupId, out var group))
            yield break;

        // We order by given prefixes into groups (mostly for stuff from different repos)
        // Then order alphabetically
        var ordered = group.Prototypes
            .OrderBy(id => GetPrefixIndex(id.Id, group.PrefixOrder))
            .ThenBy(id => GetPrefixSuffix(id.Id, group.PrefixOrder), StringComparer.OrdinalIgnoreCase);

        foreach (var id in ordered)
        {
            if (_prototypes.TryIndex(id, out var prototype) && IsValidTarget(prototype, groupId))
                yield return id;
        }
    }

    private static int GetPrefixIndex(string id, List<string> prefixes)
    {
        for (var i = 0; i < prefixes.Count; i++)
        {
            if (id.StartsWith(prefixes[i], StringComparison.Ordinal))
                return i;
        }

        return prefixes.Count;
    }

    private static string GetPrefixSuffix(string id, List<string> prefixes)
    {
        foreach (var prefix in prefixes)
            if (id.StartsWith(prefix, StringComparison.Ordinal))
                return id[prefix.Length..];

        return id;
    }

    protected virtual void ApplySprite(EntityUid uid, EntityPrototype prototype)
    {
    }
}
