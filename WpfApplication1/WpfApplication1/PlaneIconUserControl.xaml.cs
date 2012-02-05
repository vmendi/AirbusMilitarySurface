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
using System.Windows.Media.Media3D;
using System.Text.RegularExpressions;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for PlaneIconUserControl.xaml
    /// </summary>
    public partial class PlaneIconUserControl : UserControl
    {

        #region Properties

        public static readonly DependencyProperty PlaneTypeIdProperty = DependencyProperty.Register("PlaneTypeIdProperty", typeof(int), typeof(PlaneIconUserControl));

        public int PlaneTypeId
        {
            get { return (int)GetValue(PlaneTypeIdProperty); }
            set { SetValue(PlaneTypeIdProperty, value); }
        }

        public static readonly DependencyProperty IsPlaneTypeControlProperty = DependencyProperty.Register("IsPlaneTypeControlProperty", typeof(bool), typeof(PlaneIconUserControl));

        public bool IsPlaneTypeControl
        {
            get { return (bool)GetValue(IsPlaneTypeControlProperty); }
            set { SetValue(IsPlaneTypeControlProperty, value); }
        }

        public static readonly DependencyProperty LatitudeProperty = DependencyProperty.Register("LatitudeProperty", typeof(string), typeof(PlaneIconUserControl));

        public string Latitude
        {
            get { return (string)GetValue(LatitudeProperty); }
            set { SetValue(LatitudeProperty, value); }
        }

        public static readonly DependencyProperty LongitudeProperty = DependencyProperty.Register("LongitudeProperty", typeof(string), typeof(PlaneIconUserControl));

        public string Longitude
        {
            get { return (string)GetValue(LongitudeProperty); }
            set { SetValue(LongitudeProperty, value); }
        }

        //

        public static readonly DependencyProperty PopupTitleTextProperty = DependencyProperty.Register("PopupTitleTextProperty", typeof(String), typeof(PlaneIconUserControl));

        public String PopupTitleText
        {
            get { return (String)GetValue(PopupTitleTextProperty); }
            set { SetValue(PopupTitleTextProperty, value); }
        }

        public static readonly DependencyProperty PopupSubtitleTextProperty = DependencyProperty.Register("PopupubtitleTextProperty", typeof(String), typeof(PlaneIconUserControl));

        public String PopupSubtitleText
        {
            get { return (String)GetValue(PopupSubtitleTextProperty); }
            set { SetValue(PopupSubtitleTextProperty, value); }
        }

        //

        public static readonly DependencyProperty PopupCardImageProperty = DependencyProperty.Register("PopupCardImageProperty", typeof(ImageSource), typeof(PlaneIconUserControl));

        public ImageSource PopupCardImage
        {
            get { return (ImageSource)GetValue(PopupCardImageProperty); }
            set { SetValue(PopupCardImageProperty, value); }
        }

        //

        public static readonly DependencyProperty PopupInfoTitleTextProperty = DependencyProperty.Register("PopupInfoTitleTextProperty", typeof(String), typeof(PlaneIconUserControl));

        public string PopupInfoTitleText
        {
            get { return (string)GetValue(PopupInfoTitleTextProperty); }
            set { SetValue(PopupInfoTitleTextProperty, value); }
        }

        public static readonly DependencyProperty PopupInfoDescriptionTextProperty = DependencyProperty.Register("PopupDescriptionTextProperty", typeof(String), typeof(PlaneIconUserControl));

        public string PopupInfoDescriptionText
        {
            get { return (string)GetValue(PopupInfoDescriptionTextProperty); }
            set { SetValue(PopupInfoDescriptionTextProperty, value); }
        }

        public static readonly DependencyProperty PopupInfoMediaFileProperty = DependencyProperty.Register("PopupMediaFileProperty", typeof(Uri), typeof(PlaneIconUserControl));

        public Uri PopupInfoMediaFile
        {
            get { return (Uri)GetValue(PopupInfoMediaFileProperty); }
            set { SetValue(PopupInfoMediaFileProperty, value); }
        }

        //

        public static readonly DependencyProperty PopupPanoramaImageProperty = DependencyProperty.Register("PopupPanoramaImageProperty", typeof(ImageSource), typeof(PlaneIconUserControl));

        public ImageSource PopupPanoramaImage
        {
            get { return (ImageSource)GetValue(PopupPanoramaImageProperty); }
            set { SetValue(PopupPanoramaImageProperty, value); }
        }

        //

        public static readonly DependencyProperty PopupVideoMediaFileProperty = DependencyProperty.Register("PopupVideoMediaFileProperty", typeof(Uri), typeof(PlaneIconUserControl));

        public Uri PopupVideoMediaFile
        {
            get { return (Uri)GetValue(PopupVideoMediaFileProperty); }
            set { SetValue(PopupVideoMediaFileProperty, value); }
        }

        #endregion

        #region Construction

        public PlaneIconUserControl()
        {
            InitializeComponent();

            Loaded += new RoutedEventHandler(PlaneIconUserControl_Loaded);
        }

        #endregion

        #region Methods

        public void FillData(PlanePopupUserControl control)
        {
            control.TitleText = PopupTitleText;
            control.SubtitleText = PopupSubtitleText;
            control.CardImage = PopupCardImage;
            control.InfoTitleText = PopupInfoTitleText;
            control.InfoDescriptionText = PopupInfoDescriptionText;
            control.InfoMediaFile = PopupInfoMediaFile;
            control.PanoramaImage = PopupPanoramaImage;
            control.VideoMediaFile = PopupVideoMediaFile;
        }

        public Point3D GetPosition()
        {
            //try to read latitude and longitude from the tag
            float sphere3dRadius = WpfApplication1.Properties.Settings.Default.WorldRadius;
            Point3D spherical = new Point3D(-1.0, -1.0, -1.0);
            Regex latitudeRegex = new Regex(@"\b(?<latitudeDegrees>\d+)\s(?<latitudeMinutes>\d+)\s(?<latitudeSign>[N,S])\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Regex longitudeRegex = new Regex(@"\b(?<longitudeDegrees>\d+)\s(?<longitudeMinutes>\d+)\s(?<longitudeSign>[W,E])\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            try
            {
                //read latitude
                Match match = latitudeRegex.Match(Latitude);
                string res = match.Result("${latitudeDegrees}");
                int latitudeDegrees = int.Parse(res);
                res = match.Result("${latitudeMinutes}");
                int latitudeMinutes = int.Parse(res);
                res = match.Result("${latitudeSign}");
                int latitudeSign = res == "N" ? 1 : -1;
                double totalLatitude = latitudeSign * (latitudeDegrees + latitudeMinutes / 60.0);

                //read longitude
                match = longitudeRegex.Match(Longitude);
                res = match.Result("${longitudeDegrees}");
                int longitudeDegrees = int.Parse(res);
                res = match.Result("${longitudeMinutes}");
                int longitudeMinutes = int.Parse(res);
                res = match.Result("${longitudeSign}");
                int longitudeSign = res == "W" ? 1 : -1;
                double totalLongitude = longitudeSign * (longitudeDegrees + longitudeMinutes / 60.0);

                //convert spherical coordinates into cartesian for easy projection
                double theta = totalLongitude * Math.PI / 180.0 + Math.PI * 0.5;
                double phi = Math.PI / 2 - totalLatitude * Math.PI / 180.0;
                Vector polar = new Vector(theta, phi);
                spherical = new Point3D(
                    sphere3dRadius * Math.Sin(polar.Y) * Math.Cos(polar.X),
                    sphere3dRadius * Math.Cos(polar.Y),
                    sphere3dRadius * Math.Sin(polar.Y) * Math.Sin(polar.X)
                    );
            }
            catch (Exception)
            {
                return new Point3D(0.0, 0.0, 0.0);
            }

            return spherical;
        }

        //hides all the children of an object except the one with a particular name (non recursive)
        private static void HideAllChildrenExcept(DependencyObject dependencyObject, string name)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);
                if (child != null)
                {
                    if (((FrameworkElement)child).Name == name) ((FrameworkElement)child).Visibility = System.Windows.Visibility.Visible;
                    else ((FrameworkElement)child).Visibility = System.Windows.Visibility.Hidden;
                }
            }
        }

        protected static FrameworkElement FindChildWithName(DependencyObject dependencyObject, string name)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);
                if (child != null)
                {
                    if (((FrameworkElement)child).Name == name) return (FrameworkElement)child;

                    FrameworkElement found = FindChildWithName(child, name);
                    if (found != null) return found;
                }
            }
            return null;
        }

        #endregion

        #region Event handlers

        void PlaneIconUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //hide all mission icons except the one corresponding to the assigned plane type
            HideAllChildrenExcept(FindChildWithName(this, "grid"), "planeIcon" + PlaneTypeId);
        }

        #endregion

    }
}
