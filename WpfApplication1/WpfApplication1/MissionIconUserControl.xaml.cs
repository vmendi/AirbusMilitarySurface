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
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MissionIconUserControl.xaml
    /// </summary>
    public partial class MissionIconUserControl : UserControl
    {

        #region Data

        private bool isChecked = true;
        private bool checkBehaviour = false;

        #endregion

        #region Properties

        public bool IsChecked
        { get { return isChecked; } }

        public bool CheckBehaviour
        {
            get { return checkBehaviour; }
            set { checkBehaviour = value; }
        }

        //

        //main image shown in the control
        public static readonly DependencyProperty IconImageProperty = DependencyProperty.Register("IconImageProperty", typeof(ImageSource), typeof(MissionIconUserControl));

        public ImageSource IconImage
        {
            get { return (ImageSource)GetValue(IconImageProperty); }
            set
            {
                SetValue(IconImageProperty, value);
                image.Source = value;
            }
        }

        //

        //id of the Mission, this is used to link us to the right MissionUserControl inside MainWindow.xaml
        //for this reason, each one of these ids should be different.
        //this isn't used when the control represents a *type* of mission
        public static readonly DependencyProperty MissionIdProperty = DependencyProperty.Register("MissionIdProperty", typeof(int), typeof(MissionIconUserControl));

        public int MissionId
        {
            get { return (int)GetValue(MissionIdProperty); }
            set { SetValue(MissionIdProperty, value); }
        }

        //id of the type of our Mission, used when showing or hiding Missions by the id of the type
        public static readonly DependencyProperty MissionTypeIdProperty = DependencyProperty.Register("MissionTypeIdProperty", typeof(int), typeof(MissionIconUserControl));

        public int MissionTypeId
        {
            get { return (int)GetValue(MissionTypeIdProperty); }
            set { SetValue(MissionTypeIdProperty, value); }
        }

        //tells us if the control represents a type of Mission or an actual Mission on the world map
        public static readonly DependencyProperty IsMissionTypeControlProperty = DependencyProperty.Register("IsMissionTypeControlProperty", typeof(bool), typeof(MissionIconUserControl));

        public bool IsMissionTypeControl
        {
            get { return (bool)GetValue(IsMissionTypeControlProperty); }
            set { SetValue(IsMissionTypeControlProperty, value); }
        }

        //latitude of the Mission on the world map (if representing an actual Mission)
        public static readonly DependencyProperty LatitudeProperty = DependencyProperty.Register("LatitudeProperty", typeof(string), typeof(MissionIconUserControl));

        public string Latitude
        {
            get { return (string)GetValue(LatitudeProperty); }
            set { SetValue(LatitudeProperty, value); }
        }

        //longitude of the Mission on the world map (if representing an actual Mission)
        public static readonly DependencyProperty LongitudeProperty = DependencyProperty.Register("LongitudeProperty", typeof(string), typeof(MissionIconUserControl));

        public string Longitude
        {
            get { return (string)GetValue(LongitudeProperty); }
            set { SetValue(LongitudeProperty, value); }
        }

        //

        //main image shown in the control
        public static readonly DependencyProperty PanelIconImageProperty = DependencyProperty.Register("PanelIconImageProperty", typeof(ImageSource), typeof(MissionIconUserControl));

        public ImageSource PanelIconImage
        {
            get { return (ImageSource)GetValue(PanelIconImageProperty); }
            set { SetValue(PanelIconImageProperty, value); }
        }

        //short description that appears on the panel that pops out when the user clicks on it
        public static readonly DependencyProperty PanelDescriptionProperty = DependencyProperty.Register("PanelDescriptionProperty", typeof(string), typeof(MissionIconUserControl));

        public string PanelDescription
        {
            get { return (string)GetValue(PanelDescriptionProperty); }
            set { SetValue(PanelDescriptionProperty, value); }
        }

        #endregion

        #region Events

        // delegate declaration 
        public delegate void CheckedHandler(object sender);

        // event declaration 
        public event CheckedHandler Checked;

        // delegate declaration 
        public delegate void UncheckedHandler(object sender);

        // event declaration 
        public event UncheckedHandler Unchecked;

        #endregion

        #region Construction

        public MissionIconUserControl()
        {
            InitializeComponent();

            ((Storyboard)Resources["check"]).Completed += new EventHandler(check_Completed);
            ((Storyboard)Resources["uncheck"]).Completed += new EventHandler(uncheck_Completed);
        }

        #endregion

        #region Methods

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (checkBehaviour)
            {
                if (IsChecked) Uncheck();
                else Check();
            }
        }

        public void Check()
        {
            if (isChecked || !checkBehaviour) return;
            ((Storyboard)Resources["check"]).Begin();
            isChecked = true;
        }

        public void Uncheck()
        {
            if (!isChecked || !checkBehaviour) return;
            ((Storyboard)Resources["uncheck"]).Begin();
            isChecked = false;
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

        #endregion

        #region Event handlers

        void uncheck_Completed(object sender, EventArgs e)
        {
            if (Unchecked != null) Unchecked(this);
        }

        void check_Completed(object sender, EventArgs e)
        {
            if (Checked != null) Checked(this);
        }

        #endregion


    }
}
