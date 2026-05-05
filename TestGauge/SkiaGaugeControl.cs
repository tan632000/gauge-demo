using System;
using System.ComponentModel;
using System.Windows.Forms;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace TestGauge
{
    // A gauge control implemented with SkiaSharp and an internal simulation timer.
    public class SkiaGaugeControl : SKControl
    {
        private double _value = 1200;
        private double _from = 0;
        private double _to = 2000;
        private string _title = "TỐC ĐỘ TRỘN (RPM)";
        private string _unit = "rpm";

        private readonly Timer _timer;

        // do not simulate by default to avoid running timers at design-time
        private bool _simulate = false;
        private int _interval = 80; // animation tick ms (faster for smoothness)
        // appearance/custom colors
        private System.Drawing.Color _progressColor = System.Drawing.Color.FromArgb(85, 170, 255);
        private System.Drawing.Color _arcBackgroundColor = System.Drawing.Color.FromArgb(60, 100, 120);
        private System.Drawing.Color _needleColor = System.Drawing.Color.LightGray;
        private System.Drawing.Color _titleColor = System.Drawing.Color.FromArgb(200, 200, 210);
        private System.Drawing.Color _setpointColor = System.Drawing.Color.FromArgb(160, 200, 230);
        // up to 4 segment colors for multi-color arc (e.g. temperature)
        private System.Drawing.Color _segmentColor1 = System.Drawing.Color.Empty;
        private System.Drawing.Color _segmentColor2 = System.Drawing.Color.Empty;
        private System.Drawing.Color _segmentColor3 = System.Drawing.Color.Empty;
        private System.Drawing.Color _segmentColor4 = System.Drawing.Color.Empty;
        private double _segmentWeight1 = 1.0;
        private double _segmentWeight2 = 1.0;
        private double _segmentWeight3 = 1.0;
        private double _segmentWeight4 = 1.0;
        private double _setpoint = 0.0;
        private double _loadPercent = 0.0;

        public SkiaGaugeControl()
        {
            DoubleBuffered = true;
            BackColor = System.Drawing.Color.FromArgb(35, 40, 45);
            PaintSurface += OnPaintSurface;

            _timer = new Timer();
            _timer.Interval = _interval;
            _timer.Tick += (s, e) => SimTick();
            // do not start timer here; start only at runtime when control handle is created
        }

        protected override void OnHandleCreated(System.EventArgs e)
        {
            base.OnHandleCreated(e);
            // ensure timer runs only at runtime, not in designer
            if (!DesignMode && LicenseManager.UsageMode != LicenseUsageMode.Designtime && _simulate)
            {
                // ensure animation starts from minimum when simulating toward setpoint
                Value = _from;
                _timer.Start();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer.Tick -= (s, e) => SimTick();
                }
            }
            base.Dispose(disposing);
        }

        [Category("Appearance")]
        [DefaultValue("TỐC ĐỘ TRỘN (RPM)")]
        public string Title { get => _title; set { _title = value ?? string.Empty; Invalidate(); } }

        [Category("Appearance")]
        [DefaultValue("rpm")]
        public string Unit { get => _unit; set { _unit = value ?? string.Empty; Invalidate(); } }

        [Category("Appearance")]
        public System.Drawing.Color ProgressColor { get => _progressColor; set { _progressColor = value; Invalidate(); } }

        [Category("Appearance")]
        public System.Drawing.Color ArcBackgroundColor { get => _arcBackgroundColor; set { _arcBackgroundColor = value; Invalidate(); } }

        [Category("Appearance")]
        public System.Drawing.Color NeedleColor { get => _needleColor; set { _needleColor = value; Invalidate(); } }

        [Category("Appearance")]
        public System.Drawing.Color TitleColor { get => _titleColor; set { _titleColor = value; Invalidate(); } }

        [Category("Appearance")]
        public System.Drawing.Color SetpointColor { get => _setpointColor; set { _setpointColor = value; Invalidate(); } }

        [Category("Appearance")]
        public System.Drawing.Color SegmentColor1 { get => _segmentColor1; set { _segmentColor1 = value; Invalidate(); } }

        [Category("Appearance")]
        public System.Drawing.Color SegmentColor2 { get => _segmentColor2; set { _segmentColor2 = value; Invalidate(); } }

        [Category("Appearance")]
        public System.Drawing.Color SegmentColor3 { get => _segmentColor3; set { _segmentColor3 = value; Invalidate(); } }

        [Category("Appearance")]
        public System.Drawing.Color SegmentColor4 { get => _segmentColor4; set { _segmentColor4 = value; Invalidate(); } }

        [Category("Gauge")]
        [DefaultValue(1.0)]
        public double SegmentWeight1 { get => _segmentWeight1; set { _segmentWeight1 = Math.Max(0.0, value); Invalidate(); } }

        [Category("Gauge")]
        [DefaultValue(1.0)]
        public double SegmentWeight2 { get => _segmentWeight2; set { _segmentWeight2 = Math.Max(0.0, value); Invalidate(); } }

        [Category("Gauge")]
        [DefaultValue(1.0)]
        public double SegmentWeight3 { get => _segmentWeight3; set { _segmentWeight3 = Math.Max(0.0, value); Invalidate(); } }

        [Category("Gauge")]
        [DefaultValue(1.0)]
        public double SegmentWeight4 { get => _segmentWeight4; set { _segmentWeight4 = Math.Max(0.0, value); Invalidate(); } }

        [Category("Gauge")]
        [DefaultValue(0.0)]
        public double Setpoint { get => _setpoint; set { _setpoint = value; Invalidate(); } }

        [Category("Gauge")]
        [DefaultValue(0.0)]
        public double LoadPercent { get => _loadPercent; set { _loadPercent = value; Invalidate(); } }

        [Category("Gauge")]
        [DefaultValue(0.0)]
        public double From { get => _from; set { _from = value; Invalidate(); } }

        [Category("Gauge")]
        [DefaultValue(2000.0)]
        public double To { get => _to; set { _to = value; Invalidate(); } }

        [Category("Gauge")]
        [DefaultValue(1200.0)]
        public double Value { get => _value; set { _value = Math.Max(_from, Math.Min(_to, value)); Invalidate(); } }

        [Category("Gauge")]
        [DefaultValue(false)]
        public bool Simulate
        {
            get => _simulate;
            set
            {
                _simulate = value;
                if (DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;
                if (_simulate) _timer.Start(); else _timer.Stop();
            }
        }

        [Category("Gauge")]
        [DefaultValue(80)]
        public int Interval { get => _interval; set { _interval = Math.Max(10, value); if (_timer != null) _timer.Interval = _interval; } }

        private void SimTick()
        {
            if (!_simulate) return;
            // animate towards Setpoint and stop when reached
            var target = Math.Max(_from, Math.Min(_to, _setpoint));
            var current = _value;
            var diff = target - current;
            if (Math.Abs(diff) < 0.5)
            {
                Value = target;
                // stop animation once reached
                _timer.Stop();
                return;
            }
            // smooth interpolation step
            var step = diff * 0.12; // tweak speed
            Value = current + step;
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;
            canvas.Clear(new SKColor(35, 40, 45));

            var w = info.Width;
            var h = info.Height;

            // parameters - adjust center to move arc up and leave room for large center text
            float padding = Math.Min(w, h) * 0.06f;
            var center = new SKPoint(w / 2f, h * 0.56f);
            float radius = Math.Min(w - padding * 2, (h - padding * 2) * 2) / 2f;

            // angles
            float startAngle = 135f;
            float sweep = 270f;

            // background arc and optional segmented arc
            var rectArc = new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius);
            var segments = new System.Collections.Generic.List<SKColor>();
            var weights = new System.Collections.Generic.List<double>();
            if (_segmentColor1 != System.Drawing.Color.Empty) { segments.Add(ToSK(_segmentColor1)); weights.Add(_segmentWeight1); }
            if (_segmentColor2 != System.Drawing.Color.Empty) { segments.Add(ToSK(_segmentColor2)); weights.Add(_segmentWeight2); }
            if (_segmentColor3 != System.Drawing.Color.Empty) { segments.Add(ToSK(_segmentColor3)); weights.Add(_segmentWeight3); }
            if (_segmentColor4 != System.Drawing.Color.Empty) { segments.Add(ToSK(_segmentColor4)); weights.Add(_segmentWeight4); }

            if (segments.Count > 0)
            {
                // calculate total weight
                double totalW = 0.0;
                foreach (var wght in weights) totalW += wght;
                if (totalW <= 0) totalW = weights.Count; // fallback to equal

                // draw full segmented background (slightly faded)
                float currentStart = startAngle;
                for (int i = 0; i < segments.Count; i++)
                {
                    float segSweep = (float)(sweep * (weights[i] / totalW));
                    var c = segments[i];
                    var faded = new SKColor(c.Red, c.Green, c.Blue, 200);
                    using (var paint = new SKPaint { IsAntialias = true, StrokeWidth = 18f, Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round, Color = faded })
                    {
                        canvas.DrawArc(rectArc, currentStart, segSweep, false, paint);
                    }
                    currentStart += segSweep;
                }

                // draw progress using same segment colors up to progressSweep
                double t = (_value - _from) / Math.Max(1.0, (_to - _from));
                float progressSweep = (float)(sweep * t);
                float remaining = progressSweep;
                currentStart = startAngle;
                for (int i = 0; i < segments.Count && remaining > 0.001f; i++)
                {
                    float segSweep = (float)(sweep * (weights[i] / totalW));
                    float draw = Math.Min(segSweep, remaining);
                    using (var paint = new SKPaint { IsAntialias = true, StrokeWidth = 14f, Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round, Color = segments[i] })
                    {
                        canvas.DrawArc(rectArc, currentStart, draw, false, paint);
                    }
                    remaining -= draw;
                    currentStart += segSweep;
                }
            }
            else
            {
                // single-color background
                using (var paint = new SKPaint { IsAntialias = true, StrokeWidth = 14f, Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Butt, Color = ToSK(_arcBackgroundColor) })
                {
                    canvas.DrawArc(rectArc, startAngle, sweep, false, paint);
                }
                // progress arc (single color)
                double t = (_value - _from) / Math.Max(1.0, (_to - _from));
                float progressSweep = (float)(sweep * t);
                using (var paint = new SKPaint { IsAntialias = true, StrokeWidth = 14f, Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round, Color = ToSK(_progressColor) })
                {
                    canvas.DrawArc(rectArc, startAngle, progressSweep, false, paint);
                }
            }

            // no separate setpoint needle: main needle animates to Setpoint

            // needle
            double t2 = (_value - _from) / Math.Max(1.0, (_to - _from));
            float progressSweep_forNeedle = (float)(sweep * t2);
            float angle = (startAngle + progressSweep_forNeedle) * (float)(Math.PI / 180.0);
            var tip = new SKPoint(center.X + (float)((radius - 36) * Math.Cos(angle)), center.Y + (float)((radius - 36) * Math.Sin(angle)));
            using (var paint = new SKPaint { IsAntialias = true, StrokeWidth = 6f, Color = ToSK(_needleColor), StrokeCap = SKStrokeCap.Round })
            {
                canvas.DrawLine(center, tip, paint);
            }

            // hub
            using (var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = ToSK(System.Drawing.Color.FromArgb(220, 220, 220)) })
            {
                canvas.DrawCircle(center, 6f, paint);
            }

            // center text value
            using (var valuePaint = new SKPaint { IsAntialias = true, Color = SKColors.AliceBlue, TextAlign = SKTextAlign.Center })
            using (var unitPaint = new SKPaint { IsAntialias = true, Color = ToSK(System.Drawing.Color.FromArgb(200, 200, 200)), TextAlign = SKTextAlign.Center })
            {
                valuePaint.TextSize = Math.Max(12, h * 0.12f);
                unitPaint.TextSize = Math.Max(8, h * 0.035f);
                var valueText = _unit == "rpm" ? ((int)_value).ToString() : _value.ToString("0.0");
                canvas.DrawText(valueText, center.X, center.Y - (valuePaint.TextSize * 0.1f), valuePaint);
                canvas.DrawText(_unit, center.X, center.Y + unitPaint.TextSize + 6, unitPaint);
            }

            // setpoint / load small label
            using (var spPaint = new SKPaint { IsAntialias = true, Color = ToSK(_setpointColor), TextAlign = SKTextAlign.Center })
            {
                spPaint.TextSize = Math.Max(8, h * 0.03f);
                string spText;
                if (!string.IsNullOrEmpty(_unit) && _unit == "A" && _loadPercent > 0.0)
                {
                    spText = $"Load: {Math.Round(_loadPercent)}%";
                }
                else
                {
                    var spVal = _unit == "rpm" ? ((int)_setpoint).ToString() : _setpoint.ToString("0.0");
                    spText = $"Setpoint: {spVal} {_unit}";
                }
                canvas.DrawText(spText, center.X, center.Y + spPaint.TextSize * 3.6f, spPaint);
            }

            // top title and max
            using (var titlePaint = new SKPaint { IsAntialias = true, Color = ToSK(_titleColor) })
            using (var maxPaint = new SKPaint { IsAntialias = true, Color = ToSK(System.Drawing.Color.FromArgb(180, 180, 190)), TextAlign = SKTextAlign.Right })
            {
                titlePaint.TextSize = Math.Max(9, h * 0.035f);
                maxPaint.TextSize = Math.Max(9, h * 0.03f);
                canvas.DrawText(_title, padding, titlePaint.TextSize + 4, titlePaint);
                canvas.DrawText(((int)_to).ToString(), w - padding, maxPaint.TextSize + 4, maxPaint);
            }

        }

        private SKColor ToSK(System.Drawing.Color c)
        {
            return new SKColor(c.R, c.G, c.B, c.A);
        }
    }
}
