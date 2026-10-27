using Robust.Shared.Prototypes;

namespace Content.Shared._AS.ChameleonDecorations;

/// <summary>
/// A list of allowed selections for chameleon decor kind of like how spray paint groups work
/// </summary>
[Prototype]
public sealed partial class ChameleonDecorationGroupPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = null!;

    [DataField(required: true)]
    public List<EntProtoId> Prototypes = [];

    /// <summary>
    /// Orders based on prefix
    /// </summary>
    [DataField]
    public List<string> PrefixOrder = [];
}
