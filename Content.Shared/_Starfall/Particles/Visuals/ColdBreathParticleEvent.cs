using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._Starfall.Particles.Visuals;

/// <summary>
/// Network event raised by the server when an entity exhales in a cold pressurized environment.
/// </summary>
[Serializable, NetSerializable]
public sealed class ColdBreathParticleEvent(MapCoordinates coords, NetEntity entity) : EntityEventArgs
{
    public MapCoordinates Coords = coords;
    public NetEntity Entity = entity;
}
