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
        public MissionMilestoneUserControl()
        {
            InitializeComponent();

            background.MouseDown += new MouseButtonEventHandler(background_MouseDown);
        }

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

        public void StartInStoryboard(string title, string longDescription, string[] mediaFiles)
        {
            Storyboard storyboard = (Storyboard)FindResource("in");
            storyboard.Begin();

            titleText.Text = title;
            descriptionText.Text = longDescription;

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
        }

        public void StartOutStoryboard()
        {
            Storyboard storyboard = (Storyboard)FindResource("out");
            storyboard.Begin();
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

        #endregion

    }
}
