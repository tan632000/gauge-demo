using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TestGauge
{
    public enum EventStatus
    {
        Run,
        Stop,
        Trip,
        Open,
        Close
    }

    public partial class EventControl : UserControl
    {
        // ── Default color palette ──
        private static readonly Color DefaultBackColor      = Color.FromArgb(0x0d, 0x11, 0x17);
        private static readonly Color DefaultBackColorHover = Color.FromArgb(0x16, 0x1b, 0x22);
        private static readonly Color DefaultTextWhite      = Color.White;
        private static readonly Color DefaultTextGray       = Color.FromArgb(0x8b, 0x94, 0x9e);
        private static readonly Color DefaultRunColor       = Color.FromArgb(0x4C, 0xAF, 0x50);
        private static readonly Color DefaultStopColor      = Color.FromArgb(0x9E, 0x9E, 0x9E);
        private static readonly Color DefaultTripColor      = Color.FromArgb(0xFF, 0x4D, 0x4D);
        private static readonly Color DefaultOpenColor      = Color.FromArgb(0x21, 0x96, 0xF3);
        private static readonly Color DefaultCloseColor     = Color.FromArgb(0xFF, 0x98, 0x00);

        private const int DefaultCornerRadius = 10;
        private const int DefaultIconSize     = 20;
        private const int BadgeHPadding       = 14;
        private const int BadgeVPadding       = 5;

        // ── Backing fields ──
        private string _title      = "Title";
        private DateTime _time     = DateTime.Now;
        private EventStatus _status = EventStatus.Run;
        private bool _hovered;

        // ── Customizable color overrides ──
        private Color? _backColorNormal;
        private Color? _backColorHover;
        private Color? _runColor;
        private Color? _stopColor;
        private Color? _tripColor;
        private Color? _openColor;
        private Color? _closeColor;
        private Color? _textColor;
        private Color? _subtextColor;
        private int _cornerRadius = DefaultCornerRadius;
        private int _iconSize     = DefaultIconSize;

        // ────────────────────────────────────────────
        //  Public Properties
        // ────────────────────────────────────────────

        [Category("Event")]
        [Description("Title text, e.g. Pump P-201, Valve V-12.")]
        public string Title
        {
            get => _title;
            set { _title = value; Invalidate(); }
        }

        [Category("Event")]
        [Description("Timestamp displayed next to the icon.")]
        public DateTime Time
        {
            get => _time;
            set { _time = value; Invalidate(); }
        }

        [Category("Event")]
        [Description("Event status: Run, Stop, Trip, Open, Close.")]
        public EventStatus Status
        {
            get => _status;
            set { _status = value; Invalidate(); }
        }

        [Category("Event.Appearance")]
        [Description("Normal background color. Null = default dark navy.")]
        public Color? BackColorNormal
        {
            get => _backColorNormal;
            set { _backColorNormal = value; Invalidate(); }
        }

        [Category("Event.Appearance")]
        [Description("Hover background color. Null = default lighter shade.")]
        public Color? BackColorHover
        {
            get => _backColorHover;
            set { _backColorHover = value; Invalidate(); }
        }

        [Category("Event.Appearance")]
        [Description("Run status color. Null = default green.")]
        public Color? RunColor
        {
            get => _runColor;
            set { _runColor = value; Invalidate(); }
        }

        [Category("Event.Appearance")]
        [Description("Stop status color. Null = default gray.")]
        public Color? StopColor
        {
            get => _stopColor;
            set { _stopColor = value; Invalidate(); }
        }

        [Category("Event.Appearance")]
        [Description("Trip status color. Null = default red.")]
        public Color? TripColor
        {
            get => _tripColor;
            set { _tripColor = value; Invalidate(); }
        }

        [Category("Event.Appearance")]
        [Description("Open status color. Null = default blue.")]
        public Color? OpenColor
        {
            get => _openColor;
            set { _openColor = value; Invalidate(); }
        }

        [Category("Event.Appearance")]
        [Description("Close status color. Null = default orange.")]
        public Color? CloseColor
        {
            get => _closeColor;
            set { _closeColor = value; Invalidate(); }
        }

        [Category("Event.Appearance")]
        [Description("Title text color. Null = default white.")]
        public Color? TextColor
        {
            get => _textColor;
            set { _textColor = value; Invalidate(); }
        }

        [Category("Event.Appearance")]
        [Description("Time text color. Null = default gray.")]
        public Color? SubtextColor
        {
            get => _subtextColor;
            set { _subtextColor = value; Invalidate(); }
        }

        [Category("Event.Appearance")]
        [Description("Corner radius in pixels.")]
        public int CornerRadius
        {
            get => _cornerRadius;
            set { _cornerRadius = value; Invalidate(); }
        }

        [Category("Event.Appearance")]
        [Description("Circle icon diameter in pixels.")]
        public int IconSize
        {
            get => _iconSize;
            set { _iconSize = value; Invalidate(); }
        }

        // ────────────────────────────────────────────
        //  Constructor
        // ────────────────────────────────────────────

        public EventControl()
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
        private Color ResolveRun()          => _runColor        ?? DefaultRunColor;
        private Color ResolveStop()         => _stopColor       ?? DefaultStopColor;
        private Color ResolveTrip()         => _tripColor       ?? DefaultTripColor;
        private Color ResolveOpen()         => _openColor       ?? DefaultOpenColor;
        private Color ResolveClose()        => _closeColor      ?? DefaultCloseColor;
        private Color ResolveText()         => _textColor       ?? DefaultTextWhite;
        private Color ResolveSubtext()      => _subtextColor    ?? DefaultTextGray;

        private Color ResolveStatusColor()
        {
            switch (_status)
            {
                case EventStatus.Run:   return ResolveRun();
                case EventStatus.Stop:  return ResolveStop();
                case EventStatus.Trip:  return ResolveTrip();
                case EventStatus.Open:  return ResolveOpen();
                case EventStatus.Close: return ResolveClose();
                default:                return DefaultRunColor;
            }
        }

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
            Color statusColor = ResolveStatusColor();
            int radius = _cornerRadius;

            // Background
            using (var path = RoundedRect(rect, radius))
            using (var bg = new SolidBrush(_hovered ? ResolveBackColorHov() : ResolveBackColor()))
                g.FillPath(bg, path);

            int x = 16;

            // Circle icon
            int circleY = (Height - _iconSize) / 2;
            using (var brush = new SolidBrush(statusColor))
                g.FillEllipse(brush, x, circleY, _iconSize, _iconSize);
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
            using (var locFont = new Font("Segoe UI", 10f, FontStyle.Bold))
            using (var locBrush = new SolidBrush(ResolveText()))
            {
                float tH = g.MeasureString(_title, locFont).Height;
                float y0 = (Height - tH) / 2;
                g.DrawString(_title, locFont, locBrush, x, y0);
            }

            // Status badge
            string badgeText = _status.ToString().ToUpper();
            using (var font = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            {
                var sz = g.MeasureString(badgeText, font);
                int bw = (int)sz.Width + BadgeHPadding * 2;
                int bh = (int)sz.Height + BadgeVPadding * 2;
                int bx = Width - bw - 16;
                int by = (Height - bh) / 2;

                var r = new Rectangle(bx, by, bw, bh);
                using (var path = RoundedRect(r, 6))
                using (var bg  = new SolidBrush(Color.FromArgb(30, statusColor)))
                using (var pen = new Pen(statusColor, 1.5f))
                using (var txt = new SolidBrush(statusColor))
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
