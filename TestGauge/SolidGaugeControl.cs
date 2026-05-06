using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;
using LiveChartsCore.Measure;

namespace TestGauge
{
    public class SolidGaugeControl : UserControl
    {
        private PieChart _chart;
        private Label _lblValue;
        private Label _lblTitle;

        private double _value = 65;
        private double _max = 100;
        private string _title = "SOLID GAUGE";
        private string _unit = "%";
        private Color _gaugeColor = Color.FromArgb(85, 170, 255);

        private readonly Timer _timer;
        private bool _simulate = true;
        private int _interval = 50;
        private double _targetValue = 65;
        private readonly Random _rnd = new Random();

        private PieSeries<double> _valueSeries;
        private PieSeries<double> _bgSeries;

        public SolidGaugeControl()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime || DesignMode)
            {
                BackColor = Color.FromArgb(35, 40, 45);
                Size = new Size(250, 250);
                var lbl = new Label
                {
                    Text = "SolidGaugeControl (design-time)",
                    Dock = DockStyle.Fill,
                    ForeColor = Color.White,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                Controls.Add(lbl);
                return;
            }

            InitializeChart();

            _timer = new Timer { Interval = _interval };
            _timer.Tick += (s, e) => SimTick();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!DesignMode && LicenseManager.UsageMode != LicenseUsageMode.Designtime && _simulate)
            {
                _timer.Start();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer?.Stop();
                _timer?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeChart()
        {
            BackColor = Color.FromArgb(35, 40, 45);

            _bgSeries = new PieSeries<double>
            {
                Values = new double[] { _max - _value },
                Fill = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(
                    new SkiaSharp.SKColor(60, 100, 120, 100)),
                InnerRadius = 80,
                Stroke = null,
                IsHoverable = false
            };

            _valueSeries = new PieSeries<double>
            {
                Values = new double[] { _value },
                Fill = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(
                    new SkiaSharp.SKColor(_gaugeColor.R, _gaugeColor.G, _gaugeColor.B)),
                InnerRadius = 80,
                Stroke = null,
                IsHoverable = false
            };

            _chart = new PieChart
            {
                Dock = DockStyle.Fill,
                Series = new ISeries[] { _bgSeries, _valueSeries },
                LegendPosition = LegendPosition.Hidden,
                InitialRotation = -225,
                MaxAngle = 270,
                MinValue = 0,
                BackColor = Color.FromArgb(35, 40, 45)
            };

            _lblTitle = new Label
            {
                Text = _title,
                ForeColor = Color.FromArgb(200, 200, 210),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 28,
                BackColor = Color.Transparent
            };

            _lblValue = new Label
            {
                Text = FormatValue(),
                ForeColor = Color.AliceBlue,
                Font = new Font("Segoe UI", 22f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            Controls.Add(_chart);
            Controls.Add(_lblValue);
            Controls.Add(_lblTitle);

            _lblValue.BringToFront();
            _lblTitle.BringToFront();

            // center the value label
            _lblValue.Location = new Point(
                (Width - _lblValue.Width) / 2,
                (Height - _lblValue.Height) / 2 + _lblTitle.Height / 2);
            _lblValue.Anchor = AnchorStyles.None;

            Size = new Size(250, 250);
        }

        private string FormatValue()
        {
            return _unit == "%" ? $"{_value:0}{_unit}" : $"{_value:0.0} {_unit}";
        }

        private void UpdateSeries()
        {
            if (_chart == null) return;
            var clamped = Math.Max(0, Math.Min(_max, _value));
            _valueSeries.Values = new double[] { clamped };
            _bgSeries.Values = new double[] { _max - clamped };
            _lblValue.Text = FormatValue();
        }

        private void SimTick()
        {
            if (!_simulate) return;
            if (Math.Abs(_targetValue - _value) < 0.5)
            {
                _targetValue = _rnd.Next(0, (int)_max + 1);
            }
            _value += (_targetValue - _value) * 0.08;
            UpdateSeries();
        }

        // --- Public properties ---

        [Category("Gauge")]
        [DefaultValue(65.0)]
        public double Value
        {
            get => _value;
            set { _value = Math.Max(0, Math.Min(_max, value)); UpdateSeries(); }
        }

        [Category("Gauge")]
        [DefaultValue(100.0)]
        public double Max
        {
            get => _max;
            set { _max = Math.Max(1, value); UpdateSeries(); }
        }

        [Category("Appearance")]
        [DefaultValue("SOLID GAUGE")]
        public string Title
        {
            get => _title;
            set { _title = value ?? string.Empty; if (_lblTitle != null) _lblTitle.Text = _title; }
        }

        [Category("Appearance")]
        [DefaultValue("%")]
        public string Unit
        {
            get => _unit;
            set { _unit = value ?? string.Empty; UpdateSeries(); }
        }

        [Category("Appearance")]
        public Color GaugeColor
        {
            get => _gaugeColor;
            set
            {
                _gaugeColor = value;
                if (_valueSeries != null)
                {
                    _valueSeries.Fill = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(
                        new SkiaSharp.SKColor(_gaugeColor.R, _gaugeColor.G, _gaugeColor.B));
                }
            }
        }

        [Category("Gauge")]
        [DefaultValue(true)]
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
        [DefaultValue(50)]
        public int Interval
        {
            get => _interval;
            set { _interval = Math.Max(10, value); if (_timer != null) _timer.Interval = _interval; }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_lblValue != null)
            {
                _lblValue.Location = new Point(
                    (Width - _lblValue.Width) / 2,
                    (Height - _lblValue.Height) / 2 + (_lblTitle?.Height ?? 0) / 2);
            }
        }
    }
}
