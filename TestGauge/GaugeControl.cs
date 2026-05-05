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
        private double _value = 1200;
        private double _from = 0;
        private double _to = 2000;

        // Simulation (moved from Form1 into the control so it can be used as a self-contained component)
        private readonly Timer _simTimer;
        private readonly Random _rnd = new Random();
        private double _simTarget;
        private bool _simulating = true;
        private int _simInterval = 800;

        // Labels / decoration
        private string _title = "TỐC ĐỘ TRỌN (RPM)";
        private bool _showTitle = true;
        private bool _showMaxLabel = true;
        private string _maxLabel = "2000";
        private bool _showSetpoint = true;
        private double _setpoint = 1200;

        [DefaultValue(0.0)]
        [Category("Gauge")]
        public double From
        {
            get => _from;
            set { _from = value; Invalidate(); }
        }

        [DefaultValue(2000.0)]
        [Category("Gauge")]
        public double To
        {
            get => _to;
            set { _to = value; Invalidate(); }
        }

        [DefaultValue(1200.0)]
        [Category("Gauge")]
        public double Value
        {
            get => _value;
            set
            {
                var v = Math.Max(_from, Math.Min(_to, value));
                if (Math.Abs(_value - v) > double.Epsilon)
                {
                    _value = v;
                    Invalidate();
                }
            }
        }

        public GaugeControl()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.FromArgb(35, 40, 45);
            // ensure properties are in a consistent state and designer won't need to set them
            this.From = _from;
            this.To = _to;
            this.Value = _value;
            // setup internal simulation timer so the control can animate itself when used as a component
            _simTimer = new Timer();
            _simTimer.Interval = _simInterval;
            _simTimer.Tick += (s, e) => SimTick();
            if (_simulating)
            {
                // pick initial target
                _simTarget = _rnd.Next((int)_from, (int)_to + 1);
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

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool ShowTitle
        {
            get => _showTitle;
            set { _showTitle = value; Invalidate(); }
        }

        [Category("Appearance")]
        public string Title
        {
            get => _title;
            set { _title = value ?? string.Empty; Invalidate(); }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool ShowMaxLabel
        {
            get => _showMaxLabel;
            set { _showMaxLabel = value; Invalidate(); }
        }

        [Category("Appearance")]
        public string MaxLabel
        {
            get => _maxLabel;
            set { _maxLabel = value ?? string.Empty; Invalidate(); }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool ShowSetpoint
        {
            get => _showSetpoint;
            set { _showSetpoint = value; Invalidate(); }
        }

        [Category("Appearance")]
        public double Setpoint
        {
            get => _setpoint;
            set { _setpoint = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = ClientRectangle;
            g.Clear(BackColor);

            // draw title and max label
            if (_showTitle)
            {
                using (var titleFont = new Font(FontFamily.GenericSansSerif, 10f, FontStyle.Bold))
                using (var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near })
                using (var brush = new SolidBrush(Color.FromArgb(200, 200, 210)))
                {
                    g.DrawString(_title, titleFont, brush, new PointF(12, 6), sf);
                }
            }
            if (_showMaxLabel)
            {
                using (var maxFont = new Font(FontFamily.GenericSansSerif, 9f))
                using (var sf = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Near })
                using (var brush = new SolidBrush(Color.FromArgb(180, 180, 190)))
                {
                    g.DrawString(_maxLabel, maxFont, brush, new RectangleF(0, 6, rect.Width - 12, 18), sf);
                }
            }

            // padding and sizes
            int padding = 20;
            var arcRect = new Rectangle(padding, padding, rect.Width - padding * 2, (rect.Height - padding * 2) * 2);

            // arc parameters
            float startAngle = 135f; // left-bottom
            float sweepAngle = 270f; // to right-bottom

            // base arc (background)
            using (var pen = new Pen(Color.FromArgb(60, 100, 120), 18f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawArc(pen, arcRect, startAngle, sweepAngle);
            }

            // progress arc
            double t = (_value - _from) / Math.Max(1.0, (_to - _from));
            float progressSweep = (float)(sweepAngle * t);
            using (var pen = new Pen(Color.FromArgb(85, 170, 255), 18f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawArc(pen, arcRect, startAngle, progressSweep);
            }

            // draw ticks and labels
            DrawTicks(g, arcRect, startAngle, sweepAngle);

            // draw needle
            DrawNeedle(g, arcRect, startAngle + progressSweep);

            // draw center text
            DrawCenterText(g);
        }

        private void DrawTicks(Graphics g, Rectangle arcRect, float startAngle, float sweepAngle)
        {
            int majorTicks = 5;
            var center = new PointF(ClientRectangle.Width / 2f, ClientRectangle.Height * 0.75f);
            float radius = Math.Min(arcRect.Width, arcRect.Height) / 2f;

            using (var pen = new Pen(Color.FromArgb(120, 160, 180), 2f))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (var font = new Font(FontFamily.GenericSansSerif, 9f))
            {
                for (int i = 0; i <= majorTicks; i++)
                {
                    float angle = (startAngle + sweepAngle * (i / (float)majorTicks)) * (float)(Math.PI / 180.0);
                    var outer = new PointF(center.X + (float)(radius * Math.Cos(angle)), center.Y + (float)(radius * Math.Sin(angle)));
                    var inner = new PointF(center.X + (float)((radius - 12) * Math.Cos(angle)), center.Y + (float)((radius - 12) * Math.Sin(angle)));
                    g.DrawLine(pen, inner, outer);

                    // label
                    double value = _from + (_to - _from) * (i / (double)majorTicks);
                    var labelPoint = new PointF(center.X + (float)((radius - 28) * Math.Cos(angle)), center.Y + (float)((radius - 28) * Math.Sin(angle)));
                    g.DrawString(((int)value).ToString(), font, Brushes.White, labelPoint, sf);
                }
            }
        }

        private void DrawNeedle(Graphics g, Rectangle arcRect, float angleDegrees)
        {
            var center = new PointF(ClientRectangle.Width / 2f, ClientRectangle.Height * 0.75f);
            float radius = Math.Min(arcRect.Width, arcRect.Height) / 2f;
            float angle = angleDegrees * (float)(Math.PI / 180.0);
            var tip = new PointF(center.X + (float)((radius - 28) * Math.Cos(angle)), center.Y + (float)((radius - 28) * Math.Sin(angle)));

            using (var pen = new Pen(Color.LightGray, 4f) { EndCap = LineCap.Round })
            {
                g.DrawLine(pen, center, tip);
            }

            // hub
            using (var brush = new SolidBrush(Color.FromArgb(220, 220, 220)))
            {
                g.FillEllipse(brush, center.X - 6, center.Y - 6, 12, 12);
            }
        }

        private void DrawCenterText(Graphics g)
        {
            string valueText = ((int)_value).ToString();
            string unit = "rpm";
            using (var valueFont = new Font(FontFamily.GenericSansSerif, 28f, FontStyle.Bold))
            using (var unitFont = new Font(FontFamily.GenericSansSerif, 10f))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                var center = new PointF(ClientRectangle.Width / 2f, ClientRectangle.Height * 0.62f);
                g.DrawString(valueText, valueFont, Brushes.AliceBlue, center, sf);
                g.DrawString(unit, unitFont, Brushes.LightGray, new PointF(center.X, center.Y + 26), sf);
                // setpoint / small label below
                if (_showSetpoint)
                {
                    using (var spFont = new Font(FontFamily.GenericSansSerif, 9f))
                    using (var spBrush = new SolidBrush(Color.FromArgb(160, 200, 230)))
                    {
                        var spText = $"Setpoint: {(int)_setpoint} rpm";
                        g.DrawString(spText, spFont, spBrush, new PointF(center.X, center.Y + 46), sf);
                    }
                }
            }
        }

        private void SimTick()
        {
            // animate value toward a random target to simulate RPM changes
            if (!_simulating) return;
            // if reached target or no target, choose a new one occasionally
            if (Math.Abs(_simTarget - _value) < 1.0)
            {
                _simTarget = _rnd.Next((int)_from, (int)_to + 1);
            }
            var current = _value;
            var newValue = current + (_simTarget - current) * 0.25;
            Value = newValue;
            // optionally update setpoint as last value so visual matches
            _setpoint = _value;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // GaugeControl
            // 
            this.Name = "GaugeControl";
            this.Size = new System.Drawing.Size(199, 146);
            this.ResumeLayout(false);

        }
    }
}
