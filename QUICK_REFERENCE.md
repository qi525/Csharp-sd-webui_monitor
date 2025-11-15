# 快速参考卡 - C# SD WebUI Monitor

## 🚀 快速开始 (30 秒)

```powershell
# 1. 编译
cd C:\个人数据\C#Code\C#sd-webui_monitor
dotnet build

# 2. 配置路径（编辑 Form1.cs 第 13 行）
ALARM_FILE_PATH = @"C:\path\to\alarm.wav"

# 3. 运行
dotnet run
```

---

## 📋 文件配置速查表

| 配置项 | 位置 | 默认值 | 修改方法 |
|--------|------|--------|---------|
| **警报音频** | Form1.cs:13 | `C:\path\to\alarm.wav` | 改为你的 .wav 文件路径 |
| **WebUI 输出目录** | PerformanceCollector.cs:18 | `C:\stable-diffusion-webui\outputs\txt2img-images` | 改为实际目录 |
| **Python 进程路径** | PerformanceCollector.cs:31 | `C:\stable-diffusion-webui\venv\python.exe` | 改为实际路径 |
| **文件检查间隔** | PerformanceCollector.cs:29 | 15 秒 | 修改 `FILE_CHECK_INTERVAL_SECONDS` |
| **警报触发周期** | PerformanceCollector.cs:30 | 2 次 (30秒) | 修改 `NO_INCREASE_THRESHOLD` |
| **UI 更新间隔** | Form1.cs:171 | 1000 ms | 修改 Timer.Interval |

---

## 🔔 WebUI 警报逻辑

```
启动 → 第15秒检查文件数
        ├─ 增加 ✓ → 正常工作（重置计数）
        └─ 不增加 → 计数 = 1

继续 → 第30秒检查文件数
        ├─ 增加 ✓ → 正常工作（重置计数）
        └─ 不增加 → 计数 = 2 → 🔔 触发警报!

警报持续直到文件数增加
```

---

## 🎮 主要类和方法

### PerformanceCollector
```csharp
.CollectData()                  // 获取所有监控数据
.CheckWebUIFileIncrease()       // 检查文件是否增加（返回 bool）
.GetWebUIMonitorStatus()        // 获取 WebUI 监控状态文本
.GetPythonProcessInfo()         // 获取 Python 进程信息
```

### ProcessMonitor
```csharp
new ProcessMonitor(processPath)  // 初始化
.GetCpuUsage()                   // CPU 占用百分比
.GetMemoryUsageMB()              // 内存占用 MB
.IsProcessRunning()              // 是否在运行
.GetProcessInfo()                // 获取进程信息文本
```

### AudioPlayer
```csharp
new AudioPlayer(filePath)        // 初始化（加载音频）
.PlayAlarm()                     // 同步播放（阻塞）
.PlayAlarmAsync()                // 异步播放（不阻塞）
.Stop()                          // 停止播放
.Dispose()                       // 释放资源
```

### Form1
```csharp
Timer_Tick()                     // 定时器触发（1秒）
UpdateUI(data)                   // 更新 UI 界面
TriggerWebUIAlarm()              // 触发 WebUI 警报
StopWebUIAlarm()                 // 停止 WebUI 警报
```

---

## 📊 数据模型 (MonitorData)

```csharp
// 时间
CurrentTime             // DateTime - 当前时间

// CPU & 内存
CpuUsagePercent         // double - CPU 占用%
PhysicalMemoryUsageText // string - 物理内存文本
VirtualMemoryUsageText  // string - 虚拟内存文本

// GPU
GpuComputePercent       // double - Compute 占用%
GpuDataCopyPercent      // double - Copy 占用%
VramUsedGB / VramTotalGB// double - 显存占用

// WebUI
GeneratedFileCount      // int - 文件数量
IsWebuiAlertTriggered   // bool - 警报状态
WebuiStatus             // string - 状态文本

// Python
PythonProcessInfo       // string - 进程信息
```

---

## 🎨 UI 颜色指示

| 元素 | 绿色 | 橙色 | 红色 |
|------|------|------|------|
| **CPU** | <50% | 50-75% | >75% |
| **内存** | <50% | 50-75% | >75% |
| **VRAM** | <90% | - | ≥90% |
| **时钟** | 正常 | - | - |
| **WebUI** | 工作中 | - | ❌ 停止 |

---

## 🐛 常见问题速解

| 问题 | 解决方案 |
|------|---------|
| 警报音不播放 | 检查 ALARM_FILE_PATH，确保 .wav 文件存在 |
| WebUI 文件监控不工作 | 检查 BASE_OUTPUT_PATH，确保目录存在 |
| Python 进程显示未运行 | 检查 PYTHON_EXE_PATH，确保 python.exe 在运行 |
| 编译失败 | 检查 .csproj 中的 TargetFramework 和 UseWindowsForms |
| GPU 数据全是 0 | 正常，当前采用模拟数据（后续会改进） |

---

## 📁 关键文件路径

```
C#sd-webui_monitor/
├── Form1.cs                        ← 修改 ALARM_FILE_PATH (第 13 行)
├── PerformanceCollector.cs         ← 修改 BASE_OUTPUT_PATH (第 18 行)
│                                     修改 PYTHON_EXE_PATH (第 31 行)
├── AudioPlayer.cs                  ← 音频播放逻辑
├── ProcessMonitor.cs               ← Python 进程监控
├── C#sd-webui_monitor.csproj       ← 项目配置
└── USAGE_GUIDE.md                  ← 详细使用说明 ⭐
```

---

## ⚡ 性能指标

| 指标 | 值 | 说明 |
|------|-----|------|
| UI 更新频率 | 1 秒 | Timer 间隔 |
| 文件检查频率 | 15 秒 | 可配置 |
| 警报检测延迟 | 30 秒 | 2 × 15 秒 |
| 内存占用 | ~50-100 MB | 典型值 |
| CPU 占用 | <5% | 待机时 |

---

## 🔗 文档导航

- 📖 [详细功能对比](./FEATURE_COMPARISON.md)
- 📚 [完整使用指南](./USAGE_GUIDE.md)
- 🏗️ [项目概览](./PROJECT_OVERVIEW.md)
- 🔧 [项目配置](./C#sd-webui_monitor.csproj)

---

## ✅ 检查清单

启动前检查：
- [ ] .NET 10.0 SDK 已安装
- [ ] `ALARM_FILE_PATH` 已配置为有效路径
- [ ] `BASE_OUTPUT_PATH` 指向正确的 WebUI 目录
- [ ] `PYTHON_EXE_PATH` 指向正确的 Python 可执行文件
- [ ] 具有管理员权限

---

## 🚨 紧急故障排除

**程序崩溃？**
```powershell
# 1. 清理旧的编译文件
dotnet clean

# 2. 重新构建
dotnet build

# 3. 运行并查看错误
dotnet run
```

**音频播放失败？**
```
1. 检查文件格式（必须是 .wav）
2. 检查文件路径（使用绝对路径，避免空格）
3. 检查文件权限（确保可读）
4. 尝试使用系统音频：C:\Windows\Media\notify.wav
```

**WebUI 监控无反应？**
```
1. 验证目录存在：打开文件管理器检查
2. 验证路径正确：检查是否有 yyyy-MM-dd 子目录
3. 检查权限：确保程序可读该目录
4. 重启程序：强制重新初始化
```

---

## 📞 获取帮助

1. 查看 [USAGE_GUIDE.md](./USAGE_GUIDE.md) 的"故障排除"部分
2. 查看 [FEATURE_COMPARISON.md](./FEATURE_COMPARISON.md) 的"功能说明"
3. 检查 Visual Studio 的 Debug Output
4. 查看系统事件查看器

---

## 版本信息

```
项目名: C# SD WebUI Monitor
版本: 1.0 Beta
目标框架: .NET 10.0-windows
更新时间: 2025-11-15
维护状态: 🟢 Active
```

---

**提示**: 收藏此页面以快速参考！🚀
