using System;
using System.Windows.Media;

namespace MediaFusionPlayer.Core.Models
{
    public class PlaylistItem
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName => System.IO.Path.GetFileName(FilePath);

        private string _displayName = string.Empty;
        public string DisplayName
        {
            get => string.IsNullOrEmpty(_displayName) ? FileName : _displayName;
            set => _displayName = value;
        }

        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = "Неизвестный исполнитель";
        public string Album { get; set; } = string.Empty;

        public TimeSpan Duration { get; set; }
        public ImageSource? CoverArt { get; set; }


        public override string ToString() => $"{DisplayName} — {Duration:mm\\:ss}";
    }
}