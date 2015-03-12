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
    /// <summary>
    /// A slider style user control for visually representing progress or level.
    /// </summary>
    public partial class ProgressSlider : UserControl, INotifyPropertyChanged, IProgressIndicator
    {
        #region Private_Variables

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

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public ProgressSlider()
        {
            InitializeComponent();
            AssignPropertyDefaults();
        }

        /// <summary>
        /// Assigns properties default values.
        /// </summary>
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

        /// <summary>
        /// Assigns a value to a property and raises the PropertyChanged event if not null.
        /// </summary>
        /// <typeparam name="T">Type Specifier.</typeparam>
        /// <param name="Property">Reference to property to assign Value to.</param>
        /// <param name="PropertyName">Property name/title for PropertyChanged event call.</param>
        /// <param name="Value">Value to be assigned to referenced property.</param>
        private void AssignPropertyValue<T>(ref T Property, String PropertyName, T Value)
        {
            Property = Value;

            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }

        /// <summary>
        /// Overriden to set slider thumb visibility to Visible.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            SliderThumbVisibility = Visibility.Visible;
        }

        /// <summary>
        /// Overriden to set slider thumb visibility to Hidden.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            SliderThumbVisibility = Visibility.Hidden;
        }

        /// <summary>
        /// Overriden to assign mouse capture to SliderThumbUIElement.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            SliderThumbUIElement.CaptureMouse();
            e.Handled = true;
        }

        /// <summary>
        /// Overriden to release mouse capture from SliderThumbUIElement if captured.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);

            if (SliderThumbUIElement.IsMouseCaptured)
            {
                SliderThumbUIElement.ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Event to be raised when Progress property value changes.
        /// </summary>
        public event EventHandler<StateChangeEventArgs<float>> ProgressUpdatedEvent;

        /// <summary>
        /// Overriden to update Progress property value if SliderThumbUIElement has mouse capture.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (SliderThumbUIElement.IsMouseCaptured)
            {
                UpdateProgressFromMouseXPos(e.GetPosition(this).X);

                e.Handled = true;
            }
        }

        /// <summary>
        /// Overriden to update SliderThumbUIElement and TrackProgressBar UIElement position when ProgressSlider is rendered.
        /// </summary>
        /// <param name="drawingContext"></param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (Progress > 0)
            {
                UpdateProgressBar();
                UpdateSliderThumb();
            }

            base.OnRender(drawingContext);
        }

        /// <summary>
        /// Sets/Gets Progress property value and updates SliderThumbUIElement and TrackProgressBar UIElement position.
        /// </summary>
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

        /// <summary>
        /// Sets/Gets the brush used by TrackProgressBar UIElement.
        /// </summary>
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

        /// <summary>
        /// Event to be raised when a public ProgressSlider property value changes.
        /// </summary>
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

        /// <summary>
        /// Sets/Gets the visibility of the top and bottom TickBars.
        /// </summary>
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

        /// <summary>
        /// Sets/Gets the brush used by the top and bottom TickBars.
        /// </summary>
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

        /// <summary>
        /// Represents the frequecy value of the top and bottom TickBars.
        /// </summary>
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

        /// <summary>
        /// Represents the length value of the top and bottom TickBars' ticks.
        /// </summary>
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

        /// <summary>
        /// Represents the corner radius of ProgressSlider track and progress bar.
        /// </summary>
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

        /// <summary>
        /// Represents the background brush used by the ProgressSlider track.
        /// </summary>
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

        /// <summary>
        /// Represents the border brush used by the ProgressSlider track.
        /// </summary>
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

        /// <summary>
        /// Represents the thickness of the ProgressSlider track border.
        /// </summary>
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

        /// <summary>
        /// Represents the padding between the ProgressSlider track and the top and bottom TickBars.
        /// </summary>
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

        /// <summary>
        /// Updates the Progress property value based on the input mouse position, relative to the ProgressSlider track.
        /// </summary>
        /// <param name="MousePosition">Mouse axis value.</param>
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

        /// <summary>
        /// Updates the width of the TrackProgressBar based on the TrackUIElement width and the Progress property value.
        /// </summary>
        public void UpdateProgressBar()
        {
            TrackProgressBarWidth = (TrackUIElement.ActualWidth * Progress);
        }

        /// <summary>
        /// Updates the position of the SliderThumbUIElement based TrackProgressBarWidth.
        /// </summary>
        /// <remarks>
        /// Compensates for visual inconsistencies caused by boundaries and the width of the SliderThumbUIElement.
        /// </remarks>
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
