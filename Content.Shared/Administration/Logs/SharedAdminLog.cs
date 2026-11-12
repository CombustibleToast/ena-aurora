using Content.Shared.Database;
using Robust.Shared.Serialization;

namespace Content.Shared.Administration.Logs;

[Serializable, NetSerializable]
// Aurora's Song - Make partial
public readonly partial record struct SharedAdminLog(
    int Id,
    LogType Type,
    LogImpact Impact,
    DateTime Date,
    string Message,
    Guid[] Players);
