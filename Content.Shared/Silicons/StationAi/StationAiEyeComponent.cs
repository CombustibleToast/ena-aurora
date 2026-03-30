using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Silicons.StationAi;

/// <summary>
/// AS: Indicates this is the invisbile eye entity of a station AI core.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StationAiEyeComponent : Component
{

    /// <summary>
    /// The AI Core this belongs too.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? CoreEntity;


}
