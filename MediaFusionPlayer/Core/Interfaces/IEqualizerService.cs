using System;
using System.Collections.Generic;

namespace MediaFusionPlayer.Core.Interfaces
{
    public class EqualizerBand
    {
        public float Frequency { get; set; }
        public float Gain { get; set; } // от -20 до +20 дБ
        public float Bandwidth { get; set; } = 1.0f; // октавы
    }

    public interface IEqualizerService : IDisposable
    {
        IReadOnlyList<EqualizerBand> Bands { get; }
        void ApplyPreset(string name);
        void SetBandGain(int index, float gainDb);
        void ResetToFlat();
        event EventHandler? BandsChanged;
    }
}