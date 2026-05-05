using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;

namespace TestGauge
{
    // Simple WinForms UserControl hosting a LiveChartsCore CartesianChart (line + column sample)
    public class LiveChartsCoreControl : UserControl
    {
        private CartesianChart chart;

        public LiveChartsCoreControl()
        {
            // avoid creating LiveCharts objects at design-time because designer may not have
            // the native/skia dependencies available and that causes FileNotFound exceptions.
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime || DesignMode)
            {
                // design-time placeholder
                this.BackColor = Color.WhiteSmoke;
                this.Size = new Size(300, 200);
                var lbl = new Label { Text = "LiveChartsCoreControl (design-time)", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
                this.Controls.Add(lbl);
            }
            else
            {
                InitializeChart();
            }
        }

        private void InitializeChart()
        {
            chart = new CartesianChart
            {
                Dock = DockStyle.Fill,
                LegendPosition = LiveChartsCore.Measure.LegendPosition.Right
            };

            // series: Mary (line) and Ana (column) with sample values
            chart.Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Name = "Mary",
                    Values = new double[] { 5, 10, 8, 4 },
                    GeometrySize = 10
                },
                new ColumnSeries<double>
                {
                    Name = "Ana",
                    Values = new double[] { 4, 7, 3, 8 }
                }
            };

            // optional: simple axis labels / styling
            chart.XAxes = new Axis[]
            {
                new Axis { LabelsRotation = 0 }
            };

            chart.YAxes = new Axis[]
            {
                new Axis { MinLimit = 0 }
            };

            this.Controls.Add(chart);
            this.BackColor = Color.White;
            this.Size = new Size(600, 320);
        }
    }
}