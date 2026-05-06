using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TestGauge
{
    public class GaugeControl : UserControl
    {
        // Default values moved into the control so it can be reused without per-form init
        private double _currentValue = 1200;
        private double _minValue = 0;
        private double _maxValue = 2000;

        // Simulation (moved from Form1 into the control so it can be used as a self-contained component)
        private readonly Timer _simTimer;
        private readonly Random _rnd = new Random();
        private double _simTarget;
        private bool _simulating = true;
        private int _simInterval = 800;

        // Value label
        private Label _lblValue;

        [Category("Gauge")]
        public Label LblValue { get => _lblValue; }

        [DefaultValue(0.0)]
        [Category("Gauge")]
        public double MinValue
        {
            get => _minValue;
            set { _minValue = value; Invalidate(); }
        }

        [DefaultValue(2000.0)]
        [Category("Gauge")]
        public double MaxValue
        {
            get => _maxValue;
            set { _maxValue = value; Invalidate(); }
        }

        [DefaultValue(1200.0)]
        [Category("Gauge")]
        public double CurrentValue
        {
            get => _currentValue;
            set
            {
                var v = Math.Max(_minValue, Math.Min(_maxValue, value));
                if (Math.Abs(_currentValue - v) > double.Epsilon)
                {
                    _currentValue = v;
                    if (_lblValue != null) _lblValue.Text = FormatValueText();
                    Invalidate();
                }
            }
        }

        public GaugeControl()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.FromArgb(35, 40, 45);

            // Value + unit label (below needle, centered)
            _lblValue = new Label
            {
                Text = FormatValueText(),
                ForeColor = Color.AliceBlue,
                Font = new Font(FontFamily.GenericSansSerif, 22f, FontStyle.Bold),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            Controls.Add(_lblValue);

            // setup internal simulation timer
            _simTimer = new Timer();
            _simTimer.Interval = _simInterval;
            _simTimer.Tick += (s, e) => SimTick();
            if (_simulating)
            {
                _simTarget = _rnd.Next((int)_minValue, (int)_maxValue + 1);
                _simTimer.Start();
            }
        }

        [Category("Gauge")]
        [DefaultValue(true)]
        public bool Simulate
        {
            get => _simulating;
            set
            {
                _simulating = value;
                if (_simulating)
                {
                    if (!_simTimer.Enabled) _simTimer.Start();
                }
                else
                {
                    if (_simTimer.Enabled) _simTimer.Stop();
                }
            }
        }

        [Category("Gauge")]
        [DefaultValue(800)]
        public int SimInterval
        {
            get => _simInterval;
            set
            {
                _simInterval = Math.Max(50, value);
                if (_simTimer != null) _simTimer.Interval = _simInterval;
            }
        }

        private string _unit = "rpm";

        // Colors
        private Color _arcBackColor = Color.FromArgb(60, 100, 120);
        private Color _progressColor = Color.FromArgb(85, 170, 255);
        private Color _needleColor = Color.LightGray;
        private Color _tickColor = Color.FromArgb(120, 160, 180);
        private Color _hubColor = Color.FromArgb(220, 220, 220);

        [Category("Appearance")]
        public string Unit
        {
            get => _unit;
            set { _unit = value ?? string.Empty; if (_lblValue != null) _lblValue.Text = FormatValueText(); }
        }

        [Category("Colors")]
        public Color ArcBackColor
        {
            get => _arcBackColor;
            set { _arcBackColor = value; Invalidate(); }
        }

        [Category("Colors")]
        public Color ProgressColor
        {
            get => _progressColor;
            set { _progressColor = value; Invalidate(); }
        }

        [Category("Colors")]
        public Color NeedleColor
        {
            get => _needleColor;
            set { _needleColor = value; Invalidate(); }
        }

        [Category("Colors")]
        public Color TickColor
        {
            get => _tickColor;
            set { _tickColor = value; Invalidate(); }
        }

        [Category("Colors")]
        public Color HubColor
        {
            get => _hubColor;
            set { _hubColor = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = ClientRectangle;
            g.Clear(BackColor);

            float cx = rect.Width / 2f;
            float cy = rect.Height * 0.62f;
            float radius = Math.Min(rect.Width / 2f - 35f, rect.Height * 0.55f);
            var arcRect = new RectangleF(cx - radius, cy - radius, radius * 2, radius * 2);

            float startAngle = 180f;
            float sweepAngle = 180f;

            using (var pen = new Pen(_arcBackColor, 18f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawArc(pen, arcRect, startAngle, sweepAngle);

            double t = (_currentValue - _minValue) / Math.Max(1.0, (_maxValue - _minValue));
            float progressSweep = (float)(sweepAngle * t);
            using (var pen = new Pen(_progressColor, 18f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawArc(pen, arcRect, startAngle, progressSweep);

            DrawTicks(g, cx, cy, radius, startAngle, sweepAngle);
            DrawNeedle(g, cx, cy, radius, startAngle + progressSweep);
        }

        private void DrawTicks(Graphics g, float cx, float cy, float radius, float startAngle, float sweepAngle)
        {
            int majorTicks = 5;

            using (var pen = new Pen(_tickColor, 2f))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (var font = new Font(FontFamily.GenericSansSerif, 9f))
            {
                for (int i = 0; i <= majorTicks; i++)
                {
                    float angle = (startAngle + sweepAngle * (i / (float)majorTicks)) * (float)(Math.PI / 180.0);
                    float cos = (float)Math.Cos(angle);
                    float sin = (float)Math.Sin(angle);

                    g.DrawLine(pen,
                        new PointF(cx + (radius - 12) * cos, cy + (radius - 12) * sin),
                        new PointF(cx + radius * cos, cy + radius * sin));

                    double value = _minValue + (_maxValue - _minValue) * (i / (double)majorTicks);
                    var labelPoint = new PointF(cx + (radius - 28) * cos, cy + (radius - 28) * sin);
                    g.DrawString(((int)value).ToString(), font, Brushes.White, labelPoint, sf);
                }
            }
        }

        private void DrawNeedle(Graphics g, float cx, float cy, float radius, float angleDegrees)
        {
            float angle = angleDegrees * (float)(Math.PI / 180.0);
            var tip = new PointF(cx + (radius - 28) * (float)Math.Cos(angle), cy + (radius - 28) * (float)Math.Sin(angle));

            using (var pen = new Pen(_needleColor, 4f) { EndCap = LineCap.Round })
                g.DrawLine(pen, new PointF(cx, cy), tip);

            using (var brush = new SolidBrush(_hubColor))
                g.FillEllipse(brush, cx - 6, cy - 6, 12, 12);
        }

        private string FormatValueText()
        {
            return $"{(int)_currentValue} {_unit}";
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_lblValue != null)
            {
                _lblValue.Location = new Point(
                    (Width - _lblValue.Width) / 2,
                    (int)(Height * 0.72f));
            }
            Invalidate();
        }

        private void SimTick()
        {
            // animate value toward a random target to simulate RPM changes
            if (!_simulating) return;
            // if reached target or no target, choose a new one occasionally
            if (Math.Abs(_simTarget - _currentValue) < 1.0)
            {
                _simTarget = _rnd.Next((int)_minValue, (int)_maxValue + 1);
            }
            var current = _currentValue;
            var newValue = current + (_simTarget - current) * 0.25;
            CurrentValue = newValue;
        }
    }
}
