using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace ZOYI
{
    public partial class ChartPanel : UserControl
    {
        private class DataPoint
        {
            public DateTime Time { get; set; }
            public float Value { get; set; }
            public string Unit { get; set; } = "";
            public string Mode { get; set; } = "";
        }

        private List<DataPoint> dataPoints = new List<DataPoint>();
        private object dataLock = new object();

        private bool isPaused = false;
        private int timeWindowSeconds = 60;
        private float yMin = 0;
        private float yMax = 10;

        private float currentMin = float.MaxValue;
        private float currentMax = float.MinValue;
        private double currentSum = 0;
        private int currentCount = 0;

        private string currentUnit = "V";
        private string currentMode = "DC";
        private float lastValue = 0;

        private System.Windows.Forms.Timer chartTimer;
        private const int UpdateIntervalMs = 100;

        public ChartPanel()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            chartTimer = new System.Windows.Forms.Timer();
            chartTimer.Interval = UpdateIntervalMs;
            chartTimer.Tick += ChartTimer_Tick;
            chartTimer.Start();
        }

        public void AddDataPoint(FrameDecoder frame_dec)
        {
            if (isPaused) return;

            float val = 0;
            bool valid = float.TryParse(frame_dec.Value, CultureInfo.InvariantCulture.NumberFormat, out val);

            if (!valid) return;
            if (frame_dec.Value != null && frame_dec.Value.Trim() == "") return;
            if (frame_dec.Value != null && frame_dec.Value.Contains("OL")) return;

            string unit = frame_dec.Unit ?? "";
            string mode = frame_dec.Mode2 ?? "";

            if (string.IsNullOrEmpty(unit)) return;

            lock (dataLock)
            {
                dataPoints.Add(new DataPoint
                {
                    Time = DateTime.Now,
                    Value = val,
                    Unit = unit,
                    Mode = mode
                });

                if (!string.IsNullOrEmpty(unit)) currentUnit = unit;
                if (!string.IsNullOrEmpty(mode)) currentMode = mode;
                lastValue = val;

                currentMax = Math.Max(currentMax, val);
                currentSum += val;
                currentCount++;

                if (Math.Abs(val) > 0.1f)
                {
                    currentMin = Math.Min(currentMin, val);
                }
            }
        }

        private void ChartTimer_Tick(object sender, EventArgs e)
        {
            CleanupOldPoints();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int width = Width;
            int height = Height;

            g.Clear(Color.FromArgb(13, 13, 13));

            int chartLeft = 70;
            int chartRight = width - 20;
            int chartTop = 60;
            int chartBottom = height - 80;
            int chartWidth = chartRight - chartLeft;
            int chartHeight = chartBottom - chartTop;

            if (chartWidth <= 0 || chartHeight <= 0) return;

            DrawGrid(g, chartLeft, chartTop, chartWidth, chartHeight);
            DrawData(g, chartLeft, chartTop, chartWidth, chartHeight);
            DrawStatistics(g, width, chartBottom);
            DrawHeader(g, width);
        }

        private void DrawGrid(Graphics g, int left, int top, int width, int height)
        {
            using (Pen gridPen = new Pen(Color.FromArgb(40, 40, 60), 1))
            {
                for (int i = 0; i <= 10; i++)
                {
                    int y = top + (int)(height * i / 10.0f);
                    g.DrawLine(gridPen, left, y, left + width, y);
                }
                for (int i = 0; i <= 10; i++)
                {
                    int x = left + (int)(width * i / 10.0f);
                    g.DrawLine(gridPen, x, top, x, top + height);
                }
            }

            using (Pen borderPen = new Pen(Color.FromArgb(80, 80, 120), 2))
            {
                g.DrawRectangle(borderPen, left, top, width, height);
            }

            using (Font labelFont = new Font("Segoe UI", 8))
            using (Brush textBrush = new SolidBrush(Color.FromArgb(150, 150, 180)))
            {
                float range = GetYRange();
                for (int i = 0; i <= 5; i++)
                {
                    float val = yMax - (range * i / 5.0f);
                    int y = top + (int)(height * i / 5.0f);
                    string label = FormatYValue(val);
                    SizeF sz = g.MeasureString(label, labelFont);
                    g.DrawString(label, labelFont, textBrush, left - sz.Width - 8, y - sz.Height / 2);
                }

                DateTime now = DateTime.Now;
                for (int i = 0; i <= 5; i++)
                {
                    int x = left + (int)(width * i / 5.0f);
                    int secondsAgo = timeWindowSeconds * (5 - i) / 5;
                    string label = secondsAgo == 0 ? "Teraz" : $"-{secondsAgo}s";
                    SizeF sz = g.MeasureString(label, labelFont);
                    g.DrawString(label, labelFont, textBrush, x - sz.Width / 2, top + height + 8);
                }
            }
        }

        private void DrawData(Graphics g, int left, int top, int width, int height)
        {
            lock (dataLock)
            {
                if (dataPoints.Count < 2) return;

                DateTime now = DateTime.Now;
                DateTime windowStart = now.AddSeconds(-timeWindowSeconds);

                List<DataPoint> visiblePoints = new List<DataPoint>();
                foreach (var dp in dataPoints)
                {
                    if (dp.Time >= windowStart)
                        visiblePoints.Add(dp);
                }

                if (visiblePoints.Count < 2) return;

                float range = GetYRange();
                if (range <= 0) range = 1;

                List<PointF> linePoints = new List<PointF>();
                foreach (var dp in visiblePoints)
                {
                    double timeFraction = (dp.Time - windowStart).TotalSeconds / timeWindowSeconds;
                    int x = left + (int)(timeFraction * width);
                    float valueFraction = (dp.Value - yMin) / range;
                    int y = top + height - (int)(valueFraction * height);
                    y = Math.Max(top, Math.Min(top + height, y));
                    linePoints.Add(new PointF(x, y));
                }

                if (linePoints.Count < 2) return;

                using (GraphicsPath fillPath = new GraphicsPath())
                {
                    fillPath.AddLine(linePoints[0].X, top + height, linePoints[0].X, linePoints[0].Y);
                    for (int i = 1; i < linePoints.Count; i++)
                    {
                        fillPath.AddLine(linePoints[i - 1].X, linePoints[i - 1].Y, linePoints[i].X, linePoints[i].Y);
                    }
                    fillPath.AddLine(linePoints[linePoints.Count - 1].X, linePoints[linePoints.Count - 1].Y, linePoints[linePoints.Count - 1].X, top + height);
                    fillPath.CloseFigure();

                    using (LinearGradientBrush fillBrush = new LinearGradientBrush(
                        new PointF(0, top), new PointF(0, top + height),
                        Color.FromArgb(60, 0, 255, 128),
                        Color.FromArgb(5, 0, 255, 128)))
                    {
                        g.FillPath(fillBrush, fillPath);
                    }
                }

                using (Pen linePen = new Pen(Color.FromArgb(0, 255, 128), 2.5f))
                {
                    g.DrawLines(linePen, linePoints.ToArray());
                }

                PointF lastPt = linePoints[linePoints.Count - 1];
                using (SolidBrush glowBrush = new SolidBrush(Color.FromArgb(100, 0, 255, 128)))
                {
                    g.FillEllipse(glowBrush, lastPt.X - 8, lastPt.Y - 8, 16, 16);
                }
                using (SolidBrush dotBrush = new SolidBrush(Color.FromArgb(0, 255, 128)))
                {
                    g.FillEllipse(dotBrush, lastPt.X - 4, lastPt.Y - 4, 8, 8);
                }

                string valueLabel = FormatValue(lastValue);
                using (Font valueFont = new Font("Segoe UI", 13, FontStyle.Bold))
                {
                    SizeF labelSize = g.MeasureString(valueLabel, valueFont);
                    float labelX = lastPt.X - labelSize.Width - 14;
                    float labelY = lastPt.Y - labelSize.Height / 2;

                    if (labelX < left) labelX = lastPt.X + 14;
                    if (labelY < top) labelY = top;
                    if (labelY + labelSize.Height > top + height) labelY = top + height - labelSize.Height;

                    float pad = 6;
                    using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(200, 13, 13, 13)))
                    {
                        g.FillRectangle(bgBrush, labelX - pad, labelY - 2, labelSize.Width + pad * 2, labelSize.Height + 4);
                    }
                    using (Pen borderPen = new Pen(Color.FromArgb(0, 255, 128), 1.5f))
                    {
                        g.DrawRectangle(borderPen, labelX - pad, labelY - 2, labelSize.Width + pad * 2, labelSize.Height + 4);
                    }
                    using (SolidBrush valueBrush = new SolidBrush(Color.FromArgb(0, 255, 128)))
                    {
                        g.DrawString(valueLabel, valueFont, valueBrush, labelX, labelY);
                    }
                }

                PointF minPt = FindVisibleExtreme(visiblePoints, left, top, width, height, true);
                PointF maxPt = FindVisibleExtreme(visiblePoints, left, top, width, height, false);

                using (Font markerFont = new Font("Segoe UI", 8, FontStyle.Bold))
                {
                    if (minPt.Y > 0)
                    {
                        using (Brush minBrush = new SolidBrush(Color.FromArgb(255, 80, 80)))
                        {
                            g.DrawString("MIN", markerFont, minBrush, minPt.X + 6, minPt.Y - 6);
                        }
                    }
                    if (maxPt.Y > 0)
                    {
                        using (Brush maxBrush = new SolidBrush(Color.FromArgb(80, 255, 80)))
                        {
                            g.DrawString("MAX", markerFont, maxBrush, maxPt.X + 6, maxPt.Y - 6);
                        }
                    }
                }
            }
        }

        private PointF FindVisibleExtreme(List<DataPoint> points, int left, int top, int width, int height, bool findMin)
        {
            DateTime now = DateTime.Now;
            DateTime windowStart = now.AddSeconds(-timeWindowSeconds);
            float range = GetYRange();

            DataPoint extremePt = null!;
            bool found = false;
            foreach (var pt in points)
            {
                if (pt.Time < windowStart) continue;
                if (!found)
                {
                    extremePt = pt;
                    found = true;
                }
                else if (findMin && pt.Value < extremePt.Value)
                {
                    extremePt = pt;
                }
                else if (!findMin && pt.Value > extremePt.Value)
                {
                    extremePt = pt;
                }
            }

            if (!found) return new PointF(0, 0);

            double timeFraction = (extremePt.Time - windowStart).TotalSeconds / timeWindowSeconds;
            int x = left + (int)(timeFraction * width);
            float valueFraction = (extremePt.Value - yMin) / range;
            int y = top + height - (int)(valueFraction * height);
            return new PointF(x, y);
        }

        private void DrawStatistics(Graphics g, int width, int chartBottom)
        {
            int statsY = chartBottom + 15;
            int statsHeight = 55;

            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(24, 24, 24)))
            {
                g.FillRectangle(bgBrush, 0, statsY - 5, width, statsHeight);
            }

            using (Pen linePen = new Pen(Color.FromArgb(60, 60, 100), 1))
            {
                g.DrawLine(linePen, 0, statsY - 5, width, statsY - 5);
            }

            string minStr, maxStr, avgStr, currentStr;
            lock (dataLock)
            {
                minStr = currentMin == float.MaxValue ? "---" : FormatValue(currentMin);
                maxStr = currentMax == float.MinValue ? "---" : FormatValue(currentMax);
                avgStr = currentCount > 0 ? FormatValue((float)(currentSum / currentCount)) : "---";
                currentStr = FormatValue(lastValue);
            }

            using (Font statFont = new Font("Segoe UI", 10))
            using (Font bigFont = new Font("Segoe UI", 14, FontStyle.Bold))
            {
                int colWidth = width / 5;

                DrawStatBox(g, "AKTUALNA", currentStr, Color.FromArgb(0, 255, 128), statFont, bigFont, 10, statsY, colWidth);
                DrawStatBox(g, "MIN", minStr, Color.FromArgb(255, 80, 80), statFont, bigFont, colWidth + 10, statsY, colWidth);
                DrawStatBox(g, "MAX", maxStr, Color.FromArgb(80, 255, 80), statFont, bigFont, colWidth * 2 + 10, statsY, colWidth);
                DrawStatBox(g, "AVG", avgStr, Color.FromArgb(255, 255, 80), statFont, bigFont, colWidth * 3 + 10, statsY, colWidth);
                DrawStatBox(g, "PRÓBEK", currentCount.ToString(), Color.FromArgb(150, 150, 200), statFont, bigFont, colWidth * 4 + 10, statsY, colWidth);
            }
        }

        private void DrawStatBox(Graphics g, string label, string value, Color color, Font smallFont, Font bigFont, int x, int y, int width)
        {
            using (Brush textBrush = new SolidBrush(Color.FromArgb(120, 120, 150)))
            {
                g.DrawString(label, smallFont, textBrush, x, y);
            }
            using (Brush valueBrush = new SolidBrush(color))
            {
                g.DrawString(value, bigFont, valueBrush, x, y + 18);
            }
        }

        private void DrawHeader(Graphics g, int width)
        {
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(24, 24, 24)))
            {
                g.FillRectangle(bgBrush, 0, 0, width, 50);
            }

            using (Pen linePen = new Pen(Color.FromArgb(60, 60, 100), 1))
            {
                g.DrawLine(linePen, 0, 50, width, 50);
            }

            string modeText = currentMode.Contains("DC") ? "DC" : (currentMode.Contains("AC") ? "AC" : "");
            string title = $"WYKRES POMIARU  |  {currentUnit}";
            if (!string.IsNullOrEmpty(modeText)) title += $"  |  {modeText}";

            using (Font titleFont = new Font("Segoe UI", 14, FontStyle.Bold))
            using (Brush titleBrush = new SolidBrush(Color.Gold))
            {
                g.DrawString(title, titleFont, titleBrush, 15, 12);
            }

            if (isPaused)
            {
                using (Font pauseFont = new Font("Segoe UI", 12, FontStyle.Bold))
                using (Brush pauseBrush = new SolidBrush(Color.FromArgb(255, 128, 0)))
                {
                    g.DrawString("PAUZA", pauseFont, pauseBrush, width - 100, 15);
                }
            }
        }

        private float GetYRange()
        {
            lock (dataLock)
            {
                if (currentCount == 0) return 10;

                float min = currentMin;
                float max = currentMax;
                float range = max - min;

                if (range < 0.001f) range = Math.Abs(max) * 0.1f;
                if (range < 0.001f) range = 1;

                float margin = range * 0.15f;
                yMin = min - margin;
                yMax = max + margin;
            }

            return yMax - yMin;
        }

        private string FormatYValue(float val)
        {
            if (Math.Abs(val) >= 1000) return $"{val / 1000:F1}k";
            if (Math.Abs(val) >= 100) return $"{val:F0}";
            if (Math.Abs(val) >= 10) return $"{val:F1}";
            if (Math.Abs(val) >= 1) return $"{val:F2}";
            if (Math.Abs(val) >= 0.01f) return $"{val:F3}";
            return $"{val:F4}";
        }

        private string FormatValue(float val)
        {
            if (float.IsNaN(val) || float.IsInfinity(val)) return "---";
            if (Math.Abs(val) >= 1000) return $"{val / 1000:F2}k {currentUnit}";
            if (Math.Abs(val) >= 100) return $"{val:F1} {currentUnit}";
            if (Math.Abs(val) >= 10) return $"{val:F2} {currentUnit}";
            if (Math.Abs(val) >= 1) return $"{val:F3} {currentUnit}";
            if (Math.Abs(val) >= 0.01f) return $"{val:F4} {currentUnit}";
            return $"{val:F5} {currentUnit}";
        }

        public void TogglePause()
        {
            isPaused = !isPaused;
        }

        public bool IsPaused => isPaused;

        public void Clear()
        {
            lock (dataLock)
            {
                dataPoints.Clear();
                currentMin = float.MaxValue;
                currentMax = float.MinValue;
                currentSum = 0;
                currentCount = 0;
                lastValue = 0;
            }
        }

        public void SetTimeWindow(int seconds)
        {
            timeWindowSeconds = seconds;
        }

        public void CleanupOldPoints()
        {
            DateTime cutoff = DateTime.Now.AddSeconds(-timeWindowSeconds * 2);
            lock (dataLock)
            {
                dataPoints.RemoveAll(dp => dp.Time < cutoff);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "ChartPanel";
            this.Size = new Size(751, 375);
            this.ResumeLayout(false);
        }
    }
}
