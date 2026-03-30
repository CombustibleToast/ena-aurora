using System.ComponentModel.DataAnnotations;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;

namespace Content.Shared._AS.License;

[RegisterComponent]
public sealed partial class LicenseComponent : Component
{
    [DataField]
    public LocId NoOwnerLoc = "license-name-no-owner";

    [DataField]
    public LocId OwnerLoc = "license-name-owner";

    [DataField]
    public string LicenseName = string.Empty;

    [DataField]
    public string? OwnerName;

    [DataField]
    public ProtoId<SecurityIconPrototype> LicenseStatusIcon = "LicenseIcon";
}
