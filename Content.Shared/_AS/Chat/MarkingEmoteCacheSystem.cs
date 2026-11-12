using System.Linq;
using Content.Shared.Body;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._AS.Chat;

/// <summary>
/// Caches a body's marking grants until its markings, organs, species, or prototypes change.
/// </summary>
public sealed partial class MarkingEmoteCacheSystem : EntitySystem
{
    [Dependency] private SharedVisualBodySystem _visualBody = null!;
    [Dependency] private IPrototypeManager ProtoMan = null!;

    private readonly Dictionary<EntityUid, CachedGrants> _cache = new();
    private EmotePrototype[] _emotesWithMarkingGrants = [];

    public override void Initialize()
    {
        base.Initialize();
        CacheEmoteRules();

        // Applying/copying markings are separate paths, so we account for both.
        SubscribeLocalEvent<VisualBodyComponent, ApplyOrganMarkingsEvent>(InvalidateBody, after: [typeof(BodySystem)]);
        SubscribeLocalEvent<VisualBodyComponent, OrganCopyAppearanceEvent>(InvalidateBody, after: [typeof(BodySystem)]);
        SubscribeLocalEvent<VisualBodyComponent, OrganInsertedIntoEvent>(InvalidateBody);
        SubscribeLocalEvent<VisualBodyComponent, OrganRemovedFromEvent>(InvalidateBody);
        SubscribeLocalEvent<VisualBodyComponent, ComponentShutdown>(InvalidateBody);

        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypeReload);
    }

    public override void Shutdown()
    {
        _cache.Clear();
        base.Shutdown();
    }

    /// <summary>
    /// Builds all marking grants on the first lookup. A granted emote can have no custom sound.
    /// </summary>
    public bool TryGetGrant(EntityUid source, ProtoId<EmotePrototype> emote, out ProtoId<EmoteSoundsPrototype>? sounds)
    {
        sounds = null;
        if (!HasComp<VisualBodyComponent>(source))
            return false;

        ProtoId<SpeciesPrototype>? species = null;
        if (TryComp<HumanoidProfileComponent>(source, out var profile))
            species = profile.Species;
        // Check species here too, since it can change without changing the markings, extremely unlikely in regular gameplay but imagine polymorph nonsense.
        if (_cache.TryGetValue(source, out var cached) && cached.Species == species)
            return cached.Grants.TryGetValue(emote, out sounds);

        if (!_visualBody.TryGatherMarkingsData(source, null, out _, out _, out var applied))
            return false;

        cached = new CachedGrants(species, BuildGrants(species, applied.Values));
        _cache[source] = cached;

        return cached.Grants.TryGetValue(emote, out sounds);
    }

    private Dictionary<ProtoId<EmotePrototype>, ProtoId<EmoteSoundsPrototype>?> BuildGrants(
        ProtoId<SpeciesPrototype>? species,
        IEnumerable<Dictionary<HumanoidVisualLayers, List<Marking>>> organs)
    {
        var grants = new Dictionary<ProtoId<EmotePrototype>, ProtoId<EmoteSoundsPrototype>?>();
        var categories = new HashSet<HumanoidVisualLayers>();
        var markings = new HashSet<ProtoId<MarkingPrototype>>();
        foreach (var organ in organs)
        {
            foreach (var (category, applied) in organ)
            {
                if (applied.Count == 0)
                    continue;

                categories.Add(category);
                foreach (var marking in applied)
                    markings.Add(marking.MarkingId);
            }
        }

        // Conflicting grants (like if two markings give one emote) will go by ID to see which takes priority.
        foreach (var marking in markings.OrderBy(id => id.Id, StringComparer.Ordinal))
        {
            if (!ProtoMan.TryIndex(marking, out var prototype))
                continue;

            foreach (var rule in prototype.EmoteGrants.Where(rule => MatchesSpecies(rule.Species, species)))
            {
                grants.TryAdd(rule.Emote, rule.Sounds);
            }
        }

        foreach (var emote in _emotesWithMarkingGrants)
        {
            if (grants.ContainsKey(emote.ID))
                continue;

            foreach (var rule in emote.MarkingGrants.Where(rule => MatchesSpecies(rule.Species, species) && (rule.Categories.Overlaps(categories) || rule.Markings.Overlaps(markings))))
            {
                grants.Add(emote.ID, rule.Sounds);
                break;
            }
        }

        return grants;
    }

    private static bool MatchesSpecies(HashSet<ProtoId<SpeciesPrototype>> whitelist, ProtoId<SpeciesPrototype>? species)
    {
        return whitelist.Count == 0 || (species is { } id && whitelist.Contains(id));
    }

    private void InvalidateBody<TEvent>(Entity<VisualBodyComponent> ent, ref TEvent args)
    {
        _cache.Remove(ent.Owner);
    }

    /// <summary>
    /// Clears the body's cache.
    /// </summary>
    public void InvalidateOrgan(EntityUid organ)
    {
        if (TryComp<OrganComponent>(organ, out var component) && component.Body is { } body)
            _cache.Remove(body);
    }

    private void OnPrototypeReload(PrototypesReloadedEventArgs args)
    {
        if (args.WasModified<EmotePrototype>())
            CacheEmoteRules();

        if (args.WasModified<MarkingPrototype>() || args.WasModified<EmotePrototype>())
            _cache.Clear();
    }

    private void CacheEmoteRules()
    {
        _emotesWithMarkingGrants = ProtoMan.EnumeratePrototypes<EmotePrototype>()
            .Where(emote => emote.MarkingGrants.Count > 0)
            .ToArray();
    }

    private sealed record CachedGrants(ProtoId<SpeciesPrototype>? Species, Dictionary<ProtoId<EmotePrototype>, ProtoId<EmoteSoundsPrototype>?> Grants);
}
