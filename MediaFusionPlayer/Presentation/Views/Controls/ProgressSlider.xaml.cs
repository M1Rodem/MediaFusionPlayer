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
                new PropertyMetadata(TimeSpan.Zero, OnPositionChanged));

        public static readonly DependencyProperty CurrentDurationProperty =
            DependencyProperty.Register(nameof(CurrentDuration), typeof(TimeSpan), typeof(ProgressSlider),
                new PropertyMetadata(TimeSpan.Zero, OnPositionChanged));

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

        private bool _isDragging = false;

        public ProgressSlider()
        {
            InitializeComponent();
            Loaded += (s, e) => CompositionTarget.Rendering += OnRender;
            Unloaded += (s, e) => CompositionTarget.Rendering -= OnRender;
            MouseEnter += (s, e) => Thumb.Visibility = Visibility.Visible;
            MouseLeave += (s, e) => { if (!_isDragging) Thumb.Visibility = Visibility.Collapsed; };
        }

        private void OnRender(object? sender, EventArgs e)
        {
            if (!_isDragging && CurrentDuration > TimeSpan.Zero)
            {
                UpdateProgress();
            }
        }

        private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProgressSlider slider && !slider._isDragging)
                slider.UpdateProgress();
        }

        private void UpdateProgress()
        {
            if (CurrentDuration == TimeSpan.Zero)
            {
                ProgressFill.Width = 0;
                Thumb.Margin = new Thickness(-7, 0, 0, 0);
                return;
            }

            double progress = CurrentPosition.TotalSeconds / CurrentDuration.TotalSeconds;
            progress = Math.Clamp(progress, 0.0, 1.0);

            double trackWidth = TrackGrid.ActualWidth;
            double fillWidth = progress * trackWidth;
            double thumbX = fillWidth - 7; // центр thumb

            ProgressFill.Width = fillWidth;
            Thumb.Margin = new Thickness(thumbX, 0, 0, 0);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            CaptureMouse();
            _isDragging = true;
            Thumb.Visibility = Visibility.Visible;
            SeekToMouse(e.GetPosition(TrackGrid));
            e.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_isDragging)
            {
                SeekToMouse(e.GetPosition(TrackGrid));
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (_isDragging)
            {
                SeekToMouse(e.GetPosition(TrackGrid));
                ReleaseMouseCapture();
                _isDragging = false;
                SeekRequested?.Invoke(this, CurrentPosition);
                if (!IsMouseOver) Thumb.Visibility = Visibility.Collapsed;
            }
        }

        private void SeekToMouse(Point pos)
        {
            double x = Math.Clamp(pos.X, 0, TrackGrid.ActualWidth);
            double ratio = x / TrackGrid.ActualWidth;
            TimeSpan newPos = TimeSpan.FromSeconds(ratio * CurrentDuration.TotalSeconds);
            CurrentPosition = newPos;
            UpdateProgress(); // мгновенное обновление
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var size = base.ArrangeOverride(arrangeBounds);
            UpdateProgress();
            return size;
        }
    }
}