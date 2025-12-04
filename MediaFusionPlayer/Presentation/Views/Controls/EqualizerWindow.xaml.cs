using MediaFusionPlayer.Presentation.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MediaFusionPlayer.Presentation.Views
{
    public partial class EqualizerWindow : Window
    {
        private readonly EqualizerViewModel _viewModel;

        // Изменяем конструктор - принимаем ViewModel через DI
        public EqualizerWindow(EqualizerViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = _viewModel;

            InitializeComponent();

            // Подписываемся на событие закрытия
            _viewModel.CloseRequested += (s, e) => Close();

            // Настраиваем владельца окна
            if (Application.Current.MainWindow != null)
            {
                Owner = Application.Current.MainWindow;
            }
        }

        // Обработчик для мгновенного применения изменений при перетаскивании слайдера
        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            // Принудительное обновление привязки
            if (sender is Slider slider)
            {
                var binding = slider.GetBindingExpression(Slider.ValueProperty);
                binding?.UpdateSource();
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Скрываем окно вместо закрытия (чтобы можно было снова открыть)
            e.Cancel = true;
            Hide();
        }
    }
}