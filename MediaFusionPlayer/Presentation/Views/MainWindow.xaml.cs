using MediaFusionPlayer.Presentation.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MediaFusionPlayer.Presentation.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel => (MainViewModel)DataContext;

        public MainWindow()
        {
            InitializeComponent();
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

        // Теперь Seek идёт через команду в ViewModel — чистейший MVVM
        private void ProgressSlider_SeekRequested(object sender, TimeSpan position)
        {
            // Перекладываем ответственность на ViewModel
            ViewModel.SeekTo(position);
        }
    }
}