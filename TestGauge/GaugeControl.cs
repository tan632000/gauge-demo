using System;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TestGauge
{
    public class GaugeControl : UserControl
    {
        private double _value = 0;
        private double _from = 0;
        private double _to = 100;

        [Category("Gauge")]
        public double From
        {
            get => _from;
            set { _from = value; Invalidate(); }
        }

        [Category("Gauge")]
        public double To
        {
            get => _to;
            set { _to = value; Invalidate(); }
        }

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
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = ClientRectangle;
            g.Clear(BackColor);

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
            }
        }
    }
}
