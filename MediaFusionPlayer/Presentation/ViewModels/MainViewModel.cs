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

        public ICommand AddFilesCommand { get; }
        public ICommand PlayPauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand PreviousCommand { get; }

        public MainViewModel(
            IFileService fileService,
            IPlaylistService playlistService,
            IMediaPlayerService mediaPlayer)
        {
            _fileService = fileService;
            _playlistService = playlistService;
            _mediaPlayer = mediaPlayer;

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
        }

        private void PlayPause()
        {
            if (_mediaPlayer.State == MediaPlaybackState.Playing)
                _mediaPlayer.Pause();
            else if (CurrentTrack != null)
                _mediaPlayer.Play(CurrentTrack);
        }

        private void Stop()
        {
            _mediaPlayer.Stop();
            CurrentPosition = TimeSpan.Zero;
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
        }
    }
}