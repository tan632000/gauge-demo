using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TestGauge
{
    public enum AlarmSeverity
    {
        Warning,
        Alarm
    }

    public partial class AlarmControl : UserControl
    {
        // ── Default color palette ──
        private static readonly Color DefaultBackColor       = Color.FromArgb(0x0d, 0x11, 0x17);
        private static readonly Color DefaultBackColorHover  = Color.FromArgb(0x16, 0x1b, 0x22);
        private static readonly Color DefaultTextWhite       = Color.White;
        private static readonly Color DefaultTextGray        = Color.FromArgb(0x8b, 0x94, 0x9e);
        private static readonly Color DefaultWarningColor    = Color.FromArgb(0xFF, 0xA5, 0x00);
        private static readonly Color DefaultAlarmColor      = Color.FromArgb(0xFF, 0x4D, 0x4D);

        private const int DefaultCornerRadius  = 10;
        private const int DefaultIconSize      = 28;
        private const int BadgeHPadding        = 14;
        private const int BadgeVPadding        = 5;

        // ── Backing fields ──
        private string _title       = "Title";
        private DateTime _time      = DateTime.Now;
        private AlarmSeverity _severity = AlarmSeverity.Warning;
        private bool _hovered;

        // ── Customizable color overrides ──
        private Color? _backColorNormal;
        private Color? _backColorHover;
        private Color? _warningColor;
        private Color? _alarmColor;
        private Color? _textColor;
        private Color? _subtextColor;
        private int _cornerRadius = DefaultCornerRadius;
        private int _iconSize     = DefaultIconSize;
        private float _iconFontScale = 0.38f;
        private float _iconTextYRatio = 0.35f;

        // ────────────────────────────────────────────
        //  Public Properties
        // ────────────────────────────────────────────

        [Category("Alarm")]
        [Description("Bold title text.")]
        public string Title
        {
            get => _title;
            set { _title = value; Invalidate(); }
        }

        [Category("Alarm")]
        [Description("Timestamp displayed next to the icon.")]
        public DateTime Time
        {
            get => _time;
            set { _time = value; Invalidate(); }
        }

        [Category("Alarm")]
        [Description("Severity level: Warning (orange) or Alarm (red).")]
        public AlarmSeverity Severity
        {
            get => _severity;
            set { _severity = value; Invalidate(); }
        }

        [Category("Alarm.Appearance")]
        [Description("Normal background color. Null = default dark navy.")]
        public Color? BackColorNormal
        {
            get => _backColorNormal;
            set { _backColorNormal = value; Invalidate(); }
        }

        [Category("Alarm.Appearance")]
        [Description("Hover background color. Null = default lighter shade.")]
        public Color? BackColorHover
        {
            get => _backColorHover;
            set { _backColorHover = value; Invalidate(); }
        }

        [Category("Alarm.Appearance")]
        [Description("Warning severity accent color. Null = default orange.")]
        public Color? WarningColor
        {
            get => _warningColor;
            set { _warningColor = value; Invalidate(); }
        }

        [Category("Alarm.Appearance")]
        [Description("Alarm severity accent color. Null = default red.")]
        public Color? AlarmColor
        {
            get => _alarmColor;
            set { _alarmColor = value; Invalidate(); }
        }

        [Category("Alarm.Appearance")]
        [Description("Title text color. Null = default white.")]
        public Color? TextColor
        {
            get => _textColor;
            set { _textColor = value; Invalidate(); }
        }

        [Category("Alarm.Appearance")]
        [Description("Description/time text color. Null = default gray.")]
        public Color? SubtextColor
        {
            get => _subtextColor;
            set { _subtextColor = value; Invalidate(); }
        }

        [Category("Alarm.Appearance")]
        [Description("Corner radius in pixels.")]
        public int CornerRadius
        {
            get => _cornerRadius;
            set { _cornerRadius = value; Invalidate(); }
        }

        [Category("Alarm.Appearance")]
        [Description("Icon size in pixels.")]
        public int IconSize
        {
            get => _iconSize;
            set { _iconSize = value; Invalidate(); }
        }

        [Category("Alarm.Appearance")]
        [DefaultValue(0.38f)]
        [Description("Font size = IconSize * this value. Adjust to fit '!' inside the triangle.")]
        public float IconFontScale
        {
            get => _iconFontScale;
            set { _iconFontScale = Math.Max(0.1f, Math.Min(1.0f, value)); Invalidate(); }
        }

        [Category("Alarm.Appearance")]
        [DefaultValue(0.35f)]
        [Description("Vertical position ratio for '!' mark (0.0 = top, 1.0 = bottom of icon).")]
        public float IconTextYRatio
        {
            get => _iconTextYRatio;
            set { _iconTextYRatio = Math.Max(0.0f, Math.Min(1.0f, value)); Invalidate(); }
        }

        // ────────────────────────────────────────────
        //  Constructor
        // ────────────────────────────────────────────

        public AlarmControl()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
            UpdateStyles();

            Size = new Size(600, 64);
            BackColor = DefaultBackColor;
            Cursor = Cursors.Hand;
        }

        // ────────────────────────────────────────────
        //  Hover tracking
        // ────────────────────────────────────────────

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hovered = false;
            Invalidate();
        }

        // ────────────────────────────────────────────
        //  Resolve helpers (override or default)
        // ────────────────────────────────────────────

        private Color ResolveBackColor()    => _backColorNormal ?? DefaultBackColor;
        private Color ResolveBackColorHov() => _backColorHover  ?? DefaultBackColorHover;
        private Color ResolveWarning()      => _warningColor    ?? DefaultWarningColor;
        private Color ResolveAlarm()        => _alarmColor      ?? DefaultAlarmColor;
        private Color ResolveText()         => _textColor       ?? DefaultTextWhite;
        private Color ResolveSubtext()      => _subtextColor    ?? DefaultTextGray;
        private Color ResolveAccent()       => _severity == AlarmSeverity.Warning ? ResolveWarning() : ResolveAlarm();

        // ────────────────────────────────────────────
        //  Paint
        // ────────────────────────────────────────────

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            Color accent = ResolveAccent();
            int radius = _cornerRadius;

            // Background
            using (var path = RoundedRect(rect, radius))
            using (var bg = new SolidBrush(_hovered ? ResolveBackColorHov() : ResolveBackColor()))
                g.FillPath(bg, path);

            int x = 16;

            // Triangle icon
            DrawTriangleIcon(g, x, (Height - _iconSize) / 2, _iconSize, accent, _iconFontScale, _iconTextYRatio);
            x += _iconSize + 12;

            // Time
            string timeStr = _time.ToString("HH:mm:ss");
            using (var font = new Font("Segoe UI", 9f))
            using (var brush = new SolidBrush(ResolveSubtext()))
            {
                var sz = g.MeasureString(timeStr, font);
                g.DrawString(timeStr, font, brush, x, (Height - sz.Height) / 2);
                x += (int)sz.Width + 16;
            }

            // Title
            using (var titleFont = new Font("Segoe UI", 10f, FontStyle.Bold))
            using (var titleBrush = new SolidBrush(ResolveText()))
            {
                float tH = g.MeasureString(_title, titleFont).Height;
                float y0 = (Height - tH) / 2;
                g.DrawString(_title, titleFont, titleBrush, x, y0);
            }

            // Status badge
            string badgeText = _severity == AlarmSeverity.Warning ? "WARNING" : "ALARM";
            using (var font = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            {
                var sz = g.MeasureString(badgeText, font);
                int bw = (int)sz.Width + BadgeHPadding * 2;
                int bh = (int)sz.Height + BadgeVPadding * 2;
                int bx = Width - bw - 16;
                int by = (Height - bh) / 2;

                var r = new Rectangle(bx, by, bw, bh);
                using (var path = RoundedRect(r, 6))
                using (var bg   = new SolidBrush(Color.FromArgb(30, accent)))
                using (var pen  = new Pen(accent, 1.5f))
                using (var txt  = new SolidBrush(accent))
                {
                    g.FillPath(bg, path);
                    g.DrawPath(pen, path);
                    g.DrawString(badgeText, font, txt,
                        bx + BadgeHPadding,
                        by + (bh - sz.Height) / 2);
                }
            }
        }

        // ────────────────────────────────────────────
        //  Drawing helpers
        // ────────────────────────────────────────────

        private static void DrawTriangleIcon(Graphics g, int x, int y, int size, Color color, float fontScale, float textYRatio)
        {
            var pts = new[]
            {
                new Point(x + size / 2, y),
                new Point(x + size,     y + size),
                new Point(x,            y + size)
            };

            using (var path = new GraphicsPath())
            {
                path.AddLines(pts);
                path.CloseFigure();
                using (var fill = new SolidBrush(color))
                    g.FillPath(fill, path);
            }

            using (var font = new Font("Segoe UI", size * fontScale, FontStyle.Bold))
            {
                const string mark = "!";
                var sz = g.MeasureString(mark, font);
                using (var brush = new SolidBrush(Color.Black))
                    g.DrawString(mark, font, brush,
                        x + (size - sz.Width) / 2,
                        y + size * textYRatio);
            }
        }

        private static GraphicsPath RoundedRect(Rectangle b, int r)
        {
            int d = r * 2;
            var p = new GraphicsPath();
            p.AddArc(b.X,          b.Y,          d, d, 180, 90);
            p.AddArc(b.Right - d,  b.Y,          d, d, 270, 90);
            p.AddArc(b.Right - d,  b.Bottom - d, d, d, 0,   90);
            p.AddArc(b.X,          b.Bottom - d, d, d, 90,  90);
            p.CloseFigure();
            return p;
        }
    }
}
