using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class RoundLabel : Label
{
    private int _cornerRadius = 20;

    [Browsable(true), DefaultValue(20)]
    public int CornerRadius
    {
        get => _cornerRadius;
        set { _cornerRadius = Math.Max(0, value); Invalidate(); }
    }

    public RoundLabel()
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
        // Do NOT call base.OnPaint: it would draw the rectangular background and text.
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        // Fill the corners with the parent's color so they look transparent
        Color parentColor = Parent?.BackColor ?? SystemColors.Control;
        e.Graphics.Clear(parentColor);

        Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

        using (var path = GetRoundRectangle(rect, _cornerRadius))
        using (var brush = new SolidBrush(BackColor))
        using (var pen = new Pen(BackColor, 1f))
        {
            e.Graphics.FillPath(brush, path);
            e.Graphics.DrawPath(pen, path);
        }

        TextFormatFlags flags = TextFormatFlags.VerticalCenter |
                                TextFormatFlags.HorizontalCenter |
                                TextFormatFlags.EndEllipsis |
                                TextFormatFlags.NoPadding;

        TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, ForeColor, flags);
    }

    private static GraphicsPath GetRoundRectangle(Rectangle r, int radius)
    {
        // Clamp so the arcs never exceed the control size
        int diameter = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
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
