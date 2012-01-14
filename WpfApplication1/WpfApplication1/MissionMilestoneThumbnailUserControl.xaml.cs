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
    /// Interaction logic for MissionMilestoneThumbnailUserControl.xaml
    /// </summary>
    public partial class MissionMilestoneThumbnailUserControl : UserControl
    {
        public static readonly DependencyProperty IdProperty = DependencyProperty.Register("IdProperty", typeof(int), typeof(MissionMilestoneThumbnailUserControl));

        public int Id
        {
            get { return (int)GetValue(IdProperty); }
            set
            {
                SetValue(IdProperty, value);
                id.Text = value.ToString();
            }
        }

        public static readonly DependencyProperty ThumbDescriptionProperty = DependencyProperty.Register("ThumbDescriptionProperty", typeof(String), typeof(MissionMilestoneThumbnailUserControl));

        public string ThumbDescription
        {
            get { return (string)GetValue(ThumbDescriptionProperty); }
            set
            {
                SetValue(ThumbDescriptionProperty, value);
                description.Text = value;
            }
        }

        public static readonly DependencyProperty ThumbTypeProperty = DependencyProperty.Register("ThumbTypeProperty", typeof(String), typeof(MissionMilestoneThumbnailUserControl));

        public string ThumbType
        {
            get { return (string)GetValue(ThumbTypeProperty); }
            set { SetValue(ThumbTypeProperty, value); }
        }

        public static readonly DependencyProperty ThumbImageProperty = DependencyProperty.Register("ThumbImageProperty", typeof(ImageSource), typeof(MissionMilestoneThumbnailUserControl));

        public ImageSource ThumbImage
        {
            get { return (ImageSource)GetValue(ThumbImageProperty); }
            set
            {
                SetValue(ThumbImageProperty, value);
                thumbnail.Source = value;
            }
        }

        public static readonly DependencyProperty PopupTitleProperty = DependencyProperty.Register("PopupTitleProperty", typeof(string), typeof(MissionMilestoneThumbnailUserControl));

        public string PopupTitle
        {
            get { return (string)GetValue(PopupTitleProperty); }
            set { SetValue(PopupTitleProperty, value); }
        }

        public static readonly DependencyProperty PopupSubtitleProperty = DependencyProperty.Register("PopupSubtitleProperty", typeof(string), typeof(MissionMilestoneThumbnailUserControl));

        public string PopupSubtitle
        {
            get { return (string)GetValue(PopupSubtitleProperty); }
            set { SetValue(PopupSubtitleProperty, value); }
        }

        public static readonly DependencyProperty PopupMediaFileProperty = DependencyProperty.Register("PopupMediaFileProperty", typeof(Uri), typeof(MissionMilestoneThumbnailUserControl));

        public Uri PopupMediaFile
        {
            get { return (Uri)GetValue(PopupMediaFileProperty); }
            set { SetValue(PopupMediaFileProperty, value); }
        }

        public static readonly DependencyProperty PopupDescriptionHeadingProperty = DependencyProperty.Register("PopupDescriptionHeadingProperty", typeof(string), typeof(MissionMilestoneThumbnailUserControl));

        public String PopupDescriptionHeading
        {
            get { return (String)GetValue(PopupDescriptionHeadingProperty); }
            set { SetValue(PopupDescriptionHeadingProperty, value); }
        }

        public static readonly DependencyProperty PopupDescriptionBodyProperty = DependencyProperty.Register("PopupDescriptionBodyProperty", typeof(string), typeof(MissionMilestoneThumbnailUserControl));

        public String PopupDescriptionBody
        {
            get { return (String)GetValue(PopupDescriptionBodyProperty); }
            set { SetValue(PopupDescriptionBodyProperty, value); }
        }

        public static readonly DependencyProperty ReverseIdProperty = DependencyProperty.Register("ReverseIdProperty", typeof(bool), typeof(MissionMilestoneThumbnailUserControl));

        public bool ReverseId
        {
            get { return (bool)GetValue(ReverseIdProperty); }
            set
            {
                SetValue(ReverseIdProperty, value);
                if (value)
                {
                    UIElement id = stackPanel.Children[0];                    
                    stackPanel.Children.RemoveAt(0);
                    stackPanel.Children.Add(id);
                }
            }
        }

        public MissionMilestoneThumbnailUserControl()
        {
            InitializeComponent();
        }
    }
}
