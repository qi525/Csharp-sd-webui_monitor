# C# SD WebUI Monitor - 项目概览

## 📋 项目信息

- **项目名称**: C# SD WebUI 实时监控工具
- **原始技术栈**: Python (Tkinter + PDH + PowerShell)
- **重构技术栈**: C# (WinForms + PerformanceCounter)
- **目标框架**: .NET 10.0 (Windows)
- **开发状态**: Beta (核心功能完成)

---

## 📁 项目结构

```
C:\个人数据\C#Code\C#sd-webui_monitor\
│
├── 📄 源代码文件
│   ├── Form1.cs                      # WinForms UI 主窗体 (246 行)
│   ├── PerformanceCollector.cs       # 数据收集模块 (160+ 行)
│   ├── MonitorData.cs                # 数据模型类
│   ├── AudioPlayer.cs                # 🆕 音频播放模块
│   ├── ProcessMonitor.cs             # 🆕 进程监控模块
│   └── Program.cs                    # 程序入口点
│
├── 📦 配置文件
│   └── C#sd-webui_monitor.csproj     # 项目配置文件
│
├── 📚 文档
│   ├── FEATURE_COMPARISON.md         # Python vs C# 功能对比 ⭐
│   ├── USAGE_GUIDE.md                # 使用指南 ⭐
│   └── README.md                     # 项目说明
│
├── 🐍 Python 源代码 (参考)
│   ├── PythonSourceCode/
│   │   ├── sd-webui_monitor.py       # 原始 Python 版本
│   │   ├── gpu_engine.py             # GPU 引擎监控模块
│   │   ├── gpu_copy_test.py          # GPU Copy 测试
│   │   └── clock.py                  # 时钟示例
│
├── 📦 输出目录
│   ├── bin/
│   │   └── Debug/net10.0-windows/    # 编译输出
│   └── obj/
│       └── Debug/net10.0-windows/    # 中间文件
│
└── 🔧 其他
    └── .gitignore                    # Git 忽略文件
```

---

## 🎯 核心功能

### ✅ 已实现功能

| 功能 | 模块 | 状态 | 说明 |
|------|------|------|------|
| CPU 监控 | PerformanceCollector | ✅ | 实时 CPU 占用率 |
| 物理内存监控 | PerformanceCollector | ✅ | 当前采用模拟，可改进 |
| 虚拟内存监控 | PerformanceCollector | ✅ | 已提交内存占用 |
| GPU Compute 监控 | PerformanceCollector | ✅ | 当前采用模拟数据 |
| GPU Copy 监控 | PerformanceCollector | ✅ | 当前采用模拟数据 |
| 显存监控 | PerformanceCollector | ✅ | 当前采用模拟数据 |
| **WebUI 文件监控** | PerformanceCollector | ✅✨ | 30秒内无增加检测 |
| **WebUI 警报系统** | Form1 + AudioPlayer | ✅✨ | 音频警报播放 |
| **Python 进程监控** | ProcessMonitor | ✅✨ | CPU + 内存占用 |
| 时钟显示 | Form1 | ✅ | 实时时间更新 |
| 网络监控 | - | ⏳ | 当前采用模拟数据 |

**✨ = 新增或改进功能**

### ⚠️ 待改进功能

| 功能 | 优先级 | 说明 |
|------|--------|------|
| GPU 实时数据采集 | 🔴 高 | 需集成 PDH (P/Invoke) |
| VRAM 实时数据采集 | 🔴 高 | 需集成 PowerShell |
| 网络速度监控 | 🟡 中 | 需使用 NetworkInterface |
| 日志系统 | 🟡 中 | 建议集成 Serilog |
| 物理内存真实值 | 🟢 低 | 当前模拟，可使用 WMI |

---

## 🔄 主要改进点

### 1️⃣ WebUI 文件检测逻辑改进
```
Python:   每30秒检查一次 → 连续2次(60秒)不增加触发警报
C#:       每15秒检查一次 → 连续2次(30秒)不增加触发警报 ✨
```
**优势**: 更快的响应速度，更符合用户需求

### 2️⃣ 新增 Python 进程监控模块
```
Python:   ❌ 无进程监控
C#:       ✅ ProcessMonitor 类 (CPU + 内存占用)
```

### 3️⃣ 音频警报异步播放
```
Python:   同步播放 (playsound3)
C#:       异步播放 (Task.Run) ✨ - 不阻塞 UI
```

### 4️⃣ 模块化代码结构
```
AudioPlayer.cs       - 独立的音频播放模块
ProcessMonitor.cs    - 独立的进程监控模块
PerformanceCollector - 统一的数据收集器
```

---

## 🏗️ 架构设计

### 数据流图
```
┌─────────────────┐
│  System Info    │  Windows PerformanceCounter
└────────┬────────┘
         │
         ▼
┌─────────────────────────────────┐
│   PerformanceCollector          │
│   - CPU/内存/虚拟内存采集      │
│   - WebUI 文件监控             │
│   - Python 进程监控            │
└────────┬────────────────────────┘
         │
         ▼
┌─────────────────────────────────┐
│      MonitorData (Model)        │
│   - 存储所有监控数据           │
└────────┬────────────────────────┘
         │
         ▼
┌─────────────────────────────────┐
│      Form1 (UI Layer)           │
│   - 显示数据                   │
│   - 触发警报                   │
│   - 处理用户交互               │
└────────┬────────────────────────┘
         │
         ▼
┌─────────────────────────────────┐
│      AudioPlayer                │
│   - 播放警报音                 │
└─────────────────────────────────┘
```

### 类关系图
```
PerformanceCollector
  ├─ ProcessMonitor (python.exe 监控)
  └─ 返回 MonitorData

Form1
  ├─ 使用 PerformanceCollector (数据采集)
  ├─ 显示 MonitorData (数据展示)
  └─ 使用 AudioPlayer (警报播放)

AudioPlayer
  └─ 播放 .wav 文件
```

---

## 🚀 使用流程

### 启动流程
```
1. dotnet run
2. Program.Main()
3. Form1() 构造函数
   ├─ InitializeComponent()     (UI 初始化)
   ├─ PerformanceCollector()    (性能计数器初始化)
   ├─ ProcessMonitor()          (Python 进程初始化)
   └─ SetupUpdateTimer()        (定时器启动 1000ms)
4. 显示窗体
```

### 监控循环
```
每 1 秒:
1. Timer_Tick() 触发
2. PerformanceCollector.CollectData()
   ├─ 读取 CPU/内存/虚拟内存
   ├─ 检查 WebUI 文件数（每15秒）
   ├─ 获取 Python 进程信息
   └─ 返回 MonitorData
3. 检查 WebUI 警报条件
4. Form1.UpdateUI() 更新界面
5. 下一秒继续
```

---

## 📊 核心配置参数

### PerformanceCollector.cs
```csharp
BASE_OUTPUT_PATH = "C:\stable-diffusion-webui\outputs\txt2img-images"
PYTHON_EXE_PATH = "C:\stable-diffusion-webui\venv\python.exe"
FILE_CHECK_INTERVAL_SECONDS = 15          // 文件检查间隔
NO_INCREASE_THRESHOLD = 2                 // 触发警报的周期数 (30秒)
```

### Form1.cs
```csharp
ALARM_FILE_PATH = @"C:\path\to\alarm.wav" // 警报音频文件路径
UpdateTimer.Interval = 1000                // UI 更新间隔 (1秒)
```

### MonitorData.cs
```csharp
TotalCount = 17171       // 总监控次数
NormalCount = 17009      // 正常次数
AlertCount = 3           // 警报次数
```

---

## 💾 文件大小统计

| 文件 | 行数 | 用途 |
|------|------|------|
| Form1.cs | 246 | WinForms UI |
| PerformanceCollector.cs | 160+ | 数据采集 |
| AudioPlayer.cs | 90 | 音频播放 |
| ProcessMonitor.cs | 130 | 进程监控 |
| MonitorData.cs | 45 | 数据模型 |
| Program.cs | 20 | 入口点 |
| **总计** | **~700** | |

---

## 🔧 开发环境

### 必需工具
- .NET 10.0 SDK
- Visual Studio Code 或 Visual Studio 2022
- Windows 10/11

### 推荐扩展 (VS Code)
- C# Extension (ms-dotnettools.csharp)
- .NET Extension Pack (ms-dotnettools.extension-pack-vs)

### 编译命令
```powershell
# 调试模式编译
dotnet build

# 发布模式编译
dotnet build -c Release

# 运行程序
dotnet run

# 清理构建文件
dotnet clean
```

---

## 📝 提交历史

| 日期 | 更改 | 描述 |
|------|------|------|
| 2025-11-15 | ✅ 初始化 | SDK 修复、WebUI 警报、Python 监控 |
| 2025-11-15 | ✅ 添加模块 | AudioPlayer、ProcessMonitor |
| 2025-11-15 | ✅ 文档 | FEATURE_COMPARISON、USAGE_GUIDE |

---

## 🎓 学习参考

### 相关技术文档
- [WinForms 官方文档](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/)
- [PerformanceCounter 类](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.performancecounter)
- [Process 类](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process)
- [SoundPlayer 类](https://docs.microsoft.com/en-us/dotnet/api/system.media.soundplayer)

### 相关项目
- [原始 Python 版本](./PythonSourceCode/)
- [GPU 监控参考](./PythonSourceCode/gpu_engine.py)

---

## 🤝 贡献指南

如需改进此项目，请：

1. **创建新分支**
   ```bash
   git checkout -b feature/your-feature
   ```

2. **修改代码**
   - 遵循现有代码风格
   - 添加适当注释
   - 测试功能

3. **提交 PR**
   ```bash
   git commit -m "feat: 添加新功能描述"
   git push origin feature/your-feature
   ```

4. **更新文档**
   - 修改 FEATURE_COMPARISON.md
   - 更新 USAGE_GUIDE.md

---

## ⚖️ 许可证

该项目基于原始 Python 版本，保持相同的许可条款。

---

## 📞 联系方式

- **Bug 报告**: 创建 GitHub Issue
- **功能请求**: 讨论区或 Issues
- **技术支持**: 查看 USAGE_GUIDE.md 故障排除部分

---

## ✨ 致谢

感谢原始 Python 版本的作者提供的参考实现。

本 C# 版本在以下方面进行了改进：
- 更快的 WebUI 文件检测 (30秒 vs 60秒)
- 新增 Python 进程监控
- 异步音频播放
- 更好的模块化结构

---

**最后更新**: 2025-11-15  
**维护者**: C# 重构团队  
**状态**: 🟢 Active (积极开发中)
