using MediaFusionPlayer.Core.Interfaces;
using MediaFusionPlayer.Core.Models;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TagLib;

namespace MediaFusionPlayer.Infrastructure.FileServices
{
    public class FileService : IFileService
    {
        private readonly string[] _supportedExtensions =
            { ".mp3", ".wav", ".flac", ".ogg", ".m4a", ".mp4", ".avi", ".mkv", ".webm", ".wma" };

        public Task<string[]> OpenFileDialogAsync(bool multiselect = true)
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = multiselect,
                Filter = "Аудио и видео|*.mp3;*.wav;*.flac;*.ogg;*.m4a;*.mp4;*.avi;*.mkv;*.webm;*.wma|Все файлы|*.*"
            };

            return Task.FromResult(dialog.ShowDialog() == true ? dialog.FileNames : Array.Empty<string>());
        }

        public async Task<PlaylistItem> LoadMetadataAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                var item = new PlaylistItem { FilePath = filePath };

                try
                {
                    using var taglibFile = TagLib.File.Create(filePath);
                    item.Duration = taglibFile.Properties.Duration;
                    var tag = taglibFile.Tag;

                    string artist = string.IsNullOrWhiteSpace(tag.JoinedPerformers) ? "" : tag.JoinedPerformers.Trim();
                    string title = string.IsNullOrWhiteSpace(tag.Title) ? "" : tag.Title.Trim();

                    if (!string.IsNullOrWhiteSpace(artist) && !string.IsNullOrWhiteSpace(title))
                    {
                        item.DisplayName = $"{artist} - {title}";
                    }
                    else if (!string.IsNullOrWhiteSpace(title))
                    {
                        item.DisplayName = title;
                    }
                    else
                    {
                        item.DisplayName = Path.GetFileNameWithoutExtension(filePath);
                    }

                    item.Artist = artist;
                    item.Album = tag.Album ?? "";

                    if (tag.Pictures.Length > 0)
                    {
                        var data = tag.Pictures[0].Data.Data;
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = new MemoryStream(data);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        item.CoverArt = bitmap;
                    }
                }
                catch
                {
                    // Если теги не читаются — просто имя файла
                    item.DisplayName = Path.GetFileNameWithoutExtension(filePath);
                }

                return item;
            });
        }

        public bool IsSupportedFile(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            return Array.Exists(_supportedExtensions, e => e == ext);
        }
    }
}