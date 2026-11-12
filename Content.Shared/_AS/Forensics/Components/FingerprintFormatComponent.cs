using Robust.Shared.GameStates;

namespace Content.Shared._AS.Forensics.Components;

/// <summary>
/// Gives an entity's forensic fingerprint a different display format.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FingerprintFormatComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public FingerprintFormat Format;
}

public enum FingerprintFormat : byte
{
    HardwareId, // Silicon/IPC Fingerprints
}
