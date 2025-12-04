using MediaFusionPlayer.Core.Interfaces;
using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaFusionPlayer.Infrastructure.Services
{
    public sealed class EqualizerService : IEqualizerService
    {
        private readonly List<EqualizerBand> _bands = new();
        private readonly Dictionary<string, float[]> _presets = new()
        {
            { "Flat", new[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "Pop", new[] { 2f, 1f, 0f, -1f, 0f, 2f, 3f, 3f, 2f, 2f } },
            { "Rock", new[] { 4f, 2f, 1f, 0f, 1f, 3f, 4f, 4f, 3f, 2f } },
            { "Jazz", new[] { 2f, 1f, 0f, 0f, 0f, -1f, 1f, 2f, 2f, 3f } },
            { "Classical", new[] { 3f, 2f, 1f, 0f, -1f, -1f, 0f, 1f, 2f, 3f } },
            { "Bass Boost", new[] { 6f, 5f, 3f, 1f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "Treble Boost", new[] { 0f, 0f, 0f, 0f, 0f, 1f, 2f, 3f, 4f, 5f } },
            { "Electronic", new[] { 3f, 2f, 1f, 0f, 2f, 3f, 4f, 4f, 3f, 2f } },
            { "Vocal Boost", new[] { -1f, 0f, 1f, 2f, 3f, 3f, 2f, 1f, 0f, -1f } }
        };

        public IReadOnlyList<EqualizerBand> Bands => _bands.AsReadOnly();

        // ДОБАВЛЯЕМ новые свойства:
        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    if (!value)
                    {
                        // При отключении сбрасываем на плоский профиль
                        ResetToFlat();
                    }
                    OnBandsChanged();
                }
            }
        }

        private bool _isVideoPlaying = false;
        public bool IsVideoPlaying
        {
            get => _isVideoPlaying;
            set
            {
                if (_isVideoPlaying != value)
                {
                    _isVideoPlaying = value;
                    // При воспроизведении видео автоматически отключаем эквалайзер
                    if (value)
                    {
                        IsEnabled = false;
                    }
                    OnBandsChanged();
                }
            }
        }

        public event EventHandler? BandsChanged;

        public EqualizerService()
        {
            // Стандартные 10-полосные частоты (ISO 31-band)
            float[] frequencies = { 32f, 64f, 125f, 250f, 500f, 1000f, 2000f, 4000f, 8000f, 16000f };

            foreach (var freq in frequencies)
            {
                _bands.Add(new EqualizerBand
                {
                    Frequency = freq,
                    Gain = 0f,
                    Bandwidth = 1.0f
                });
            }
        }

        public void ApplyPreset(string name)
        {
            if (!_presets.ContainsKey(name))
                throw new ArgumentException($"Пресет '{name}' не найден.");

            // Если видео играет - не применяем эквалайзер
            if (_isVideoPlaying) return;

            var gains = _presets[name];
            for (int i = 0; i < Math.Min(gains.Length, _bands.Count); i++)
            {
                _bands[i].Gain = gains[i];
            }

            OnBandsChanged();
        }

        public void SetBandGain(int index, float gainDb)
        {
            if (index < 0 || index >= _bands.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            // Если видео играет - не изменяем эквалайзер
            if (_isVideoPlaying) return;

            _bands[index].Gain = Math.Clamp(gainDb, -20f, 20f);
            OnBandsChanged();
        }

        public void ResetToFlat()
        {
            foreach (var band in _bands)
            {
                band.Gain = 0f;
            }
            OnBandsChanged();
        }

        public IEnumerable<BiQuadFilter> CreateFilters(int sampleRate)
        {
            // Если эквалайзер выключен или играет видео - возвращаем пустой список
            if (!_isEnabled || _isVideoPlaying)
                return Enumerable.Empty<BiQuadFilter>();

            var filters = new List<BiQuadFilter>();

            foreach (var band in _bands)
            {
                if (Math.Abs(band.Gain) < 0.01f) // Пропускаем нейтральные полосы
                    continue;

                // Создаем пиковый фильтр (Peak EQ)
                var filter = BiQuadFilter.PeakingEQ(
                    sampleRate,
                    band.Frequency,
                    band.Bandwidth,
                    band.Gain);

                filters.Add(filter);
            }

            return filters;
        }

        private void OnBandsChanged()
        {
            BandsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            // Ничего не нужно освобождать
        }
    }
}