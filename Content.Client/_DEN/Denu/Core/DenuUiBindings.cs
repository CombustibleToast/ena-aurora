using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._DEN.Denu.Core;

public static class DenuUiBindings
{
    public static BoundCheckBox BindCheckBox(CheckBox checkBox, Action<bool> onChanged)
    {
        return new BoundCheckBox(checkBox, onChanged);
    }

    public static BoundColorSelector BindColorSelector(ColorSelectorSliders selector, Action<Color> onChanged)
    {
        return new BoundColorSelector(selector, onChanged);
    }

    public static BoundSlider BindSliderOnRelease(Slider slider, Label label, Action<float> onPreview, Action<float> onCommit, int roundingDecimals = 2, string labelFormat = "F1")
    {
        return new BoundSlider(slider, label, onPreview, onCommit, roundingDecimals, labelFormat);
    }

    public sealed class BoundCheckBox
    {
        private readonly CheckBox _checkBox;
        private readonly Action<bool> _onChanged;
        private bool _suppress;

        public BoundCheckBox(CheckBox checkBox, Action<bool> onChanged)
        {
            _checkBox = checkBox;
            _onChanged = onChanged;

            _checkBox.OnToggled += args =>
            {
                if (_suppress)
                    return;

                _onChanged(args.Pressed);
            };
        }

        public void Refresh(bool value)
        {
            _suppress = true;
            _checkBox.Pressed = value;
            _suppress = false;
        }
    }

    public sealed class BoundColorSelector
    {
        private readonly ColorSelectorSliders _selector;
        private readonly Action<Color> _onChanged;
        private bool _suppress;

        public BoundColorSelector(ColorSelectorSliders selector, Action<Color> onChanged)
        {
            _selector = selector;
            _onChanged = onChanged;

            _selector.OnColorChanged += color =>
            {
                if (_suppress)
                    return;

                _onChanged(color);
            };
        }

        public void Refresh(Color color)
        {
            _suppress = true;
            _selector.Color = color;
            _suppress = false;
        }
    }

    public sealed class BoundSlider
    {
        private readonly Slider _slider;
        private readonly Label _label;
        private readonly Action<float> _onPreview;
        private readonly Action<float> _onCommit;
        private readonly int _roundingDecimals;
        private readonly string _labelFormat;
        private bool _suppress;
        private bool _dragging;

        public BoundSlider(Slider slider, Label label, Action<float> onPreview, Action<float> onCommit, int roundingDecimals, string labelFormat)
        {
            _slider = slider;
            _label = label;
            _onPreview = onPreview;
            _onCommit = onCommit;
            _roundingDecimals = roundingDecimals;
            _labelFormat = labelFormat;

            _slider.OnValueChanged += args =>
            {
                if (_suppress)
                    return;

                _label.Text = args.Value.ToString(_labelFormat);
                _onPreview(args.Value);
            };

            _slider.OnGrabbed += _ => _dragging = true;

            _slider.OnReleased += _ =>
            {
                _dragging = false;
                float value = _slider.Value;
                _label.Text = value.ToString(_labelFormat);
                _onCommit(value);
            };
        }

        public bool IsDragging => _dragging;

        public void Refresh(float value)
        {
            if (_dragging)
                return;

            _suppress = true;
            _slider.SetValueWithoutEvent(value);
            _suppress = false;

            _label.Text = value.ToString(_labelFormat);
        }

        private float Round(float value)
        {
            return (float)Math.Round(value, _roundingDecimals);
        }
    }
}
