using MediaFusionPlayer.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MediaFusionPlayer.Presentation.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // === Обработчики Drag & Drop ===
        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private async void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && DataContext is MainViewModel viewModel)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var validFiles = files.Where(File.Exists).ToArray();

                if (validFiles.Length > 0)
                {
                    await viewModel.AddFilesFromDropAsync(validFiles);
                }
            }
        }

        // === Обработчики кнопок заголовка ===
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // === Кнопка эквалайзера ===
        private EqualizerWindow? _equalizerWindow;

        private void EqualizerButton_Click(object sender, RoutedEventArgs e)
        {
            if (_equalizerWindow == null)
            {
                _equalizerWindow = App.Current.Services.GetRequiredService<EqualizerWindow>();
            }

            if (_equalizerWindow.IsVisible)
            {
                _equalizerWindow.Hide();
            }
            else
            {
                _equalizerWindow.Show();
                _equalizerWindow.Activate();
            }
        }

        // === Обработка перемещения окна ===
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}