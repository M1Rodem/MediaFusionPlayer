using LibVLCSharp.Shared;
using System;

namespace MediaFusionPlayer.Core.Interfaces
{
    public interface IVideoPlayerService : IDisposable
    {
        bool IsPlaying { get; }
        bool IsInitialized { get; }
        string? CurrentVideoPath { get; }
        LibVLC? LibVLC { get; }

        void Initialize();
        void PlayVideo(string videoPath);
        void PauseVideo();
        void StopVideo();
        void SeekVideo(TimeSpan position);
        void SetVideoOutput(IntPtr handle);

        event EventHandler<Exception>? VideoError;
    }
}