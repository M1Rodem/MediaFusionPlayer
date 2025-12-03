using MediaFusionPlayer.Core.Models;
using System;

namespace MediaFusionPlayer.Core.Interfaces
{
    public enum MediaPlaybackState
    {
        Stopped,
        Playing,
        Paused
    }

    public interface IMediaPlayerService : IDisposable
    {
        MediaPlaybackState State { get; }
        float Volume { get; set; }
        TimeSpan Position { get; }
        TimeSpan Duration { get; }
        bool IsSeeking { get; }

        void Play(PlaylistItem track);
        void Pause();
        void Stop();
        void Seek(TimeSpan position);
        void BeginSeek();
        void EndSeek(TimeSpan position);

        event EventHandler<MediaPlaybackState> PlaybackStateChanged;
        event EventHandler<TimeSpan> PositionChanged;
        event EventHandler<PlaylistItem> TrackFinished;
        event EventHandler<bool> IsSeekingChanged;
    }
}