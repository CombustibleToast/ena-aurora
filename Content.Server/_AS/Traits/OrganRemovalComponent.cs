using Content.Shared.Body;
using Robust.Shared.Prototypes;

namespace Content.Server._AS.Traits;

/// <summary>
/// Removes organs in the categories when this component starts.
/// </summary>
[RegisterComponent]
public sealed partial class OrganRemovalComponent : Component
{
    [DataField(required: true)]
    public HashSet<ProtoId<OrganCategoryPrototype>> Organs = [];
}
