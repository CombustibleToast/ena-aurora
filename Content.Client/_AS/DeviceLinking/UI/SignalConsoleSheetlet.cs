using Content.Client.Stylesheets;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client._AS.DeviceLinking.UI;

[CommonSheetlet]
public sealed class SignalConsoleSheetlet : Sheetlet<PalettedStylesheet>
{
    public override StyleRule[] GetRules(PalettedStylesheet sheet, object config)
    {
        // greeennnnn
        var normalBackground = Color.FromHex("#182321");
        var hoverBackground = Color.FromHex("#243A35");
        var pressedBackground = Color.FromHex("#10201D");
        var normalBorder = Color.FromHex("#4F8175");
        var nameText = Color.FromHex("#F0F5F3");
        var actionText = Color.FromHex("#E4B85F");
        var activeBackground = Color.FromHex("#17382B");
        var activeHoverBackground = Color.FromHex("#20513D");
        var activePressedBackground = Color.FromHex("#102A20");
        var activeBorder = Color.FromHex("#5BE09A");
        var activeText = Color.FromHex("#8AF2B8");

        StyleBoxFlat ButtonBox(Color background, Color border, float thickness = 1f) => new()
        {
            BackgroundColor = background,
            BorderColor = border,
            BorderThickness = new Thickness(thickness),
        };

        return
        [
            E<SignalConsoleChannelButton>()
                .PseudoNormal()
                .Box(ButtonBox(normalBackground, normalBorder)),
            E<SignalConsoleChannelButton>()
                .PseudoHovered()
                .Box(ButtonBox(hoverBackground, normalBorder, 2f)),
            E<SignalConsoleChannelButton>()
                .PseudoPressed()
                .Box(ButtonBox(pressedBackground, actionText, 2f)),

            E<SignalConsoleChannelButton>()
                .Class(SignalConsoleChannelButton.ActiveStyleClass)
                .PseudoNormal()
                .Box(ButtonBox(activeBackground, activeBorder, 2f)),
            E<SignalConsoleChannelButton>()
                .Class(SignalConsoleChannelButton.ActiveStyleClass)
                .PseudoHovered()
                .Box(ButtonBox(activeHoverBackground, activeBorder, 2f)),
            E<SignalConsoleChannelButton>()
                .Class(SignalConsoleChannelButton.ActiveStyleClass)
                .PseudoPressed()
                .Box(ButtonBox(activePressedBackground, activeBorder, 2f)),

            E<Label>()
                .Class(SignalConsoleChannelButton.NameStyleClass)
                .FontColor(nameText),
            E<Label>()
                .Class(SignalConsoleChannelButton.ActionStyleClass)
                .FontColor(actionText),
            E<Label>()
                .Class(SignalConsoleChannelButton.CornerStyleClass)
                .FontColor(actionText),
            E<SignalConsoleChannelButton>()
                .Class(SignalConsoleChannelButton.ActiveStyleClass)
                .ParentOf(E<Label>().Class(SignalConsoleChannelButton.NameStyleClass))
                .FontColor(activeText),
            E<SignalConsoleChannelButton>()
                .Class(SignalConsoleChannelButton.ActiveStyleClass)
                .ParentOf(E<Label>().Class(SignalConsoleChannelButton.ActionStyleClass))
                .FontColor(activeText),

            E<PanelContainer>()
                .Class(SignalConsoleWindow.EditorCardStyleClass)
                .Panel(ButtonBox(sheet.SecondaryPalette.BackgroundLight, sheet.SecondaryPalette.Element)),
            E<Label>()
                .Class(SignalConsoleWindow.MutedLabelStyleClass)
                .FontColor(actionText),
        ];
    }
}
