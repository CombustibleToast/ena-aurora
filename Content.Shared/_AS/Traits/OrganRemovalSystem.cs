using System.Reflection.Metadata;
using Content.Shared.Body;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;

namespace Content.Shared._AS.Traits;

public sealed partial class OrganRemovalSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<OrganRemovalComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<OrganRemovalComponent> entity, ref ComponentStartup args)
    {
        if (!TryComp<BodyComponent>(entity, out var bodyComp) || bodyComp.Organs == null)
            return;

        foreach(var targetOrgan in bodyComp.Organs.ContainedEntities)
        {  
            var Organ = ;
            if (Organ.Prototype.Name == "Head")
            {
                QueueDel(targetOrgan);
            }
        }
    }
}