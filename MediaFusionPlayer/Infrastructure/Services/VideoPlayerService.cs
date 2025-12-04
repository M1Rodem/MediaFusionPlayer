using LibVLCSharp.Shared;
using MediaFusionPlayer.Core.Interfaces;
using System;
using System.IO;

namespace MediaFusionPlayer.Infrastructure.Services
{
    public sealed class VideoPlayerService : IVideoPlayerService
    {
        private LibVLC? _libVlc;
        public MediaPlayer? MediaPlayer { get; private set; }

        public bool IsPlaying => MediaPlayer?.IsPlaying ?? false;
        public bool IsInitialized => _libVlc != null;
        public string? CurrentVideoPath { get; private set; }
        public LibVLC? LibVLC => _libVlc;

        public event EventHandler<Exception>? VideoError;

        public VideoPlayerService()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (IsInitialized) return;

            try
            {
                LibVLCSharp.Shared.Core.Initialize(); // ПРАВИЛЬНО!
                _libVlc = new LibVLC("--no-audio", "--avcodec-hw=dxva2", "--intf=dummy", "--no-osd");
                MediaPlayer = new MediaPlayer(_libVlc!)
                {
                    EnableHardwareDecoding = true,
                    EnableMouseInput = false,
                    EnableKeyInput = false
                };
            }
            catch (Exception ex)
            {
                VideoError?.Invoke(this, ex);
            }
        }

        public void PlayVideo(string videoPath)
        {
            if (!File.Exists(videoPath) || MediaPlayer == null || _libVlc == null) return;

            try
            {
                StopVideo();
                CurrentVideoPath = videoPath;

                using var media = new Media(_libVlc, new Uri(videoPath));
                media.AddOption(":no-audio");
                MediaPlayer.Play(media);
            }
            catch (Exception ex)
            {
                VideoError?.Invoke(this, ex);
            }
        }

        public void PauseVideo() => MediaPlayer?.Pause();
        public void StopVideo() => MediaPlayer?.Stop();

        public void SeekVideo(TimeSpan position)
        {
            if (MediaPlayer != null && MediaPlayer.IsSeekable)
                MediaPlayer.Time = (long)position.TotalMilliseconds;
        }

        public void SetVideoOutput(IntPtr handle) { } // не нужен

        public void Dispose()
        {
            MediaPlayer?.Stop();
            MediaPlayer?.Dispose();
            _libVlc?.Dispose();
        }
    }
}