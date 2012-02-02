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
    /// Interaction logic for PlaneInfoUserControl.xaml
    /// </summary>
    public partial class PlaneInfoUserControl : UserControl
    {

        #region Data

        private bool isMediaElementPlaying = false;

        #endregion

        #region Properties

        public static readonly DependencyProperty TitleTextProperty = DependencyProperty.Register("TitleTextProperty", typeof(String), typeof(PlaneInfoUserControl));

        public string TitleText
        {
            get { return (string)GetValue(TitleTextProperty); }
            set
            {
                SetValue(TitleTextProperty, value);
                titleText.Text = value;
            }
        }

        public static readonly DependencyProperty DescriptionTextProperty = DependencyProperty.Register("DescriptionTextProperty", typeof(String), typeof(PlaneInfoUserControl));

        public string DescriptionText
        {
            get { return (string)GetValue(DescriptionTextProperty); }
            set
            {
                SetValue(DescriptionTextProperty, value);
                descriptionText.Text = value;
            }
        }

        public static readonly DependencyProperty MediaFileProperty = DependencyProperty.Register("MediaFileProperty", typeof(Uri), typeof(PlaneInfoUserControl));

        public Uri MediaFile
        {
            get { return (Uri)GetValue(MediaFileProperty); }
            set
            {
                SetValue(MediaFileProperty, value);
                mediaElement.Source = value;
            }
        }

        #endregion

        #region Construction

        public PlaneInfoUserControl()
        {
            InitializeComponent();

            mediaElement.MediaEnded += new RoutedEventHandler(mediaElement_MediaEnded);
            mediaElement.MouseDown += new MouseButtonEventHandler(mediaElement_MouseDown);
            playVideoButton.MouseDown += new MouseButtonEventHandler(playVideoButton_MouseDown);

            mediaElement.LoadedBehavior = MediaState.Manual;
            mediaElement.Position = new TimeSpan(0, 0, 0, 0);
            mediaElement.Stop();
            isMediaElementPlaying = false;
        }

        #endregion

        #region Event handlers

        //this gets called when the video is finished playing
        void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            playVideoButton.Visibility = System.Windows.Visibility.Visible;
            isMediaElementPlaying = false;
            mediaElement.Stop();
            mediaElement.Position = new TimeSpan(0, 0, 0, 0);
        }

        //this gets called when the user clicks on the "play video" button
        void playVideoButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mediaElement.LoadedBehavior = MediaState.Manual;
            mediaElement.Position = new TimeSpan(0, 0, 0, 0);
            mediaElement.Play();
            playVideoButton.Visibility = System.Windows.Visibility.Hidden;
            isMediaElementPlaying = true;
        }

        //this gets called when the user clicks on the video
        void mediaElement_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!isMediaElementPlaying)
            {
                mediaElement.LoadedBehavior = MediaState.Manual;
                mediaElement.Play();
                playVideoButton.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                mediaElement.LoadedBehavior = MediaState.Pause;
                playVideoButton.Visibility = System.Windows.Visibility.Visible;
            }

            isMediaElementPlaying = !isMediaElementPlaying;
        }

        #endregion

    }
}
