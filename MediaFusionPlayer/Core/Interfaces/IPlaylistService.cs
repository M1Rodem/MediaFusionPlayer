using MediaFusionPlayer.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MediaFusionPlayer.Core.Interfaces
{
    public interface IPlaylistService
    {
        ObservableCollection<PlaylistItem> Items { get; }

        PlaylistItem? CurrentTrack { get; set; }
        int CurrentIndex { get; }

        void AddFiles(IEnumerable<string> filePaths);
        Task AddFilesWithMetadataAsync(IEnumerable<string> filePaths);

        void Remove(PlaylistItem item);
        void Clear();

        void MoveToNext();
        void MoveToPrevious();
        void SetCurrentTrack(PlaylistItem? track);

        event Action<PlaylistItem?>? CurrentTrackChanged;
    }
}