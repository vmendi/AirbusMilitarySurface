using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for WorldUserControl.xaml
    /// </summary>
    public partial class WorldUserControl : UserControl
    {

        #region Classes

        //represents the data from a single mission, as loaded from the XAML
        private class MissionInfo
        {

            #region Data

            private Button button;//this is the button on the world map which represents this mission
            private Point3D position;
            private int missionTypeId;
            private int missionIconId;//this is the id of the icon that gets displayed on the panel that appears when we click on a mission button
            private string description;//for the mission panel

            #endregion

            #region Properties

            public Button Button
            {get { return button; }}

            public Point3D Position
            {get { return position; }}

            public int MissionTypeId
            {get { return missionTypeId; }}

            public int MissionIconId
            { get { return missionIconId; } }

            public string Description
            { get { return description; } }

            #endregion

            #region Construction

            public MissionInfo(Button button, Point3D position, int missionTypeId, int missionIconId, string description)
            {
                this.button = button;
                this.position = position;
                this.missionTypeId = missionTypeId;
                this.missionIconId = missionIconId;
                this.description = description;

                //hide all mission icons except the one corresponding to the assigned mission
                HideAllChildrenExcept(FindChildWithName(button, "iconGrid"), "missionIcon" + missionTypeId);
            }

            #endregion

        }

        private class PlaneInfo
        {

            #region Data

            private Button button;//this is the button on the world map which represents this plane
            private Point3D position;
            private int planeTypeId;

            #endregion

            #region Properties

            public Button Button
            { get { return button; } }

            public Point3D Position
            {
                get { return position; }
                set { position = value; }
            }

            public int PlaneTypeId
            { get { return planeTypeId; } }

            #endregion

            #region Construction

            public PlaneInfo(Button button, Point3D position, int planeTypeId)
            {
                this.button = button;
                this.position = position;
                this.planeTypeId = planeTypeId;

                position = new Point3D();

                //hide all mission icons except the one corresponding to the assigned plane type
                HideAllChildrenExcept(FindChildWithName(button, "grid"), "planeIcon" + planeTypeId);
            }

            #endregion

        }

        private class PlaneTypeInfo : PlaneInfo
        {

            #region Data

            private bool isOnWorld;
            private Thickness originalPosition;
            private int originalZIndex;

            #endregion

            #region Properties

            public bool IsOnWorld
            {
                get { return isOnWorld; }
                set { isOnWorld = value; }
            }

            public int OriginalZIndex
            { get { return originalZIndex; } }

            #endregion

            #region Construction

            public PlaneTypeInfo(Button button, int planeTypeId) : base(button, new Point3D(0.0f, 0.0f, 0.0f), planeTypeId)
            {
                isOnWorld = false;

                originalPosition = button.Margin;
                originalZIndex = Grid.GetZIndex(button);
            }

            #endregion

            #region Methods

            public void ResetPosition()
            {
                Button.Margin = originalPosition;
                Button.IsEnabled = true;
                isOnWorld = false;
                Grid.SetZIndex(Button, originalZIndex);
            }

            #endregion

        }

        private enum PlaneTypeButtonStates
        {
            Normal,
            MouseDown,
            Dragging,
            Popup,
        }

        private enum DisplayMode
        {
            Missions,
            Planes,
        }

        #endregion

        #region Data

        //private Dictionary<int, TouchDevice> touchDownPoints = new Dictionary<int, TouchDevice>();
        private DateTime lastTick;

        private bool isMouseDown = false;
        private bool movedOnSphere = false;
        private Vector3D spherePointHit;
        private Point lastMousePos;

        private Vector3D lastSpherePoint = new Vector3D(0.0f, 0.0f, 0.0f);

        Quaternion cameraAngularVelocity = new Quaternion(new Vector3D(0.0f, 1.0f, 0.0f), 0.0f);
        float cameraZoomVelocity = 0.0f;

        //camera settings
        private readonly float cameraAngularVelocityDamp = 1.0f;
        private readonly float cameraZoomVelocityDamp = 1.0f;
        private readonly float cameraRotationInputMultiplier = 1.0f;
        private readonly float cameraZoomInputMultiplier = 1.0f;
        private readonly float cameraZoomMax = 1.0f;
        private readonly float cameraZoomMinRelative = 0.35f;
        private readonly float cameraAngularVelocityMax = 1.0f;
        private readonly float cameraZoomVelocityMax = 1.0f;

        private readonly float sphere3dRadius = 25.0f;

        //information about all the missions in the world map, as loaded from the xaml
        private readonly List<MissionInfo> missionInfos = new List<MissionInfo>();

        //array holding the buttons representing the bottom row of available mission types
        private readonly ToggleButton[] missionTypeToggleButtons;

        //array holding information about the planes on the world map
        private List<PlaneInfo> planeInfos = new List<PlaneInfo>();

        //array holding the buttons representing the bottom row of available plane types
        private PlaneTypeInfo[] planeTypeInfos;

        private PlaneTypeInfo planeTypeInfoButtonMouseDown;
        private DateTime planeTypeInfoButtonMouseDownTime;        
        private Point planeTypeInfoButtonMouseDownPosition;
        private PlaneTypeButtonStates planeTypeButtonState = PlaneTypeButtonStates.Normal;

        private DisplayMode displayMode = DisplayMode.Missions;

        #endregion

        #region Constructor

        public WorldUserControl()
		{
			InitializeComponent();

            //store mission type buttons
            missionTypeToggleButtons = new ToggleButton[5];
            missionTypeToggleButtons[0] = missionType0;
            missionTypeToggleButtons[1] = missionType1;
            missionTypeToggleButtons[2] = missionType2;
            missionTypeToggleButtons[3] = missionType3;
            missionTypeToggleButtons[4] = missionType4;

            //subscribe to the checked/unchecked events in the mission type toggle buttons
            for (int iMissionType = 0; iMissionType < missionTypeToggleButtons.Length; iMissionType++)
            {
                missionTypeToggleButtons[iMissionType].Checked += new RoutedEventHandler(missionTypeToggleButton_Checked);
                missionTypeToggleButtons[iMissionType].PreviewMouseDown += new MouseButtonEventHandler(missionTypeToggleButton_PreviewMouseDown);

                missionTypeToggleButtons[iMissionType].Unchecked += new RoutedEventHandler(missionTypeToggleButton_Unchecked);

                missionTypeToggleButtons[iMissionType].IsChecked = true;
            }

            //read settings
            cameraAngularVelocityDamp = WpfApplication1.Properties.Settings.Default.WorldCameraAngularVelocityDamp;
            cameraZoomVelocityDamp = WpfApplication1.Properties.Settings.Default.WorldCameraZoomVelocityDamp;
            cameraRotationInputMultiplier = WpfApplication1.Properties.Settings.Default.WorldCameraRotationInputMultiplier;
            cameraZoomInputMultiplier = WpfApplication1.Properties.Settings.Default.WorldCameraZoomInputMultiplier;
            cameraZoomMax = WpfApplication1.Properties.Settings.Default.WorldCameraZoomMax;
            cameraZoomMinRelative = WpfApplication1.Properties.Settings.Default.WorldCameraZoomMinRelative;
            cameraAngularVelocityMax = WpfApplication1.Properties.Settings.Default.WorldCameraAngularVelocityMax;
            cameraZoomVelocityMax = WpfApplication1.Properties.Settings.Default.WorldCameraZoomVelocityMax;
            sphere3dRadius = WpfApplication1.Properties.Settings.Default.WorldRadius;

            //get called when the user wants to see the locations of a plane type
            planePopOutUserControl.ShowPlaneLocationsEvent += new PlanePopOutUserControl.ShowPlaneLocationsEventHandler(planePopOutUserControl_ShowPlaneLocationsEvent);

            //attach ourselves to rendering event
            System.Windows.Media.CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);

            Loaded += new RoutedEventHandler(MainWindow_Loaded);
		}

        #endregion

        #region Methods

        //this hides or shows the mission buttons which represent mission types with a particular id
        private void SetVisibilityMissionInfoButtonsWithId(int missionTypeId, bool visible)
        {
            for (int iMissionInfo = 0; iMissionInfo < missionInfos.Count; iMissionInfo++)
            {
                Button button = missionInfos[iMissionInfo].Button;
                if (missionInfos[iMissionInfo].MissionTypeId == missionTypeId)
                {
                    if (!visible)
                    {
                        Storyboard storyboard = (Storyboard)button.Template.Resources["out"];
                        storyboard.Begin((Grid)button.Template.FindName("layoutGrid", button));
                    }
                    else
                    {
                        Storyboard storyboard = (Storyboard)button.Template.Resources["in"];
                        storyboard.Begin((Grid)button.Template.FindName("layoutGrid", button));
                    }
                }
            }
        }

        //this hides or shows the plane buttons which represent plane types with a particular id
        private void ShowPlaneInfoButtonsWithId(int planeTypeId)
        {
            for (int iPlaneInfo = 0; iPlaneInfo < planeInfos.Count; iPlaneInfo++)
            {
                Button button = planeInfos[iPlaneInfo].Button;
                if (planeInfos[iPlaneInfo].PlaneTypeId == planeTypeId)
                {
                    Storyboard storyboard = (Storyboard)button.Template.Resources["fadeIn"];
                    storyboard.Begin((Grid)button.Template.FindName("grid", button));
                }
                else
                {
                    Storyboard storyboard = (Storyboard)button.Template.Resources["fadeOut"];
                    storyboard.Begin((Grid)button.Template.FindName("grid", button));
                }
            }
        }

        //move plane type buttons to their original positions
        private void ResetPlaneTypeInfoButtonPositions()
        {            
            //(check for null because this gets called while initializing the window, before we´ve initialised planeTypeInfos)
            if (planeTypeInfos != null)
            {
                for (int iPlaneTypeInfo = 0; iPlaneTypeInfo < planeTypeInfos.Length; iPlaneTypeInfo++)
                {
                    planeTypeInfos[iPlaneTypeInfo].ResetPosition();
                }
            }
        }

        private void SwitchToDisplayMode(DisplayMode displayMode)
        {
            if (this.displayMode == displayMode) return;

            switch (displayMode)
            {
                case DisplayMode.Missions:

                    //show all mission buttons in the world
                    for (int iMissionInfo = 0; iMissionInfo < missionInfos.Count; iMissionInfo++)
                    {
                        SetVisibilityMissionInfoButtonsWithId(missionInfos[iMissionInfo].MissionTypeId, (bool) missionTypeToggleButtons[missionInfos[iMissionInfo].MissionTypeId].IsChecked);
                    }

                    planePopOutUserControl.PopOut();

                    ResetPlaneTypeInfoButtonPositions();

                    ShowPlaneInfoButtonsWithId(-1);

                    break;

                case DisplayMode.Planes:

                    viewMissionUserControl.HideAnimated();

                    //hide all mission buttons in the world that aren´t already hidden
                    for (int iMissionInfo = 0; iMissionInfo < missionInfos.Count; iMissionInfo++)
                    {
                        if ((bool)missionTypeToggleButtons[missionInfos[iMissionInfo].MissionTypeId].IsChecked)
                        {
                            SetVisibilityMissionInfoButtonsWithId(missionInfos[iMissionInfo].MissionTypeId, false);
                        }
                    }

                    break;
            }

            //advance state
            this.displayMode = displayMode;
        }

        //this returns true if the point is on top of the plane types rectangle (the point must be specified in local rectangle coordinates)
        private bool IsOnPlaneTypesRectangle(Point localP)
        {
            return localP.X >= 0.0 && localP.Y >= 0.0 && localP.X < planeTypesRectangle.ActualWidth && localP.Y < planeTypesRectangle.ActualHeight;
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

        //we use this to decrease the displaced amount when the camera goes up close to the world
        private float CalculateVelocityZoomedMultiplier()
        {
            Vector3D cameraPosition = new Vector3D(myPerspectiveCamera.Position.X, myPerspectiveCamera.Position.Y, myPerspectiveCamera.Position.Z);
            double distance = cameraPosition.LengthSquared;
            return (float)(distance * 0.0001f);
        }

        private Quaternion ClampAngle(Quaternion value, float min, float max)
        {
            if (value.Angle > max) return new Quaternion(value.Axis, max);
            if (value.Angle < min) return new Quaternion(value.Axis, min);
            return value;
        }

        private float Clamp(float value, float min, float max)
        {
            if (value > max) return max;
            else if (value < min) return min;
            return value;
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

        //find the mission type id that is associated to a particular mission ToggleButton
        private int MissionTypeIdByToggleButton(ToggleButton toggleButton)
        {
            for (int iMissionTypeToggleButton = 0; iMissionTypeToggleButton < missionTypeToggleButtons.Length; iMissionTypeToggleButton++)
            {
                if (toggleButton == missionTypeToggleButtons[iMissionTypeToggleButton]) return iMissionTypeToggleButton;
            }

            return -1;
        }

        //find the PlaneTypeInfo that is associated to a particular plane type Button
        private PlaneTypeInfo PlaneTypeInfoByButton(Button button)
        {
            for (int iPlaneTypeInfo = 0; iPlaneTypeInfo < planeTypeInfos.Length; iPlaneTypeInfo++)
            {
                if (button == planeTypeInfos[iPlaneTypeInfo].Button) return planeTypeInfos[iPlaneTypeInfo];
            }

            return null;
        }

        private MissionInfo MissionInfoByButton(Button button)
        {
            for (int iMissionInfo = 0; iMissionInfo < missionInfos.Count; iMissionInfo++)
            {
                if (button == missionInfos[iMissionInfo].Button)
                {
                    return missionInfos[iMissionInfo];
                }
            }

            return null;
        }

        bool ExtractTypeData(Button button, string typeName, out Point3D spherical, out int typeId)
        {
            bool readOk = true;

            //ignore buttons with no string
            if (button.Tag == null || button.Tag.GetType() != typeof(string))
            {
                spherical = new Point3D(-1.0, -1.0f, -1.0f);
                typeId = -1;
                return false;
            }

            //try to read latitude and longitude from the tag
            spherical = new Point3D(-1.0, -1.0, -1.0);
            Regex latitudeRegex = new Regex(@"\blatitude=(?<latitudeDegrees>\d+)\s(?<latitudeMinutes>\d+)\s(?<latitudeSign>[N,S])\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Regex longitudeRegex = new Regex(@"\blongitude=(?<longitudeDegrees>\d+)\s(?<longitudeMinutes>\d+)\s(?<longitudeSign>[W,E])\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            try
            {
                //read latitude
                Match match = latitudeRegex.Match((string)button.Tag);
                string res = match.Result("${latitudeDegrees}");
                int latitudeDegrees = int.Parse(res);
                res = match.Result("${latitudeMinutes}");
                int latitudeMinutes = int.Parse(res);
                res = match.Result("${latitudeSign}");
                int latitudeSign = res == "N" ? 1 : -1;
                double totalLatitude = latitudeSign * (latitudeDegrees + latitudeMinutes / 60.0);

                //read longitude
                match = longitudeRegex.Match((string)button.Tag);
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
                readOk = false;
            }

            //read mission id from the tag
            typeId = -1;
            Regex missionTypeIdRegex = new Regex(@"\b" + typeName + @"TypeId=(?<typeId>\d{0,})\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            try
            {
                Match match = missionTypeIdRegex.Match((string)button.Tag);
                if (match != Match.Empty)
                {
                    string res = match.Result("${typeId}");
                    typeId = int.Parse(res);
                }
                else
                {
                    typeId = -1;
                    readOk = false;
                }
            }
            catch (Exception)
            {
                readOk = false;
            }

            return readOk;
        }

        bool ExtractMissionData(Button button, out int iconId, out string description)
        {
            bool readOk = true;

            iconId = -1;
            description = null;

            //ignore buttons with no string
            if (button.Tag == null || button.Tag.GetType() != typeof(string))
            {
                return false;
            }

            Regex iconIdregex = new Regex(@"\biconId=(?<iconId>\d+)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            try
            {
                //read iconId
                Match match = iconIdregex.Match((string)button.Tag);
                string res = match.Result("${iconId}");
                iconId = int.Parse(res);
            }
            catch (Exception)
            {
                readOk = false;
            }

            Regex descriptionRegex = new Regex(@"\bdescription=_(?<description>.*)_\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            try
            {
                //read iconId
                Match match = descriptionRegex.Match((string)button.Tag);
                description = match.Result("${description}");
            }
            catch (Exception)
            {
                readOk = false;
            }

            return readOk;
        }

        //this projects a point3d to its 2d position on-screen and sets the z-index
        private void ProjectPoint(Button button, Point3D point, Matrix3D m, RotateTransform3D rt, bool planeType, bool adjustTop)
        {
            // Transform the 3D point to 2D
            Point3D transformedPoint = m.Transform(point);
            if (!planeType)
            {
                double x = transformedPoint.X - button.Width / 2;
                double y = adjustTop ? transformedPoint.Y - button.Height : transformedPoint.Y - button.Height / 2;
                button.Margin = new Thickness(x, y, 0.0, 0.0);
            }
            else
            {
                double x = transformedPoint.X + button.Width / 2;
                double y = transformedPoint.Y + button.Height / 2;
                button.Margin = new Thickness(0, 0, ((Grid)button.Parent).ActualWidth - x, ((Grid)button.Parent).ActualHeight - y);
            }

            //set z-index of the mission button according to the side of the sphere that it's on
            Vector3D lookAt = new Vector3D(rt.Value.M31, rt.Value.M32, rt.Value.M33);
            Vector3D p1 = (Vector3D)point;
            double dot = Vector3D.DotProduct(lookAt, p1);
            Grid.SetZIndex(button, dot > 0.0 ? 3 : 1);
        }

        //---------------------------------------------------------
        //TODO touch events

        protected override void OnTouchDown(TouchEventArgs e)
        {
            base.OnTouchDown(e);

            viewport.CaptureTouch(e.TouchDevice);

            TouchPoint p = e.GetTouchPoint(viewport);
            //touchDownPoints[e.TouchDevice.Id] = p;

        }

        protected override void OnTouchMove(TouchEventArgs e)
        {
            base.OnTouchMove(e);

            TouchPoint p = e.GetTouchPoint(viewport);
            System.Diagnostics.Debug.WriteLine(p);
        }

        protected override void OnTouchUp(TouchEventArgs e)
        {
            base.OnTouchUp(e);

            viewport.ReleaseTouchCapture(e.TouchDevice);
        }

        #endregion

        #region Method handlers

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            //calculate new camera zoom velocity
            cameraZoomVelocity += e.Delta * 0.002f * cameraZoomInputMultiplier;

            //clamp camera zoom velocity
            float cameraZoomVelocityMaxScale = 0.8f;
            cameraZoomVelocity = Clamp(cameraZoomVelocity, -cameraZoomVelocityMax * cameraZoomVelocityMaxScale, cameraZoomVelocityMax * cameraZoomVelocityMaxScale);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            viewport.CaptureMouse();

            isMouseDown = true;

            Point p = e.GetPosition(viewport);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
                        
            Point p = e.GetPosition(viewport);
            lastMousePos = p;

            //see if we´re dragging a plane type button
            if (planeTypeButtonState == PlaneTypeButtonStates.Dragging)
            {
                planeTypeInfoButtonMouseDown.IsOnWorld = false;
                Button planeTypeButton = planeTypeInfoButtonMouseDown.Button;
                planeTypeButton.Margin = new Thickness(0, 0, grid.ActualWidth - p.X - planeTypeButton.ActualWidth / 2, grid.ActualHeight - p.Y - planeTypeButton.ActualHeight / 2);

                //if the plane type button is over the plane types rectangle, it´s always enabled
                Point localP = e.GetPosition(planeTypesRectangle);
                if (IsOnPlaneTypesRectangle(localP))
                {
                    planeTypeInfoButtonMouseDown.Button.IsEnabled = true;
                }
                else
                {
                    //if the plane type button is not over the plane types rectangle, we enable or disable
                    //plane type button according to whether the mouse is over the 3d world or not
                    RayMeshGeometry3DHitTestResult rayMeshResult =
                        (RayMeshGeometry3DHitTestResult)
                          VisualTreeHelper.HitTest(viewport, p);

                    if (rayMeshResult != null)
                    {
                        planeTypeInfoButtonMouseDown.Button.IsEnabled = true;
                    }
                    else
                    {
                        planeTypeInfoButtonMouseDown.Button.IsEnabled = false;
                    }
                }
            }
            else if (planeTypeButtonState == PlaneTypeButtonStates.MouseDown)
            {
            }
            else if (planeTypeButtonState == PlaneTypeButtonStates.Popup)
            {
            }
            else
            {
                //if the mouse is being dragged in normal mode, hide the plane type panel
                if (isMouseDown)
                {
                    planePopOutUserControl.PopOut();

                    if (viewMissionUserControl.Opacity == 1.0)
                    {
                        //hide view mission panel
                        viewMissionUserControl.HideAnimated();

                        if (viewMissionUserControl.SourceButton != null)
                        {
                            //show mission button again (it was hidden when the view mission panel appeared)
                            Storyboard storyboard = (Storyboard)viewMissionUserControl.SourceButton.Template.Resources["in"];
                            storyboard.Begin((Grid)viewMissionUserControl.SourceButton.Template.FindName("layoutGrid", viewMissionUserControl.SourceButton));
                        }
                    }
                }

                /*
                //scale bounds to [0,0] - [2,2]
                double x = p.X / (viewport.ActualWidth / 2);
                double y = p.Y / (viewport.ActualHeight / 2);

                //translate 0,0 to the center
                x = x - 1;

                //flip so +Y is up instead of down
                y = 1 - y;

                //solve for Z since our sphere is of radius = 1
                double z2 = 1 - x * x - y * y;
                double z = z2 > 0 ? Math.Sqrt(z2) : 0;
                Vector3D spherePoint = new Vector3D(x, y, z);
                spherePoint.Normalize();
                */

                Vector3D cameraPosition = new Vector3D(myPerspectiveCamera.Position.X, myPerspectiveCamera.Position.Y, myPerspectiveCamera.Position.Z);
                double distance = cameraPosition.Length;
                double sphereRadius = 140.0 / distance;

                //center coordinates
                double x = p.X - viewport.ActualWidth * 0.5;
                double y = 1.0 - (p.Y - viewport.ActualHeight * 0.5);

                x /= (viewport.ActualHeight * 0.5);
                y /= (viewport.ActualHeight * 0.5);

                x /= sphereRadius;
                y /= sphereRadius;

                //solve for Z
                double z2 = (sphereRadius * sphereRadius) - x * x - y * y;
                double z = z2 > 0 ? Math.Sqrt(z2) : 0;
                Vector3D spherePoint = new Vector3D(x, y, z);
                spherePoint.Normalize();

                RayMeshGeometry3DHitTestResult rayMeshResult =
                    (RayMeshGeometry3DHitTestResult)
                      VisualTreeHelper.HitTest(viewport, e.GetPosition(viewport));

                if (rayMeshResult != null)
                {
                    movedOnSphere = true;
                    spherePointHit = new Vector3D(rayMeshResult.PointHit.X, rayMeshResult.PointHit.Y, rayMeshResult.PointHit.Z);

                    //how to traverse intersection results
                    /*
                    PartialModel found = null;
                    foreach (PartialModel pm in vstuff.models)
                    {
                        if (pm.mesh == rayMeshResult.MeshHit)
                        {
                            found = pm;
                            break;
                        }
                    }

                    if (found != null)
                    {
                        if (IsSelected(found.bag.solid))
                        {
                            Unselect(found.bag.solid);
                        }
                        else
                        {
                            Select(found.bag.solid);
                        }
                    }
                    */
                }

                if (movedOnSphere && isMouseDown && lastSpherePoint.X != 0.0f && lastSpherePoint.Y != 0.0f && lastSpherePoint.Z != 0.0f)
                {
                    //trackball implementation from http://viewport3d.com/trackball.htm

                    //find rotation transform that takes the camera from lastSpherePoint to spherePoint
                    Vector3D axis = Vector3D.CrossProduct(lastSpherePoint, spherePoint);
                    double theta = Vector3D.AngleBetween(lastSpherePoint, spherePoint);

                    if (axis.Length != 0.0)
                    {
                        //negate the angle because we are rotating the camera.
                        //this is not needed if we are rotating the scene instead.
                        Quaternion delta = new Quaternion(axis, -theta);

                        /*
                        //get the current orientantion from the RotateTransform3D
                        RotateTransform3D rt = (RotateTransform3D)myPerspectiveCamera.Transform;
                        AxisAngleRotation3D r = (AxisAngleRotation3D)rt.Rotation;
                        Quaternion q = new Quaternion(r.Axis, r.Angle);

                        //compose the delta with the previous orientation
                        q *= delta;

                        //write the new orientation back to the Rotation3D
                        r.Axis = q.Axis;
                        r.Angle = q.Angle;
                        */

                        //apply delta to camera angular velocity
                        cameraAngularVelocity = Quaternion.Slerp(cameraAngularVelocity, delta, 0.1f);

                        //clamp camera angular velocity
                        float cameraAngularVelocityMaxScale = 5.0f;
                        cameraAngularVelocity = ClampAngle(cameraAngularVelocity, -cameraAngularVelocityMax * cameraAngularVelocityMaxScale, cameraAngularVelocityMax * cameraAngularVelocityMaxScale);
                    }
                }

                movedOnSphere = false;

                //update lastSpherePoint for next time round
                lastSpherePoint = spherePoint;
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            isMouseDown = false;

            //note this also gets called when we´re dragging a plane type button
            if (planeTypeButtonState == PlaneTypeButtonStates.Dragging)
            {
                //see if the plane type was dropped on the plane types rectangle
                Point localP = e.GetPosition(planeTypesRectangle);
                if (IsOnPlaneTypesRectangle(localP))
                {
                    //if dropped on the rectangle, move back to original position
                    planeTypeInfoButtonMouseDown.ResetPosition();

                    //start displaying missions instead of planes
                    SwitchToDisplayMode(DisplayMode.Missions);
                }
                else
                {
                    //see if the plane type button was dropped on the world
                    Point p = e.GetPosition(viewport);
                    RayMeshGeometry3DHitTestResult rayMeshResult = (RayMeshGeometry3DHitTestResult)VisualTreeHelper.HitTest(viewport, p);
                    if (rayMeshResult != null)
                    {
                        //if dropped on the world, store new 3d coordinates
                        planeTypeInfoButtonMouseDown.IsOnWorld = true;
                        planeTypeInfoButtonMouseDown.Position = rayMeshResult.PointHit;
                    }
                    else
                    {
                        //if not dropped on the world, move back to original position
                        planeTypeInfoButtonMouseDown.ResetPosition();
                    }
                }
            }

            planeTypeInfoButtonMouseDown = null;
            planeTypeButtonState = PlaneTypeButtonStates.Normal;

            viewport.ReleaseMouseCapture();
        }

        #endregion

        #region Event handlers

        //this gets called when the user clicks on one of the buttons which represents an actual mission
        //on the world map
        void missionButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            MissionInfo missionInfo = MissionInfoByButton(button);

            //display panel with mission information
            Point p = new Point(button.Margin.Left + button.ActualWidth / 2, button.Margin.Top + button.ActualWidth);
            viewMissionUserControl.ViewMissionInfo(missionInfo.MissionIconId, missionInfo.Description, p, button);

            //hide mission icon
            Storyboard storyboard = (Storyboard)button.Template.Resources["out"];
            storyboard.Begin((Grid)button.Template.FindName("layoutGrid", button));
        }

        //this gets called when the user wants to show the locations of a plane
        void planePopOutUserControl_ShowPlaneLocationsEvent(object sender, PlanePopOutUserControl.ShowPlaneLocationsEventArgs e)
        {
            ShowPlaneInfoButtonsWithId(e.planeTypeId);
        }

        //this gets called when the mouse goes down on one of the plane type buttons
        void planeTypeButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Button button = (Button)sender;
            
            //record the fact that a plane type button was clicked
            planeTypeInfoButtonMouseDown = PlaneTypeInfoByButton(button);
            planeTypeInfoButtonMouseDownTime = DateTime.Now;
            planeTypeInfoButtonMouseDownPosition = e.GetPosition(viewport);
            planeTypeButtonState = PlaneTypeButtonStates.MouseDown;

            ShowPlaneInfoButtonsWithId(-1);

            SwitchToDisplayMode(DisplayMode.Planes);

            //when the mouse goes down it captures the mouse, so we need to claim it back
            viewport.CaptureMouse();
        }

        //this gets called when one of the mission type toggle buttons is clicked
        //we preview the event so we can switch to the missions display mode and
        //not toggle the state of the button
        void missionTypeToggleButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (displayMode == DisplayMode.Planes)
            {
                SwitchToDisplayMode(DisplayMode.Missions);
                e.Handled = true;
            }
        }

        //this gets called when one of the mission type toggle buttons is unchecked
        void missionTypeToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            SetVisibilityMissionInfoButtonsWithId(MissionTypeIdByToggleButton((ToggleButton)sender), false);            
        }

        //this gets called when one of the mission type toggle buttons is checked
        void missionTypeToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            SetVisibilityMissionInfoButtonsWithId(MissionTypeIdByToggleButton((ToggleButton)sender), true);
        }

        //this gets called when the main window is loaded
        //do some additional initialization which can´t take place in constructor
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //extract mission information from the Button controls representing missions and store it for later use
            int buttonCount = 0;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(grid); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(grid, i);
                if (child != null && child.GetType() == typeof(Button))
                {
                    Button button = (Button)child;

                    //see if this represents a mission or a plane on the world map
                    Point3D spherical;
                    int typeId;
                    if (ExtractTypeData(button, "mission", out spherical, out typeId))
                    {
                        int iconId;
                        string description;
                        if (ExtractMissionData(button, out iconId, out description))
                        {
                            missionInfos.Add(new MissionInfo(button, spherical, typeId, iconId, description));
                            button.Click += new RoutedEventHandler(missionButton_Click);
                        }
                    }
                    else if (ExtractTypeData(button, "plane", out spherical, out typeId))
                    {
                        planeInfos.Add(new PlaneInfo(button, spherical, typeId));
                    }

                    buttonCount++;
                }
            }

            //go through the mission type icons and only show the right icon for each mission type
            for (int iMissionType = 0; iMissionType < missionTypeToggleButtons.Length; iMissionType++)
            {
                //hide all mission icons except the one corresponding to the assigned mission
                HideAllChildrenExcept(FindChildWithName(missionTypeToggleButtons[iMissionType], "grid"), "missionIcon" + iMissionType);
            }

            //create plane type infos
            planeTypeInfos = new PlaneTypeInfo[5];
            planeTypeInfos[0] = new PlaneTypeInfo(planeType0, 0);
            planeTypeInfos[1] = new PlaneTypeInfo(planeType1, 1);
            planeTypeInfos[2] = new PlaneTypeInfo(planeType2, 2);
            planeTypeInfos[3] = new PlaneTypeInfo(planeType3, 3);
            planeTypeInfos[4] = new PlaneTypeInfo(planeType4, 4);

            //attach to the mouse down event of the plane type buttons
            //note we use "preview" because of bubbling events
            for (int iPlaneTypeInfo = 0; iPlaneTypeInfo < planeTypeInfos.Length; iPlaneTypeInfo++)
            {
                planeTypeInfos[iPlaneTypeInfo].Button.PreviewMouseDown += new MouseButtonEventHandler(planeTypeButton_MouseDown);
            }

            //hide plane type pop up
            planePopOutUserControl.Visibility = System.Windows.Visibility.Hidden;

            //TODO
            //hide view mission user control
            //viewMissionUserControl.Visibility = System.Windows.Visibility.Hidden;

            //hide all planes on the world map
            ShowPlaneInfoButtonsWithId(-1);
        }

        //this gets called once per frame
        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            TimeSpan elapsed = now - lastTick;
            lastTick = now;

            //see if we have to start dragging a plane type button or if it´s time to show the popup for a plane type button
            if (planeTypeButtonState == PlaneTypeButtonStates.MouseDown)
            {
                TimeSpan elapsedSinceMouseDown = now - planeTypeInfoButtonMouseDownTime;
                Vector d = (planeTypeInfoButtonMouseDownPosition - lastMousePos);

                //see if the mouse moved far enough to start dragging the plane type button
                if (Math.Abs(d.X) > SystemParameters.MinimumHorizontalDragDistance
                    || Math.Abs(d.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    //start dragging
                    planeTypeButtonState = PlaneTypeButtonStates.Dragging;
                    planePopOutUserControl.PopOut();

                    //reset Z index so it goes back to being on top of the plane type rectangle
                    Grid.SetZIndex(planeTypeInfoButtonMouseDown.Button, planeTypeInfoButtonMouseDown.OriginalZIndex);
                    planeTypeInfoButtonMouseDown.IsOnWorld = false;

                    //move plane type buttons to their original positions (except for the one which we started dragging)
                    for (int iPlaneTypeInfo = 0; iPlaneTypeInfo < planeTypeInfos.Length; iPlaneTypeInfo++)
                    {
                       if (planeTypeInfos[iPlaneTypeInfo] != planeTypeInfoButtonMouseDown)
                           planeTypeInfos[iPlaneTypeInfo].ResetPosition();
                    }
                }
                else if (elapsedSinceMouseDown.TotalMilliseconds > WpfApplication1.Properties.Settings.Default.PlaneTypePanelShowSeconds * 1000)
                {
                    //if the mouse went down on a plane type button but the drag didn´t take place in time, we show the panel
                    if (!planeTypeInfoButtonMouseDown.IsOnWorld)
                    {
                        planeTypeButtonState = PlaneTypeButtonStates.Popup;
                        planePopOutUserControl.PopIn(new Point(((Grid)planeTypeInfoButtonMouseDown.Button.Parent).ActualWidth - planeTypeInfoButtonMouseDown.Button.Margin.Right - planeTypeInfoButtonMouseDown.Button.ActualWidth / 2 - planePopOutUserControl.ActualWidth / 2, planeTypeInfoButtonMouseDown.Button.Margin.Bottom + planeTypeInfoButtonMouseDown.Button.ActualHeight), planeTypeInfoButtonMouseDown.PlaneTypeId);
                        ResetPlaneTypeInfoButtonPositions();
                    }
                }
            }

            // Apply angular velocity to camera

            //get the current orientantion from the RotateTransform3D
            RotateTransform3D rt = (RotateTransform3D)myPerspectiveCamera.Transform;
            AxisAngleRotation3D r = (AxisAngleRotation3D)rt.Rotation;
            Quaternion q = new Quaternion(r.Axis, r.Angle);

            //calculate velocity quaternion given passed time
            Quaternion v = new Quaternion(cameraAngularVelocity.Axis, cameraAngularVelocity.Angle * elapsed.Milliseconds * 0.125f * cameraRotationInputMultiplier);

            //apply velocity quaternion
            q *= v;
            r.Axis = q.Axis;
            r.Angle = q.Angle;

            //damp camera angular velocity
            Quaternion zeroVelocity = new Quaternion(cameraAngularVelocity.Axis, 0.0f);
            cameraAngularVelocity = Quaternion.Slerp(cameraAngularVelocity, zeroVelocity, elapsed.Milliseconds * 0.01f * cameraAngularVelocityDamp);

            //apply zoom velocity to camera
            Vector3D cameraPosition = new Vector3D(myPerspectiveCamera.Position.X, myPerspectiveCamera.Position.Y, myPerspectiveCamera.Position.Z);
            double distance = cameraPosition.Length;
            distance += cameraZoomVelocity * elapsed.Milliseconds * 0.2f;
            float distanceScale = 80.0f;
            if (distance > cameraZoomMax * distanceScale) distance = cameraZoomMax * distanceScale;
            else if (distance < cameraZoomMinRelative * distanceScale) distance = cameraZoomMinRelative * distanceScale;
            cameraPosition.Normalize();
            cameraPosition *= distance;
            myPerspectiveCamera.Position = new Point3D(cameraPosition.X, cameraPosition.Y, cameraPosition.Z);

            //damp camera zoom velocity
            cameraZoomVelocity *= elapsed.Milliseconds * 0.05f * cameraZoomVelocityDamp;

            //project 3d positions
            bool matrixOk;
            Viewport3DVisual viewport3DVisual = VisualTreeHelper.GetParent(viewport.Children[0]) as Viewport3DVisual;
            Matrix3D m = _3DTools.MathUtils.TryWorldToViewportTransform(viewport3DVisual, out matrixOk);
            if (matrixOk)
            {
                //MissionInfos
                for (int iMissionInfo = 0; iMissionInfo < missionInfos.Count; iMissionInfo++)
                {
                    ProjectPoint(missionInfos[iMissionInfo].Button, missionInfos[iMissionInfo].Position, m, rt, false, true);
                }

                //PlaneInfos
                for (int iPlaneInfo = 0; iPlaneInfo < planeInfos.Count; iPlaneInfo++)
                {
                    ProjectPoint(planeInfos[iPlaneInfo].Button, planeInfos[iPlaneInfo].Position, m, rt, false, false);
                }

                //PlaneTypeInfos
                for (int iPlaneTypeInfo = 0; iPlaneTypeInfo < planeTypeInfos.Length; iPlaneTypeInfo++)
                {
                    if (planeTypeInfos[iPlaneTypeInfo].IsOnWorld)
                    {
                        ProjectPoint(planeTypeInfos[iPlaneTypeInfo].Button, planeTypeInfos[iPlaneTypeInfo].Position, m, rt, true, true);
                    }
                }
            }
        }

        #endregion

    }
}
