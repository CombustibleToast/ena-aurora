using System.Globalization;

namespace Content.Client.PDA;

// Local shift time tooltips
public sealed partial class PdaMenu
{
    private void UpdateLocalShiftTimeTooltips(TimeSpan stationTime)
    {
        StationTimeButton.ToolTip = Loc.GetString("comp-pda-ui-shift-start-time-local", ("date", GetLocalTime(-stationTime)));

        if (_roundEndTime is not { } roundEndTime)
        {
            RemainingTimeButton.ToolTip = null;
            return;
        }

        var remainingTime = roundEndTime.Subtract(_gameTiming.CurTime);
        RemainingTimeButton.ToolTip = Loc.GetString("comp-pda-ui-shift-end-time-local", ("date", GetLocalTime(remainingTime)));
    }

    private static string GetLocalTime(TimeSpan offset)
    {
        var localTime = DateTime.Now.Add(offset);
        return localTime.ToString("f", CultureInfo.CurrentCulture);
    }
}
