using LibVLCSharp.Shared;
using LibVLCSharp.WPF;
using MediaFusionPlayer.Core.Interfaces;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MediaFusionPlayer.Presentation.Views.Controls
{
    public partial class VideoView : UserControl
    {
        private LibVLCSharp.Shared.MediaPlayer? _vlcMediaPlayer;
        private IVideoPlayerService? _videoService;

        public static readonly DependencyProperty VideoServiceProperty =
            DependencyProperty.Register("VideoService", typeof(IVideoPlayerService), typeof(VideoView),
                new PropertyMetadata(null, OnVideoServiceChanged));

        public IVideoPlayerService? VideoService
        {
            get => (IVideoPlayerService?)GetValue(VideoServiceProperty);
            set => SetValue(VideoServiceProperty, value);
        }

        public VideoView()
        {
            InitializeComponent();
            Loaded += VideoView_Loaded;
            Unloaded += VideoView_Unloaded;
            SizeChanged += VideoView_SizeChanged;
        }

        private void VideoView_Loaded(object sender, RoutedEventArgs e)
        {
            // Жестко ограничиваем размер
            this.MaxHeight = 250;
            this.MaxWidth = 1200;

            if (VideoService != null)
            {
                VideoService.Initialize();

                if (VideoService.LibVLC != null)
                {
                    _vlcMediaPlayer = new LibVLCSharp.Shared.MediaPlayer(VideoService.LibVLC);
                    VideoControl.MediaPlayer = _vlcMediaPlayer;
                }
            }
        }

        private void VideoView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Принудительно устанавливаем размер
            VideoContainer.Width = e.NewSize.Width;
            VideoContainer.Height = e.NewSize.Height;
            VideoControl.Width = e.NewSize.Width;
            VideoControl.Height = e.NewSize.Height;
        }

        private void VideoControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Передаем handle окна VLC
            if (VideoService != null && VideoControl.IsLoaded)
            {
                var handle = GetWindowHandle(VideoControl);
                if (handle != IntPtr.Zero)
                {
                    VideoService.SetVideoOutput(handle);
                }
            }
        }

        private void VideoView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_vlcMediaPlayer != null)
            {
                _vlcMediaPlayer.Stop();
                _vlcMediaPlayer.Dispose();
                _vlcMediaPlayer = null;
            }

            VideoControl.MediaPlayer = null;
        }

        private static void OnVideoServiceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (VideoView)d;

            if (e.NewValue is IVideoPlayerService newService)
            {
                if (control.IsLoaded)
                {
                    control.VideoView_Loaded(control, new RoutedEventArgs());
                }
            }
        }

        private IntPtr GetWindowHandle(FrameworkElement element)
        {
            var hwndSource = PresentationSource.FromVisual(element) as System.Windows.Interop.HwndSource;
            return hwndSource?.Handle ?? IntPtr.Zero;
        }

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            // Добавляем визуальную обрезку
            var clip = new System.Windows.Media.RectangleGeometry(
                new Rect(0, 0, this.ActualWidth, this.ActualHeight));
            drawingContext.PushClip(clip);
        }
    }
}