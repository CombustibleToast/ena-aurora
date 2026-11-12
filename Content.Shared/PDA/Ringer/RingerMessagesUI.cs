using Robust.Shared.Serialization;

namespace Content.Shared.PDA.Ringer;

[Serializable, NetSerializable]
public sealed class RingerPlayRingtoneMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class RingerSetRingtoneMessage : BoundUserInterfaceMessage
{
    public Note[] Ringtone { get; }
      public float Volume { get; } // Aurora's Song

    public RingerSetRingtoneMessage(Note[] ringTone, float volume) // Aurora's Song - Add volume
    {
        Ringtone = ringTone;
        Volume = volume; // Aurora's Song
    }
}
