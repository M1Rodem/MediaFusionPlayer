using MediaFusionPlayer.Core.Interfaces;
using MediaFusionPlayer.Presentation.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace MediaFusionPlayer.Presentation.ViewModels
{
    public sealed class EqualizerViewModel : ViewModelBase
    {
        private readonly IEqualizerService _equalizerService;
        private bool _isEnabled = true;
        private string _selectedPreset = "Flat";

        public bool IsEnabled
        {
            get => _equalizerService.IsEnabled;
            set
            {
                if (_equalizerService.IsEnabled != value)
                {
                    _equalizerService.IsEnabled = value;
                    OnPropertyChanged(nameof(IsEnabled));

                    if (!value)
                    {
                        _equalizerService.ResetToFlat();
                    }
                }
            }
        }

        public string SelectedPreset
        {
            get => _selectedPreset;
            set
            {
                if (SetProperty(ref _selectedPreset, value))
                {
                    try
                    {
                        _equalizerService.ApplyPreset(value);
                    }
                    catch (ArgumentException)
                    {
                        // Если пресет не найден, возвращаем предыдущее значение
                        SetProperty(ref _selectedPreset, "Flat", nameof(SelectedPreset));
                    }
                }
            }
        }

        public ObservableCollection<string> Presets { get; } = new()
        {
            "Flat", "Pop", "Rock", "Jazz", "Classical",
            "Bass Boost", "Treble Boost", "Electronic", "Vocal Boost"
        };

        public ObservableCollection<EqualizerBandViewModel> Bands { get; } = new();

        public ICommand ResetCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand SelectPresetCommand { get; }

        public event EventHandler? CloseRequested;

        public EqualizerViewModel(IEqualizerService equalizerService)
        {
            _equalizerService = equalizerService;

            // Инициализируем полосы
            InitializeBands();

            ResetCommand = new RelayCommand(_ => Reset());
            CloseCommand = new RelayCommand(_ => Close());
            SelectPresetCommand = new RelayCommand(param =>
            {
                if (param is string presetName)
                {
                    SelectedPreset = presetName;
                }
            });

            // Подписываемся на изменения в сервисе
            _equalizerService.BandsChanged += OnBandsChanged;
        }

        private void InitializeBands()
        {
            Bands.Clear();
            for (int i = 0; i < _equalizerService.Bands.Count; i++)
            {
                var band = _equalizerService.Bands[i];
                Bands.Add(new EqualizerBandViewModel(band, i, (index, gain) =>
                {
                    _equalizerService.SetBandGain(index, gain);
                }));
            }
        }

        private void OnBandsChanged(object? sender, EventArgs e)
        {
            // Синхронизируем значения
            for (int i = 0; i < Math.Min(Bands.Count, _equalizerService.Bands.Count); i++)
            {
                var serviceBand = _equalizerService.Bands[i];
                Bands[i].UpdateFromService(serviceBand.Gain);
            }

            // Обновляем состояние включения
            OnPropertyChanged(nameof(IsEnabled));
        }

        private void Reset()
        {
            _equalizerService.ResetToFlat();
            SelectedPreset = "Flat";
        }

        private void Close()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    public class EqualizerBandViewModel : ViewModelBase
    {
        private readonly int _index;
        private readonly Action<int, float> _gainChangedAction;
        private float _gain;
        private string _frequencyLabel;

        public float Gain
        {
            get => _gain;
            set
            {
                if (SetProperty(ref _gain, value))
                {
                    _gainChangedAction?.Invoke(_index, value);
                }
            }
        }

        public string FrequencyLabel => _frequencyLabel;

        public float MinGain => -20f;
        public float MaxGain => 20f;
        public float TickFrequency => 5f;

        public EqualizerBandViewModel(Core.Interfaces.EqualizerBand band, int index, Action<int, float> gainChangedAction)
        {
            _index = index;
            _gainChangedAction = gainChangedAction;
            _gain = band.Gain;

            // Форматируем подпись частоты
            if (band.Frequency < 1000)
            {
                _frequencyLabel = $"{band.Frequency} Hz";
            }
            else
            {
                _frequencyLabel = $"{(band.Frequency / 1000):0.#} kHz";
            }
        }

        public void UpdateFromService(float gain)
        {
            SetProperty(ref _gain, gain, nameof(Gain));
        }
    }
}