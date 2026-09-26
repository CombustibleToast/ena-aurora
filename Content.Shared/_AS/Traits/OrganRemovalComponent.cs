using Robust.Shared.Prototypes;

namespace Content.Shared._AS.Traits;

/// <summary>
/// Removes a list of organs from the container in the Body component upon MapInit.
/// </summary>
[RegisterComponent]
public sealed partial class OrganRemovalComponent : Component
{
    /// <summary>
    /// Organs to remove.
    /// </summary>
    [DataField(required : true)]
    public List<EntProtoId> Organs = new();
};
