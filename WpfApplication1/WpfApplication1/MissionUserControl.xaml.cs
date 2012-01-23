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
using System.Text.RegularExpressions;
using System.Runtime.Remoting;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MissionUserControl.xaml
    /// </summary>
    public partial class MissionUserControl : UserControl
    {

        #region Classes

        public class BackEventArgs
        {

            #region Construction

            public BackEventArgs()
            {
            }

            #endregion

        }

        #endregion

        #region Data

        private int missionId = -1;
        private string introVideo = string.Empty;

        private List<MissionMilestoneThumbnailUserControl> milestoneControls = new List<MissionMilestoneThumbnailUserControl>();
        private MissionMilestoneThumbnailUserControl lastClickedControl = null;

        #endregion

        #region Properties

        public int MissionId
        { get { return missionId; } }

        #endregion

        #region Events

        // delegate declaration 
        public delegate void BackEventHandler(object sender, BackEventArgs e);

        // event declaration 
        public event BackEventHandler BackEvent;

        #endregion

        #region Constructor

        public MissionUserControl()
        {
            InitializeComponent();

            //so we get called when the user selects a milestone in the timeline
            missionTimeline.ClickedMilestoneEvent += new MissionTimelineUserControl.ClickedMilestoneEventHandler(missionTimeline_ClickedMilestoneEvent);

            //to complete initialisation
            Loaded += new RoutedEventHandler(MissionUserControl_Loaded);

            //so we get called when the intro video skip animation ends
            Storyboard storyboard = (Storyboard)videoGrid.FindResource("skipIntroVideo");
            storyboard.Completed += new EventHandler(storyboard_Completed);

            introMediaElement.MediaEnded += new RoutedEventHandler(introMediaElement_MediaEnded);

            missionMilestoneUserControl.BackEvent += new MissionMilestoneUserControl.BackEventHandler(missionMilestoneUserControl_BackEvent);
        }

        #endregion

        #region Methods

        private void Reset()
        {
            introMediaElement.LoadedBehavior = MediaState.Manual;
            introMediaElement.Position = new TimeSpan(0, 0, 0, 0);
            introMediaElement.Play();

            missionMilestoneUserControl.StartOutStoryboard();

            missionTimeline.SelectMilestone(-1);

            if (lastClickedControl != null)
            {
                Storyboard storyboard = (Storyboard)lastClickedControl.Resources["unselect"];
                storyboard.Begin();
            }
        }

        public void StartShowStoryboard()
        {
            Reset();

            //start show animation
            Storyboard storyboard = (Storyboard)mainGrid.FindResource("show");
            storyboard.Begin();
        }

        public void StartHideStoryboard()
        {
            //start hide animation
            Storyboard storyboard = (Storyboard)mainGrid.FindResource("hide");
            storyboard.Begin();
        }

        //extract mission id from tag and load the UserControl representing the mission
        private bool ReadTag(out UserControl missionUserControl, out string introVideo)
        {
            //see if this represents a mission
            
            if (ExtractTagData(out missionId, out introVideo))
            {
                string typeName = "WpfApplication1.Mission" + missionId + "UserControl";
                Type type = System.Type.GetType(typeName);
                missionUserControl = (UserControl)Activator.CreateInstance(type);
                blurGrid.Children.Add(missionUserControl);
                return true;
            }

            missionUserControl = null;
            introVideo = string.Empty;
            return false;
        }

        bool ExtractTagData(out int missionId, out string introVideo)
        {
            bool readOk = true;

            //ignore if we have no string
            if (Tag == null || Tag.GetType() != typeof(string))
            {
                missionId = -1;
                introVideo = string.Empty;
                return false;
            }

            //read mission id from the tag
            missionId = -1;
            Regex missionIdRegex = new Regex(@"\bmissionId=(?<missionId>\d{0,})\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            try
            {
                Match match = missionIdRegex.Match((string)Tag);
                if (match != Match.Empty)
                {
                    string res = match.Result("${missionId}");
                    missionId = int.Parse(res);
                }
                else
                {
                    missionId = -1;
                    readOk = false;
                }
            }
            catch (Exception)
            {
                readOk = false;
            }

            //read intro video from the tag
            introVideo = string.Empty;
            Regex introVideoRegex = new Regex(@"\bintroVideo=_(?<introVideo>[^_]*)_\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            try
            {
                Match match = introVideoRegex.Match((string)Tag);
                if (match != Match.Empty)
                {
                    introVideo = match.Result("${introVideo}");
                }
                else
                {
                    introVideo = string.Empty;
                    readOk = false;
                }
            }
            catch (Exception)
            {
                readOk = false;
            }

            return readOk;
        }

        private void ExtractMilestoneControls(FrameworkElement frameworkElement, List<MissionMilestoneThumbnailUserControl> milestoneControls)
        {
            //if the current Control is a milestone store it
            if (frameworkElement.GetType() == typeof(MissionMilestoneThumbnailUserControl))
            {
                MissionMilestoneThumbnailUserControl c = (MissionMilestoneThumbnailUserControl)frameworkElement;
                milestoneControls.Add(c);
            }

            //go through children
            foreach (object logicalChild in LogicalTreeHelper.GetChildren(frameworkElement))
            {
                if (logicalChild.GetType().IsSubclassOf(typeof(FrameworkElement)))
                {
                    ExtractMilestoneControls((FrameworkElement)logicalChild, milestoneControls);
                }
            }            
        }

        private void ClickedMapMilestoneControl(MissionMilestoneThumbnailUserControl control)
        {
            if (lastClickedControl != control)
            {
                if (lastClickedControl != null)
                {
                    Storyboard storyboard = (Storyboard)lastClickedControl.Resources["unselect"];
                    storyboard.Begin();
                }

                {
                    Storyboard storyboard = (Storyboard)control.Resources["select"];
                    storyboard.Begin();
                }

                lastClickedControl = control;
            }

            //make the mission milestone user control appear
            missionMilestoneUserControl.StartInStoryboard(control.PopupTitle, control.PopupSubtitle, control.PopupMediaFile.ToString(), control.PopupDescriptionHeading, control.PopupDescriptionBody);

            //blur
            Storyboard blurStoryboard = (Storyboard)Resources["blur"];
            blurStoryboard.Begin();
        }

        MissionMilestoneThumbnailUserControl MilestoneControlById(int id)
        {
            for (int iMilestoneControl = 0; iMilestoneControl < milestoneControls.Count; iMilestoneControl++)
            {
                if (milestoneControls[iMilestoneControl].Id == id)
                {
                    return milestoneControls[iMilestoneControl];
                }
            }

            return null;
        }

        #endregion

        #region Event handlers

        void MissionUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //this will add the MissionUserControl referenced in our tag, and return it
            UserControl missionUserControl;
            string introVideo;
            bool readTagOk = ReadTag(out missionUserControl, out introVideo);

            if (readTagOk)
            {
                //load intro video
                introMediaElement.Source = new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location + "/../../../" + introVideo);

                //store milestone controls                
                ExtractMilestoneControls(missionUserControl, milestoneControls);
                milestoneControls.Sort(delegate(MissionMilestoneThumbnailUserControl c1, MissionMilestoneThumbnailUserControl c2) { return c1.Id.CompareTo(c2.Id); });
                
                //make sure we get called when the user clicks on a milestone in the map
                for (int iMilestoneControl = 0; iMilestoneControl < milestoneControls.Count; iMilestoneControl++)
                {
                    milestoneControls[iMilestoneControl].PreviewMouseDown += new MouseButtonEventHandler(milestoneInfoControl_PreviewMouseDown);
                }

                //add mission timeline
                string[] timelineTexts = new string[milestoneControls.Count];
                string[] timelineNumbers = new string[milestoneControls.Count];
                for (int iMilestoneControl = 0; iMilestoneControl < milestoneControls.Count; iMilestoneControl++)
                {
                    timelineTexts[iMilestoneControl] = milestoneControls[iMilestoneControl].ThumbType;
                    timelineNumbers[iMilestoneControl] = milestoneControls[iMilestoneControl].Id.ToString();
                }

                missionTimeline.Initialise(timelineTexts, timelineNumbers);
            }
        }

        //this gets called when the user clicks on an element representing a milestone
        void milestoneInfoControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() == typeof(Control) || sender.GetType()== typeof(MissionMilestoneThumbnailUserControl))
            {
                MissionMilestoneThumbnailUserControl milestoneControl = (MissionMilestoneThumbnailUserControl)sender;

                //if the user clicks on an element representing a milestone we need to select it in the timeline
                missionTimeline.SelectMilestone(milestoneControls.IndexOf(milestoneControl));

                ClickedMapMilestoneControl(milestoneControl);
            }
        }

        //this gets called when the user selects a milestone in the timeline
        void missionTimeline_ClickedMilestoneEvent(object sender, MissionTimelineUserControl.ClickedMilestoneEventArgs e)
        {
            //note we're given in the eventargs the index of the milestone that was clicked
            MissionMilestoneThumbnailUserControl milestoneControl = milestoneControls[e.milestoneId];
            ClickedMapMilestoneControl(milestoneControl);
        }

        //this gets called when the user clicks the back button
        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            StartHideStoryboard();

            if (BackEvent != null)
            {
                BackEvent(this, new BackEventArgs());
            }
        }

        //this gets called when the user clicks the skip button in the video intro
        private void introSkipButton_Click(object sender, RoutedEventArgs e)
        {
            //start intro video skip animation
            Storyboard storyboard = (Storyboard)videoGrid.FindResource("skipIntroVideo");
            storyboard.Begin();
        }

        //this gets called when the video intro "skip" animation completes
        void storyboard_Completed(object sender, EventArgs e)
        {
            introMediaElement.LoadedBehavior = MediaState.Manual;
            introMediaElement.Stop();
        }

        //this gets called when the video intro is finished playing
        void introMediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            //start intro video skip animation
            Storyboard storyboard = (Storyboard)videoGrid.FindResource("skipIntroVideo");
            storyboard.Begin();
        }

        //this gets called when the user clicks on the back button of the popup that appears after clicking on a milestone control
        void missionMilestoneUserControl_BackEvent(object sender, MissionMilestoneUserControl.BackEventArgs e)
        {
            //unblur
            Storyboard storyboard = (Storyboard)Resources["unblur"];
            storyboard.Begin();
        }

        #endregion

    }
}
