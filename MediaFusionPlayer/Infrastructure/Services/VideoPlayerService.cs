using LibVLCSharp.Shared;
using MediaFusionPlayer.Core.Interfaces;
using System;
using System.IO;
using System.Windows;

namespace MediaFusionPlayer.Infrastructure.Services
{
    public sealed class VideoPlayerService : IVideoPlayerService
    {
        private LibVLC? _libVlc;
        private MediaPlayer? _mediaPlayer;
        private Media? _currentMedia;
        private string? _currentVideoPath;

        public bool IsPlaying => _mediaPlayer?.IsPlaying ?? false;
        public bool IsInitialized => _libVlc != null;
        public string? CurrentVideoPath => _currentVideoPath;
        public LibVLC? LibVLC => _libVlc; // Реализуем свойство

        public event EventHandler<Exception>? VideoError;

        public VideoPlayerService()
        {
            // Инициализация будет отложенной
        }

        public void Initialize()
        {
            try
            {
                if (!IsInitialized)
                {
                    // Инициализация ядра VLC
                    LibVLCSharp.Shared.Core.Initialize();

                    // Отключаем аудио в VLC, т.к. звук будет через NAudio
                    _libVlc = new LibVLC(":no-audio");

                    _mediaPlayer = new MediaPlayer(_libVlc);
                }
            }
            catch (Exception ex)
            {
                VideoError?.Invoke(this, ex);
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации VLC: {ex.Message}");
            }
        }

        public void PlayVideo(string videoPath)
        {
            if (!File.Exists(videoPath) || _libVlc == null || _mediaPlayer == null)
                return;

            try
            {
                StopVideo();

                _currentVideoPath = videoPath;

                // Добавляем параметры для лучшей производительности
                var options = new[]
                {
            ":no-audio",
            ":avcodec-hw=dxva2", // Аппаратное ускорение
            ":network-caching=300", // Кэширование
            ":clock-jitter=0",
            ":clock-synchro=0"
        };

                _currentMedia = new Media(_libVlc, videoPath);
                foreach (var option in options)
                {
                    _currentMedia.AddOption(option);
                }

                _mediaPlayer.Media = _currentMedia;
                _mediaPlayer.Play();
            }
            catch (Exception ex)
            {
                VideoError?.Invoke(this, ex);
                System.Diagnostics.Debug.WriteLine($"Ошибка воспроизведения видео: {ex.Message}");
            }
        }

        public void PauseVideo()
        {
            if (_mediaPlayer?.IsPlaying == true)
            {
                _mediaPlayer.Pause();
            }
        }

        public void StopVideo()
        {
            _currentVideoPath = null;

            if (_mediaPlayer != null)
            {
                _mediaPlayer.Stop();
                _currentMedia?.Dispose();
                _currentMedia = null;
            }
        }

        public void SeekVideo(TimeSpan position)
        {
            if (_mediaPlayer != null && _mediaPlayer.IsSeekable)
            {
                _mediaPlayer.Time = (long)position.TotalMilliseconds;
            }
        }

        public void SetVideoOutput(IntPtr handle)
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Hwnd = handle;
            }
        }

        public void Dispose()
        {
            StopVideo();

            _mediaPlayer?.Dispose();
            _libVlc?.Dispose();

            _mediaPlayer = null;
            _libVlc = null;
        }
    }
}