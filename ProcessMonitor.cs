using System;
using System.Diagnostics;
using System.Linq;

namespace CSharpSdWebuiMonitor
{
    /// <summary>
    /// 进程监控模块 - 用于监控特定进程的资源占用
    /// </summary>
    public class ProcessMonitor
    {
        private string _processName;
        private Process? _targetProcess;
        private PerformanceCounter? _cpuCounter;
        private PerformanceCounter? _memoryCounter;

        public ProcessMonitor(string processPath)
        {
            _processName = Path.GetFileNameWithoutExtension(processPath);
        }

        /// <summary>
        /// 初始化性能计数器
        /// </summary>
        private void InitializeCounters()
        {
            try
            {
                var process = GetProcess();
                if (process == null) return;

                // CPU 计数器
                _cpuCounter = new PerformanceCounter(
                    "Process",
                    "% Processor Time",
                    process.ProcessName,
                    true
                );

                // 内存计数器（工作集 - 物理内存）
                _memoryCounter = new PerformanceCounter(
                    "Process",
                    "Working Set",
                    process.ProcessName,
                    true
                );

                // 预热
                _cpuCounter.NextValue();
                _memoryCounter.NextValue();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"初始化进程计数器失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取目标进程
        /// </summary>
        private Process? GetProcess()
        {
            if (_targetProcess != null && !_targetProcess.HasExited)
            {
                return _targetProcess;
            }

            try
            {
                var processes = Process.GetProcessesByName(_processName);
                if (processes.Length > 0)
                {
                    _targetProcess = processes[0];
                    InitializeCounters();
                    return _targetProcess;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"获取进程失败: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 获取进程CPU占用百分比
        /// </summary>
        public double GetCpuUsage()
        {
            try
            {
                if (_cpuCounter != null)
                {
                    return _cpuCounter.NextValue();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"获取CPU占用失败: {ex.Message}");
            }
            return 0.0;
        }

        /// <summary>
        /// 获取进程内存占用（MB）
        /// </summary>
        public double GetMemoryUsageMB()
        {
            try
            {
                var process = GetProcess();
                if (process != null)
                {
                    return process.WorkingSet64 / (1024.0 * 1024.0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"获取内存占用失败: {ex.Message}");
            }
            return 0.0;
        }

        /// <summary>
        /// 检查进程是否仍在运行
        /// </summary>
        public bool IsProcessRunning()
        {
            try
            {
                var process = GetProcess();
                return process != null && !process.HasExited;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取进程信息
        /// </summary>
        public string GetProcessInfo()
        {
            try
            {
                var process = GetProcess();
                if (process != null)
                {
                    var cpuUsage = GetCpuUsage();
                    var memoryUsage = GetMemoryUsageMB();
                    return $"进程 {_processName} - CPU: {cpuUsage:F1}% | 内存: {memoryUsage:F2} MB";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"获取进程信息失败: {ex.Message}");
            }

            return $"进程 {_processName} 未运行";
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            _cpuCounter?.Dispose();
            _memoryCounter?.Dispose();
            _targetProcess?.Dispose();
        }
    }
}
