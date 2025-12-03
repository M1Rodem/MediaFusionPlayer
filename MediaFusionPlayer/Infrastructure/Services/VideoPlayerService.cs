//using System;
//using LibVLCSharp.Shared;
//using MediaFusionPlayer.Core.Interfaces;
//using System.Windows;
//using System.Windows.Interop;

//namespace MediaFusionPlayer.Infrastructure.Services
//{
//    public class VideoPlayerService : IVideoPlayerService
//    {
//        private LibVLC? _libVLC;
//        private MediaPlayer? _mediaPlayer;
//        private IntPtr _windowHandle;
//        private bool _isInitialized = false;
//        private bool _isDisposed = false;

//        public bool IsVideoPlaying => _mediaPlayer?.IsPlaying ?? false;

//        public event EventHandler<bool>? VideoStateChanged;

//        public VideoPlayerService()
//        {
//            try
//            {
//                Core.Initialize();
//                _libVLC = new LibVLC("--no-video-title-show");
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"Ошибка инициализации LibVLC: {ex.Message}", "Ошибка",
//                    MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }

//        public void InitializeVideo(string videoFilePath, IntPtr windowHandle)
//        {
//            if (_isDisposed || _libVLC == null) return;

//            _windowHandle = windowHandle;

//            // Очищаем предыдущий медиаплеер
//            _mediaPlayer?.Stop();
//            _mediaPlayer?.Dispose();

//            // Создаем новый медиаплеер
//            _mediaPlayer = new MediaPlayer(_libVLC);
//            _mediaPlayer.SetHWND(_windowHandle);

//            // Создаем медиа из файла
//            using var media = new Media(_libVLC, videoFilePath, FromType.FromPath);
//            _mediaPlayer.Media = media;

//            _isInitialized = true;

//            // Подписываемся на события
//            _mediaPlayer.Playing += (s, e) => OnVideoStateChanged(true);
//            _mediaPlayer.Paused += (s, e) => OnVideoStateChanged(false);
//            _mediaPlayer.Stopped += (s, e) => OnVideoStateChanged(false);
//            _mediaPlayer.EndReached += (s, e) => OnVideoStateChanged(false);
//        }

//        public void PlayVideo()
//        {
//            if (!_isInitialized || _mediaPlayer == null) return;

//            if (_mediaPlayer.IsPlaying)
//                return;

//            _mediaPlayer.Play();
//        }

//        public void PauseVideo()
//        {
//            if (!_isInitialized || _mediaPlayer == null) return;

//            if (_mediaPlayer.IsPlaying)
//                _mediaPlayer.Pause();
//        }

//        public void StopVideo()
//        {
//            if (!_isInitialized || _mediaPlayer == null) return;

//            _mediaPlayer.Stop();
//            _mediaPlayer.Media?.Dispose();
//            _mediaPlayer.Media = null;
//            _isInitialized = false;
//        }

//        public void SeekVideo(TimeSpan position)
//        {
//            if (!_isInitialized || _mediaPlayer?.Media == null) return;

//            _mediaPlayer.SeekTo(position);
//        }

//        public void SetVideoVolume(float volume)
//        {
//            if (!_isInitialized || _mediaPlayer == null) return;

//            _mediaPlayer.Volume = (int)(volume * 100);
//        }

//        public TimeSpan GetVideoDuration()
//        {
//            if (!_isInitialized || _mediaPlayer?.Media == null)
//                return TimeSpan.Zero;

//            var duration = _mediaPlayer.Media.Duration;
//            return duration > 0 ? TimeSpan.FromMilliseconds(duration) : TimeSpan.Zero;
//        }

//        public TimeSpan GetVideoPosition()
//        {
//            if (!_isInitialized || _mediaPlayer?.Media == null)
//                return TimeSpan.Zero;

//            var position = _mediaPlayer.Position;
//            var duration = GetVideoDuration();

//            return duration.TotalMilliseconds > 0
//                ? TimeSpan.FromMilliseconds(position * duration.TotalMilliseconds)
//                : TimeSpan.Zero;
//        }

//        private void OnVideoStateChanged(bool isPlaying)
//        {
//            Application.Current.Dispatcher.BeginInvoke(() =>
//            {
//                VideoStateChanged?.Invoke(this, isPlaying);
//            });
//        }

//        public void Dispose()
//        {
//            if (_isDisposed) return;
//            _isDisposed = true;

//            StopVideo();

//            _mediaPlayer?.Dispose();
//            _mediaPlayer = null;

//            _libVLC?.Dispose();
//            _libVLC = null;
//        }
//    }
//}