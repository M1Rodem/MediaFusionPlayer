using MediaFusionPlayer.Core.Interfaces;
using MediaFusionPlayer.Core.Models;
using NAudio.Wave;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace MediaFusionPlayer.Infrastructure.Services
{
    public sealed class MediaPlayerService : IMediaPlayerService, IDisposable
    {
        private IWavePlayer? _wavePlayer;
        private AudioFileReader? _audioFile;
        private DispatcherTimer? _positionTimer;
        private readonly object _syncLock = new();
        private bool _isDisposed = false;
        private bool _isSeeking = false;
        private bool _trackFinished = false;

        private MediaPlaybackState _state = MediaPlaybackState.Stopped;
        public MediaPlaybackState State
        {
            get => _state;
            private set
            {
                if (_state != value)
                {
                    _state = value;
                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        PlaybackStateChanged?.Invoke(this, _state);
                    });
                }
            }
        }

        public bool IsSeeking
        {
            get => _isSeeking;
            private set
            {
                if (_isSeeking != value)
                {
                    _isSeeking = value;
                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        IsSeekingChanged?.Invoke(this, value);
                    });
                }
            }
        }

        private float _volume = 0.7f;
        public float Volume
        {
            get => _volume;
            set
            {
                _volume = Math.Clamp(value, 0f, 1f);
                if (_wavePlayer != null) _wavePlayer.Volume = _volume;
            }
        }

        public TimeSpan Position => _audioFile?.CurrentTime ?? TimeSpan.Zero;
        public TimeSpan Duration => _audioFile?.TotalTime ?? TimeSpan.Zero;

        public event EventHandler<MediaPlaybackState>? PlaybackStateChanged;
        public event EventHandler<TimeSpan>? PositionChanged;
        public event EventHandler<PlaylistItem>? TrackFinished;
        public event EventHandler<bool>? IsSeekingChanged;

        private string? _currentFilePath;
        private TimeSpan _pausedPosition = TimeSpan.Zero;
        private PlaylistItem? _currentTrack;

        public MediaPlayerService()
        {
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            _positionTimer = new DispatcherTimer(DispatcherPriority.Render, Application.Current.Dispatcher)
            {
                Interval = TimeSpan.FromMilliseconds(30)
            };
            _positionTimer.Tick += PositionTimer_Tick;
        }

        private void StartTimer()
        {
            if (_positionTimer != null && !_positionTimer.IsEnabled)
            {
                _positionTimer.Start();
            }
        }

        private void StopTimer()
        {
            if (_positionTimer != null && _positionTimer.IsEnabled)
            {
                _positionTimer.Stop();
            }
        }

        private void PositionTimer_Tick(object? sender, EventArgs e)
        {
            if (_isDisposed || _audioFile == null || State != MediaPlaybackState.Playing || IsSeeking)
                return;

            var pos = _audioFile.CurrentTime;
            
            // Проверяем завершение трека
            if (!_trackFinished && pos.TotalSeconds >= _audioFile.TotalTime.TotalSeconds - 0.1)
            {
                _trackFinished = true;
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    TrackFinished?.Invoke(this, _currentTrack!);
                });
                return;
            }

            PositionChanged?.Invoke(this, pos);
        }

        public void Play(PlaylistItem track)
        {
            if (track == null || _isDisposed) return;

            bool isResumingSameTrack = State == MediaPlaybackState.Paused &&
                                       _currentFilePath == track.FilePath &&
                                       _audioFile != null;

            if (!isResumingSameTrack)
            {
                // Останавливаем и чистим старое
                InternalStop();
                DisposeCurrent();
            }

            try
            {
                if (!isResumingSameTrack)
                {
                    // Создаём новый AudioFileReader для нового трека
                    _audioFile = new AudioFileReader(track.FilePath);
                    _currentFilePath = track.FilePath;
                    _currentTrack = track;
                    _trackFinished = false;
                }
                else if (_audioFile != null)
                {
                    // Возобновляем с прежней позиции
                    _audioFile.CurrentTime = _pausedPosition;
                }

                if (_wavePlayer == null)
                {
                    _wavePlayer = new WaveOutEvent();
                    _wavePlayer.Init(_audioFile);
                    _wavePlayer.Volume = Volume;
                    _wavePlayer.PlaybackStopped += OnPlaybackStopped;
                }

                _wavePlayer.Play();
                State = MediaPlaybackState.Playing;
                _pausedPosition = TimeSpan.Zero;

                // Сразу обновляем UI
                var currentPos = _audioFile?.CurrentTime ?? TimeSpan.Zero;
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    PositionChanged?.Invoke(this, currentPos);
                });

                // Запускаем таймер
                StartTimer();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка воспроизведения: {ex.Message}");
                State = MediaPlaybackState.Stopped;
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    TrackFinished?.Invoke(this, track);
                });
            }
        }

        public void Pause()
        {
            if (State != MediaPlaybackState.Playing || _wavePlayer == null || _audioFile == null) 
                return;

            // Запоминаем позицию перед паузой
            _pausedPosition = _audioFile.CurrentTime;

            _wavePlayer.Pause();
            StopTimer();
            State = MediaPlaybackState.Paused;

            // Фиксируем последнюю позицию
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                PositionChanged?.Invoke(this, _pausedPosition);
            });
        }

        public void Stop()
        {
            InternalStop();
            DisposeCurrent();
            _pausedPosition = TimeSpan.Zero;
            _currentFilePath = null;
            _currentTrack = null;
            _trackFinished = false;
            State = MediaPlaybackState.Stopped;

            Application.Current.Dispatcher.BeginInvoke(() =>
                PositionChanged?.Invoke(this, TimeSpan.Zero));
        }

        private void InternalStop()
        {
            _wavePlayer?.Stop();
            StopTimer();
        }

        public void Seek(TimeSpan position)
        {
            if (_audioFile == null) return;

            var clamped = TimeSpan.FromTicks(Math.Clamp(position.Ticks, 0L, _audioFile.TotalTime.Ticks));
            _audioFile.CurrentTime = clamped;

            Application.Current.Dispatcher.BeginInvoke(() =>
                PositionChanged?.Invoke(this, clamped));
        }

        public void BeginSeek() => IsSeeking = true;
        
        public void EndSeek(TimeSpan position)
        {
            Seek(position);
            IsSeeking = false;
        }

        private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            if (_isDisposed || _audioFile == null) return;

            // Не отправляем TrackFinished, если это был ручной стоп или пауза
            if (State == MediaPlaybackState.Stopped)
                return;

            // Проверяем, действительно ли трек завершился
            if (_audioFile.CurrentTime >= _audioFile.TotalTime - TimeSpan.FromSeconds(0.5))
            {
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    State = MediaPlaybackState.Stopped;
                    PositionChanged?.Invoke(this, TimeSpan.Zero);
                    TrackFinished?.Invoke(this, _currentTrack!);
                });
            }
        }

        private void DisposeCurrent()
        {
            StopTimer();

            if (_wavePlayer != null)
            {
                _wavePlayer.PlaybackStopped -= OnPlaybackStopped;
                _wavePlayer.Dispose();
                _wavePlayer = null;
            }
            
            _audioFile?.Dispose();
            _audioFile = null;
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            lock (_syncLock)
            {
                Stop();
                DisposeCurrent();
                
                if (_positionTimer != null)
                {
                    _positionTimer.Tick -= PositionTimer_Tick;
                    _positionTimer.Stop();
                    _positionTimer = null;
                }
            }
        }
    }
}