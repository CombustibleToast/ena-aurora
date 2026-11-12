using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared.Administration.Logs;

public readonly partial record struct SharedAdminLog
{
    /// <summary>
    /// The message contains private chat and was redacted by the server.
    /// </summary>
    public bool Sensitive { get; init; }
}

public static partial class AdminLogsEuiMsg
{
    [Serializable, NetSerializable]
    public sealed class RevealSensitiveLogRequest : EuiMessageBase
    {
        public RevealSensitiveLogRequest(int logId, string reason)
        {
            LogId = logId;
            Reason = reason;
        }

        public int LogId { get; }
        public string Reason { get; }
    }

    [Serializable, NetSerializable]
    public sealed class RevealedSensitiveLog : EuiMessageBase
    {
        public RevealedSensitiveLog(SharedAdminLog log)
        {
            Log = log;
        }

        public SharedAdminLog Log { get; }
    }
}
