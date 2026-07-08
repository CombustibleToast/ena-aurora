// SPDX-FileCopyrightText: 2025 Cam
// SPDX-FileCopyrightText: 2025 Cami
// SPDX-FileCopyrightText: 2025 sleepyyapril
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Client._DEN.Denu.Core;
using Content.Client._DEN.Denu.Modules.Chat;
using Content.Client._DEN.UserInterface.Systems.Chat.Controls;
using Content.Client.Chat.TypingIndicator;
using Content.Shared._DEN.Denu;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;

namespace Content.Client._DEN.Denu.UI.Denu;

public sealed partial class DenuUIController : UIController
{
    [UISystemDependency] private TypingIndicatorSystem _typingIndicatorSystem = default!;
    [Dependency] private IOverlayManager _overlayManager = default!;

    private readonly DenuModuleRegistry _moduleRegistry = new();
    private ChatFormatterDenuModule? _formatterModule;
    private EarmuffsDenuModule? _earmuffsModule;

    public event Action? OnSettingsChanged;
    public event Action<bool>? OnOpenStateChanged;

    public DenuModuleRegistry ModuleRegistry => _moduleRegistry;

    public bool AutoFormatterEnabled
    {
        get => _formatterModule?.Enabled ?? false;
        set
        {
            if (_formatterModule != null)
                _formatterModule.SetEnabled(value);
        }
    }

    private bool _isOpen;

    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            if (_isOpen == value)
                return;

            _isOpen = value;
            OnOpenStateChanged?.Invoke(value);
        }
    }

    private static readonly MessageFormatter.FormatterConfig DefaultFormatterConfig = new MessageFormatter.FormatterConfig
    {
        Rules = new()
        {
            new("***", "[bolditalic]", "[/bolditalic]", false, false),
            new("**", "[bold]", "[/bold]", false, false),
            new("\"", "[color={DialogueColor}]\"", "\"[/color]", false, true),
            new("*", "[italic]", "[/italic]", true, false),
            new("*", "[italic][color={EmoteColor}]*", "*[/color][/italic]", false, false),
        },
        Replacements = new()
        {
            { "DialogueColor", "#FFFFFF" },
            { "EmoteColor", "#FF13FF" }
        },
        AllowEscaping = true,
        EscapableTokens = new() { '*', '"', '\\' },
        RemoveAsterisks = false
    };

    public MessageFormatter.FormatterConfig FormatterConfig =>
        _formatterModule?.FormatterConfig ?? DefaultFormatterConfig;

    private DenuWindow? _denuWindow;
    private DenuWindow? _windowWithEvents;
    private CircleOverlay? _circleOverlay;

    public override void Initialize()
    {
        base.Initialize();

        _moduleRegistry.Initialize();
        RegisterModules();

        SubscribeNetworkEvent<DenuSettingsSyncEvent>(OnSettingsSync);
        SubscribeNetworkEvent<DenuSettingsSaveResponse>(OnSaveResponse);
    }

    private void RegisterModules()
    {
        _formatterModule = new ChatFormatterDenuModule();
        _earmuffsModule = new EarmuffsDenuModule();

        _moduleRegistry.RegisterModule(_formatterModule);
        _moduleRegistry.RegisterModule(_earmuffsModule);
    }

    private void OnSettingsSync(DenuSettingsSyncEvent ev, EntitySessionEventArgs args)
    {
        _moduleRegistry.ApplySync(ev.Settings, ev.CurrentProfileId);

        OnSettingsChanged?.Invoke();
    }

    private void OnSaveResponse(DenuSettingsSaveResponse ev, EntitySessionEventArgs args)
    {
        if (!ev.Success)
        {
            Log.Error($"Failed to save Denu settings: {ev.ErrorMessage}");
            return;
        }

        _moduleRegistry.ApplySaveEcho(ev.Settings, ev.CurrentProfileId);
        OnSettingsChanged?.Invoke();
    }

    public float GetCurrentEarmuffRange()
    {
        return _earmuffsModule?.CurrentRange ?? 10.0f;
    }

    public void CreateWindow()
    {
        if (!UIManager.TryGetFirstWindow(out _denuWindow))
            _denuWindow = UIManager.CreateWindow<DenuWindow>();

        if (_windowWithEvents == _denuWindow)
            return;

        var window = _denuWindow!;
        window.OnOpen += () =>
        {
            window.RecenterWindow(new(0.5f, 0.5f));
            IsOpen = true;
        };

        window.OnClose += () => IsOpen = false;
        _windowWithEvents = window;
    }

    public void OpenWindow()
    {
        if (_denuWindow is not { Disposed: false })
            CreateWindow();

        _denuWindow!.OpenCentered();
    }

    public void CloseWindow()
    {
        _denuWindow?.Close();
    }

    public Color GetColorReplacement(string replacementName)
    {
        if (replacementName == "DialogueColor")
            return _formatterModule?.GetDialogueColor() ?? Color.White;

        if (replacementName == "EmoteColor")
            return _formatterModule?.GetEmoteColor() ?? Color.Magenta;

        return Color.Magenta;
    }

    public void SetColorReplacement(string replacementName, Color color)
    {
        if (_formatterModule == null)
            return;

        if (replacementName == "DialogueColor")   _formatterModule.SetDialogueColor(color);
        else if (replacementName == "EmoteColor") _formatterModule.SetEmoteColor(color);
    }

    public string FormatMessage(string message, bool allowEscape = false) =>
        _formatterModule?.FormatMessage(message) ?? message;

    public void ShowTypingIndicator() =>
        _typingIndicatorSystem.ClientChangedChatText();

    public void HideTypingIndicator() =>
        _typingIndicatorSystem.ClientSubmittedChatText();

    public void SetEarmuffRange(float range, bool sendUpdate)
    {
        if (_circleOverlay == null)
        {
            _circleOverlay = new();
            _circleOverlay.OnFullyFaded += RemoveCircleOverlay;
            _overlayManager.AddOverlay(_circleOverlay);
        }

        _circleOverlay.Range = range;
        _circleOverlay.ShowCircle();

        if (!sendUpdate)
            return;

        _earmuffsModule?.SetRange(range, persist: true);
    }

    private void RemoveCircleOverlay()
    {
        if (_circleOverlay != null)
        {
            _overlayManager.RemoveOverlay(_circleOverlay);
            _circleOverlay = null;
        }
    }
}
