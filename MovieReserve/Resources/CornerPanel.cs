using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class CornerPanel : Panel
{
    private int _topLeft = 20, _topRight = 20, _bottomRight = 20, _bottomLeft = 20, _general = 20;
    private int _borderLeft, _borderTop, _borderRight, _borderBottom;
    private Color _borderColor = Color.Gray;
    private bool _useRegion = false;

    // ---------- Corner radii ----------
    [Browsable(true), DefaultValue(20)]
    public int TopLeftRadius
    {
        get => _topLeft;
        set { _topLeft = Math.Max(0, value); Refresh2(); }
    }

    [Browsable(true), DefaultValue(20)]
    public int TopRightRadius
    {
        get => _topRight;
        set { _topRight = Math.Max(0, value); Refresh2(); }
    }

    [Browsable(true), DefaultValue(20)]
    public int BottomRightRadius
    {
        get => _bottomRight;
        set { _bottomRight = Math.Max(0, value); Refresh2(); }
    }

    [Browsable(true), DefaultValue(20)]
    public int BottomLeftRadius
    {
        get => _bottomLeft;
        set { _bottomLeft = Math.Max(0, value); Refresh2(); }
    }

    // Sets all four corners at once
    [Browsable(true), DefaultValue(20)]
    public int CornerRadius
    {
        get => _general;
        set
        {
            _topLeft = _topRight = _bottomRight = _bottomLeft = Math.Max(0, value);
            Refresh2();
        }
    }

    // ---------- Border sizes (per side) ----------
    [Browsable(true), DefaultValue(0)]
    public int BorderLeft
    {
        get => _borderLeft;
        set { _borderLeft = Math.Max(0, value); Invalidate(); }
    }

    [Browsable(true), DefaultValue(0)]
    public int BorderTop
    {
        get => _borderTop;
        set { _borderTop = Math.Max(0, value); Invalidate(); }
    }

    [Browsable(true), DefaultValue(0)]
    public int BorderRight
    {
        get => _borderRight;
        set { _borderRight = Math.Max(0, value); Invalidate(); }
    }

    [Browsable(true), DefaultValue(0)]
    public int BorderBottom
    {
        get => _borderBottom;
        set { _borderBottom = Math.Max(0, value); Invalidate(); }
    }

    // Sets all four sides at once
    [Browsable(true), DefaultValue(0)]
    public int BorderSize
    {
        get => _borderLeft;
        set
        {
            _borderLeft = _borderTop = _borderRight = _borderBottom = Math.Max(0, value);
            Invalidate();
        }
    }

    [Browsable(true)]
    public Color BorderColor
    {
        get => _borderColor;
        set { _borderColor = value; Invalidate(); }
    }

    // false = smooth anti-aliased edges, corners filled with the parent's color
    // true  = clips the control itself (for nesting inside another round panel
    //         or over a gradient/image; outer edge is not anti-aliased)
    [Browsable(true), DefaultValue(false)]
    public bool UseRegion
    {
        get => _useRegion;
        set { _useRegion = value; Refresh2(); }
    }

    private bool ShouldSerializeBorderColor() => _borderColor != Color.Gray;
    private void ResetBorderColor() => _borderColor = Color.Gray;

    public CornerPanel()
    {
        SetStyle(ControlStyles.UserPaint |
                 ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        DoubleBuffered = true;
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateRegion();
    }

    private void Refresh2()
    {
        UpdateRegion();
        Invalidate();
    }

    private void UpdateRegion()
    {
        if (!_useRegion)
        {
            if (Region != null) { Region.Dispose(); Region = null; }
            return;
        }

        if (Width <= 0 || Height <= 0) return;

        using (var path = GetOuterPath())
        {
            Region?.Dispose();
            Region = new Region(path);
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        g.Clear(_useRegion ? BackColor : (Parent?.BackColor ?? SystemColors.Control));

        bool hasBorder = _borderLeft > 0 || _borderTop > 0 ||
                         _borderRight > 0 || _borderBottom > 0;

        // 1) Outer shape: border color (or back color when there is no border)
        using (var outer = GetOuterPath())
        using (var outerBrush = new SolidBrush(hasBorder ? _borderColor : BackColor))
        {
            g.FillPath(outerBrush, outer);
        }

        // 2) Inner shape: back color, inset by each side's border width
        if (hasBorder)
        {
            using (var inner = GetInnerPath())
            {
                if (inner != null)
                    using (var innerBrush = new SolidBrush(BackColor))
                        g.FillPath(innerBrush, inner);
            }
        }
    }

    private GraphicsPath GetOuterPath()
    {
        var r = new RectangleF(0, 0, Width, Height);
        float max = Math.Min(r.Width, r.Height) / 2f;

        float tl = Math.Min(_topLeft, max);
        float tr = Math.Min(_topRight, max);
        float br = Math.Min(_bottomRight, max);
        float bl = Math.Min(_bottomLeft, max);

        return BuildPath(r, tl, tl, tr, tr, br, br, bl, bl);
    }

    private GraphicsPath GetInnerPath()
    {
        float w = Width - _borderLeft - _borderRight;
        float h = Height - _borderTop - _borderBottom;
        if (w <= 0 || h <= 0) return null;

        var r = new RectangleF(_borderLeft, _borderTop, w, h);

        float max = Math.Min(Width, Height) / 2f;
        float tl = Math.Min(_topLeft, max);
        float tr = Math.Min(_topRight, max);
        float br = Math.Min(_bottomRight, max);
        float bl = Math.Min(_bottomLeft, max);

        float hw = w / 2f, hh = h / 2f;

        // Each inner corner = outer radius minus the adjacent border widths
        float tlx = Math.Min(Math.Max(0, tl - _borderLeft), hw);
        float tly = Math.Min(Math.Max(0, tl - _borderTop), hh);
        float trx = Math.Min(Math.Max(0, tr - _borderRight), hw);
        float try_ = Math.Min(Math.Max(0, tr - _borderTop), hh);
        float brx = Math.Min(Math.Max(0, br - _borderRight), hw);
        float bry = Math.Min(Math.Max(0, br - _borderBottom), hh);
        float blx = Math.Min(Math.Max(0, bl - _borderLeft), hw);
        float bly = Math.Min(Math.Max(0, bl - _borderBottom), hh);

        return BuildPath(r, tlx, tly, trx, try_, brx, bry, blx, bly);
    }

    // rx = horizontal radius, ry = vertical radius for each corner
    private static GraphicsPath BuildPath(RectangleF r,
        float tlx, float tly, float trx, float try_,
        float brx, float bry, float blx, float bly)
    {
        var path = new GraphicsPath();

        // Top-left
        if (tlx > 0 && tly > 0) path.AddArc(r.X, r.Y, tlx * 2, tly * 2, 180, 90);
        else path.AddLine(r.X, r.Y, r.X, r.Y);

        // Top-right
        if (trx > 0 && try_ > 0) path.AddArc(r.Right - trx * 2, r.Y, trx * 2, try_ * 2, 270, 90);
        else path.AddLine(r.Right, r.Y, r.Right, r.Y);

        // Bottom-right
        if (brx > 0 && bry > 0) path.AddArc(r.Right - brx * 2, r.Bottom - bry * 2, brx * 2, bry * 2, 0, 90);
        else path.AddLine(r.Right, r.Bottom, r.Right, r.Bottom);

        // Bottom-left
        if (blx > 0 && bly > 0) path.AddArc(r.X, r.Bottom - bly * 2, blx * 2, bly * 2, 90, 90);
        else path.AddLine(r.X, r.Bottom, r.X, r.Bottom);

        path.CloseFigure();
        return path;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) Region?.Dispose();
        base.Dispose(disposing);
    }
}