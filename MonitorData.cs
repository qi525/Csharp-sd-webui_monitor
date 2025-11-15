using System;
using System.Drawing; // 引入 System.Drawing 以便在 WinForms 中使用 Color

namespace CSharpSdWebuiMonitor
{
    // 用于存储所有实时监控数据的模型
    public class MonitorData
    {
        // --- 1. 头部信息 ---
        public DateTime CurrentTime { get; set; }
        public string GpuName { get; set; } = "GPU: Intel Arc A770 16GB"; // 示例值

        // --- 2. 性能指标 (CPU / 内存) ---
        public double CpuUsagePercent { get; set; }
        public double PhysicalMemoryUsagePercent { get; set; }
        public string PhysicalMemoryUsageText { get; set; } = ""; // 例如 "31.5 GB / 31.9 GB"
        public double VirtualMemoryUsagePercent { get; set; }
        public string VirtualMemoryUsageText { get; set; } = ""; // 例如 "69.9 GB / 80.7 GB"

        // --- 3. GPU 性能指标 ---
        public double GpuComputePercent { get; set; }
        public double GpuDataCopyPercent { get; set; }
        public double Gpu3DRenderPercent { get; set; }

        // --- 4. 显存 (VRAM) ---
        public double VramUsedGB { get; set; }
        public double VramTotalGB { get; set; }
        public double VramUsagePercent { get; set; }
        public string VramStatusText { get; set; } = "";
        public Color VramStatusColor { get; set; } = Color.Green; // 用于设置 VRAM 文本颜色

        // --- 5. 网络速度 ---
        public double DownloadSpeedMBps { get; set; }
        public double UploadSpeedMBps { get; set; }
        public double MaxSpeedMBps { get; set; } = 100.0; // 上限值

        // --- 6. WebUI 监控 ---
        public string WebuiStatus { get; set; } = "";
        public int GeneratedFileCount { get; set; }
        public string MonitorPath { get; set; } = "";

        // --- 7. 统计数据 ---
        public int TotalCount { get; set; } = 17171; // 模拟值
        public int NormalCount { get; set; } = 17009; // 模拟值
        public int AlertCount { get; set; } = 3; // 模拟值
    }
}