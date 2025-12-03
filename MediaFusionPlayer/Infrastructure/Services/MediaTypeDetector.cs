using System.IO;
using MediaFusionPlayer.Core.Interfaces;

namespace MediaFusionPlayer.Infrastructure.Services
{
    public class MediaTypeDetector : IMediaTypeDetector
    {
        private static readonly string[] AudioExtensions =
        {
            ".mp3", ".wav", ".flac", ".aac", ".ogg", ".m4a", ".wma",
            ".opus", ".amr", ".aiff", ".mid", ".midi"
        };

        private static readonly string[] VideoExtensions =
        {
            ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".webm",
            ".mpeg", ".mpg", ".m4v", ".3gp", ".ts", ".m2ts", ".divx",
            ".xvid", ".rm", ".rmvb", ".asf", ".vob"
        };

        public MediaType DetectMediaType(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return MediaType.Unknown;

            var extension = Path.GetExtension(filePath)?.ToLowerInvariant() ?? string.Empty;

            if (AudioExtensions.Contains(extension))
                return MediaType.Audio;

            if (VideoExtensions.Contains(extension))
                return MediaType.Video;

            return MediaType.Unknown;
        }

        public bool IsAudioFile(string filePath)
            => DetectMediaType(filePath) == MediaType.Audio;

        public bool IsVideoFile(string filePath)
            => DetectMediaType(filePath) == MediaType.Video;
    }
}