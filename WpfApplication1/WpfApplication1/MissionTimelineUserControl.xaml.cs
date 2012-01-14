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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MissionTimelineUserControl.xaml
    /// </summary>
    public partial class MissionTimelineUserControl : UserControl
    {

        #region Classes

        private enum MarkerStates
        {
            Animating,
            Stopped,
        }

        public class ClickedMilestoneEventArgs
        {
            public int milestoneId;

            public ClickedMilestoneEventArgs(int milestoneId)
            {
                this.milestoneId = milestoneId;
            }
        }

        #endregion

        #region Data

        private MarkerStates markerState = MarkerStates.Stopped;

        private DateTime markerAnimationStartTime;
        private double markerAnimationStartX;
        private double markerAnimationEndX;

        private ToggleButton[] milestoneButtons;

        private bool initialisedMarker = false;

        #endregion

        #region Events

        // delegate declaration 
        public delegate void ClickedMilestoneEventHandler(object sender, ClickedMilestoneEventArgs e);

        // event declaration 
        public event ClickedMilestoneEventHandler ClickedMilestoneEvent;

        #endregion

        #region Construction

        public MissionTimelineUserControl()
        {
            InitializeComponent();

            //attach ourselves to rendering event
            System.Windows.Media.CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }

        #endregion

        #region Methods

        private static Rect GetBoundingBox(FrameworkElement element, FrameworkElement parent)
        {
            GeneralTransform transform = element.TransformToAncestor(parent);
            Point topLeft = transform.Transform(new Point(0.0, 0.0));
            Point bottomRight = transform.Transform(new Point(element.ActualWidth, element.ActualHeight));
            return new Rect(topLeft, bottomRight);
        }

        public void SelectMilestone(int iMilestone)
        {
            if (iMilestone == -1)
            {
                marker.Visibility = Visibility.Hidden;
                marker.Margin = new Thickness(line.Margin.Left, marker.Margin.Top, marker.Margin.Right, marker.Margin.Bottom);
            }
            else
            {
                StartMarkerAnimation(milestoneButtons[iMilestone]);
            }

            for (int iMilestoneButton = 0; iMilestoneButton < milestoneButtons.Length; iMilestoneButton++)
            {
                if (iMilestoneButton != iMilestone)
                {
                    milestoneButtons[iMilestoneButton].IsChecked = false;
                }
                else
                {
                    milestoneButtons[iMilestoneButton].IsChecked = true;
                }
            }
        }

        private void StartMarkerAnimation(ToggleButton milestoneButton)
        {
            markerState = MarkerStates.Animating;
            markerAnimationStartTime = DateTime.Now;
            markerAnimationStartX = marker.Margin.Left;

            Rect milestoneButtonRect = GetBoundingBox(milestoneButton, uniformGrid);
            markerAnimationEndX = milestoneButtonRect.Left + milestoneButtonRect.Width * 0.5 + uniformGrid.Margin.Left;

            marker.Visibility = Visibility.Visible;
        }

        public void Initialise(string[] milestoneTexts)
        {
            //add milestone buttons
            milestoneButtons = new ToggleButton[milestoneTexts.Length];
            for (int iMilestone = 0; iMilestone < milestoneTexts.Length; iMilestone++)
            {
                ToggleButton milestoneButton = new ToggleButton();
                milestoneButton.Template = (ControlTemplate)FindResource("MilestoneControlTemplate");
                milestoneButton.Margin = new Thickness(0.0, 0.0, 0.0, 0.0);

                uniformGrid.Children.Add(milestoneButton);

                bool ok = milestoneButton.ApplyTemplate();
                Grid grid = (Grid)milestoneButton.Template.FindName("grid2", milestoneButton);

                //set milestone number
                TextBlock numberTextBlock = (TextBlock)grid.FindName("number");
                numberTextBlock.Text = iMilestone.ToString();

                //set milestone description
                TextBlock descriptionTextBlock = (TextBlock)grid.FindName("description");
                descriptionTextBlock.Text = (string)milestoneTexts[iMilestone];

                milestoneButton.PreviewMouseDown += new MouseButtonEventHandler(milestoneButton_PreviewMouseDown);

                milestoneButtons[iMilestone] = milestoneButton;
            }
        }

        #endregion

        #region Event handlers

        //this gets called once per frame
        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (!initialisedMarker && milestoneButtons != null)
            {
                if (milestoneButtons.Length > 0)
                {
                    Rect milestoneButtonRect = GetBoundingBox(milestoneButtons[0], uniformGrid);
                    double x = milestoneButtonRect.Left + milestoneButtonRect.Width * 0.5 + uniformGrid.Margin.Left;
                    marker.Margin = new Thickness(x, marker.Margin.Top, marker.Margin.Right, marker.Margin.Bottom);
                }
                initialisedMarker = true;
            }

            if (markerState == MarkerStates.Animating)
            {
                DateTime now = DateTime.Now;
                TimeSpan elapsed = now - markerAnimationStartTime;
                double t = elapsed.TotalMilliseconds / (WpfApplication1.Properties.Settings.Default.MissionTimelineMarkerAnimateSeconds * 1000.0);
                if (t > 1.0)
                {
                    t = 1.0;
                    markerState = MarkerStates.Stopped;
                }

                //apply easing
                t = 1.0f - t;
                t = t * t * t;
                t = 1.0f - t;

                double x = (1.0f - t) * markerAnimationStartX + t * markerAnimationEndX;
                marker.Margin = new Thickness(x, marker.Margin.Top, marker.Margin.Right, marker.Margin.Bottom);
            }
        }

        //this gets called when one of the milestone buttons get clicked
        void milestoneButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            for (int iMilestoneButton = 0; iMilestoneButton < milestoneButtons.Length; iMilestoneButton++)
            {
                if (milestoneButtons[iMilestoneButton] == sender)
                {
                    if (milestoneButtons[iMilestoneButton].IsChecked == false)
                    {
                        SelectMilestone(iMilestoneButton);
                    }

                    //note we raise the event even if the user clicks two times on the same control in a row
                    //to allow for the MissionMilestoneUserControl to be displayed even if the user keeps clicking
                    //on the same button
                    if (ClickedMilestoneEvent != null)
                    {
                        ClickedMilestoneEvent(this, new ClickedMilestoneEventArgs(iMilestoneButton));
                    }
                }
            }

            e.Handled = true;
        }

        #endregion        

    }
}
