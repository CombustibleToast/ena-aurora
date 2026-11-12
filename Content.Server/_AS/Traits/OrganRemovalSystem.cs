using Content.Shared.Body;
using Robust.Shared.Prototypes;

namespace Content.Server._AS.Traits;

/// <summary>
/// Deletes an organ based on the given categories.
/// <seealso cref="OrganRemovalComponent" />
/// </summary>
public sealed partial class OrganRemovalSystem : EntitySystem
{
    [Dependency] private BodySystem _bodySystem = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<OrganRemovalComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<OrganComponent, BodyRelayedEvent<FindOrgansToRemoveEvent>>(OnFindOrgan);
    }

    private void OnStartup(Entity<OrganRemovalComponent> entity, ref ComponentStartup args)
    {
        if (!TryComp<BodyComponent>(entity, out var body))
            return;

        var ev = new FindOrgansToRemoveEvent(entity.Comp.Organs);
        _bodySystem.RelayEvent((entity.Owner, body), ev);

        foreach (var organ in ev.MatchingOrgans)
            QueueDel(organ);
    }

    private static void OnFindOrgan(Entity<OrganComponent> organ, ref BodyRelayedEvent<FindOrgansToRemoveEvent> args)
    {
        if (organ.Comp.Category is not { } category || !args.Args.Categories.Contains(category))
            return;

        args.Args.MatchingOrgans.Add(organ.Owner);
    }

    private sealed class FindOrgansToRemoveEvent(HashSet<ProtoId<OrganCategoryPrototype>> categories)
    {
        public readonly HashSet<ProtoId<OrganCategoryPrototype>> Categories = categories;
        public readonly HashSet<EntityUid> MatchingOrgans = [];
    }
}
