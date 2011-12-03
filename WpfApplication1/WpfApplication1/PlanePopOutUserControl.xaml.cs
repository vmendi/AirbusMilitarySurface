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
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class PlanePopOutUserControl : UserControl
    {

        #region Classes

        public class ShowPlaneLocationsEventArgs
        {
            public int planeTypeId;

            public ShowPlaneLocationsEventArgs(int planeTypeId)
            {
                this.planeTypeId = planeTypeId;
            }
        }

        private enum States
        {
            PoppingIn,
            PoppingOut,
            PoppedIn,
            PoppedOut,
        }

        #endregion

        #region Data

        private Storyboard popInStoryboard;
        private Storyboard popOutStoryboard;

        private Point newPosition;
        bool triggerPopInAfterPopOut = false;

        //id of the plane type that caused us to pop up
        private int planeTypeId;

        private States state = States.PoppedOut;

        #endregion

        #region Events

        // delegate declaration 
        public delegate void ShowPlaneLocationsEventHandler(object sender, ShowPlaneLocationsEventArgs e);

        // event declaration 
        public event ShowPlaneLocationsEventHandler ShowPlaneLocationsEvent; 

        #endregion

        #region Construction

        public PlanePopOutUserControl()
        {
            InitializeComponent();

            popInStoryboard = Resources["PopIn"] as Storyboard;
            popOutStoryboard = Resources["PopOut"] as Storyboard;

            popOutStoryboard.Completed += new EventHandler(popOutStoryboard_Completed);
            popInStoryboard.Completed += new EventHandler(popInStoryboard_Completed);
        }

        #endregion

        #region Methods

        public void PopIn(Point newPosition, int planeTypeId)
        {
            this.newPosition = newPosition;
            this.planeTypeId = planeTypeId;

            state = States.PoppingOut;
            popOutStoryboard.Begin();

            triggerPopInAfterPopOut = true;
        }

        public void PopOut()
        {
            if (state == States.PoppedOut || state == States.PoppingOut) return;

            state = States.PoppingOut;
            popOutStoryboard.Begin();
            triggerPopInAfterPopOut = false;
        }

        #endregion

        #region Event handlers

        void popOutStoryboard_Completed(object sender, EventArgs e)
        {
            if (triggerPopInAfterPopOut)
            {
                Margin = new Thickness(newPosition.X, 0, 0, newPosition.Y);
                Visibility = System.Windows.Visibility.Visible;

                state = States.PoppingIn;
                popInStoryboard.Begin();
            }
            else
            {
                state = States.PoppedOut;
                Visibility = System.Windows.Visibility.Hidden;
            }
        }

        void popInStoryboard_Completed(object sender, EventArgs e)
        {
            state = States.PoppedIn;
        }

        private void locationsButton_Click(object sender, RoutedEventArgs e)
        {
            if (ShowPlaneLocationsEvent != null)
            {
                ShowPlaneLocationsEvent(this, new ShowPlaneLocationsEventArgs(planeTypeId));
            }

            PopOut();
        }

        #endregion

    }
}
