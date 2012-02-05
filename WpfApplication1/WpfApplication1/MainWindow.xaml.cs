using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace WpfApplication1
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
    {

        #region Data

        private readonly MissionUserControl[] missionUserControls;

        #endregion

        #region Construction

        public MainWindow()
        {
            InitializeComponent();

            //find MissionUserControls
            List<MissionUserControl> tMissionUserControls = new List<MissionUserControl>();
            FindMissionUserControls(mainGrid, tMissionUserControls);
            missionUserControls = tMissionUserControls.ToArray();

            for (int iMissionUserControl = 0; iMissionUserControl < missionUserControls.Length; iMissionUserControl++)
            {
                missionUserControls[iMissionUserControl].BackEvent += new MissionUserControl.BackEventHandler(missionUserControl_BackEvent);
            }

            worldUserControl.ViewMissionEvent += new WorldUserControl.ViewMissionEventHandler(worldUserControl_ViewMissionEvent);
            worldUserControl.PlanePopupUserControl = planePopup;

            planePopup.Visibility = System.Windows.Visibility.Hidden;
        }

        #endregion

        #region Methods

        private void FindMissionUserControls(FrameworkElement frameworkElement, List<MissionUserControl> missionUserControls)
        {
            if (frameworkElement.GetType().IsSubclassOf(typeof(MissionUserControl)) || frameworkElement.GetType() == typeof(MissionUserControl))
            {
                missionUserControls.Add((MissionUserControl)frameworkElement);
            }

            //go through children
            foreach (object logicalChild in LogicalTreeHelper.GetChildren(frameworkElement))
            {
                if (logicalChild.GetType().IsSubclassOf(typeof(FrameworkElement)))
                {
                    FindMissionUserControls((FrameworkElement)logicalChild, missionUserControls);
                }
            }
        }

        #endregion

        #region Event handlers

        //this gets called when the user clicks the back button in a MissionUserControl
        void missionUserControl_BackEvent(object sender, MissionUserControl.BackEventArgs e)
        {
            worldUserControl.StartShow();
            ((MissionUserControl)sender).StartHideStoryboard();
        }

        //this gets called when the worldUserControl user control tells us that the user wants to view one of the missions
        void worldUserControl_ViewMissionEvent(object sender, WorldUserControl.ViewMissionEventArgs e)
        {
            worldUserControl.StartZoom();

            //go through all the MissionUserControls we found and see if any of those have the specified mission id
            for (int iMissionUserControl = 0; iMissionUserControl < missionUserControls.Length; iMissionUserControl++)
            {
                if (e.MissionId == missionUserControls[iMissionUserControl].MissionId)
                {
                    missionUserControls[iMissionUserControl].StartShowStoryboard();
                }
            }
        }

        #endregion

    }
}