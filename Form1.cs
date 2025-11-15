using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CSharpSdWebuiMonitor;

public class Form1 : Form
{
    private PerformanceCollector _collector = null!;
    private System.Windows.Forms.Timer _updateTimer = null!;
    private MonitorData _currentData = null!;
    
    // 音频播放和警报
    private AudioPlayer? _audioPlayer;
    private bool _isAlarmPlaying = false;
    private const string ALARM_FILE_PATH = @"C:\个人数据\C#Code\C#sd-webui_monitor\7 you.wav"; // 用户需要配置此路径

    // UI 控件
    private Label? lblCurrentTime;
    private Label? lblGpuName;
    private Label? lblCpuUsage;
    private ColoredProgressBar? pbCpu;

    private Label? lblPhysMem;
    private ColoredProgressBar? pbPhysMem;

    private Label? lblVirtMem;
    private ColoredProgressBar? pbVirtMem;

    // GPU 相关的标签和进度条
    private Label? lblGpuCompute;
    private ColoredProgressBar? pbGpuCompute;

    private Label? lblGpuCopy;
    private ColoredProgressBar? pbGpuCopy;

    private Label? lblGpu3D; // GPU 3D 渲染
    private ColoredProgressBar? pbGpu3D;

    private Label? lblVramStatus; // 显存状态
    private ColoredProgressBar? pbVramStatus; // 显存状态进度条
    
    // 网络速度
    private Label? lblDownloadSpeed;
    private Label? lblUploadSpeed;

    private Label? lblStats; // 总数/正常/警报
    
    // 新增：详细监控信息标签
    private Label? lblVramStatusInfo; // VRAM状态：达标/不达标
    private Label? lblVmRiskWarning;  // 虚拟内存风险警告
    private Label? lblWebuiStatusInfo; // WebUI状态详情

    // 构造函数，初始化窗体和控件
    public Form1()
    {
        InitializeComponent();
        _collector = new PerformanceCollector();
        SetupUpdateTimer();
    }

    // 设置窗体和所有控件的布局
    private void InitializeComponent()
    {
        this.Text = "Intel Arc A770 实时监控 (GPU 核心引擎细分增强)";
        this.BackColor = Color.FromArgb(24, 24, 24); // 深色背景
        this.ForeColor = Color.White;
        this.ClientSize = new Size(700, 740); // 再减少 25% (990 → 740)
        this.FormBorderStyle = FormBorderStyle.Sizable; // 允许调整窗口大小
        this.MaximizeBox = true;
        this.MinimumSize = new Size(600, 540);

        // 1. 当前时间 (顶部标题)
        lblCurrentTime = CreateLabel("当前时间: 2025-11-15 11:28:15", 15, 8, true, 14, Color.Cyan);
        lblCurrentTime.Size = new Size(this.ClientSize.Width - 30, 25);
        this.Controls.Add(lblCurrentTime);

        // 2. GPU 名称 (副标题)
        lblGpuName = CreateLabel("GPU: Intel Arc A770 16GB", 15, 35, true, 11, Color.FromArgb(173, 216, 230));
        lblGpuName.Size = new Size(this.ClientSize.Width - 30, 22);
        this.Controls.Add(lblGpuName);

        int yPos = 62;

        // 3. CPU 占用
        lblCpuUsage = CreateLabel("CPU 利用率: 0.0%", 15, yPos, false, 9, Color.White);
        pbCpu = CreateProgressBar(15, yPos + 20, Color.Red, 100, 12);
        yPos += 48;

        // 4. 物理内存
        lblPhysMem = CreateLabel("物理内存占用: 0.0 GB / 0.0 GB (0.0%)", 15, yPos, false, 9, Color.White);
        pbPhysMem = CreateProgressBar(15, yPos + 20, Color.Red, 100, 12);
        yPos += 48;

        // 5. 虚拟内存
        lblVirtMem = CreateLabel("虚拟内存占用 (已提交): 0.0 GB / 0.0 GB (0.0%)", 15, yPos, false, 9, Color.White);
        pbVirtMem = CreateProgressBar(15, yPos + 20, Color.Red, 100, 12);
        yPos += 48;

        // 6. GPU 计算着色器
        lblGpuCompute = CreateLabel("GPU 计算着色器 (AI/挖矿/并行): 0.0%", 15, yPos, false, 9, Color.White);
        pbGpuCompute = CreateProgressBar(15, yPos + 20, Color.Red, 100, 12);
        yPos += 48;

        // 7. GPU 数据复制
        lblGpuCopy = CreateLabel("GPU 数据复制 (显存/内存传输): 0.0%", 15, yPos, false, 9, Color.White);
        pbGpuCopy = CreateProgressBar(15, yPos + 20, Color.Orange, 100, 12);
        yPos += 48;

        // 7.5. GPU 3D 引擎 (新增)
        lblGpu3D = CreateLabel("GPU 3D 渲染 (游戏/图形加速): 0.0%", 15, yPos, false, 9, Color.White);
        pbGpu3D = CreateProgressBar(15, yPos + 20, Color.Green, 100, 12);
        yPos += 48;

        // 8. 显存占用 (新增进度条)
        lblVramStatus = CreateLabel("专有显存占用: 0.00 GB / 0.00 GB (0.0%)", 15, yPos, false, 9, Color.White);
        pbVramStatus = CreateProgressBar(15, yPos + 20, Color.Cyan, 100, 12);
        yPos += 48;
        
        // 9. 下载速度
        lblDownloadSpeed = CreateLabel("下载速度: 0.00 MB/s (上限 100 MB/s)", 15, yPos, false, 9, Color.White);
        pbDownload = CreateProgressBar(15, yPos + 20, Color.LightGray, 100, 12);
        yPos += 48;

        // 10. 上传速度
        lblUploadSpeed = CreateLabel("上传速度: 0.00 MB/s (上限 100 MB/s)", 15, yPos, false, 9, Color.White);
        pbUpload = CreateProgressBar(15, yPos + 20, Color.LightGray, 100, 12);
        yPos += 48;

        // 【新增】11. VRAM 状态详情（达标/不达标 + 具体 GB 值）
        lblVramStatusInfo = CreateLabel("VRAM状态：达标（0.00 GB）", 0, yPos, true, 10, Color.Green);
        lblVramStatusInfo.Size = new Size(this.ClientSize.Width, 28);
        lblVramStatusInfo.TextAlign = ContentAlignment.MiddleCenter;
        this.Controls.Add(lblVramStatusInfo);
        yPos += 35;

        // 【新增】12. VM 风险警告（>80GB 时以橙色显示）
        lblVmRiskWarning = CreateLabel("风险：VM 正常", 0, yPos, true, 10, Color.LightGreen);
        lblVmRiskWarning.Size = new Size(this.ClientSize.Width, 28);
        lblVmRiskWarning.TextAlign = ContentAlignment.MiddleCenter;
        this.Controls.Add(lblVmRiskWarning);
        yPos += 35;

        // 【新增】13. WebUI 状态详情（强调今日文件数）
        lblWebuiStatusInfo = CreateLabel("webui状态：正在生成（文件数 0）【今日文件】", 0, yPos, true, 10, Color.LightGreen);
        lblWebuiStatusInfo.Size = new Size(this.ClientSize.Width, 28);
        lblWebuiStatusInfo.TextAlign = ContentAlignment.MiddleCenter;
        this.Controls.Add(lblWebuiStatusInfo);
        yPos += 35;

        // 16. 统计数据 (底部)
        lblStats = CreateLabel("总次数: 0 | 正常: 0 | 警报触发: 0", 15, yPos, false, 9, Color.Gray);
        this.Controls.Add(lblStats);
    }
    
    // 简化创建 Label 控件的辅助方法
    private Label CreateLabel(string text, int x, int y, bool isCentered, float fontSize, Color color)
    {
        Label label = new Label();
        label.Text = text;
        label.Location = new Point(x, y);
        label.AutoSize = false;
        label.Width = this.ClientSize.Width - (x * 2);
        label.Height = 22;
        label.Font = new Font("Microsoft YaHei UI", fontSize, FontStyle.Regular);
        if (isCentered)
        {
            label.Font = new Font(label.Font, FontStyle.Bold);
        }
        label.ForeColor = color;
        label.BackColor = Color.Transparent;
        this.Controls.Add(label);
        return label;
    }

    // 简化创建 ProgressBar 控件的辅助方法
    private ColoredProgressBar CreateProgressBar(int x, int y, Color color, int maximum = 100, int height = 15)
    {
        ColoredProgressBar pb = new ColoredProgressBar();
        pb.Location = new Point(x, y);
        pb.Size = new Size(this.ClientSize.Width - (x * 2), height);
        pb.Maximum = maximum;
        pb.Minimum = 0;
        pb.Value = 0;
        pb.BarColor = color;
        // 启用自定义绘制
        pb.Style = ProgressBarStyle.Continuous;
        this.Controls.Add(pb);
        return pb;
    }

    private ColoredProgressBar? pbDownload; // 下载速度进度条
    private ColoredProgressBar? pbUpload; // 上传速度进度条

    // 设置定时器，用于实时刷新数据
    private void SetupUpdateTimer()
    {
        _updateTimer = new System.Windows.Forms.Timer();
        _updateTimer.Interval = 1000; // 每 1000ms (1秒) 刷新一次
        _updateTimer.Tick += new EventHandler(Timer_Tick);
        _updateTimer.Start();
    }

    // 定时器触发的事件处理函数
    private void Timer_Tick(object? sender, EventArgs e)
    {
        // 异步调用数据收集，避免阻塞 UI 线程，但 WinForms 的 Timer 默认在 UI 线程运行，
        // 为了演示简洁，此处直接同步调用，如果数据收集耗时，应改用 Task.Run()
        _currentData = _collector.CollectData();
        
        // 【新增】检查 WebUI 文件数量是否在30秒内有增加
        bool isWebUIIncreasing = _collector.CheckWebUIFileIncrease();
        _currentData.IsWebuiAlertTriggered = !isWebUIIncreasing;
        
        // 【新增】触发警报逻辑
        if (!isWebUIIncreasing)
        {
            TriggerWebUIAlarm();
        }
        else
        {
            StopWebUIAlarm();
        }
        
        // 【新增】获取 Python 进程信息
        _currentData.PythonProcessInfo = _collector.GetPythonProcessInfo();
        
        UpdateUI(_currentData);
    }

    /// <summary>
    /// 触发 WebUI 警报 - 30秒内文件数未增加
    /// </summary>
    private void TriggerWebUIAlarm()
    {
        if (_isAlarmPlaying)
            return; // 已在播放

        _isAlarmPlaying = true;
        
        // 在后台线程播放音频，避免阻塞 UI
        Task.Run(() =>
        {
            try
            {
                if (_audioPlayer == null && File.Exists(ALARM_FILE_PATH))
                {
                    _audioPlayer = new AudioPlayer(ALARM_FILE_PATH);
                }

                if (_audioPlayer != null)
                {
                    _audioPlayer.PlayAlarmAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"播放警报音失败: {ex.Message}");
            }
        });
    }

    /// <summary>
    /// 停止 WebUI 警报
    /// </summary>
    private void StopWebUIAlarm()
    {
        if (!_isAlarmPlaying)
            return;

        _isAlarmPlaying = false;
        
        try
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.Stop();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"停止警报音失败: {ex.Message}");
        }
    }

    // 根据收集到的数据更新所有 UI 控件
    private void UpdateUI(MonitorData data)
    {
        // 头部信息
        if (lblCurrentTime != null)
            lblCurrentTime.Text = $"当前时间: {data.CurrentTime:yyyy-MM-dd HH:mm:ss}";

        // CPU
        if (lblCpuUsage != null)
            lblCpuUsage.Text = $"CPU 利用率: {data.CpuUsagePercent:F1}%";
        if (pbCpu != null)
        {
            pbCpu.Value = (int)Math.Min(data.CpuUsagePercent, 100);
            // 动态颜色：50%以上橙色，80%以上红色
            if (data.CpuUsagePercent >= 80.0)
                pbCpu.BarColor = Color.Red;
            else if (data.CpuUsagePercent >= 50.0)
                pbCpu.BarColor = Color.Orange;
            else
                pbCpu.BarColor = Color.Green;
        }

        // 物理内存
        if (lblPhysMem != null)
            lblPhysMem.Text = $"物理内存占用: {data.PhysicalMemoryUsageText}";
        if (pbPhysMem != null)
        {
            pbPhysMem.Value = (int)Math.Min(data.PhysicalMemoryUsagePercent, 100);
            // 动态颜色：50%以上橙色，80%以上红色
            if (data.PhysicalMemoryUsagePercent >= 80.0)
                pbPhysMem.BarColor = Color.Red;
            else if (data.PhysicalMemoryUsagePercent >= 50.0)
                pbPhysMem.BarColor = Color.Orange;
            else
                pbPhysMem.BarColor = Color.Green;
        }

        // 虚拟内存
        if (lblVirtMem != null)
            lblVirtMem.Text = $"虚拟内存占用 (已提交): {data.VirtualMemoryUsageText}";
        if (pbVirtMem != null)
        {
            pbVirtMem.Value = (int)Math.Min(data.VirtualMemoryUsagePercent, 100);
            // 动态颜色：50%以上橙色，80%以上红色
            if (data.VirtualMemoryUsagePercent >= 80.0)
                pbVirtMem.BarColor = Color.Red;
            else if (data.VirtualMemoryUsagePercent >= 50.0)
                pbVirtMem.BarColor = Color.Orange;
            else
                pbVirtMem.BarColor = Color.Green;
        }

        // GPU 性能
        if (lblGpuCompute != null)
            lblGpuCompute.Text = $"GPU 计算着色器 (AI/挖矿/并行): {data.GpuComputePercent:F2}%";
        if (pbGpuCompute != null)
        {
            pbGpuCompute.Value = (int)Math.Min(data.GpuComputePercent, 100);
            // 动态颜色：50%以上橙色，80%以上红色
            if (data.GpuComputePercent >= 80.0)
                pbGpuCompute.BarColor = Color.Red;
            else if (data.GpuComputePercent >= 50.0)
                pbGpuCompute.BarColor = Color.Orange;
            else
                pbGpuCompute.BarColor = Color.Green;
        }

        if (lblGpuCopy != null)
            lblGpuCopy.Text = $"GPU 数据复制 (显存/内存传输): {data.GpuDataCopyPercent:F2}%";
        if (pbGpuCopy != null)
        {
            pbGpuCopy.Value = (int)Math.Min(data.GpuDataCopyPercent, 100);
            // 动态颜色：50%以上橙色，80%以上红色
            if (data.GpuDataCopyPercent >= 80.0)
                pbGpuCopy.BarColor = Color.Red;
            else if (data.GpuDataCopyPercent >= 50.0)
                pbGpuCopy.BarColor = Color.Orange;
            else
                pbGpuCopy.BarColor = Color.Orange; // 本身就是橙色
        }

        // GPU 3D 渲染
        if (lblGpu3D != null)
            lblGpu3D.Text = $"GPU 3D 渲染 (游戏/图形加速): {data.Gpu3DRenderPercent:F2}%";
        if (pbGpu3D != null)
        {
            pbGpu3D.Value = (int)Math.Min(data.Gpu3DRenderPercent, 100);
            // 动态颜色：50%以上橙色，80%以上红色
            if (data.Gpu3DRenderPercent >= 80.0)
                pbGpu3D.BarColor = Color.Red;
            else if (data.Gpu3DRenderPercent >= 50.0)
                pbGpu3D.BarColor = Color.Orange;
            else
                pbGpu3D.BarColor = Color.Green;
        }

        // 显存状态
        if (lblVramStatus != null)
            lblVramStatus.Text = $"专有显存占用: {data.VramUsedGB:F2} GB / {data.VramTotalGB:F2} GB ({data.VramUsagePercent:F1}%)";
        if (pbVramStatus != null)
        {
            pbVramStatus.Value = (int)Math.Min(data.VramUsagePercent, 100);
            // 动态颜色：50%以上橙色，80%以上红色
            if (data.VramUsagePercent >= 80.0)
                pbVramStatus.BarColor = Color.Red;
            else if (data.VramUsagePercent >= 50.0)
                pbVramStatus.BarColor = Color.Orange;
            else
                pbVramStatus.BarColor = Color.Cyan;
        }

        // 网络速度 (进度条基于 MaxSpeedMBps)
        if (lblDownloadSpeed != null)
            lblDownloadSpeed.Text = $"下载速度: {data.DownloadSpeedMBps:F2} MB/s (上限 {data.MaxSpeedMBps:F0} MB/s)";
        if (pbDownload != null)
        {
            double downloadPercent = data.DownloadSpeedMBps / data.MaxSpeedMBps * 100;
            pbDownload.Value = (int)Math.Min(downloadPercent, 100);
            // 动态颜色：50%以上橙色，80%以上红色
            if (downloadPercent >= 80.0)
                pbDownload.BarColor = Color.Red;
            else if (downloadPercent >= 50.0)
                pbDownload.BarColor = Color.Orange;
            else
                pbDownload.BarColor = Color.LightGray;
        }
        
        if (lblUploadSpeed != null)
            lblUploadSpeed.Text = $"上传速度: {data.UploadSpeedMBps:F2} MB/s (上限 {data.MaxSpeedMBps:F0} MB/s)";
        if (pbUpload != null)
        {
            double uploadPercent = data.UploadSpeedMBps / data.MaxSpeedMBps * 100;
            pbUpload.Value = (int)Math.Min(uploadPercent, 100);
            // 动态颜色：50%以上橙色，80%以上红色
            if (uploadPercent >= 80.0)
                pbUpload.BarColor = Color.Red;
            else if (uploadPercent >= 50.0)
                pbUpload.BarColor = Color.Orange;
            else
                pbUpload.BarColor = Color.LightGray;
        }

        // 【新增】三行详细监控信息
        
        // 1. VRAM 状态：达标/不达标 + 具体 GB 值
        if (lblVramStatusInfo != null)
        {
            bool isVramOk = data.VramUsagePercent < 90.0;
            string statusText = isVramOk ? "达标" : "不达标";
            lblVramStatusInfo.Text = $"VRAM状态：{statusText}（{data.VramUsedGB:F2} GB）";
            lblVramStatusInfo.ForeColor = isVramOk ? Color.Green : Color.Red;
        }

        // 2. VM 风险警告：>80GB 时以橙色显示
        if (lblVmRiskWarning != null)
        {
            // 从 VirtualMemoryUsageText 中解析当前使用的 GB 值
            // 格式："69.9 GB / 80.7 GB (86.6%)"
            string vmText = data.VirtualMemoryUsageText;
            double vmUsedGB = 0.0;
            
            try
            {
                if (!string.IsNullOrEmpty(vmText))
                {
                    // 提取第一个数字（已使用的 GB）
                    int spaceIndex = vmText.IndexOf(' ');
                    if (spaceIndex > 0 && double.TryParse(vmText.Substring(0, spaceIndex), out var parsed))
                    {
                        vmUsedGB = parsed;
                    }
                }
            }
            catch { }
            
            if (vmUsedGB > 80.0)
            {
                lblVmRiskWarning.Text = $"风险：VM {vmUsedGB:F1}GB（高于80GB存在爆内存风险）";
                lblVmRiskWarning.ForeColor = Color.Orange; // 橙色警告
            }
            else
            {
                lblVmRiskWarning.Text = $"风险：VM {vmUsedGB:F1}GB（正常）";
                lblVmRiskWarning.ForeColor = Color.LightGreen;
            }
        }

        // 3. WebUI 状态详情：强调今日文件数
        if (lblWebuiStatusInfo != null)
        {
            lblWebuiStatusInfo.Text = $"webui状态：正在生成（文件数 {data.GeneratedFileCount}）【今日文件】";
            lblWebuiStatusInfo.ForeColor = data.IsWebuiAlertTriggered ? Color.Red : Color.LightGreen;
        }

        // 统计数据
        if (lblStats != null)
            lblStats.Text = $"总次数: {data.TotalCount} | 正常: {data.NormalCount} | 警报触发: {data.AlertCount}";
    }
}