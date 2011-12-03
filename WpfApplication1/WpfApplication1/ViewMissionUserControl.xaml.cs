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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for ViewMissionUserControl.xaml
    /// </summary>
    public partial class ViewMissionUserControl : UserControl
    {

        #region Classes

        public class ViewMissionEventArgs
        {

            #region Data

            private int missionId;

            #endregion

            #region Properties

            public int MissionId
            { get { return missionId; } }

            #endregion

            #region Construction

            public ViewMissionEventArgs(int missionId)
            {
                this.missionId = missionId;
            }

            #endregion

        }

        public enum States
        {
            Showing,
            Hiding,
            Visible,
            Invisible,
        }

        #endregion

        #region Data

        private Storyboard showStoryboard;
        private Storyboard hideStoryboard;

        private Point3D position;

        private Point3D newPosition = new Point3D(0.0f, 0.0f, 0.0f);
        private int newMissionIconId;
        private string newDescription;
        private bool triggerShowAfterHidden = false;
        private Button sourceButton; //this is the button of the mission on the world map that was clicked on to display us
        private int missionId; //id of the mission that we´re displaying

        private States state = States.Visible;

        #endregion

        #region Properties

        public Button SourceButton
        { get { return sourceButton; } }

        public States State
        { get { return state; } }

        public Point3D Position
        { get { return position; } }

        #endregion

        #region Events

        // delegate declaration 
        public delegate void ViewMissionEventHandler(object sender, ViewMissionEventArgs e);

        // event declaration 
        public event ViewMissionEventHandler ViewMissionEvent;

        #endregion

        #region Constructor

        public ViewMissionUserControl()
        {
            InitializeComponent();

            showStoryboard = Resources["show"] as Storyboard;
            hideStoryboard = Resources["hide"] as Storyboard;

            showStoryboard.Completed += new EventHandler(showStoryboard_Completed);
            hideStoryboard.Completed += new EventHandler(hideStoryboard_Completed);            
        }

        #endregion

        #region Methods

        public void Update()
        {
        }

        private void UpdateData(Point3D newPosition, int newMissionIconId, string newDescription)
        {
            position = newPosition;

            //replace the mission icon
            string strUri = Directory.GetCurrentDirectory() + @"\..\..\icons\missionIconId" + newMissionIconId + ".png";
            missionIcon.Source = new BitmapImage(new Uri(strUri));

            //set the text description
            description.Text = newDescription;
        }

        public void ViewMissionInfo(int missionIconId, string description, Point3D position, Button button, int missionId)
        {
            newPosition = position;
            newMissionIconId = missionIconId;
            newDescription = description;
            sourceButton = button;
            this.missionId = missionId;

            if (state == States.Showing || state == States.Visible)
            {
                //we need to hide before displaying data
                state = States.Hiding;
                triggerShowAfterHidden = true;
                hideStoryboard.Begin();
            }
            else
            {
                //we're already hidden so we can just update the data and show
                UpdateData(position, missionIconId, description);

                state = States.Showing;
                showStoryboard.Begin();
            }
        }

        public void HideAnimated()
        {
            if (state == States.Invisible || state == States.Hiding) return;

            sourceButton = null;

            state = States.Hiding;
            triggerShowAfterHidden = false;
            hideStoryboard.Begin();
        }

        #endregion

        #region Event handlers

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ViewMissionEvent != null)
            {
                ViewMissionEvent(this, new ViewMissionEventArgs(missionId));
            }

            HideAnimated();
        }

        void hideStoryboard_Completed(object sender, EventArgs e)
        {
            if (triggerShowAfterHidden)
            {
                UpdateData(newPosition, newMissionIconId, newDescription);

                state = States.Showing;
                showStoryboard.Begin();
            }
            else
            {
                state = States.Invisible;
            }
        }

        void showStoryboard_Completed(object sender, EventArgs e)
        {
            state = States.Visible;
        }

        #endregion

    }
}