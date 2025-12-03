using MediaFusionPlayer.Presentation.ViewModels;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace MediaFusionPlayer.Presentation.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel => (MainViewModel)DataContext;

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_COMPOSITED = 0x2000000;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_NOMOVE = 0x0002;        // Добавил
        private const uint SWP_NOSIZE = 0x0001;        // Добавил
        private const uint SWP_FRAMECHANGED = 0x0020;  // Добавил
        private const uint SWP_SHOWWINDOW = 0x0040;    // Добавил

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            SourceInitialized += MainWindow_SourceInitialized;
            SizeChanged += MainWindow_SizeChanged;
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            // Дополнительная инициализация после создания окна
            UpdateVideoContainerSize();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Устанавливаем стили окна для ограничения VLC
            var handle = new WindowInteropHelper(this).Handle;

            // Добавляем стиль для ограничения
            int exStyle = GetWindowLong(handle, GWL_EXSTYLE);
            exStyle |= WS_EX_COMPOSITED; // Включаем двойную буферизацию
            SetWindowLong(handle, GWL_EXSTYLE, exStyle);

            // Обновляем окно
            SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0,
                SWP_NOZORDER | SWP_NOACTIVATE | SWP_NOMOVE | SWP_NOSIZE | SWP_FRAMECHANGED);

            // Принудительно обновляем размеры контейнера видео
            UpdateVideoContainerSize();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // При изменении размера окна обновляем контейнер видео
            UpdateVideoContainerSize();
        }

        private void UpdateVideoContainerSize()
        {
            // Жестко ограничиваем размер контейнера видео
            if (VideoOuterContainer != null)
            {
                // Максимальные размеры для видео
                double maxWidth = Math.Min(1200, this.ActualWidth - 100);
                VideoOuterContainer.MaxWidth = maxWidth;
                VideoOuterContainer.MaxHeight = 250;
                VideoOuterContainer.Width = maxWidth;
                VideoOuterContainer.Height = 250;

                // Центрируем
                VideoOuterContainer.HorizontalAlignment = HorizontalAlignment.Center;
                VideoOuterContainer.VerticalAlignment = VerticalAlignment.Center;
            }
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

        private void ProgressSlider_SeekRequested(object sender, TimeSpan position)
        {
            ViewModel.SeekTo(position);
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