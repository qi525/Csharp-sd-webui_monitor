using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CSharpSdWebuiMonitor
{
    /// <summary>
    /// 自定义进度条控件，支持动态颜色变化
    /// </summary>
    public class ColoredProgressBar : ProgressBar
    {
        private Color _barColor = Color.Green;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color BarColor
        {
            get { return _barColor; }
            set
            {
                _barColor = value;
                this.Invalidate(); // 重绘
            }
        }

        public ColoredProgressBar()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // 绘制背景
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(40, 40, 40)), this.ClientRectangle);

            // 计算填充宽度
            if (this.Maximum > 0)
            {
                int fillWidth = (int)((double)this.Value / this.Maximum * this.Width);
                Rectangle fillRect = new Rectangle(0, 0, fillWidth, this.Height);
                e.Graphics.FillRectangle(new SolidBrush(_barColor), fillRect);
            }

            // 绘制边框
            e.Graphics.DrawRectangle(new Pen(Color.Gray), 0, 0, this.Width - 1, this.Height - 1);
        }
    }
}
