using MediaFusionPlayer.Core.Interfaces;
using MediaFusionPlayer.Core.Models;
using MediaFusionPlayer.Presentation.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MediaFusionPlayer.Presentation.ViewModels
{
    public sealed class MainViewModel : ViewModelBase
    {
        private readonly IFileService _fileService;
        private readonly IPlaylistService _playlistService;
        private readonly IMediaPlayerService _mediaPlayer;
        private readonly IVideoPlayerService _videoService; // НОВОЕ

        public ObservableCollection<PlaylistItem> Playlist => _playlistService.Items;

        private PlaylistItem? _currentTrack;
        public PlaylistItem? CurrentTrack
        {
            get => _currentTrack;
            set => SetProperty(ref _currentTrack, value);
        }

        private TimeSpan _currentPosition;
        public TimeSpan CurrentPosition
        {
            get => _currentPosition;
            set => SetProperty(ref _currentPosition, value);
        }

        public TimeSpan CurrentDuration => CurrentTrack?.Duration ?? TimeSpan.Zero;

        private float _volume = 0.7f;
        public float Volume
        {
            get => _volume;
            set
            {
                if (SetProperty(ref _volume, value))
                    _mediaPlayer.Volume = value;
            }
        }

        // НОВОЕ: свойства для видео
        private bool _isVideo;
        public bool IsVideo
        {
            get => _isVideo;
            private set => SetProperty(ref _isVideo, value);
        }

        private string? _videoPath;
        public string? VideoPath
        {
            get => _videoPath;
            private set => SetProperty(ref _videoPath, value);
        }

        public IVideoPlayerService VideoService => _videoService; // Для привязки в XAML

        public ICommand AddFilesCommand { get; }
        public ICommand PlayPauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand PreviousCommand { get; }

        public MainViewModel(
            IFileService fileService,
            IPlaylistService playlistService,
            IMediaPlayerService mediaPlayer,
            IVideoPlayerService videoService) // НОВЫЙ параметр
        {
            _fileService = fileService;
            _playlistService = playlistService;
            _mediaPlayer = mediaPlayer;
            _videoService = videoService;

            AddFilesCommand = new RelayCommand(_ => AddFiles());
            PlayPauseCommand = new RelayCommand(_ => PlayPause(), _ => CurrentTrack != null);
            StopCommand = new RelayCommand(_ => Stop(), _ => _mediaPlayer.State != MediaPlaybackState.Stopped);
            NextCommand = new RelayCommand(_ => _playlistService.MoveToNext());
            PreviousCommand = new RelayCommand(_ => _playlistService.MoveToPrevious());

            // Подписки
            _playlistService.CurrentTrackChanged += OnPlaylistTrackChanged;
            _mediaPlayer.PlaybackStateChanged += (s, e) => CommandManager.InvalidateRequerySuggested();
            _mediaPlayer.PositionChanged += OnPositionChanged;
            _mediaPlayer.TrackFinished += (s, e) => _playlistService.MoveToNext();
            _mediaPlayer.IsSeekingChanged += (s, seeking) => { };

            // НОВОЕ: подписка на изменение типа медиа
            _mediaPlayer.IsVideoChanged += OnIsVideoChanged;
        }

        private void OnIsVideoChanged(object? sender, bool isVideo)
        {
            IsVideo = isVideo;

            if (isVideo && !string.IsNullOrEmpty(_mediaPlayer.VideoPath))
            {
                VideoPath = _mediaPlayer.VideoPath;
                // Запускаем видео через VideoService
                _videoService.PlayVideo(_mediaPlayer.VideoPath);

                // Синхронизируем позицию
                _videoService.SeekVideo(_mediaPlayer.Position);
            }
            else
            {
                VideoPath = null;
                _videoService.StopVideo();
            }
        }

        private void OnPlaylistTrackChanged(PlaylistItem? track)
        {
            CurrentTrack = track;

            // Сбрасываем позицию ДО Play() — это важно!
            CurrentPosition = TimeSpan.Zero;
            OnPropertyChanged(nameof(CurrentDuration));

            if (track != null)
                _mediaPlayer.Play(track);
        }

        private void OnPositionChanged(object? sender, TimeSpan position)
        {
            // Защита от рекурсии при dragging + потокобезопасность
            if (_mediaPlayer.IsSeeking)
                return;

            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    CurrentPosition = position));
            }
            else
            {
                CurrentPosition = position;
            }

            // Синхронизируем видео если оно играет
            if (IsVideo && _videoService.IsPlaying)
            {
                _videoService.SeekVideo(position);
            }
        }

        private void PlayPause()
        {
            if (_mediaPlayer.State == MediaPlaybackState.Playing)
            {
                _mediaPlayer.Pause();
                if (IsVideo) _videoService.PauseVideo();
            }
            else if (CurrentTrack != null)
            {
                _mediaPlayer.Play(CurrentTrack);
            }
        }

        private void Stop()
        {
            _mediaPlayer.Stop();
            _videoService.StopVideo();
            CurrentPosition = TimeSpan.Zero;
            IsVideo = false;
            VideoPath = null;
        }

        private async void AddFiles()
        {
            var files = await _fileService.OpenFileDialogAsync();
            if (files.Length > 0)
                await _playlistService.AddFilesWithMetadataAsync(files);
        }

        public async Task AddFilesFromDropAsync(IEnumerable<string> files)
        {
            var valid = files.Where(File.Exists);
            if (valid.Any())
                await _playlistService.AddFilesWithMetadataAsync(valid);
        }

        public void SeekTo(TimeSpan position)
        {
            if (CurrentTrack == null) return;

            _mediaPlayer.BeginSeek();
            _mediaPlayer.Seek(position);
            _mediaPlayer.EndSeek(position);

            // Синхронизируем видео
            if (IsVideo)
            {
                _videoService.SeekVideo(position);
            }
        }
    }
}