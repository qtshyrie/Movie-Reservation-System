using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class RoundButton : Button
{
    private int _cornerRadius = 20;
    private int _borderSize = 0;
    private Color _borderColor = Color.Gray;
    private Color _hoverBackColor = Color.Empty;
    private Color _pressedBackColor = Color.Empty;

    private bool _isHovering;
    private bool _isPressed;

    [Browsable(true), DefaultValue(20)]
    public int CornerRadius
    {
        get => _cornerRadius;
        set { _cornerRadius = Math.Max(0, value); Invalidate(); }
    }

    [Browsable(true), DefaultValue(0)]
    public int BorderSize
    {
        get => _borderSize;
        set { _borderSize = Math.Max(0, value); Invalidate(); }
    }

    [Browsable(true)]
    public Color BorderColor
    {
        get => _borderColor;
        set { _borderColor = value; Invalidate(); }
    }

    // Color shown while the mouse is over the button.
    // If left empty, a lighter version of BackColor is used automatically.
    [Browsable(true)]
    public Color HoverBackColor
    {
        get => _hoverBackColor;
        set { _hoverBackColor = value; Invalidate(); }
    }

    // Color shown while the button is held down.
    // If left empty, a darker version of BackColor is used automatically.
    [Browsable(true)]
    public Color PressedBackColor
    {
        get => _pressedBackColor;
        set { _pressedBackColor = value; Invalidate(); }
    }

    public RoundButton()
    {
        SetStyle(ControlStyles.UserPaint |
                 ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        DoubleBuffered = true;

        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        BackColor = Color.SteelBlue;
        ForeColor = Color.White;
        Cursor = Cursors.Hand;
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _isHovering = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _isHovering = false;
        _isPressed = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs mevent)
    {
        if (mevent.Button == MouseButtons.Left)
        {
            _isPressed = true;
            Invalidate();
        }
        base.OnMouseDown(mevent);
    }

    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        _isPressed = false;
        Invalidate();
        base.OnMouseUp(mevent);
    }

    private Color GetCurrentBackColor()
    {
        if (!Enabled)
            return ControlPaint.LightLight(BackColor);

        if (_isPressed)
            return _pressedBackColor != Color.Empty
                ? _pressedBackColor
                : ControlPaint.Dark(BackColor, 0.1f);

        if (_isHovering)
            return _hoverBackColor != Color.Empty
                ? _hoverBackColor
                : ControlPaint.Light(BackColor, 0.3f);

        return BackColor;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        // Paint the corners with the parent's color so they look transparent
        e.Graphics.Clear(Parent?.BackColor ?? SystemColors.Control);

        float inset = _borderSize / 2f;
        RectangleF rect = new RectangleF(inset, inset,
                                         Width - 1 - _borderSize,
                                         Height - 1 - _borderSize);

        using (var path = GetRoundRectangle(rect, _cornerRadius))
        using (var brush = new SolidBrush(GetCurrentBackColor()))
        {
            e.Graphics.FillPath(brush, path);

            if (_borderSize > 0)
            {
                using (var pen = new Pen(_borderColor, _borderSize))
                    e.Graphics.DrawPath(pen, path);
            }
        }

        TextFormatFlags flags = TextFormatFlags.VerticalCenter |
                                TextFormatFlags.HorizontalCenter |
                                TextFormatFlags.EndEllipsis |
                                TextFormatFlags.NoPadding;

        Color textColor = Enabled ? ForeColor : SystemColors.GrayText;
        TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, textColor, flags);
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        Invalidate();
    }

    private static GraphicsPath GetRoundRectangle(RectangleF r, int radius)
    {
        // Clamp so the arcs never exceed the control size
        float diameter = Math.Min(radius * 2f, Math.Min(r.Width, r.Height));
        var path = new GraphicsPath();

        if (diameter <= 0)
        {
            path.AddRectangle(r);
            return path;
        }

        path.AddArc(r.X, r.Y, diameter, diameter, 180, 90);
        path.AddArc(r.Right - diameter, r.Y, diameter, diameter, 270, 90);
        path.AddArc(r.Right - diameter, r.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(r.X, r.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}