using System;
using System.IO;
using System.Media;

namespace CSharpSdWebuiMonitor
{
    /// <summary>
    /// 音频播放模块 - 用于播放警报铃声
    /// </summary>
    public class AudioPlayer : IDisposable
    {
        private SoundPlayer? _soundPlayer;
        private string? _soundFilePath;

        /// <summary>
        /// 初始化音频播放器
        /// </summary>
        /// <param name="soundFilePath">WAV文件路径</param>
        public AudioPlayer(string soundFilePath)
        {
            _soundFilePath = soundFilePath;
            
            if (!File.Exists(soundFilePath))
            {
                throw new FileNotFoundException($"音频文件未找到: {soundFilePath}");
            }

            try
            {
                _soundPlayer = new SoundPlayer(soundFilePath);
                // 预加载音频文件
                _soundPlayer.LoadAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"初始化音频播放器失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 播放警报音
        /// </summary>
        public void PlayAlarm()
        {
            try
            {
                if (_soundPlayer != null)
                {
                    // 同步播放（会阻塞直到播放完毕）
                    _soundPlayer.PlaySync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"播放音频失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 异步播放警报音（后台播放，不阻塞）
        /// </summary>
        public void PlayAlarmAsync()
        {
            try
            {
                if (_soundPlayer != null)
                {
                    _soundPlayer.Play();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"异步播放音频失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        public void Stop()
        {
            try
            {
                if (_soundPlayer != null)
                {
                    _soundPlayer.Stop();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"停止播放失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _soundPlayer?.Dispose();
            _soundPlayer = null;
        }
    }
}
