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
    private System.Windows.Forms.Timer? _alarmTimer;
    private const string ALARM_FILE_PATH = @"C:\个人数据\C#Code\C#sd-webui_monitor\7 you.wav"; // 用户需要配置此路径

    // UI 控件
    private Label? lblCurrentTime;
    private Label? lblGpuName;
    private Label? lblCpuUsage;
    private ProgressBar? pbCpu;

    private Label? lblPhysMem;
    private ProgressBar? pbPhysMem;

    private Label? lblVirtMem;
    private ProgressBar? pbVirtMem;

    // GPU 相关的标签和进度条
    private Label? lblGpuCompute;
    private ProgressBar? pbGpuCompute;

    private Label? lblGpuCopy;
    private ProgressBar? pbGpuCopy;

    private Label? lblVramStatus; // 显存状态
    
    // 网络速度
    private Label? lblDownloadSpeed;
    private Label? lblUploadSpeed;

    // WebUI 状态
    private Label? lblWebuiStatus;
    private Label? lblStats; // 总数/正常/警报

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
        this.ClientSize = new Size(500, 580);
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;

        // 1. 当前时间
        lblCurrentTime = CreateLabel("当前时间: 2025-11-15 11:28:15", 20, 10, true, 16, Color.Cyan);
        lblCurrentTime.Size = new Size(this.ClientSize.Width - 40, 30);
        this.Controls.Add(lblCurrentTime);

        // 2. GPU 名称
        lblGpuName = CreateLabel("GPU: Intel Arc A770 16GB", 20, 50, true, 12, Color.FromArgb(173, 216, 230));
        this.Controls.Add(lblGpuName);

        int yPos = 85;

        // 3. CPU 占用
        lblCpuUsage = CreateLabel("CPU 利用率: 0.0%", 20, yPos, false, 10, Color.White);
        pbCpu = CreateProgressBar(20, yPos + 25, Color.Red);
        yPos += 60;

        // 4. 物理内存
        lblPhysMem = CreateLabel("物理内存占用: 0.0 GB / 0.0 GB (0.0%)", 20, yPos, false, 10, Color.White);
        pbPhysMem = CreateProgressBar(20, yPos + 25, Color.Red);
        yPos += 60;

        // 5. 虚拟内存
        lblVirtMem = CreateLabel("虚拟内存占用 (已提交): 0.0 GB / 0.0 GB (0.0%)", 20, yPos, false, 10, Color.White);
        pbVirtMem = CreateProgressBar(20, yPos + 25, Color.Red);
        yPos += 60;

        // 6. GPU 计算着色器
        lblGpuCompute = CreateLabel("GPU 计算着色器 (AI/挖矿/并行): 0.0%", 20, yPos, false, 10, Color.White);
        pbGpuCompute = CreateProgressBar(20, yPos + 25, Color.Red);
        yPos += 60;

        // 7. GPU 数据复制
        lblGpuCopy = CreateLabel("GPU 数据复制 (显存/内存传输): 0.0%", 20, yPos, false, 10, Color.White);
        pbGpuCopy = CreateProgressBar(20, yPos + 25, Color.Orange);
        yPos += 60;

        // 8. 显存占用 (特殊处理，不使用进度条，直接显示文本)
        lblVramStatus = CreateLabel("专有显存占用: 0.00 GB / 0.00 GB (0.0%)", 20, yPos, false, 10, Color.White);
        yPos += 30;
        
        // 9. 下载速度
        lblDownloadSpeed = CreateLabel("下载速度: 0.00 MB/s (上限 100 MB/s)", 20, yPos, false, 10, Color.Orange);
        pbDownload = CreateProgressBar(20, yPos + 25, Color.LightGray, 100, 100); // MaxValue设为100
        yPos += 60;

        // 10. 上传速度
        lblUploadSpeed = CreateLabel("上传速度: 0.00 MB/s (上限 100 MB/s)", 20, yPos, false, 10, Color.White);
        pbUpload = CreateProgressBar(20, yPos + 25, Color.LightGray, 100, 100); // MaxValue设为100
        yPos += 60;

        // 11. VRAM 状态文本 (底部)
        lblVramStatusExtra = CreateLabel("VRAM 状态: 达标 (0.00 GB)", 0, yPos + 5, true, 11, Color.Green);
        lblVramStatusExtra.Size = new Size(this.ClientSize.Width, 30);
        lblVramStatusExtra.TextAlign = ContentAlignment.MiddleCenter;
        yPos += 40;

        // 12. Webui 状态
        lblWebuiStatus = CreateLabel("Webui 状态: 正在生成 (文件数 0)", 0, yPos + 5, true, 11, Color.LightGreen);
        lblWebuiStatus.Size = new Size(this.ClientSize.Width, 30);
        lblWebuiStatus.TextAlign = ContentAlignment.MiddleCenter;
        yPos += 40;

        // 13. 统计数据
        lblStats = CreateLabel("总次数: 0 | 正常: 0 | 警报触发: 0", 20, this.ClientSize.Height - 30, false, 9, Color.Gray);
        this.Controls.Add(lblStats);
    }
    
    // 简化创建 Label 控件的辅助方法
    private Label CreateLabel(string text, int x, int y, bool isCentered, float fontSize, Color color)
    {
        Label label = new Label();
        label.Text = text;
        label.Location = new Point(x, y);
        label.AutoSize = false;
        label.Width = this.ClientSize.Width - 40;
        label.Height = 20;
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
    private ProgressBar CreateProgressBar(int x, int y, Color color, int maximum = 100, int height = 15)
    {
        ProgressBar pb = new ProgressBar();
        pb.Location = new Point(x, y);
        pb.Size = new Size(this.ClientSize.Width - 40, height);
        pb.Maximum = maximum;
        pb.Minimum = 0;
        pb.Value = 0;
        // 注意：WinForms 默认 ProgressBar 颜色不易修改，需要自定义绘制，此处使用默认样式
        this.Controls.Add(pb);
        return pb;
    }

    private ProgressBar? pbDownload; // 临时定义用于 CreateProgressBar 的引用
    private ProgressBar? pbUpload;
    private Label? lblVramStatusExtra;

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
            pbCpu.Value = (int)Math.Min(data.CpuUsagePercent, 100);

        // 物理内存
        if (lblPhysMem != null)
            lblPhysMem.Text = $"物理内存占用: {data.PhysicalMemoryUsageText}";
        if (pbPhysMem != null)
            pbPhysMem.Value = (int)Math.Min(data.PhysicalMemoryUsagePercent, 100);

        // 虚拟内存
        if (lblVirtMem != null)
            lblVirtMem.Text = $"虚拟内存占用 (已提交): {data.VirtualMemoryUsageText}";
        if (pbVirtMem != null)
            pbVirtMem.Value = (int)Math.Min(data.VirtualMemoryUsagePercent, 100);

        // GPU 性能
        if (lblGpuCompute != null)
            lblGpuCompute.Text = $"GPU 计算着色器 (AI/挖矿/并行): {data.GpuComputePercent:F2}%";
        if (pbGpuCompute != null)
            pbGpuCompute.Value = (int)Math.Min(data.GpuComputePercent, 100);

        if (lblGpuCopy != null)
            lblGpuCopy.Text = $"GPU 数据复制 (显存/内存传输): {data.GpuDataCopyPercent:F2}%";
        if (pbGpuCopy != null)
            pbGpuCopy.Value = (int)Math.Min(data.GpuDataCopyPercent, 100);

        // 显存状态
        if (lblVramStatus != null)
            lblVramStatus.Text = $"专有显存占用: {data.VramUsedGB:F2} GB / {data.VramTotalGB:F2} GB ({data.VramUsagePercent:F1}%)";
        if (lblVramStatusExtra != null)
        {
            lblVramStatusExtra.Text = $"VRAM 状态: {data.VramStatusText}";
            lblVramStatusExtra.ForeColor = data.VramStatusColor;
        }

        // 网络速度 (进度条基于 MaxSpeedMBps)
        if (lblDownloadSpeed != null)
            lblDownloadSpeed.Text = $"下载速度: {data.DownloadSpeedMBps:F2} MB/s (上限 {data.MaxSpeedMBps:F0} MB/s)";
        if (pbDownload != null)
            pbDownload.Value = (int)Math.Min(data.DownloadSpeedMBps / data.MaxSpeedMBps * 100, 100);
        
        if (lblUploadSpeed != null)
            lblUploadSpeed.Text = $"上传速度: {data.UploadSpeedMBps:F2} MB/s (上限 {data.MaxSpeedMBps:F0} MB/s)";
        if (pbUpload != null)
            pbUpload.Value = (int)Math.Min(data.UploadSpeedMBps / data.MaxSpeedMBps * 100, 100);

        // WebUI 状态
        if (lblWebuiStatus != null)
            lblWebuiStatus.Text = data.WebuiStatus;

        // 统计数据
        if (lblStats != null)
            lblStats.Text = $"总次数: {data.TotalCount} | 正常: {data.NormalCount} | 警报触发: {data.AlertCount}";
    }
}