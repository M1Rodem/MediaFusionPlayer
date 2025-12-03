using MediaFusionPlayer.Core.Interfaces;
using System;
using System.Windows.Threading;

namespace MediaFusionPlayer.Infrastructure.Services
{
    public sealed class VideoSyncService : IDisposable
    {
        private readonly IMediaPlayerService _audioPlayer;
        private readonly IVideoPlayerService _videoPlayer;
        private DispatcherTimer? _syncTimer;
        private bool _isVideoPlaying = false;
        private TimeSpan _lastSyncTime = TimeSpan.Zero;

        public VideoSyncService(IMediaPlayerService audioPlayer, IVideoPlayerService videoPlayer)
        {
            _audioPlayer = audioPlayer;
            _videoPlayer = videoPlayer;

            Initialize();
        }

        private void Initialize()
        {
            // Подписываемся на события
            _audioPlayer.PlaybackStateChanged += OnAudioPlaybackStateChanged;
            _audioPlayer.PositionChanged += OnAudioPositionChanged;
            _audioPlayer.IsVideoChanged += OnIsVideoChanged;

            // Таймер для синхронизации
            _syncTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
            };
            _syncTimer.Tick += OnSyncTimerTick;
        }

        private void OnIsVideoChanged(object? sender, bool isVideo)
        {
            _isVideoPlaying = isVideo;

            if (isVideo && _audioPlayer.VideoPath != null)
            {
                _videoPlayer.PlayVideo(_audioPlayer.VideoPath);
                _syncTimer?.Start();
            }
            else
            {
                _syncTimer?.Stop();
                _videoPlayer.StopVideo();
                _lastSyncTime = TimeSpan.Zero;
            }
        }

        private void OnAudioPlaybackStateChanged(object? sender, MediaPlaybackState state)
        {
            if (!_isVideoPlaying) return;

            switch (state)
            {
                case MediaPlaybackState.Playing:
                    if (_audioPlayer.VideoPath != null)
                        _videoPlayer.PlayVideo(_audioPlayer.VideoPath);
                    break;
                case MediaPlaybackState.Paused:
                    _videoPlayer.PauseVideo();
                    break;
                case MediaPlaybackState.Stopped:
                    _videoPlayer.StopVideo();
                    break;
            }
        }

        private void OnAudioPositionChanged(object? sender, TimeSpan position)
        {
            if (!_isVideoPlaying || _audioPlayer.IsSeeking) return;

            _lastSyncTime = position;
            _videoPlayer.SeekVideo(position);
        }

        private void OnSyncTimerTick(object? sender, EventArgs e)
        {
            if (!_isVideoPlaying || _audioPlayer.State != MediaPlaybackState.Playing)
                return;

            var audioPos = _audioPlayer.Position;

            // Если рассинхронизация больше 50мс - корректируем
            if (Math.Abs((audioPos - _lastSyncTime).TotalMilliseconds) > 50)
            {
                _videoPlayer.SeekVideo(audioPos);
                _lastSyncTime = audioPos;
            }
        }

        public void Dispose()
        {
            _syncTimer?.Stop();
            _syncTimer = null;

            _audioPlayer.PlaybackStateChanged -= OnAudioPlaybackStateChanged;
            _audioPlayer.PositionChanged -= OnAudioPositionChanged;
            _audioPlayer.IsVideoChanged -= OnIsVideoChanged;
        }
    }
}