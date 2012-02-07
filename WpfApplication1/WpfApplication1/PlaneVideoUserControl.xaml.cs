using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for PlaneVideoUserControl.xaml
    /// </summary>
    public partial class PlaneVideoUserControl : UserControl
    {

        #region Data

        private bool isMouseDown = false;
        private double tCurrent = 0.0;
        private double tTarget = 0.0;
        private DateTime lastTick;
        private double videoSeconds = 0.0;

        private bool isVideoClosed = false;

        private bool displayingImage = true;

        #endregion

        #region Properties

        public static readonly DependencyProperty MediaFileProperty = DependencyProperty.Register("MediaFileProperty", typeof(Uri), typeof(PlaneVideoUserControl));

        public Uri MediaFile
        {
            get { return (Uri)GetValue(MediaFileProperty); }
            set { SetValue(MediaFileProperty, value); }
        }

        #endregion

        #region Construction

        public PlaneVideoUserControl()
        {
            InitializeComponent();

            mediaElement.MouseUp += new MouseButtonEventHandler(mediaElement_MouseUp);
            mediaElement.MouseDown += new MouseButtonEventHandler(mediaElement_MouseDown);
            mediaElement.MouseMove += new MouseEventHandler(mediaElement_MouseMove);
            mediaElement.MediaOpened += new RoutedEventHandler(mediaElement_MediaOpened);

            //attach ourselves to rendering event
            System.Windows.Media.CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }

        #endregion

        #region Methods

        private bool IsMovieMediaFile(string filename)
        {
            return filename.EndsWith(".mov")
                || filename.EndsWith(".qt")
                || filename.EndsWith(".avi")
                || filename.EndsWith(".wmv")
                || filename.EndsWith(".mp4")
                ;
        }

        public void UpdateControls()
        {
            displayingImage = !IsMovieMediaFile(MediaFile.ToString());
        }

        public void StartVideo()
        {
            isVideoClosed = false;

            tCurrent = 0.0;
            tTarget = 0.0;

            //for some reason if we set the relative uri it works in the designer but doesn't work when we run the app normally.
            //if we set the full path then it raises an exception when inside the designer
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                mediaElement.Source = MediaFile;
            }
            else
            {
                if (MediaFile != null)
                {
                    Uri n = new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location + @"/../../../" + MediaFile.ToString(), UriKind.Absolute);
                    mediaElement.Source = n;
                }
            }

            //because the length of the video gets loaded when we're told that the media has opened
            videoSeconds = -1.0f;

            //this is needed even if viewing an image
            mediaElement.LoadedBehavior = MediaState.Manual;
            mediaElement.Pause();
        }

        public void CloseVideo()
        {
            isVideoClosed = true;

            if (!displayingImage)
            {
                mediaElement.LoadedBehavior = MediaState.Manual;
                mediaElement.Close();
                isMouseDown = false;
            }
        }

        #endregion

        #region Event handlers

        //this gets called once per frame
        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (isVideoClosed) return;

            if (!displayingImage)
            {
                if (videoSeconds != -1.0 && Math.Abs(tTarget - tCurrent) > 0.005)
                {
                    DateTime now = DateTime.Now;
                    TimeSpan elapsed = now - lastTick;
                    lastTick = now;

                    if (Math.Abs(tTarget - tCurrent) < 0.01)
                    {
                        tCurrent = tTarget;
                    }
                    else
                    {
                        double delta = (tTarget - tCurrent) * elapsed.TotalMilliseconds * 0.005 * WpfApplication1.Properties.Settings.Default.PanoramaCameraRotationInputMultiplier;
                        if (delta > 0.01f) delta = 0.01f;
                        if (delta < -0.01f) delta = -0.01f;
                        tCurrent += delta;
                        if (tCurrent < 0.1) tCurrent = 0.1;
                        if (tCurrent > 0.9) tCurrent = 0.9;
                    }

                    TimeSpan time = TimeSpan.FromSeconds(tCurrent * videoSeconds);
                    mediaElement.Position = time;
                }
            }
        }

        //this gets called when the user clicks on the video
        void mediaElement_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isVideoClosed) return;

            if (!displayingImage)
            {
                isMouseDown = true;

                Point p = e.GetPosition(mediaElement);
                double t = p.X / mediaElement.ActualWidth;
                if (t < 0.1) t = 0.1;
                if (t > 0.9) t = 0.9;
                tTarget = t;
            }
        }

        //this gets called when the user clicks on the video
        void mediaElement_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isVideoClosed) return;

            if (!displayingImage)
            {
                isMouseDown = false;
            }
        }

        void  mediaElement_MouseMove(object sender, MouseEventArgs e)
        {
            if (isVideoClosed) return;

            if (!displayingImage)
            {
                if (isMouseDown)
                {
                    Point p = e.GetPosition(mediaElement);
                    double t = p.X / mediaElement.ActualWidth;
                    if (t < 0.1) t = 0.1;
                    if (t > 0.9) t = 0.9;
                    tTarget = t;
                }
            }
        }

        void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (!displayingImage)
            {
                try
                {
                    videoSeconds = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                    System.Diagnostics.Debug.WriteLine("loaded video");
                }
                catch (Exception)
                {
                }
            }
        }

        #endregion

    }
}
