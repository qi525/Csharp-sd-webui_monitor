using System;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

namespace CSharpSdWebuiMonitor
{
    // 负责收集和整理所有监控数据
    public class PerformanceCollector
    {
        private PerformanceCounter? cpuCounter;
        private PerformanceCounter? committedBytesCounter;
        private PerformanceCounter? commitLimitCounter;
        
        // 注意: WinForms 的 ProgressBar 最大值是 Int32.MaxValue (2147483647)
        private const string BASE_OUTPUT_PATH = @"C:\stable-diffusion-webui\outputs\txt2img-images";
        private const long ONE_GB = 1024L * 1024L * 1024L;

        public PerformanceCollector()
        {
            InitializeCounters();
        }

        // 初始化性能计数器
        private void InitializeCounters()
        {
            try
            {
                // CPU 占用率 (总计)
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);

                // 虚拟内存 (已提交和上限)
                committedBytesCounter = new PerformanceCounter("Memory", "Committed Bytes", true);
                commitLimitCounter = new PerformanceCounter("Memory", "Commit Limit", true);

                // 第一次 NextValue() 调用通常返回 0 或不可信数据，需要预热
                cpuCounter.NextValue();
                committedBytesCounter.NextValue();
                commitLimitCounter.NextValue();
            }
            catch (Exception ex)
            {
                // 生产环境中应使用日志记录
                Debug.WriteLine($"初始化计数器失败: {ex.Message}");
            }
        }

        // 获取当天的 WebUI 输出文件夹路径
        private string GetDailyMonitorPath()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            return Path.Combine(BASE_OUTPUT_PATH, today);
        }

        // 核心方法：收集所有数据并返回 MonitorData 对象
        public MonitorData CollectData()
        {
            var data = new MonitorData();
            data.CurrentTime = DateTime.Now;

            // --- 1. CPU 占用 ---
            try
            {
                data.CpuUsagePercent = cpuCounter != null ? Math.Round(cpuCounter.NextValue(), 1) : 0.0;
            }
            catch (Exception)
            {
                data.CpuUsagePercent = 0.0; // 失败时清零
            }

            // --- 2. 虚拟内存占用 (已提交/上限) ---
            try
            {
                long committedBytes = committedBytesCounter != null ? (long)committedBytesCounter.NextValue() : 0;
                long commitLimitBytes = commitLimitCounter != null ? (long)commitLimitCounter.NextValue() : 0;

                data.VirtualMemoryUsagePercent = commitLimitBytes > 0 
                    ? Math.Round((double)committedBytes / commitLimitBytes * 100, 1) 
                    : 0.0;
                
                double committedGB = committedBytes / (double)ONE_GB;
                double commitLimitGB = commitLimitBytes / (double)ONE_GB;

                data.VirtualMemoryUsageText = 
                    $"{committedGB:F1} GB / {commitLimitGB:F1} GB ({data.VirtualMemoryUsagePercent}%)";
            }
            catch (Exception)
            {
                data.VirtualMemoryUsageText = "N/A / N/A (0.0%)";
            }

            // --- 3. 物理内存占用 (此处使用 WMI 复杂，暂时使用 Environment.WorkingSet 和模拟) ---
            // 真实物理内存获取需要依赖 WMI 或 GetPhysicallyInstalledSystemMemory API
            // 假设我们有总物理内存 32 GB
            double totalPhysMemGB = 32.0;
            // 模拟一个使用的值
            data.PhysicalMemoryUsagePercent = Math.Round(80.0 + (data.CpuUsagePercent * 0.1), 1); // 模拟
            double usedPhysMemGB = Math.Round(totalPhysMemGB * (data.PhysicalMemoryUsagePercent / 100.0), 1);
            data.PhysicalMemoryUsageText = 
                $"{usedPhysMemGB:F1} GB / {totalPhysMemGB:F1} GB ({data.PhysicalMemoryUsagePercent}%)";


            // --- 4. WebUI 文件数量监控 ---
            data.MonitorPath = GetDailyMonitorPath();
            try
            {
                if (Directory.Exists(data.MonitorPath))
                {
                    // 仅计算第一级目录的文件
                    data.GeneratedFileCount = Directory.GetFiles(data.MonitorPath, "*.*", SearchOption.TopDirectoryOnly).Length;
                    data.WebuiStatus = "正在生成 (文件数 " + data.GeneratedFileCount + ")";
                }
                else
                {
                    data.WebuiStatus = "目录不存在或空闲";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"文件监控失败: {ex.Message}");
                data.WebuiStatus = "监控异常";
                data.GeneratedFileCount = 0;
            }

            // --- 5. 模拟 GPU / 显存 / 网络数据 (真实实现需要更复杂的API) ---
            SimulateGPUData(data);
            SimulateNetworkData(data);

            return data;
        }

        // 由于 GPU 性能计数器复杂，这里使用模拟数据
        private void SimulateGPUData(MonitorData data)
        {
            // 基于 CPU 负载进行轻微浮动模拟
            double fluctuation = Math.Sin(DateTime.Now.Ticks * 0.0000001) * 5.0; 
            
            data.GpuComputePercent = Math.Round(80.0 + (data.CpuUsagePercent * 0.1) + fluctuation, 2);
            data.GpuDataCopyPercent = Math.Round(50.0 + fluctuation * 0.5, 2);
            data.Gpu3DRenderPercent = Math.Round(10.0 + fluctuation * 0.3, 2);

            data.VramTotalGB = 16.00;
            data.VramUsedGB = Math.Round(9.0 + (data.GpuComputePercent / 100.0) * 5.0, 2);
            data.VramUsagePercent = Math.Round(data.VramUsedGB / data.VramTotalGB * 100, 1);
            data.VramStatusText = $"达标 ({data.VramUsedGB:F2} GB)";
            data.VramStatusColor = data.VramUsagePercent > 90 ? Color.Red : Color.Green;
        }

        // 模拟网络数据
        private void SimulateNetworkData(MonitorData data)
        {
            // 简单的随机模拟
            Random rand = new Random();
            data.DownloadSpeedMBps = Math.Round(rand.NextDouble() * 0.5, 2); 
            data.UploadSpeedMBps = Math.Round(rand.NextDouble() * 0.5, 2);
        }
    }
}