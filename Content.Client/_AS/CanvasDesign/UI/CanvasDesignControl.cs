using System.Numerics;
using Content.Shared._AS.CanvasDesign;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;
using Robust.Shared.Timing;

namespace Content.Client._AS.CanvasDesign.UI;

public sealed class CanvasDesignControl : Control
{
    private const int DefaultPixelScale = 16;
    private const int ViewportPadding = 18;
    private static readonly int[] ZoomLevels = [1, 2, 3, 4, 6, 8, 12, 16, 24, 32, 48, 64]; // This is probably a fine way to do this
    private bool _drawing;
    private bool _manuallyZoomed;
    private Vector2i _shapeStart;
    private uint[]? _shapeBase;
    private int _width = 1;
    private int _height = 1;
    private int _pixelScale = DefaultPixelScale;
    private Vector2 _viewportSize = new(512, 384);
    private uint _background = 0xFFFFFFFFu;
    private uint[] _pixels = [0xFFFFFFFFu];
    public Color SelectedColor = Color.White;
    public CanvasDesignTool Tool = CanvasDesignTool.Pencil;
    public bool Editable { get; set; } = true;
    public event Action? Changed;
    public event Action<Color>? Picked;

    public CanvasDesignControl()
    {
        SetSize = new Vector2(DefaultPixelScale, DefaultPixelScale);
        MouseFilter = MouseFilterMode.Stop;
    }

    /// <summary>
    /// Configures the canvas size and background color. Resets the pixel buffer to the background color.
    /// </summary>
    public void Configure(int width, int height, uint background)
    {
        if (!CanvasDesignComponent.DimensionsWithinLimit(width, height))
            return;

        if (_width != width || _height != height)
            _manuallyZoomed = false;

        _width = width;
        _height = height;
        _background = background;
        if (_pixels.Length != width * height)
        {
            _pixels = new uint[width * height];
            Array.Fill(_pixels, _background);
        }
        UpdateFitScale();
    }

    /// <summary>
    /// Updates the space available for the canvas and fits it there until the user manually zooms.
    /// </summary>
    public void FitToViewport(Vector2 size)
    {
        if (size.X <= 0 || size.Y <= 0 || _viewportSize == size)
            return;

        _viewportSize = size;
        if (!_manuallyZoomed)
            UpdateFitScale();
    }

    public uint[] GetPixels() => (uint[]) _pixels.Clone();
    public void Clear()
    {
        Array.Fill(_pixels, _background);
        Changed?.Invoke();
        InvalidateMeasure();
    }

    public void SetPixels(uint[] pixels)
    {
        if (pixels.Length == _width * _height)
            _pixels = (uint[]) pixels.Clone();
        InvalidateMeasure();
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        var step = _pixelScale * UIScale;
        var cellSize = Math.Max(1, _pixelScale - 1) * UIScale;
        for (var y = 0; y < _height; y++)
        for (var x = 0; x < _width; x++)
        {
            var value = _pixels[y * _width + x];
            var color = new Color((byte) (value >> 16), (byte) (value >> 8), (byte) value,
                (byte) (value >> 24));
            var box = UIBox2.FromDimensions(new Vector2(x, y) * step, new Vector2(cellSize));
            handle.DrawRect(box, color);
        }
    }

    /// <summary>
    /// Handles mouse input for drawing on the canvas.
    /// </summary>
    protected override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        base.KeyBindDown(args);
        if (!Editable || args.Function != EngineKeyFunctions.UIClick)
            return;
        _drawing = true;
        if (IsShapeTool())
        {
            _shapeStart = PixelAt(args.RelativePosition);
            _shapeBase = (uint[]) _pixels.Clone();
        }
        UseAt(args.RelativePosition);
        args.Handle();
    }

    protected override void KeyBindUp(GUIBoundKeyEventArgs args)
    {
        base.KeyBindUp(args);
        if (args.Function == EngineKeyFunctions.UIClick)
        {
            _drawing = false;
            if (_shapeBase != null)
            {
                _shapeBase = null;
                Changed?.Invoke();
            }
        }
    }

    protected override void MouseMove(GUIMouseMoveEventArgs args)
    {
        base.MouseMove(args);
        if (_drawing && (Tool is CanvasDesignTool.Pencil or CanvasDesignTool.Eraser || IsShapeTool()))
            UseAt(args.RelativePosition);
    }

    /// <summary>
    /// Zooms the displayed canvas without changing its logical dimensions or pixel data.
    /// </summary>
    protected override void MouseWheel(GUIMouseWheelEventArgs args)
    {
        base.MouseWheel(args);
        var direction = Math.Sign(args.Delta.Y);
        if (direction == 0)
            return;

        var nextScale = NextZoomLevel(direction);
        if (nextScale == _pixelScale)
        {
            args.Handle();
            return;
        }

        var oldScale = _pixelScale;
        var cursor = args.RelativePosition;
        var viewport = Parent as ScrollContainer;
        var viewportCursor = viewport == null
            ? Vector2.Zero
            : GlobalPosition - viewport.GlobalPosition + cursor;
        _manuallyZoomed = true;
        SetPixelScale(nextScale);

        if (viewport != null)
        {
            var scaleChange = (float) nextScale / oldScale - 1f;
            Timer.Spawn(0, () =>
            {
                if (viewport.Parent == null)
                    return;

                viewport.HScrollTarget = cursor.X * (scaleChange + 1f) - viewportCursor.X;
                viewport.VScrollTarget = cursor.Y * (scaleChange + 1f) - viewportCursor.Y;
            });
        }

        args.Handle();
    }

    private void UpdateFitScale()
    {
        var availableWidth = Math.Max(1, (int) _viewportSize.X - ViewportPadding);
        var availableHeight = Math.Max(1, (int) _viewportSize.Y - ViewportPadding);
        var scale = Math.Max(1,
            Math.Min(DefaultPixelScale,
                Math.Min(availableWidth / _width, availableHeight / _height)));
        SetPixelScale(scale);
    }

    private void SetPixelScale(int scale)
    {
        _pixelScale = scale;
        SetSize = new Vector2(_width * scale, _height * scale);
        InvalidateMeasure();
    }

    private int NextZoomLevel(int direction)
    {
        if (direction > 0)
        {
            foreach (var level in ZoomLevels)
            {
                if (level > _pixelScale)
                    return level;
            }

            return ZoomLevels[^1];
        }

        for (var i = ZoomLevels.Length - 1; i >= 0; i--)
        {
            if (ZoomLevels[i] < _pixelScale)
                return ZoomLevels[i];
        }

        return ZoomLevels[0];
    }

    /// <summary>
    /// Applies the current tool at the given position on the canvas.
    /// </summary>
    private void UseAt(Vector2 position)
    {
        var point = PixelAt(position);
        var x = point.X;
        var y = point.Y;
        if (x < 0 || y < 0 || x >= _width || y >= _height)
            return;
        var index = y * _width + x;
        if (Tool == CanvasDesignTool.Picker)
        {
            var value = _pixels[index];
            Picked?.Invoke(new Color((byte) (value >> 16), (byte) (value >> 8), (byte) value));
            return;
        }

        var color = Tool == CanvasDesignTool.Eraser ? _background : PackedSelectedColor();
        if (IsShapeTool())
        {
            if (_shapeBase == null)
                return;
            Array.Copy(_shapeBase, _pixels, _pixels.Length);
            DrawShape(_shapeStart, point, color);
            InvalidateMeasure();
            return;
        }

        if (Tool == CanvasDesignTool.Fill)
            FloodFill(x, y, color);
        else
            _pixels[index] = color;

        Changed?.Invoke();
        InvalidateMeasure();
    }

    private uint PackedSelectedColor() => 0xFF000000u | ((uint) SelectedColor.RByte << 16) |
                                          ((uint) SelectedColor.GByte << 8) | SelectedColor.BByte;

    private Vector2i PixelAt(Vector2 position) => new(
        Math.Clamp((int) (position.X / _pixelScale), 0, _width - 1),
        Math.Clamp((int) (position.Y / _pixelScale), 0, _height - 1));

    private bool IsShapeTool() => Tool is CanvasDesignTool.Line or CanvasDesignTool.Square or CanvasDesignTool.Circle;

    private void DrawShape(Vector2i start, Vector2i end, uint color)
    {
        switch (Tool)
        {
            case CanvasDesignTool.Line:
                DrawLine(start.X, start.Y, end.X, end.Y, color);
                break;
            case CanvasDesignTool.Square:
                DrawRectangle(start, end, color);
                break;
            case CanvasDesignTool.Circle:
                DrawEllipse(start, end, color);
                break;
        }
    }

    private void DrawLine(int x0, int y0, int x1, int y1, uint color)
    {
        var dx = Math.Abs(x1 - x0);
        var sx = x0 < x1 ? 1 : -1;
        var dy = -Math.Abs(y1 - y0);
        var sy = y0 < y1 ? 1 : -1;
        var error = dx + dy;
        while (true)
        {
            SetPixel(x0, y0, color);
            if (x0 == x1 && y0 == y1)
                break;
            var twiceError = error * 2;
            if (twiceError >= dy)
            {
                error += dy;
                x0 += sx;
            }
            if (twiceError <= dx)
            {
                error += dx;
                y0 += sy;
            }
        }
    }

    private void DrawRectangle(Vector2i start, Vector2i end, uint color)
    {
        var left = Math.Min(start.X, end.X);
        var right = Math.Max(start.X, end.X);
        var top = Math.Min(start.Y, end.Y);
        var bottom = Math.Max(start.Y, end.Y);
        for (var x = left; x <= right; x++)
        {
            SetPixel(x, top, color);
            SetPixel(x, bottom, color);
        }
        for (var y = top; y <= bottom; y++)
        {
            SetPixel(left, y, color);
            SetPixel(right, y, color);
        }
    }

    private void DrawEllipse(Vector2i start, Vector2i end, uint color)
    {
        var centerX = (start.X + end.X) / 2f;
        var centerY = (start.Y + end.Y) / 2f;
        var radiusX = Math.Max(Math.Abs(end.X - start.X) / 2f, 0.5f);
        var radiusY = Math.Max(Math.Abs(end.Y - start.Y) / 2f, 0.5f);
        var steps = Math.Max(12, (int) Math.Ceiling(Math.PI * (radiusX + radiusY) * 2));
        Vector2i? previous = null;
        for (var i = 0; i <= steps; i++)
        {
            var angle = Math.Tau * i / steps;
            var point = new Vector2i(
                (int) Math.Round(centerX + radiusX * Math.Cos(angle)),
                (int) Math.Round(centerY + radiusY * Math.Sin(angle)));
            if (previous != null)
                DrawLine(previous.Value.X, previous.Value.Y, point.X, point.Y, color);
            previous = point;
        }
    }

    private void SetPixel(int x, int y, uint color)
    {
        if (x >= 0 && y >= 0 && x < _width && y < _height)
            _pixels[y * _width + x] = color;
    }

    private void FloodFill(int startX, int startY, uint replacement)
    {
        var target = _pixels[startY * _width + startX];
        if (target == replacement)
            return;

        var pending = new Queue<(int X, int Y)>();
        pending.Enqueue((startX, startY));
        while (pending.TryDequeue(out var point))
        {
            if (point.X < 0 || point.Y < 0 ||
                point.X >= _width || point.Y >= _height)
                continue;

            var index = point.Y * _width + point.X;
            if (_pixels[index] != target)
                continue;

            _pixels[index] = replacement;
            pending.Enqueue((point.X - 1, point.Y));
            pending.Enqueue((point.X + 1, point.Y));
            pending.Enqueue((point.X, point.Y - 1));
            pending.Enqueue((point.X, point.Y + 1));
        }
    }
}

public enum CanvasDesignTool : byte
{
    Pencil,
    Eraser,
    Picker,
    Fill,
    Line,
    Square,
    Circle
}
