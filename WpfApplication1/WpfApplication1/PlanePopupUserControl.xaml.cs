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

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for PlanePopupUserControl.xaml
    /// </summary>
    public partial class PlanePopupUserControl : UserControl
    {

        #region Classes

        private enum Content
        {
            Info,
            Card,
            Video,
            Panorama
        }

        public class HidingEventArgs
        {

            #region Data

            private object tag;

            #endregion

            #region Properties

            public object Tag
            { get { return tag; } }

            #endregion

            #region Construction

            public HidingEventArgs(object tag)
            {
                this.tag = tag;
            }

            #endregion

        }

        #endregion

        #region Properties

        public static readonly DependencyProperty TitleTextProperty = DependencyProperty.Register("TitleTextProperty", typeof(String), typeof(PlanePopupUserControl));

        public String TitleText
        {
            get { return (String)GetValue(TitleTextProperty); }
            set
            {
                SetValue(TitleTextProperty, value);
                titleText.Text = value;
            }
        }

        public static readonly DependencyProperty SubtitleTextProperty = DependencyProperty.Register("SubtitleTextProperty", typeof(String), typeof(PlanePopupUserControl));

        public String SubtitleText
        {
            get { return (String)GetValue(SubtitleTextProperty); }
            set
            {
                SetValue(SubtitleTextProperty, value);
                subtitleText.Text = value;
            }
        }

        //

        public static readonly DependencyProperty CardImageProperty = DependencyProperty.Register("CardImageProperty", typeof(ImageSource), typeof(PlanePopupUserControl));

        public ImageSource CardImage
        {
            get { return (ImageSource)GetValue(CardImageProperty); }
            set
            {
                SetValue(CardImageProperty, value);
                card.CardImage = value;
            }
        }

        //

        public static readonly DependencyProperty InfoTitleTextProperty = DependencyProperty.Register("InfoTitleTextProperty", typeof(String), typeof(PlanePopupUserControl));

        public string InfoTitleText
        {
            get { return (string)GetValue(InfoTitleTextProperty); }
            set
            {
                SetValue(InfoTitleTextProperty, value);
                info.TitleText = value;
            }
        }

        public static readonly DependencyProperty InfoDescriptionTextProperty = DependencyProperty.Register("DescriptionTextProperty", typeof(String), typeof(PlanePopupUserControl));

        public string InfoDescriptionText
        {
            get { return (string)GetValue(InfoDescriptionTextProperty); }
            set
            {
                SetValue(InfoDescriptionTextProperty, value);
                info.DescriptionText = value;
            }
        }

        public static readonly DependencyProperty InfoMediaFileProperty = DependencyProperty.Register("MediaFileProperty", typeof(Uri), typeof(PlanePopupUserControl));

        public Uri InfoMediaFile
        {
            get { return (Uri)GetValue(InfoMediaFileProperty); }
            set
            {
                SetValue(InfoMediaFileProperty, value);
                info.MediaFile = value;
            }
        }

        //

        public static readonly DependencyProperty PanoramaImageProperty = DependencyProperty.Register("PanoramaImageProperty", typeof(ImageSource), typeof(PlanePopupUserControl));

        public ImageSource PanoramaImage
        {
            get { return (ImageSource)GetValue(PanoramaImageProperty); }
            set
            {
                SetValue(PanoramaImageProperty, value);
                panorama.PanoramaImage = value;
            }
        }

        //

        public static readonly DependencyProperty VideoMediaFileProperty = DependencyProperty.Register("VideoMediaFileProperty", typeof(Uri), typeof(PlanePopupUserControl));

        public Uri VideoMediaFile
        {
            get { return (Uri)GetValue(VideoMediaFileProperty); }
            set
            {
                SetValue(VideoMediaFileProperty, value);
                video.MediaFile = value;
            }
        }

        #endregion

        #region Events

        // delegate declaration 
        public delegate void HidingEventHandler(object sender, HidingEventArgs e);

        // event declaration 
        public event HidingEventHandler HidingEvent;

        #endregion

        #region Data

        private Content displayingContent = Content.Info;
        private object showHideEventTag = null;

        #endregion

        #region Construction

        public PlanePopupUserControl()
        {
            InitializeComponent();

            ((Storyboard)Resources["hideInfo"]).Completed += new EventHandler(hideInfo_Completed);
            ((Storyboard)Resources["hideVideo"]).Completed += new EventHandler(hideVideo_Completed);

            card.Visibility = System.Windows.Visibility.Hidden;
            video.Visibility = System.Windows.Visibility.Hidden;
            panorama.Visibility = System.Windows.Visibility.Hidden;

            background.MouseDown += new MouseButtonEventHandler(background_MouseDown);
        }

        #endregion

        #region Methods

        public void Show(object tag)
        {
            ((Storyboard)Resources["show"]).Begin();
            displayingContent = Content.Info;
            showHideEventTag = tag;
        }

        public void Hide()
        {
            ((Storyboard)Resources["hide"]).Begin();
            video.CloseVideo();
            info.StopVideo();

            //tell listeners that we're hiding.  we pass the tag so the listener
            //that showed us can tell that it should unblur itself or whatever
            if (HidingEvent != null)
            {
                HidingEvent(this, new HidingEventArgs(showHideEventTag));
            }
        }

        #endregion

        #region Event handlers

        //this gets called when the background rectangle is clicked on
        void background_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Hide();
        }

        private void infoButton_Click(object sender, RoutedEventArgs e)
        {
            if (displayingContent != Content.Info)
            {
                ((Storyboard)Resources["showInfo"]).Begin();
                ((Storyboard)Resources["hideCard"]).Begin();
                ((Storyboard)Resources["hideVideo"]).Begin();
                ((Storyboard)Resources["hidePanorama"]).Begin();
                displayingContent = Content.Info;
            }
        }

        private void fichaButton_Click(object sender, RoutedEventArgs e)
        {
            if (displayingContent != Content.Card)
            {
                ((Storyboard)Resources["hideInfo"]).Begin();
                ((Storyboard)Resources["showCard"]).Begin();
                ((Storyboard)Resources["hideVideo"]).Begin();
                ((Storyboard)Resources["hidePanorama"]).Begin();
                displayingContent = Content.Card;
            }
        }

        private void videoButton_Click(object sender, RoutedEventArgs e)
        {
            if (displayingContent != Content.Video)
            {
                ((Storyboard)Resources["hideInfo"]).Begin();
                ((Storyboard)Resources["hideCard"]).Begin();
                ((Storyboard)Resources["showVideo"]).Begin();
                ((Storyboard)Resources["hidePanorama"]).Begin();
                displayingContent = Content.Video;
            }
            video.CloseVideo();
            video.StartVideo();
        }

        private void hatchButton_Click(object sender, RoutedEventArgs e)
        {
            if (displayingContent != Content.Panorama)
            {
                ((Storyboard)Resources["hideInfo"]).Begin();
                ((Storyboard)Resources["hideCard"]).Begin();
                ((Storyboard)Resources["hideVideo"]).Begin();
                ((Storyboard)Resources["showPanorama"]).Begin();
                displayingContent = Content.Panorama;

                System.Diagnostics.Trace.WriteLine("hatch");
            }
        }

        //this gets called when the "hide info" animation completes
        void hideInfo_Completed(object sender, EventArgs e)
        {
            info.StopVideo();
        }

        //this gets called when the "hide video" animation completes
        void hideVideo_Completed(object sender, EventArgs e)
        {
            video.CloseVideo();
        }

        //this gets called when the close button is clicked
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        #endregion

    }
}
