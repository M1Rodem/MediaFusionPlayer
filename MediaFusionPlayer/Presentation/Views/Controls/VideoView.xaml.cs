using LibVLCSharp.Shared;
using System.Windows;
using System.Windows.Controls;

namespace MediaFusionPlayer.Presentation.Views.Controls
{
    public partial class VideoView : UserControl
    {
        public static readonly DependencyProperty VideoMediaPlayerProperty =
            DependencyProperty.Register(nameof(VideoMediaPlayer), typeof(LibVLCSharp.Shared.MediaPlayer), typeof(VideoView), new PropertyMetadata(null));

        public static readonly DependencyProperty IsVideoProperty =
            DependencyProperty.Register(nameof(IsVideo), typeof(bool), typeof(VideoView), new PropertyMetadata(false));

        public LibVLCSharp.Shared.MediaPlayer? VideoMediaPlayer
        {
            get => (LibVLCSharp.Shared.MediaPlayer?)GetValue(VideoMediaPlayerProperty);
            set => SetValue(VideoMediaPlayerProperty, value);
        }

        public bool IsVideo
        {
            get => (bool)GetValue(IsVideoProperty);
            set => SetValue(IsVideoProperty, value);
        }

        public VideoView() => InitializeComponent();
    }
}