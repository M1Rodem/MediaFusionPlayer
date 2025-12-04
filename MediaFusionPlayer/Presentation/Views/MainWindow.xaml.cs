using MediaFusionPlayer.Presentation.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MediaFusionPlayer.Presentation.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel => (MainViewModel)DataContext;

        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void Maximize_Click(object sender, RoutedEventArgs e)
            => WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        public MainWindow()
        {
            InitializeComponent();
            // Убрали все лишние инициализации
        }

        private async void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
                {
                    await ViewModel.AddFilesFromDropAsync(files);
                }
            }
            e.Handled = true;
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        // ЭТОТ МЕТОД НУЖНО УДАЛИТЬ - он не используется в MVVM
        // private void ProgressSlider_SeekRequested(object sender, TimeSpan position)
        // {
        //     ViewModel.SeekTo(position);
        // }
        private void ProgressSlider_SeekRequested(object sender, TimeSpan position)
        {
            ViewModel?.SeekTo(position);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (ViewModel == null) return;

            switch (e.Key)
            {
                case Key.Space:
                    ViewModel.PlayPauseCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.Left:
                    ViewModel.PreviousCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.Right:
                    ViewModel.NextCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.Escape:
                    ViewModel.StopCommand.Execute(null);
                    e.Handled = true;
                    break;
            }
        }
    }
}