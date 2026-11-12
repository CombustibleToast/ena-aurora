namespace Content.Shared._RMC14.Weapons.Common;

// Aurora's Song Start - Allows rejecting a Unique Action before its handlers run. (RMC style pump/breech)
public sealed class CatchUniqueActionEvent(EntityUid userUid) : CancellableEntityEventArgs
{
    public readonly EntityUid UserUid = userUid;
}
// Aurora's Song End

public sealed class UniqueActionEvent(EntityUid userUid) : HandledEntityEventArgs
{
    public readonly EntityUid UserUid = userUid;
}
