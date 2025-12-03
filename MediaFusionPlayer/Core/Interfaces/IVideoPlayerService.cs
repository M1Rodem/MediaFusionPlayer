using System;

namespace MediaFusionPlayer.Core.Interfaces
{
    public interface IVideoPlayerService : IDisposable
    {
        bool IsVideoPlaying { get; }
        event EventHandler<bool> VideoStateChanged;

        void InitializeVideo(string videoFilePath, IntPtr windowHandle);
        void PlayVideo();
        void PauseVideo();
        void StopVideo();
        void SeekVideo(TimeSpan position);
        void SetVideoVolume(float volume); // 0-1
        TimeSpan GetVideoDuration();
        TimeSpan GetVideoPosition();
    }
}