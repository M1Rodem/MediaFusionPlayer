using MediaFusionPlayer.Core.Interfaces;
using MediaFusionPlayer.Core.Models;
using MediaFusionPlayer.Infrastructure.FileServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MediaFusionPlayer.Infrastructure.Services
{
    public class PlaylistService : IPlaylistService
    {
        public ObservableCollection<PlaylistItem> Items { get; } = new();

        public PlaylistItem? CurrentTrack { get; set; }
        public int CurrentIndex { get; private set; } = -1;

        public event Action<PlaylistItem?>? CurrentTrackChanged;

        public async Task AddFilesWithMetadataAsync(IEnumerable<string> filePaths)
        {
            var fileService = new FileService();
            var validPaths = filePaths.Where(p => fileService.IsSupportedFile(p));

            var tasks = validPaths.Select(p => fileService.LoadMetadataAsync(p));
            var items = await Task.WhenAll(tasks);

            foreach (var item in items)
            {
                Items.Add(item);
            }

            // Если плейлист был пустой — сразу выбираем первый трек
            if (CurrentTrack == null && Items.Any())
            {
                SetCurrentTrack(Items[0]);
            }
        }

        public void AddFiles(IEnumerable<string> filePaths) =>
            Task.Run(() => AddFilesWithMetadataAsync(filePaths));

        public void Remove(PlaylistItem item) => Items.Remove(item);

        public void Clear() => Items.Clear();

        public void MoveToNext()
        {
            if (Items.Count == 0) return;
            var next = (CurrentIndex + 1) % Items.Count;
            SetCurrentTrack(Items[next]);
        }

        public void MoveToPrevious()
        {
            if (Items.Count == 0) return;
            var prev = CurrentIndex <= 0 ? Items.Count - 1 : CurrentIndex - 1;
            SetCurrentTrack(Items[prev]);
        }

        public void SetCurrentTrack(PlaylistItem? track)
        {
            if (CurrentTrack == track) return;

            CurrentTrack = track;
            CurrentIndex = track != null ? Items.IndexOf(track) : -1;
            CurrentTrackChanged?.Invoke(CurrentTrack);
        }
    }
}