using MediaFusionPlayer.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaFusionPlayer.Core.Interfaces
{
    public interface IFileService
    {
        /// <summary>
        /// Открывает диалог выбора файлов и возвращает пути
        /// </summary>
        Task<string[]> OpenFileDialogAsync(bool multiselect = true);

        /// <summary>
        /// Извлекает метаданные и обложку из файла
        /// </summary>
        Task<PlaylistItem> LoadMetadataAsync(string filePath);

        /// <summary>
        /// Проверяет, поддерживается ли файл (аудио/видео)
        /// </summary>
        bool IsSupportedFile(string filePath);
    }
}