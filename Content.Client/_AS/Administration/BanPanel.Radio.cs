using System.Linq;
using Content.Shared.Radio;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;

namespace Content.Client.Administration.UI.BanPanel;

/// <summary>
/// Radio Ban UI
/// </summary>
public sealed partial class BanPanel
{
    private readonly Dictionary<ProtoId<RadioChannelPrototype>, CheckBox> _radioChannels = new();

    private void InitializeRadioBanControls()
    {
        foreach (var channel in _protoMan.EnumeratePrototypes<RadioChannelPrototype>().OrderBy(x => x.ID))
        {
            var enabled = new CheckBox
            {
                Text = channel.LocalizedName,
                HorizontalExpand = true,
                Modulate = channel.Color,
                Margin = new Thickness(4),
            };

            RadioChannelsContainer.AddChild(enabled);
            _radioChannels.Add(channel.ID, enabled);
        }
    }

    private void UpdateRadioBanTab()
    {
        Tabs.SetTabVisible((int)TabNumbers.Radio, TypeOption.SelectedId == (int)Types.Radio);
        Tabs.CurrentTab = TypeOption.SelectedId switch
        {
            (int)Types.Role => (int)TabNumbers.Roles,
            (int)Types.Radio => (int)TabNumbers.Radio,
            _ => (int)TabNumbers.BasicInfo,
        };
    }

    private bool TryGetRadioBanChannels(out ProtoId<RadioChannelPrototype>[]? channels)
    {
        channels = null;
        if (TypeOption.SelectedId != (int)Types.Radio)
            return true;

        channels = _radioChannels
            .Where(pair => pair.Value.Pressed)
            .Select(pair => pair.Key)
            .ToArray();

        if (channels.Length != 0)
            return true;

        Tabs.CurrentTab = (int)TabNumbers.Radio;
        return false;
    }
}
