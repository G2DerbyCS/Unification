using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Unification.Models;
using Unification.Models.Interfaces;

namespace Unification.Views.Controls
{
    public partial class ProgressSlider : UserControl, INotifyPropertyChanged, IProgressIndicator
    {
        private float      _Progress;
        private Brush      _ProgressBarColorBrush;
        private Thickness  _SliderThumbMargin;
        private Visibility _SliderThumbVisibility;
        private Visibility _TickBarVisibility;
        private Brush      _TicksColorBrush;
        private int        _TicksFrequency;
        private int        _TickBarLength;
        private double     _TrackCornerRadius;
        private Brush      _TrackBackgroudBrush;
        private Brush      _TrackBorderBrush;
        private Thickness  _TrackBorderThickness;
        private Thickness  _TrackPadding;
        private double     _TrackProgressBarWidth;

        public ProgressSlider()
        {
            InitializeComponent();
            AssignPropertyDefaults();
        }

        private void AssignPropertyDefaults()
        {
            Progress              = 0.0f;
            ProgressBarColorBrush = new SolidColorBrush(Colors.Black);
            SliderThumbVisibility = Visibility.Hidden;
            TickBarVisibility     = Visibility.Collapsed;
            TicksColorBrush       = new SolidColorBrush(Colors.DarkGray);
            TicksFrequency        = 1;
            TickBarLength         = 4;
            TrackCornerRadius     = 4;
            TrackBackgroudBrush   = new SolidColorBrush(Color.FromArgb(255, 231, 234, 234));
            TrackBorderThickness  = new Thickness(2);
            TrackBorderBrush      = new LinearGradientBrush(Color.FromArgb(255, 174, 177, 175), Colors.Transparent, 90);
            TrackPadding          = new Thickness(2);
        }

        private void AssignPropertyValue<T>(ref T Property, String PropertyName, T Value)
        {
            Property = Value;

            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            SliderThumbVisibility = Visibility.Visible;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            SliderThumbVisibility = Visibility.Hidden;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            SliderThumbUIElement.CaptureMouse();
            e.Handled = true;
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);

            if (SliderThumbUIElement.IsMouseCaptured)
            {
                SliderThumbUIElement.ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        public event EventHandler<StateChangeEventArgs<float>> ProgressUpdatedEvent;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (SliderThumbUIElement.IsMouseCaptured)
            {
                UpdateProgressFromMouseXPos(e.GetPosition(this).X);

                e.Handled = true;
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (Progress > 0)
            {
                UpdateProgressBar();
                UpdateSliderThumb();
            }

            base.OnRender(drawingContext);
        }

        public float Progress
        {
            set
            {
                if (value > 1 || value < 0)
                    throw new ArgumentException("Progress must be a float value between 0.0f and 1.0f");

                if (ProgressUpdatedEvent != null)
                    ProgressUpdatedEvent(this, new StateChangeEventArgs<float>(value, _Progress));

                AssignPropertyValue(ref _Progress, "Progress", value);

                UpdateProgressBar();
                UpdateSliderThumb();
            }

            get
            {
                return _Progress;
            }
        }

        public Brush ProgressBarColorBrush
        {
            set
            {
                AssignPropertyValue(ref _ProgressBarColorBrush, "ProgressBarColorBrush", value);
            }
            get
            {
                return _ProgressBarColorBrush;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Thickness SliderThumbMargin
        {
            private set
            {
                AssignPropertyValue(ref _SliderThumbMargin, "SliderThumbMargin", value);
            }

            get
            {
                return _SliderThumbMargin;
            }
        }

        public Visibility SliderThumbVisibility
        {
            private set
            {
                AssignPropertyValue(ref _SliderThumbVisibility, "SliderThumbVisibility", value);
            }

            get
            {
                return _SliderThumbVisibility;
            }
        }

        public Visibility TickBarVisibility
        {
            set
            {
                AssignPropertyValue(ref _TickBarVisibility, "TickBarVisibility", value);
            }

            get
            {
                return _TickBarVisibility;
            }
        }

        public Brush TicksColorBrush
        {
            set
            {
                AssignPropertyValue(ref _TicksColorBrush, "TicksColorBrush", value);
            }

            get
            {
                return _TicksColorBrush;
            }
        }

        public int TicksFrequency
        {
            set
            {
                AssignPropertyValue(ref _TicksFrequency, "TicksFrequency", value);
            }

            get
            {
                return _TicksFrequency;
            }
        }

        public int TickBarLength
        {
            set
            {
                AssignPropertyValue(ref _TickBarLength, "TickBarLength", value);
            }

            get
            {
                return _TickBarLength;
            }
        }

        public double TrackCornerRadius
        {
            set
            {
                AssignPropertyValue(ref _TrackCornerRadius, "TrackCornerRadius", value);
            }

            get
            {
                return _TrackCornerRadius;
            }
        }

        public Brush TrackBackgroudBrush
        {
            set
            {
                AssignPropertyValue(ref _TrackBackgroudBrush, "TrackBackgroudBrush", value);
            }

            get
            {
                return _TrackBackgroudBrush;
            }
        }

        public Brush TrackBorderBrush
        {
            set
            {
                AssignPropertyValue(ref _TrackBorderBrush, "TrackBorderBrush", value);
            }

            get
            {
                return _TrackBorderBrush;
            }
        }

        public Thickness TrackBorderThickness
        {
            set
            {
                AssignPropertyValue(ref _TrackBorderThickness, "TrackBorderThickness", value);
            }

            get
            {
                return _TrackBorderThickness;
            }
        }

        public Thickness TrackPadding
        {
            set
            {
                AssignPropertyValue(ref _TrackPadding, "TrackPadding", value);
            }

            get
            {
                return _TrackPadding;
            }
        }

        public double TrackProgressBarWidth
        {
            private set
            {
                AssignPropertyValue(ref _TrackProgressBarWidth, "TrackProgressBarWidth", value);
            }
            get
            {
                return _TrackProgressBarWidth;
            }
        }

        private void UpdateProgressFromMouseXPos(double MousePosition)
        {
            double RawProgress = (MousePosition / TrackUIElement.ActualWidth);

            if (RawProgress < 0)
            {
                Progress = 0.0f;
            }
            else if (RawProgress > 1)
            {
                Progress = 1.0f;
            }
            else
            {
                Progress = (float)RawProgress;
            }
        }

        public void UpdateProgressBar()
        {
            TrackProgressBarWidth = (TrackUIElement.ActualWidth * Progress);
        }

        public void UpdateSliderThumb()
        {
            double offset = (TrackProgressBarWidth - (SliderThumbUIElement.ActualWidth / 2));

            if (offset < 0)
            {
                offset = 0.0f;
            }
            else if (offset > (ActualWidth - SliderThumbUIElement.ActualWidth))
            {
                offset = (ActualWidth - 15);
            }

            SliderThumbMargin = new Thickness(offset, 0, 0, 0);
        }

    }
}
