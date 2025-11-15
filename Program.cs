using System;
using System.Windows.Forms;

namespace CSharpSdWebuiMonitor
{
    // C#sd-webui_monitor 项目的程序入口点
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 启用应用程序中的视觉样式
            Application.EnableVisualStyles();
            // 设置兼容的文本渲染
            Application.SetCompatibleTextRenderingDefault(false);
            // 运行主窗体
            Application.Run(new Form1());
        }
    }
}