# C# SD WebUI 监控工具 - 使用指南

## 概述
这是一个用 C# (WinForms) 重构的 SD WebUI 实时监控工具，功能包括：
- 系统资源监控（CPU、内存）
- WebUI 文件生成监控
- Python 进程监控
- 自动警报系统

---

## 快速开始

### 1. 前置条件
- Windows 10 / 11
- .NET 10.0 SDK
- 管理员权限（推荐）

### 2. 编译运行
```powershell
cd C:\个人数据\C#Code\C#sd-webui_monitor
dotnet run
```

### 3. 配置文件路径

打开 `Form1.cs`，找到第 13 行：
```csharp
private const string ALARM_FILE_PATH = @"C:\path\to\your\alarm.wav"; // 修改为你的音频文件路径
```

打开 `PerformanceCollector.cs`，检查以下路径是否正确：
```csharp
// 第 18 行 - WebUI 输出目录
private const string BASE_OUTPUT_PATH = @"C:\stable-diffusion-webui\outputs\txt2img-images";

// 第 31 行 - Python 进程路径
private const string PYTHON_EXE_PATH = @"C:\stable-diffusion-webui\venv\python.exe";
```

---

## 功能详解

### 📊 系统资源监控
- **CPU 利用率**: 实时显示系统CPU占用百分比
- **物理内存**: 当前物理内存占用情况
- **虚拟内存**: 已提交的虚拟内存占用

### 🎮 GPU 监控 (当前为模拟数据)
- **Compute 利用率**: GPU计算着色器占用率
- **Copy 利用率**: GPU数据复制占用率
- **显存占用**: 专有显存(VRAM)实时占用

⚠️ **注意**: GPU 数据当前为模拟值，后续可集成 PDH 或 PowerShell 获取实际数据

### 🔍 WebUI 文件监控 (核心功能)

#### 工作原理
1. **检查间隔**: 每15秒检查一次 WebUI 输出目录的文件数量
2. **检测逻辑**:
   - 如果文件数 **增加** → 重置计数器（正常工作）
   - 如果文件数 **不增加** → 计数器 +1
   - 当计数器达到 **2次**（30秒） → 触发警报 🔔

#### 警报行为
- 播放音频警报（需配置音频文件路径）
- 在 UI 中显示警报状态
- 文件数有增加后自动停止警报

#### 关键特性
✅ 自动处理文件归档（文件数清零也被视为正常现象）  
✅ 灵活的检查间隔（可在代码中调整）  
✅ 快速响应（30秒内检测到问题）

### 🐍 Python 进程监控 (新增功能)

监控 `python.exe` 的资源占用：
- **CPU占用率**: 该进程对CPU的占用百分比
- **内存占用**: 该进程的物理内存占用（MB）
- **运行状态**: 是否仍在运行

在控制台输出会显示类似信息：
```
进程 python - CPU: 12.5% | 内存: 2345.67 MB
```

---

## UI 布局说明

```
┌─────────────────────────────────────────┐
│  当前时间: 2025-11-15 11:28:15         │  ← 实时时钟
├─────────────────────────────────────────┤
│  GPU: Intel Arc A770 16GB               │  ← GPU 信息
├─────────────────────────────────────────┤
│  CPU 利用率: 45.2%          [███░░░░] │  ← 进度条展示
├─────────────────────────────────────────┤
│  物理内存占用: 8.5 GB / 32.0 GB         │
│                           [███░░░░] │
├─────────────────────────────────────────┤
│  虚拟内存占用 (已提交): 12.3 GB / 15.0 GB
│                           [████░░░] │
├─────────────────────────────────────────┤
│  GPU 计算着色器: 78.45%    [███████░] │
│  GPU 数据复制: 52.12%      [████░░░░] │
├─────────────────────────────────────────┤
│  专有显存占用: 9.50 GB / 16.00 GB      │
│               VRAM 状态: 达标 (9.50 GB) │ ← 绿色 = 正常
├─────────────────────────────────────────┤
│  下载速度: 0.23 MB/s (上限 100 MB/s)   │
│                           [░░░░░░░░] │
│  上传速度: 0.12 MB/s (上限 100 MB/s)   │
│                           [░░░░░░░░] │
├─────────────────────────────────────────┤
│  Webui 状态: 正在生成 (文件数 1563)     │ ← 绿色 = 工作中
├─────────────────────────────────────────┤
│  总次数: 18000 | 正常: 17900 | 警报触发: 3
└─────────────────────────────────────────┘
```

---

## 警报配置

### 🔔 WebUI 文件停止生成警报

#### 启用方法
1. 获取一个 `.wav` 音频文件（可使用系统自带铃声，如 `notification.wav`）
2. 修改 `Form1.cs` 第 13 行:
```csharp
private const string ALARM_FILE_PATH = @"C:\Users\YourName\Music\alarm.wav";
```

#### 可选系统音频文件位置
```
C:\Windows\Media\
  ├── Alarm01.wav
  ├── Alarm02.wav
  └── notify.wav
```

### 其他警报规则（可扩展）

#### VRAM 低于阈值 (8GB)
目前在 Python 版本中实现，C# 版本可添加：
```csharp
if (_currentData.VramUsedGB < 8.0)
{
    TriggerVramAlarm(); // 需实现
}
```

#### 虚拟内存超过阈值 (80GB)
显示橙色提示（可在 UpdateUI 中实现）

---

## 故障排除

### ❌ 编译失败

**错误**: `未能找到类型或命名空间名"Form"`
```
解决方案: 确保 .csproj 文件包含:
  <UseWindowsForms>true</UseWindowsForms>
```

**错误**: `未能在命名空间"System.Diagnostics"中找到类型名"PerformanceCounter"`
```
解决方案: 确保 .csproj 文件包含:
  <TargetFramework>net10.0-windows</TargetFramework>
```

### ⚠️ 警报音无法播放

**原因**: 音频文件路径错误或不存在
```
解决方案:
1. 检查 ALARM_FILE_PATH 是否正确
2. 确保 WAV 文件存在且可读
3. 检查文件权限
4. 尝试使用绝对路径而非相对路径
```

### ❌ Python 进程监控显示"未运行"

**原因**: python.exe 进程不存在或路径不正确
```
解决方案:
1. 检查 SD WebUI 是否启动
2. 验证 PYTHON_EXE_PATH 是否正确
3. 使用任务管理器检查 python.exe 是否在运行
```

### ❌ WebUI 文件监控不工作

**原因**: 输出目录路径不正确
```
解决方案:
1. 验证 BASE_OUTPUT_PATH 是否指向正确的目录
2. 检查目录是否存在且有读权限
3. 确保当前日期子目录存在（格式: yyyy-MM-dd）
```

---

## 性能指标说明

### CPU 占用率
- **< 30%**: 绿色（正常）
- **30-60%**: 橙色（中等）
- **> 60%**: 红色（高负荷）

### 内存占用
- **< 50%**: 绿色
- **50-75%**: 橙色
- **> 75%**: 红色

### 显存占用
- **< 90%**: 绿色（达标）
- **≥ 90%**: 红色（紧张）

---

## 高级配置

### 修改检查间隔

在 `PerformanceCollector.cs` 中修改:
```csharp
private const int FILE_CHECK_INTERVAL_SECONDS = 15;  // 改为你需要的秒数
private const int NO_INCREASE_THRESHOLD = 2;         // 改为触发警报所需的周期数
```

例如：
- 改为 10 秒检查一次，3 次不增加（30秒）触发
- 改为 30 秒检查一次，1 次不增加（30秒）触发

### 修改警报路径

在 `Form1.cs` 中修改:
```csharp
private const string ALARM_FILE_PATH = @"新的音频文件路径";
```

在 `PerformanceCollector.cs` 中修改:
```csharp
private const string BASE_OUTPUT_PATH = @"新的输出目录";
private const string PYTHON_EXE_PATH = @"新的Python路径";
```

---

## 监控数据含义

### WebUI 状态示例

| 状态 | 含义 | 行为 |
|-----|------|------|
| 正在生成 (文件数 1563) | WebUI 正常工作 | 未触发警报 |
| 目录不存在或空闲 | 输出目录未找到 | 未触发警报 |
| 监控异常 | 文件访问错误 | 未触发警报（等待恢复） |

### Python 进程状态示例

| 状态 | 含义 |
|-----|------|
| 进程 python - CPU: 25.3% \| 内存: 3048.50 MB | 进程正在运行 |
| 进程 python 未运行 | 进程未找到 |

---

## 日志和调试

目前 C# 版本使用 `Debug.WriteLine()` 输出调试信息。

如需查看调试输出，在 Visual Studio 中运行：
```
View → Output → Show output from: Debug
```

或在 Visual Studio Code 中配置 `launch.json`：
```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": ".NET Core Launch (console)",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/bin/Debug/net10.0-windows/C#sd-webui_monitor.dll",
            "args": [],
            "cwd": "${workspaceFolder}",
            "stopAtEntry": false,
            "console": "externalTerminal"
        }
    ]
}
```

---

## 后续功能计划

### 即将添加 (高优先级)
- [ ] GPU 实时数据采集 (使用 P/Invoke 调用 PDH)
- [ ] VRAM 实时数据采集 (使用 PowerShell)
- [ ] VRAM 低于阈值的自动警报

### 可选增强 (中优先级)
- [ ] 网络速度监控
- [ ] 日志系统集成 (Serilog)
- [ ] 历史数据导出

### 长期计划 (低优先级)
- [ ] 系统托盘最小化
- [ ] 自定义警报规则
- [ ] 数据图表展示
- [ ] 远程监控接口

---

## 常见问题 (FAQ)

**Q: 为什么 GPU 数据是模拟的？**
A: 获取实际 GPU 数据需要调用 Windows PDH API 或 PowerShell，目前为简化集成而使用模拟数据。后续会补充。

**Q: 文件数归档清零时会触发警报吗？**
A: 不会。当文件数清零时，系统会将其视为"新的基线"，不会触发警报，除非30秒内没有新增。

**Q: 可以同时监控多个 WebUI 输出目录吗？**
A: 当前支持单个目录。如需多个，可在 PerformanceCollector 中扩展逻辑。

**Q: Python 进程监控的准确性如何？**
A: 使用 Windows PerformanceCounter，准确性接近任务管理器显示的值。

**Q: 能否在后台运行？**
A: 当前为 WinForms 窗体应用。可扩展为系统托盘图标，在后台运行。

---

## 技术支持

如有问题，请检查：
1. ✅ .NET 10.0 SDK 已安装
2. ✅ 所有配置路径正确
3. ✅ 获取管理员权限
4. ✅ 查看 Debug Output 信息

---

**版本**: 1.0  
**最后更新**: 2025-11-15  
**作者**: C# 重构版本  
**原始作者**: Python 版本 作者
