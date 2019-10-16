﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FluidProgressBar.xaml.cs" company="WildGums">
//   Copyright (c) 2008 - 2016 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Animation;

    /// <summary>
    /// Interaction logic for FluidProgressBar.xaml
    /// </summary>
    public partial class FluidProgressBar : UserControl, IDisposable
    {
        #region Fields
        private readonly Dictionary<int, KeyFrameDetails> _keyFrameMap = null;
        private readonly Dictionary<int, KeyFrameDetails> _opKeyFrameMap = null;
        private bool _isStoryboardRunning;
        private Storyboard _sb;
        #endregion

        #region Constructors

        #region Construction / Initialization
        public FluidProgressBar()
        {
            InitializeComponent();

            _keyFrameMap = new Dictionary<int, KeyFrameDetails>();
            _opKeyFrameMap = new Dictionary<int, KeyFrameDetails>();

            GetKeyFramesFromStoryboard();

            SizeChanged += OnSizeChanged;
            Loaded += OnLoaded;
            IsVisibleChanged += OnIsVisibleChanged;
        }
        #endregion

        #endregion

        #region Internal class
        private class KeyFrameDetails
        {
            #region Properties
            public KeyTime KeyFrameTime { get; set; }
            public List<DoubleKeyFrame> KeyFrames { get; set; }
            #endregion
        }
        #endregion

        #region Delay
        /// <summary>
        /// Delay Dependency Property
        /// </summary>
        public static readonly DependencyProperty DelayProperty =
            DependencyProperty.Register("Delay", typeof(Duration), typeof(FluidProgressBar),
                new FrameworkPropertyMetadata(new Duration(TimeSpan.FromMilliseconds(100)), new PropertyChangedCallback(OnDelayChanged)));

        /// <summary>
        /// Gets or sets the Delay property. This dependency property 
        /// indicates the delay between adjacent animation timelines.
        /// </summary>
        public Duration Delay
        {
            get { return (Duration)GetValue(DelayProperty); }
            set { SetValue(DelayProperty, value); }
        }

        /// <summary>
        /// Handles changes to the Delay property.
        /// </summary>
        /// <param name="d">FluidProgressBar</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnDelayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pBar = (FluidProgressBar)d;
            var oldDelay = (Duration)e.OldValue;
            var newDelay = pBar.Delay;
            pBar.OnDelayChanged(oldDelay, newDelay);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the Delay property.
        /// </summary>
        /// <param name="oldDelay">Old Value</param>
        /// <param name="newDelay">New Value</param>
        protected virtual void OnDelayChanged(Duration oldDelay, Duration newDelay)
        {
            var isActive = _isStoryboardRunning;
            if (isActive)
            {
                StopFluidAnimation();
            }

            UpdateTimelineDelay(newDelay);

            if (isActive)
            {
                StartFluidAnimation();
            }
        }
        #endregion

        #region DotWidth
        /// <summary>
        /// DotWidth Dependency Property
        /// </summary>
        public static readonly DependencyProperty DotWidthProperty =
            DependencyProperty.Register("DotWidth", typeof(double), typeof(FluidProgressBar),
                new FrameworkPropertyMetadata(4.0, new PropertyChangedCallback(OnDotWidthChanged)));

        /// <summary>
        /// Gets or sets the DotWidth property. This dependency property 
        /// indicates the width of each of the dots.
        /// </summary>
        public double DotWidth
        {
            get { return (double)GetValue(DotWidthProperty); }
            set { SetValue(DotWidthProperty, value); }
        }

        /// <summary>
        /// Handles changes to the DotWidth property.
        /// </summary>
        /// <param name="d">FluidProgressBar</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnDotWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pBar = (FluidProgressBar)d;
            var oldDotWidth = (double)e.OldValue;
            var newDotWidth = pBar.DotWidth;
            pBar.OnDotWidthChanged(oldDotWidth, newDotWidth);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the DotWidth property.
        /// </summary>
        /// <param name="oldDotWidth">Old Value</param>
        /// <param name="newDotWidth">New Value</param>
        protected virtual void OnDotWidthChanged(double oldDotWidth, double newDotWidth)
        {
            if (_isStoryboardRunning)
            {
                RestartStoryboardAnimation();
            }
        }
        #endregion

        #region DotHeight
        /// <summary>
        /// DotHeight Dependency Property
        /// </summary>
        public static readonly DependencyProperty DotHeightProperty =
            DependencyProperty.Register("DotHeight", typeof(double), typeof(FluidProgressBar),
                new FrameworkPropertyMetadata(4.0, new PropertyChangedCallback(OnDotHeightChanged)));

        /// <summary>
        /// Gets or sets the DotHeight property. This dependency property 
        /// indicates the height of each of the dots.
        /// </summary>
        public double DotHeight
        {
            get { return (double)GetValue(DotHeightProperty); }
            set { SetValue(DotHeightProperty, value); }
        }

        /// <summary>
        /// Handles changes to the DotHeight property.
        /// </summary>
        /// <param name="d">FluidProgressBar</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnDotHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pBar = (FluidProgressBar)d;
            var oldDotHeight = (double)e.OldValue;
            var newDotHeight = pBar.DotHeight;
            pBar.OnDotHeightChanged(oldDotHeight, newDotHeight);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the DotHeight property.
        /// </summary>
        /// <param name="oldDotHeight">Old Value</param>
        /// <param name="newDotHeight">New Value</param>
        protected virtual void OnDotHeightChanged(double oldDotHeight, double newDotHeight)
        {
            if (_isStoryboardRunning)
            {
                RestartStoryboardAnimation();
            }
        }
        #endregion

        #region DotRadiusX
        /// <summary>
        /// DotRadiusX Dependency Property
        /// </summary>
        public static readonly DependencyProperty DotRadiusXProperty =
            DependencyProperty.Register("DotRadiusX", typeof(double), typeof(FluidProgressBar),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnDotRadiusXChanged)));

        /// <summary>
        /// Gets or sets the DotRadiusX property. This dependency property 
        /// indicates the corner radius width of each of the dot.
        /// </summary>
        public double DotRadiusX
        {
            get { return (double)GetValue(DotRadiusXProperty); }
            set { SetValue(DotRadiusXProperty, value); }
        }

        /// <summary>
        /// Handles changes to the DotRadiusX property.
        /// </summary>
        /// <param name="d">FluidProgressBar</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnDotRadiusXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pBar = (FluidProgressBar)d;
            var oldDotRadiusX = (double)e.OldValue;
            var newDotRadiusX = pBar.DotRadiusX;
            pBar.OnDotRadiusXChanged(oldDotRadiusX, newDotRadiusX);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the DotRadiusX property.
        /// </summary>
        /// <param name="oldDotRadiusX">Old Value</param>
        /// <param name="newDotRadiusX">New Value</param>
        protected virtual void OnDotRadiusXChanged(double oldDotRadiusX, double newDotRadiusX)
        {
            if (_isStoryboardRunning)
            {
                RestartStoryboardAnimation();
            }
        }
        #endregion

        #region DotRadiusY
        /// <summary>
        /// DotRadiusY Dependency Property
        /// </summary>
        public static readonly DependencyProperty DotRadiusYProperty =
            DependencyProperty.Register("DotRadiusY", typeof(double), typeof(FluidProgressBar),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnDotRadiusYChanged)));

        /// <summary>
        /// Gets or sets the DotRadiusY property. This dependency property 
        /// indicates the corner height of each of the dots.
        /// </summary>
        public double DotRadiusY
        {
            get { return (double)GetValue(DotRadiusYProperty); }
            set { SetValue(DotRadiusYProperty, value); }
        }

        /// <summary>
        /// Handles changes to the DotRadiusY property.
        /// </summary>
        /// <param name="d">FluidProgressBar</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnDotRadiusYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pBar = (FluidProgressBar)d;
            var oldDotRadiusY = (double)e.OldValue;
            var newDotRadiusY = pBar.DotRadiusY;
            pBar.OnDotRadiusYChanged(oldDotRadiusY, newDotRadiusY);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the DotRadiusY property.
        /// </summary>
        /// <param name="oldDotRadiusY">Old Value</param>
        /// <param name="newDotRadiusY">New Value</param>
        protected virtual void OnDotRadiusYChanged(double oldDotRadiusY, double newDotRadiusY)
        {
            if (_isStoryboardRunning)
            {
                RestartStoryboardAnimation();
            }
        }
        #endregion

        #region DurationA
        /// <summary>
        /// DurationA Dependency Property
        /// </summary>
        public static readonly DependencyProperty DurationAProperty =
            DependencyProperty.Register("DurationA", typeof(Duration), typeof(FluidProgressBar),
                new FrameworkPropertyMetadata(new Duration(TimeSpan.FromSeconds(0.5)), new PropertyChangedCallback(OnDurationAChanged)));

        /// <summary>
        /// Gets or sets the DurationA property. This dependency property 
        /// indicates the duration of the animation from the start point till KeyFrameA.
        /// </summary>
        public Duration DurationA
        {
            get { return (Duration)GetValue(DurationAProperty); }
            set { SetValue(DurationAProperty, value); }
        }

        /// <summary>
        /// Handles changes to the DurationA property.
        /// </summary>
        /// <param name="d">FluidProgressBar</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnDurationAChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pBar = (FluidProgressBar)d;
            var oldDurationA = (Duration)e.OldValue;
            var newDurationA = pBar.DurationA;
            pBar.OnDurationAChanged(oldDurationA, newDurationA);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the DurationA property.
        /// </summary>
        /// <param name="oldDurationA">Old Value</param>
        /// <param name="newDurationA">New Value</param>
        protected virtual void OnDurationAChanged(Duration oldDurationA, Duration newDurationA)
        {
            var isActive = _isStoryboardRunning;
            if (isActive)
            {
                StopFluidAnimation();
            }

            UpdateKeyTimes(1, newDurationA);

            if (isActive)
            {
                StartFluidAnimation();
            }
        }
        #endregion

        #region DurationB
        /// <summary>
        /// DurationB Dependency Property
        /// </summary>
        public static readonly DependencyProperty DurationBProperty =
            DependencyProperty.Register("DurationB", typeof(Duration), typeof(FluidProgressBar),
                new FrameworkPropertyMetadata(new Duration(TimeSpan.FromSeconds(1.5)), new PropertyChangedCallback(OnDurationBChanged)));

        /// <summary>
        /// Gets or sets the DurationB property. This dependency property 
        /// indicates the duration of the animation from the KeyFrameA till KeyFrameB.
        /// </summary>
        public Duration DurationB
        {
            get { return (Duration)GetValue(DurationBProperty); }
            set { SetValue(DurationBProperty, value); }
        }

        /// <summary>
        /// Handles changes to the DurationB property.
        /// </summary>
        /// <param name="d">FluidProgressBar</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnDurationBChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pBar = (FluidProgressBar)d;
            var oldDurationB = (Duration)e.OldValue;
            var newDurationB = pBar.DurationB;
            pBar.OnDurationBChanged(oldDurationB, newDurationB);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the DurationB property.
        /// </summary>
        /// <param name="oldDurationB">Old Value</param>
        /// <param name="newDurationB">New Value</param>
        protected virtual void OnDurationBChanged(Duration oldDurationB, Duration newDurationB)
        {
            var isActive = _isStoryboardRunning;
            if (isActive)
            {
                StopFluidAnimation();
            }

            UpdateKeyTimes(2, newDurationB);

            if (isActive)
            {
                StartFluidAnimation();
            }
        }
        #endregion

        #region DurationC
        /// <summary>
        /// DurationC Dependency Property
        /// </summary>
        public static readonly DependencyProperty DurationCProperty =
            DependencyProperty.Register("DurationC", typeof(Duration), typeof(FluidProgressBar),
                new FrameworkPropertyMetadata(new Duration(TimeSpan.FromSeconds(0.5)), new PropertyChangedCallback(OnDurationCChanged)));

        /// <summary>
        /// Gets or sets the DurationC property. This dependency property 
        /// indicates the duration of the animation from KeyFrameB till the end point.
        /// </summary>
        public Duration DurationC
        {
            get { return (Duration)GetValue(DurationCProperty); }
            set { SetValue(DurationCProperty, value); }
        }

        /// <summary>
        /// Handles changes to the DurationC property.
        /// </summary>
        /// <param name="d">FluidProgressBar</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnDurationCChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pBar = (FluidProgressBar)d;
            var oldDurationC = (Duration)e.OldValue;
            var newDurationC = pBar.DurationC;
            pBar.OnDurationCChanged(oldDurationC, newDurationC);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the DurationC property.
        /// </summary>
        /// <param name="oldDurationC">Old Value</param>
        /// <param name="newDurationC">New Value</param>
        protected virtual void OnDurationCChanged(Duration oldDurationC, Duration newDurationC)
        {
            bool isActive = _isStoryboardRunning;
            if (isActive)
            {
                StopFluidAnimation();
            }

            UpdateKeyTimes(3, newDurationC);

            if (isActive)
            {
                StartFluidAnimation();
            }
        }
        #endregion

        #region KeyFrameA
        /// <summary>
        /// KeyFrameA Dependency Property
        /// </summary>
        public static readonly DependencyProperty KeyFrameAProperty =
            DependencyProperty.Register("KeyFrameA", typeof(double), typeof(FluidProgressBar),
                new FrameworkPropertyMetadata(0.33, new PropertyChangedCallback(OnKeyFrameAChanged)));

        /// <summary>
        /// Gets or sets the KeyFrameA property. This dependency property 
        /// indicates the first KeyFrame position after the initial keyframe.
        /// </summary>
        public double KeyFrameA
        {
            get { return (double)GetValue(KeyFrameAProperty); }
            set { SetValue(KeyFrameAProperty, value); }
        }

        /// <summary>
        /// Handles changes to the KeyFrameA property.
        /// </summary>
        /// <param name="d">FluidProgressBar</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnKeyFrameAChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pBar = (FluidProgressBar)d;
            var oldKeyFrameA = (double)e.OldValue;
            var newKeyFrameA = pBar.KeyFrameA;
            pBar.OnKeyFrameAChanged(oldKeyFrameA, newKeyFrameA);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the KeyFrameA property.
        /// </summary>
        /// <param name="oldKeyFrameA">Old Value</param>
        /// <param name="newKeyFrameA">New Value</param>
        protected virtual void OnKeyFrameAChanged(double oldKeyFrameA, double newKeyFrameA)
        {
            RestartStoryboardAnimation();
        }
        #endregion

        #region KeyFrameB
        /// <summary>
        /// KeyFrameB Dependency Property
        /// </summary>
        public static readonly DependencyProperty KeyFrameBProperty =
            DependencyProperty.Register("KeyFrameB", typeof(double), typeof(FluidProgressBar),
                new FrameworkPropertyMetadata(0.63, new PropertyChangedCallback(OnKeyFrameBChanged)));

        /// <summary>
        /// Gets or sets the KeyFrameB property. This dependency property 
        /// indicates the second KeyFrame position after the initial keyframe.
        /// </summary>
        public double KeyFrameB
        {
            get { return (double)GetValue(KeyFrameBProperty); }
            set { SetValue(KeyFrameBProperty, value); }
        }

        /// <summary>
        /// Handles changes to the KeyFrameB property.
        /// </summary>
        /// <param name="d">FluidProgressBar</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnKeyFrameBChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pBar = (FluidProgressBar)d;
            var oldKeyFrameB = (double)e.OldValue;
            var newKeyFrameB = pBar.KeyFrameB;
            pBar.OnKeyFrameBChanged(oldKeyFrameB, newKeyFrameB);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the KeyFrameB property.
        /// </summary>
        /// <param name="oldKeyFrameB">Old Value</param>
        /// <param name="newKeyFrameB">New Value</param>
        protected virtual void OnKeyFrameBChanged(double oldKeyFrameB, double newKeyFrameB)
        {
            RestartStoryboardAnimation();
        }
        #endregion

        #region Oscillate
        /// <summary>
        /// Oscillate Dependency Property
        /// </summary>
        public static readonly DependencyProperty OscillateProperty =
            DependencyProperty.Register("Oscillate", typeof(bool), typeof(FluidProgressBar),
                new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnOscillateChanged)));

        /// <summary>
        /// Gets or sets the Oscillate property. This dependency property 
        /// indicates whether the animation should oscillate.
        /// </summary>
        public bool Oscillate
        {
            get { return (bool)GetValue(OscillateProperty); }
            set { SetValue(OscillateProperty, value); }
        }

        /// <summary>
        /// Handles changes to the Oscillate property.
        /// </summary>
        /// <param name="d">FluidProgressBar</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnOscillateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pBar = (FluidProgressBar)d;
            var oldOscillate = (bool)e.OldValue;
            var newOscillate = pBar.Oscillate;
            pBar.OnOscillateChanged(oldOscillate, newOscillate);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the Oscillate property.
        /// </summary>
        /// <param name="oldOscillate">Old Value</param>
        /// <param name="newOscillate">New Value</param>
        protected virtual void OnOscillateChanged(bool oldOscillate, bool newOscillate)
        {
            if (_sb == null)
            {
                return;
            }

            StopFluidAnimation();

            _sb.SetCurrentValue(Timeline.AutoReverseProperty, newOscillate);
            _sb.SetCurrentValue(Timeline.DurationProperty, newOscillate ? ReverseDuration : TotalDuration);

            StartFluidAnimation();
        }
        #endregion

        #region ReverseDuration
        /// <summary>
        /// ReverseDuration Dependency Property
        /// </summary>
        public static readonly DependencyProperty ReverseDurationProperty =
            DependencyProperty.Register("ReverseDuration", typeof(Duration), typeof(FluidProgressBar),
                new FrameworkPropertyMetadata(new Duration(TimeSpan.FromSeconds(2.9)), new PropertyChangedCallback(OnReverseDurationChanged)));

        /// <summary>
        /// Gets or sets the ReverseDuration property. This dependency property 
        /// indicates the duration of the total animation in reverse.
        /// </summary>
        public Duration ReverseDuration
        {
            get { return (Duration)GetValue(ReverseDurationProperty); }
            set { SetValue(ReverseDurationProperty, value); }
        }

        /// <summary>
        /// Handles changes to the ReverseDuration property.
        /// </summary>
        /// <param name="d">FluidProgressBar</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnReverseDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pBar = (FluidProgressBar)d;
            var oldReverseDuration = (Duration)e.OldValue;
            var newReverseDuration = pBar.ReverseDuration;
            pBar.OnReverseDurationChanged(oldReverseDuration, newReverseDuration);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the ReverseDuration property.
        /// </summary>
        /// <param name="oldReverseDuration">Old Value</param>
        /// <param name="newReverseDuration">New Value</param>
        protected virtual void OnReverseDurationChanged(Duration oldReverseDuration, Duration newReverseDuration)
        {
            if (_sb == null || !Oscillate)
            {
                return;
            }

            _sb.SetCurrentValue(Timeline.DurationProperty, newReverseDuration);
            RestartStoryboardAnimation();
        }
        #endregion

        #region TotalDuration
        /// <summary>
        /// TotalDuration Dependency Property
        /// </summary>
        public static readonly DependencyProperty TotalDurationProperty =
            DependencyProperty.Register("TotalDuration", typeof(Duration), typeof(FluidProgressBar),
                new FrameworkPropertyMetadata(new Duration(TimeSpan.FromSeconds(4.4)), new PropertyChangedCallback(OnTotalDurationChanged)));

        /// <summary>
        /// Gets or sets the TotalDuration property. This dependency property 
        /// indicates the duration of the complete animation.
        /// </summary>
        public Duration TotalDuration
        {
            get { return (Duration)GetValue(TotalDurationProperty); }
            set { SetValue(TotalDurationProperty, value); }
        }

        /// <summary>
        /// Handles changes to the TotalDuration property.
        /// </summary>
        /// <param name="d">FluidProgressBar</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnTotalDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pBar = (FluidProgressBar)d;
            var oldTotalDuration = (Duration)e.OldValue;
            var newTotalDuration = pBar.TotalDuration;
            pBar.OnTotalDurationChanged(oldTotalDuration, newTotalDuration);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the TotalDuration property.
        /// </summary>
        /// <param name="oldTotalDuration">Old Value</param>
        /// <param name="newTotalDuration">New Value</param>
        protected virtual void OnTotalDurationChanged(Duration oldTotalDuration, Duration newTotalDuration)
        {
            if (_sb == null || Oscillate)
            {
                return;
            }

            _sb.SetCurrentValue(Timeline.DurationProperty, newTotalDuration);

            RestartStoryboardAnimation();
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the Loaded event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">EventArgs</param>
        void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Update the key frames
            UpdateKeyFrames();
            // Start the animation
            StartFluidAnimation();
        }

        /// <summary>
        /// Handles the SizeChanged event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">EventArgs</param>
        void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Restart the animation
            RestartStoryboardAnimation();
        }

        /// <summary>
        /// Handles the IsVisibleChanged event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">EventArgs</param>
        void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                UpdateKeyFrames();
                StartFluidAnimation();
            }
            else
            {
                StopFluidAnimation();
            }
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Starts the animation
        /// </summary>
        private void StartFluidAnimation()
        {
            if (_sb == null || (_isStoryboardRunning))
            {
                return;
            }

            _sb.Begin();
            _isStoryboardRunning = true;
        }

        /// <summary>
        /// Stops the animation
        /// </summary>
        private void StopFluidAnimation()
        {
            if (_sb == null || (!_isStoryboardRunning))
            {
                return;
            }

            // Move the timeline to the end and stop the animation
            _sb.SeekAlignedToLastTick(TimeSpan.FromSeconds(0));
            _sb.Stop();
            _isStoryboardRunning = false;
        }

        /// <summary>
        /// Stops the animation, updates the keyframes and starts the animation
        /// </summary>
        private void RestartStoryboardAnimation()
        {
            StopFluidAnimation();
            UpdateKeyFrames();
            StartFluidAnimation();
        }

        /// <summary>
        /// Obtains the keyframes for each animation in the storyboard so that
        /// they can be updated when required.
        /// </summary>
        private void GetKeyFramesFromStoryboard()
        {
            _sb = (Storyboard)Resources["FluidStoryboard"];
            if (_sb == null)
            {
                return;
            }

            foreach (var timeline in _sb.Children)
            {
                if (!(timeline is DoubleAnimationUsingKeyFrames dakeys))
                {
                    continue;
                }

                var targetName = Storyboard.GetTargetName(dakeys);
                ProcessDoubleAnimationWithKeys(dakeys, !targetName.StartsWith("Trans"));
            }
        }

        /// <summary>
        /// Gets the keyframes in the given animation and stores them in a map
        /// </summary>
        /// <param name="dakeys">Animation containg keyframes</param>
        /// <param name="isOpacityAnim">Flag to indicate whether the animation targets the opacity or the translate transform</param>
        private void ProcessDoubleAnimationWithKeys(DoubleAnimationUsingKeyFrames dakeys, bool isOpacityAnim = false)
        {
            // Get all the keyframes in the instance.
            for (var i = 0; i < dakeys.KeyFrames.Count; i++)
            {
                var frame = dakeys.KeyFrames[i];

                Dictionary<int, KeyFrameDetails> targetMap = null;

                targetMap = isOpacityAnim ? _opKeyFrameMap : _keyFrameMap;

                if (!targetMap.ContainsKey(i))
                {
                    targetMap[i] = new KeyFrameDetails() { KeyFrames = new List<DoubleKeyFrame>() };
                }

                // Update the keyframe time and add it to the map
                targetMap[i].KeyFrameTime = frame.KeyTime;
                targetMap[i].KeyFrames.Add(frame);
            }
        }

        /// <summary>
        /// Update the key value of each keyframe based on the current width of the FluidProgressBar
        /// </summary>
        private void UpdateKeyFrames()
        {
            // Get the current width of the FluidProgressBar
            var width = ActualWidth;
            // Update the values only if the current width is greater than Zero and is visible
            if ((width > 0.0) && (Visibility == Visibility.Visible))
            {
                var point0 = -10;
                var pointA = width * KeyFrameA;
                var pointB = width * KeyFrameB;
                var pointC = width + 10;

                // Update the keyframes stored in the map
                UpdateKeyFrame(0, point0);
                UpdateKeyFrame(1, pointA);
                UpdateKeyFrame(2, pointB);
                UpdateKeyFrame(3, pointC);
            }
        }

        /// <summary>
        /// Update the key value of the keyframes stored in the map
        /// </summary>
        /// <param name="key">Key of the dictionary</param>
        /// <param name="newValue">New value to be given to the key value of the keyframes</param>
        private void UpdateKeyFrame(int key, double newValue)
        {
            if (!_keyFrameMap.ContainsKey(key))
            {
                return;
            }

            foreach (var frame in _keyFrameMap[key].KeyFrames.Where(frame => frame is LinearDoubleKeyFrame || frame is EasingDoubleKeyFrame))
            {
                frame.SetCurrentValue(DoubleKeyFrame.ValueProperty, newValue);
            }
        }

        /// <summary>
        /// Updates the duration of each of the keyframes stored in the map
        /// </summary>
        /// <param name="key">Key of the dictionary</param>
        /// <param name="newDuration">The new duration.</param>
        private void UpdateKeyTimes(int key, Duration newDuration)
        {
            switch (key)
            {
                case 1:
                    UpdateKeyTime(1, newDuration);
                    UpdateKeyTime(2, newDuration + DurationB);
                    UpdateKeyTime(3, newDuration + DurationB + DurationC);
                    break;

                case 2:
                    UpdateKeyTime(2, DurationA + newDuration);
                    UpdateKeyTime(3, DurationA + newDuration + DurationC);
                    break;

                case 3:
                    UpdateKeyTime(3, DurationA + DurationB + newDuration);
                    break;

                default:
                    break;
            }

            // Update the opacity animation duration based on the complete duration
            // of the animation
            UpdateOpacityKeyTime(1, DurationA + DurationB + DurationC);
        }

        /// <summary>
        /// Updates the duration of each of the keyframes stored in the map
        /// </summary>
        /// <param name="key">Key of the dictionary</param>
        /// <param name="newDuration">New value to be given to the duration value of the keyframes</param>
        private void UpdateKeyTime(int key, Duration newDuration)
        {
            if (!_keyFrameMap.ContainsKey(key))
            {
                return;
            }

            var newKeyTime = KeyTime.FromTimeSpan(newDuration.TimeSpan);
            _keyFrameMap[key].KeyFrameTime = newKeyTime;

            foreach (var frame in _keyFrameMap[key].KeyFrames.Where(frame => frame is LinearDoubleKeyFrame || frame is EasingDoubleKeyFrame))
            {
                frame.SetCurrentValue(DoubleKeyFrame.KeyTimeProperty, newKeyTime);
            }
        }

        /// <summary>
        /// Updates the duration of the second keyframe of all the opacity animations
        /// </summary>
        /// <param name="key">Key of the dictionary</param>
        /// <param name="newDuration">New value to be given to the duration value of the keyframes</param>
        private void UpdateOpacityKeyTime(int key, Duration newDuration)
        {
            if (!_opKeyFrameMap.ContainsKey(key))
            {
                return;
            }

            var newKeyTime = KeyTime.FromTimeSpan(newDuration.TimeSpan);
            _opKeyFrameMap[key].KeyFrameTime = newKeyTime;

            foreach (var frame in _opKeyFrameMap[key].KeyFrames.OfType<DiscreteDoubleKeyFrame>())
            {
                frame.SetCurrentValue(DoubleKeyFrame.KeyTimeProperty, newKeyTime);
            }
        }

        /// <summary>
        /// Updates the delay between consecutive timelines
        /// </summary>
        /// <param name="newDelay">Delay duration</param>
        private void UpdateTimelineDelay(Duration newDelay)
        {
            var nextDelay = new Duration(TimeSpan.FromSeconds(0));

            if (_sb == null)
            {
                return;
            }

            for (var i = 0; i < _sb.Children.Count; i++)
            {
                // The first five animations are for translation
                // The next five animations are for opacity
                if (i == 5)
                {
                    nextDelay = newDelay;
                }
                else
                {
                    nextDelay += newDelay;
                }

                if (_sb.Children[i] is DoubleAnimationUsingKeyFrames timeline)
                {
                    timeline.SetCurrentValue(Timeline.BeginTimeProperty, nextDelay.TimeSpan);
                }
            }
        }
        #endregion

        #region IDisposable Implementation
        /// <summary>
        /// Releases all resources used by an instance of the FluidProgressBar class.
        /// </summary>
        /// <remarks>
        /// This method calls the virtual Dispose(bool) method, passing in 'true', and then suppresses 
        /// finalization of the instance.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged resources before an instance of the FluidProgressBar class is reclaimed by garbage collection.
        /// </summary>
        /// <remarks>
        /// NOTE: Leave out the finalizer altogether if this class doesn't own unmanaged resources itself, 
        /// but leave the other methods exactly as they are.
        /// This method releases unmanaged resources by calling the virtual Dispose(bool), passing in 'false'.
        /// </remarks>
        ~FluidProgressBar()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases the unmanaged resources used by an instance of the FluidProgressBar class and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">'true' to release both managed and unmanaged resources; 'false' to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            // free managed resources here
            SizeChanged -= OnSizeChanged;
            Loaded -= OnLoaded;
            IsVisibleChanged -= OnIsVisibleChanged;

            // free native resources if there are any.			
        }
        #endregion
    }
}