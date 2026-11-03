using Robust.Shared.GameStates;

namespace Content.Shared._AS.Forensics.Components;

/// <summary>
/// Gives an entity's forensic DNA a different display format.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DnaFormatComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public DnaFormat Format;
}

public enum DnaFormat : byte
{
    MacAddress, // Silicon/IPC DNA
}
