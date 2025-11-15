# Python vs C# 功能对比 - SD WebUI 实时监控

## 总体功能架构

### Python 版本 (sd-webui_monitor.py)
- **UI框架**: Tkinter (GUI)
- **核心功能**: CPU/内存/GPU/VRAM/网络/WebUI监控
- **平台**: Windows (使用 PDH、PowerShell、WMI)
- **线程**: 使用 ThreadPoolExecutor 实现后台数据收集
- **音频**: playsound3 播放警报音

### C# 版本 (C#sd-webui_monitor)
- **UI框架**: Windows Forms (WinForms)
- **核心功能**: CPU/内存/GPU/VRAM/网络/WebUI监控 + Python进程监控
- **平台**: Windows (使用 PerformanceCounter)
- **线程**: System.Windows.Forms.Timer 或异步Task
- **音频**: System.Media.SoundPlayer 播放警报音

---

## 功能详细对比

| 功能模块 | Python 实现 | C# 实现 | 状态 |
|---------|-----------|--------|------|
| **系统监控** | | | |
| CPU占用率 | PerformanceCounter | PerformanceCounter | ✅ |
| 物理内存占用 | psutil.virtual_memory() | PerformanceCounter + 模拟 | ✅ (可改进) |
| 虚拟内存占用 | _get_windows_commit_charge() | PerformanceCounter | ✅ |
| **GPU 监控** | | | |
| GPU Compute利用率 | PDH (win32pdh) | 模拟数据 | ⚠️ (需集成PDH) |
| GPU Copy利用率 | PDH (win32pdh) | 模拟数据 | ⚠️ (需集成PDH) |
| GPU 3D渲染利用率 | PDH (win32pdh) | 模拟数据 | ⚠️ (需集成PDH) |
| **显存 (VRAM) 监控** | | | |
| 专有显存占用 | PowerShell性能计数器 | 模拟数据 | ⚠️ (需集成PowerShell) |
| 显存占用百分比 | 计算得出 | 模拟数据 | ⚠️ |
| **网络监控** | | | |
| 下载速度 | psutil.net_io_counters() | 模拟数据 | ⚠️ (需集成) |
| 上传速度 | psutil.net_io_counters() | 模拟数据 | ⚠️ (需集成) |
| **WebUI 文件监控** | | | |
| 文件数量检测 | 定期查询目录 | 定期查询目录 (15秒间隔) | ✅ |
| 30秒内文件增加检测 | 2个30秒周期检测 | **改进**: 2个15秒周期检测 = 30秒 | ✅✨ |
| 文件数不增加警报 | 播放音频 (playsound3) | **新增**: System.Media.SoundPlayer | ✅ |
| **Python进程监控** | | | |
| python.exe 检测 | ❌ 无 | **新增**: ProcessMonitor 模块 | ✨✅ |
| CPU占用率 | ❌ | ✅ 支持 | ✨✅ |
| 内存占用 (MB) | ❌ | ✅ 支持 | ✨✅ |
| **警报系统** | | | |
| WebUI文件停止生成 | 铃声警报 | **新增**: 铃声警报 + 异步播放 | ✅ |
| VRAM 低于阈值 | 铃声警报 | ✅ (可添加) | ⚠️ |
| 虚拟内存超过阈值 | 橙色提示 | ✅ (可添加) | ⚠️ |
| **日志系统** | | | |
| loguru日志 | ✅ 完整 | ❌ 无 | ⚠️ (可添加) |
| 周期性VM增长记录 | ✅ (每30秒) | ❌ | ⚠️ (可添加) |

---

## 新增/改进功能 (C# 中独有)

### ✨ 新增功能
1. **Python 进程监控 (ProcessMonitor.cs)**
   - 实时监控 `python.exe` 的 CPU 占用
   - 实时监控 `python.exe` 的内存占用 (MB)
   - 自动检测进程是否仍在运行

2. **优化的 WebUI 文件检查**
   - 从 Python 的 30 秒固定周期改为灵活的 15 秒周期
   - 更快的响应时间（如果需要可调整）
   - 文件归档（被清零）的自动处理

3. **异步音频播放**
   - 使用 `Task.Run()` 在后台线程播放警报音
   - 避免 UI 阻塞

4. **专用音频模块 (AudioPlayer.cs)**
   - 封装 SoundPlayer 功能
   - 支持同步和异步播放
   - 错误处理和资源释放

---

## 待改进/需补充的功能

### 高优先级 (核心功能)
| 功能 | Python 实现方式 | C# 需要补充 |
|-----|---------------|-----------|
| GPU 引擎细分 (Compute/Copy/3D) | PDH + win32pdh | ⚠️ 需集成 win32pdh.dll (P/Invoke) |
| VRAM 实时占用 | PowerShell 性能计数器 | ⚠️ 需集成 PowerShell 调用 |
| 网络速度 | psutil | ⚠️ 需集成 NetworkInterface 或 SNMP |

### 中优先级 (增强功能)
| 功能 | Python 实现方式 | C# 建议 |
|-----|---------------|--------|
| 日志记录 | loguru + 控制台输出 | 建议集成 Serilog 或 log4net |
| 周期性统计 | 定时记录VM增长 | 建议添加到 PerformanceCollector |
| VRAM 警报 | 低于8GB触发 | 建议在 Form1 中实现 |
| 虚拟内存警告 | 超过80GB橙色提示 | 建议在 UpdateUI 中实现 |
| PDH 重试机制 | 60秒冷却重试 | 建议集成到 PerformanceCollector |

### 低优先级 (优化)
- 物理内存获取（当前模拟数据，可使用 WMI）
- 进程优先级管理
- 历史数据记录和导出

---

## 文件数量监控逻辑改进

### Python 原实现
```
每30秒检测一次：
- 如果文件数 > 上次记录：重置计数器
- 如果文件数 <= 上次记录：计数器 +1
- 当计数器 >= 2（60秒无增加）：触发警报
```

### C# 改进实现
```
每15秒检测一次：
- 如果文件数 > 上次记录：重置计数器 (文件增加 ✓)
- 如果文件数 <= 上次记录：计数器 +1
- 当计数器 >= 2（30秒无增加）：触发警报 ✨
- 自动处理文件归档场景（文件数清零也被视为正常）

优势：
1. 检测周期更快 (30秒 vs 60秒)
2. 更灵活的周期配置 (常量可调)
3. 符合用户需求的"30秒内无增加"定义
```

---

## 代码模块结构

### C# 项目结构
```
C#sd-webui_monitor/
├── Form1.cs                    # WinForms UI 主窗体
├── PerformanceCollector.cs     # 系统/GPU/WebUI 数据收集
├── MonitorData.cs              # 监控数据模型
├── AudioPlayer.cs              # 音频播放模块 (NEW)
├── ProcessMonitor.cs           # Python进程监控模块 (NEW)
├── Program.cs                  # 程序入口
└── C#sd-webui_monitor.csproj   # 项目配置
```

### 核心类关系
```
Form1 (UI)
  ├─→ PerformanceCollector (数据收集)
  │    ├─→ ProcessMonitor (Python进程)
  │    └─→ MonitorData (数据容器)
  └─→ AudioPlayer (音频播放) 
```

---

## 部署/配置须知

### 必需文件
1. **警报音频文件**: 用户需要提供 WAV 文件
   - 路径: `ALARM_FILE_PATH` (Form1.cs 第13行)
   - 需要修改为实际音频文件路径

2. **WebUI 输出目录** (PerformanceCollector.cs 第18行)
   - 默认: `C:\stable-diffusion-webui\outputs\txt2img-images`
   - 如不同，需修改常量 `BASE_OUTPUT_PATH`

3. **Python 进程路径** (PerformanceCollector.cs 第31行)
   - 默认: `C:\stable-diffusion-webui\venv\python.exe`
   - 如不同，需修改常量 `PYTHON_EXE_PATH`

### 权限要求
- Windows 管理员权限（读取 PerformanceCounter）
- 访问WebUI输出目录的权限

---

## 后续开发建议

### Phase 1 (当前完成)
- ✅ 基础系统监控 (CPU/内存)
- ✅ WebUI 文件监控 + 警报
- ✅ Python 进程监控
- ✅ UI 框架

### Phase 2 (推荐实现)
- ⚠️ 集成 GPU 引擎细分 (使用 P/Invoke 调用 win32pdh.dll)
- ⚠️ 集成 PowerShell 获取 VRAM 实时占用
- ⚠️ 日志系统 (Serilog)

### Phase 3 (可选增强)
- 网络速度监控
- 数据导出和历史记录
- 自定义警报规则
- 系统托盘最小化

---

## 快速迁移清单

- [x] 系统监控基础
- [x] WebUI 文件监控  
- [x] 警报系统框架
- [x] Python 进程监控
- [ ] GPU 引擎细分 (需 P/Invoke)
- [ ] VRAM PowerShell 集成
- [ ] 网络监控
- [ ] 日志系统
- [ ] 异常处理加强
- [ ] 单元测试

---

**最后更新**: 2025-11-15  
**状态**: 核心功能完成，可投入基础使用。建议补充 GPU 和 VRAM 实时数据采集。
