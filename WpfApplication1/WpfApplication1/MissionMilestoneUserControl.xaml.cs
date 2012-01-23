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
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Surface.Presentation.Controls;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MissionMilestoneUserControl.xaml
    /// </summary>
    public partial class MissionMilestoneUserControl : UserControl
    {

        #region Classes

        public class BackEventArgs
        {

            #region Construction

            public BackEventArgs()
            {
            }

            #endregion

        }

        #endregion

        #region Data

        private bool isMediaElementPlaying = false;

        #endregion

        #region Events

        // delegate declaration 
        public delegate void BackEventHandler(object sender, BackEventArgs e);

        // event declaration 
        public event BackEventHandler BackEvent;

        #endregion

        #region Construction

        public MissionMilestoneUserControl()
        {
            InitializeComponent();

            background.MouseDown += new MouseButtonEventHandler(background_MouseDown);
            mediaElement.MediaEnded += new RoutedEventHandler(mediaElement_MediaEnded);
            mediaElement.MouseDown += new MouseButtonEventHandler(mediaElement_MouseDown);
            playVideoButton.MouseDown += new MouseButtonEventHandler(playVideoButton_MouseDown);
        }

        #endregion

        #region Methods

        private bool IsImageMediaFile(string filename)
        {
            return filename.EndsWith(".png")
                || filename.EndsWith(".jpg")
                || filename.EndsWith(".jpeg")
                || filename.EndsWith(".gif")
                || filename.EndsWith(".tga");
        }

        private bool IsMovieMediaFile(string filename)
        {
            return filename.EndsWith(".mov")
                || filename.EndsWith(".qt")
                || filename.EndsWith(".avi")
                || filename.EndsWith(".wmv");
        }

        public void StartInStoryboard(string title, string subtitle, string mediaFile, string descriptionHeading, string descriptionBody)
        {
            Storyboard storyboard = (Storyboard)FindResource("in");
            storyboard.Begin();

            titleText.Text = title;
            subtitleText.Text = subtitle;
            descriptionHeadingText.Text = descriptionHeading;
            descriptionBodyText.Text = descriptionBody;

            mediaElement.Source = new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location + "/../../../" + mediaFile);
            mediaElement.LoadedBehavior = MediaState.Manual;
            mediaElement.Position = new TimeSpan(0, 0, 0, 0);
            mediaElement.Stop();
            isMediaElementPlaying = false;

            playVideoButton.Visibility = System.Windows.Visibility.Visible;

            //TODO
            /*
            scatterView.Items.Clear();
            for (int iMediaFile = 0; iMediaFile < mediaFiles.Length; iMediaFile++)
            {
                if (IsImageMediaFile(mediaFiles[iMediaFile]))
                {
                    ScatterViewItem item = new ScatterViewItem();
                    Image img = new Image();
                    img.Source = new BitmapImage(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location + "/../../../" + mediaFiles[iMediaFile]));
                    scatterView.Items.Add(img);
                }
                else if (IsMovieMediaFile(mediaFiles[iMediaFile]))
                {
                    ScatterViewItem item = new ScatterViewItem();
                    MediaElement mediaElement = new MediaElement();
                    mediaElement.Source = new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location + "/../../../" + mediaFiles[iMediaFile]);
                    scatterView.Items.Add(mediaElement);
                }
            }
            */
        }

        public void StartOutStoryboard()
        {
            Storyboard storyboard = (Storyboard)FindResource("out");
            storyboard.Begin();

            if (BackEvent != null)
            {
                BackEvent(this, new BackEventArgs());
            }

            mediaElement.LoadedBehavior = MediaState.Manual;
            mediaElement.Stop();
        }

        #endregion

        #region Event handlers

        void background_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StartOutStoryboard();
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            StartOutStoryboard();
        }

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
