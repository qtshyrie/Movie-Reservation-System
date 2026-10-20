using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class RoundPanel : Panel
{
    private int _cornerRadius = 20;
    private int _borderSize = 0;
    private Color _borderColor = Color.Gray;

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

    public RoundPanel()
    {
        SetStyle(ControlStyles.UserPaint |
                 ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        // Paint the corners with the parent's color so they look transparent
        e.Graphics.Clear(Parent?.BackColor ?? SystemColors.Control);

        // Inset by half the border so the stroke isn't clipped
        float inset = _borderSize / 2f;
        RectangleF rect = new RectangleF(inset, inset,
                                         Width - 1 - _borderSize,
                                         Height - 1 - _borderSize);

        using (var path = GetRoundRectangle(rect, _cornerRadius))
        using (var brush = new SolidBrush(BackColor))
        {
            e.Graphics.FillPath(brush, path);

            if (_borderSize > 0)
            {
                using (var pen = new Pen(_borderColor, _borderSize))
                    e.Graphics.DrawPath(pen, path);
            }
        }
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
