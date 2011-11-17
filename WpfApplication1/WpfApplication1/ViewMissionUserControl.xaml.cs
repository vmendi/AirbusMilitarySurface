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
            public ViewMissionEventArgs()
            {
            }
        }

        private enum States
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

        private Point newPosition;
        private int newMissionIconId;
        private string newDescription;
        private bool triggerShowAfterHidden = false;
        private Button sourceButton; //this is the button of the mission on the world map that was clicked on to display us

        private States state = States.Invisible;

        #endregion

        #region Properties

        public Button SourceButton
        { get { return sourceButton; } }

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

        private void UpdateData(Point newPosition, int newMissionIconId, string newDescription)
        {
            Margin = new Thickness(newPosition.X, newPosition.Y - ActualHeight / 2, 0.0, 0.0);

            //replace the mission icon
            string strUri = Directory.GetCurrentDirectory() + @"\..\..\icons\missionIconId" + newMissionIconId + ".png";
            missionIcon.Source = new BitmapImage(new Uri(strUri));

            //set the text description
            description.Text = newDescription;
        }

        public void ViewMissionInfo(int missionIconId, string description, Point position, Button button)
        {
            newPosition = position;
            newMissionIconId = missionIconId;
            newDescription = description;
            sourceButton = button;

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
                ViewMissionEvent(this, new ViewMissionEventArgs());
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