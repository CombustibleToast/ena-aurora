using Content.Shared.Radio;

namespace Content.Server.Radio;

public partial record struct RadioSendAttemptEvent
{
    // Aurora's Song - includes MessageSource to allow for radio bans.
    public RadioSendAttemptEvent(RadioChannelPrototype channel, EntityUid radioSource, EntityUid messageSource) : this(channel, radioSource)
    {
        MessageSource = messageSource;
    }
}
