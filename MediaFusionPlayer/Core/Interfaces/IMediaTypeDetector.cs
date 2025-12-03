using System;

namespace MediaFusionPlayer.Core.Interfaces
{
    public enum MediaType
    {
        Audio,
        Video,
        Unknown
    }

    public interface IMediaTypeDetector
    {
        MediaType DetectMediaType(string filePath);
        bool IsAudioFile(string filePath);
        bool IsVideoFile(string filePath);
    }
}