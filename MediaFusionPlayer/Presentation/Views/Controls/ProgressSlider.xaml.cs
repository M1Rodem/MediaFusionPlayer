using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MediaFusionPlayer.Presentation.Views.Controls
{
    public partial class ProgressSlider : UserControl
    {
        public static readonly DependencyProperty CurrentPositionProperty =
            DependencyProperty.Register(nameof(CurrentPosition), typeof(TimeSpan), typeof(ProgressSlider),
                new FrameworkPropertyMetadata(TimeSpan.Zero, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        public static readonly DependencyProperty CurrentDurationProperty =
            DependencyProperty.Register(nameof(CurrentDuration), typeof(TimeSpan), typeof(ProgressSlider),
                new PropertyMetadata(TimeSpan.Zero, OnValueChanged));

        public TimeSpan CurrentPosition
        {
            get => (TimeSpan)GetValue(CurrentPositionProperty);
            set => SetValue(CurrentPositionProperty, value);
        }

        public TimeSpan CurrentDuration
        {
            get => (TimeSpan)GetValue(CurrentDurationProperty);
            set => SetValue(CurrentDurationProperty, value);
        }

        public event EventHandler<TimeSpan>? SeekRequested;

        private bool _isDragging;

        public ProgressSlider()
        {
            InitializeComponent();
            Loaded += (s, e) => CompositionTarget.Rendering += OnRendering;
            Unloaded += (s, e) => CompositionTarget.Rendering -= OnRendering;
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            if (!_isDragging) UpdateVisual();
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProgressSlider ps && !ps._isDragging) ps.UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (CurrentDuration == TimeSpan.Zero)
            {
                ProgressFill.Width = 0;
                Thumb.Margin = new Thickness(-8, 0, 0, 0);
                return;
            }

            double ratio = Math.Clamp(CurrentPosition.TotalSeconds / CurrentDuration.TotalSeconds, 0, 1);
            double w = TrackGrid.ActualWidth;
            double fill = ratio * w;
            ProgressFill.Width = fill;
            Thumb.Margin = new Thickness(fill - 8, 0, 0, 0);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!TrackGrid.IsMouseOver) return;
            CaptureMouse();
            _isDragging = true;
            Thumb.Visibility = Visibility.Visible;
            Seek(e.GetPosition(TrackGrid));
            e.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_isDragging) Seek(e.GetPosition(TrackGrid));
            Thumb.Visibility = TrackGrid.IsMouseOver ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                Seek(e.GetPosition(TrackGrid));
                ReleaseMouseCapture();
                _isDragging = false;
                if (!TrackGrid.IsMouseOver) Thumb.Visibility = Visibility.Collapsed;
            }
        }

        public static readonly DependencyProperty SeekCommandProperty =
            DependencyProperty.Register(nameof(SeekCommand), typeof(ICommand), typeof(ProgressSlider));

        public ICommand SeekCommand
        {
            get => (ICommand)GetValue(SeekCommandProperty);
            set => SetValue(SeekCommandProperty, value);
        }
        private void Seek(Point p)
        {
            double x = Math.Clamp(p.X, 0, TrackGrid.ActualWidth);
            double ratio = TrackGrid.ActualWidth > 0 ? x / TrackGrid.ActualWidth : 0;
            TimeSpan pos = TimeSpan.FromSeconds(ratio * CurrentDuration.TotalSeconds);
            CurrentPosition = pos;
            UpdateVisual();

            // Вызываем команду если она есть
            if (SeekCommand?.CanExecute(pos) == true)
                SeekCommand.Execute(pos);

            SeekRequested?.Invoke(this, pos);
        }

        protected override Size ArrangeOverride(Size size)
        {
            var r = base.ArrangeOverride(size);
            UpdateVisual();
            return r;
        }
    }
}