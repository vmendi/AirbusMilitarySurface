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

        private class MilestoneInfo
        {

            #region Data

            int id;
            string shortDescription;
            string title;
            string longDescription;
            string[] mediaFiles;
            Control control;

            #endregion

            #region Properties

            public int Id
            { get { return id; } }

            public string ShortDesctiption
            { get { return shortDescription; } }

            public string Title
            { get { return title; } }

            public string LongDesctiption
            { get { return longDescription; } }

            public string[] Mediafiles
            { get { return mediaFiles; } }

            public Control Control
            { get { return control; } }

            #endregion

            #region Construction

            public MilestoneInfo(int id, string shortDescription, string title, string longDescription, string[] mediaFiles, Control control)
            {
                this.id = id;
                this.shortDescription = shortDescription;
                this.title = title;
                this.longDescription = longDescription;
                this.mediaFiles = mediaFiles;
                this.control = control;
            }

            #endregion

        }

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

        int missionId = -1;
        string introVideo = string.Empty;

        private List<MilestoneInfo> milestoneInfos = new List<MilestoneInfo>();
        private Control lastClickedControl = null;

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
        }

        #endregion

        #region Methods

        private void Reset()
        {
            introMediaElement.LoadedBehavior = MediaState.Manual;
            introMediaElement.Position = new TimeSpan(0, 0, 0, 0);
            introMediaElement.Play();
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
                mainGrid.Children.Add(missionUserControl);
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

        private void ExtractMilestoneInfos(FrameworkElement frameworkElement, List<MilestoneInfo> milestoneInfos)
        {
            //if the current Control is a milestone, create MilestoneInfo
            //and store it
            if (frameworkElement.GetType().IsSubclassOf(typeof(Control)))
            {
                MilestoneInfo milestoneInfo = ExtractMilestoneInfo((Control)frameworkElement);
                if (milestoneInfo != null)
                {
                    milestoneInfos.Add(milestoneInfo);
                }
            }

            //go through children
            foreach (object logicalChild in LogicalTreeHelper.GetChildren(frameworkElement))
            {
                if (logicalChild.GetType().IsSubclassOf(typeof(FrameworkElement)))
                {
                    ExtractMilestoneInfos((FrameworkElement)logicalChild, milestoneInfos);
                }
            }            
        }

        private MilestoneInfo ExtractMilestoneInfo(Control control)
        {
            //ignore elements with no string
            if (control.Tag == null || control.Tag.GetType() != typeof(string))
            {
                return null;
            }

            bool readOk = true;

            //read milestone id
            int milestoneId = -1;
            Regex milestoneIdRegex = new Regex(@"\bmilestoneId=(?<milestoneId>\d{0,})\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            try
            {
                Match match = milestoneIdRegex.Match((string)control.Tag);
                if (match != Match.Empty)
                {
                    string res = match.Result("${milestoneId}");
                    milestoneId = int.Parse(res);
                }
                else
                {
                    milestoneId = -1;
                    readOk = false;
                }
            }
            catch (Exception)
            {
                readOk = false;
            }

            //read short description
            string shortDescription = string.Empty;
            Regex shortDescriptionRegex = new Regex(@"\bshortDescription=_(?<shortDescription>[^_]*)_\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            try
            {
                Match match = shortDescriptionRegex.Match((string)control.Tag);
                shortDescription = match.Result("${shortDescription}");
            }
            catch (Exception)
            {
                readOk = false;
            }

            //read title
            string title = string.Empty;
            Regex titleRegex = new Regex(@"\btitle=_(?<title>[^_]*)_\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            try
            {
                Match match = titleRegex.Match((string)control.Tag);
                title = match.Result("${title}");
            }
            catch (Exception)
            {
                readOk = false;
            }

            //read long description
            string longDescription = string.Empty;
            Regex longDescriptionRegex = new Regex(@"\blongDescription=_(?<longDescription>[^_]*)_\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            try
            {
                Match match = longDescriptionRegex.Match((string)control.Tag);
                longDescription = match.Result("${longDescription}");
            }
            catch (Exception)
            {
                readOk = false;
            }

            //read media files
            Regex mediaFileRegex = new Regex(@"\bmediaFile=_(?<mediaFile>[^_]*)_\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            int iStart = 0;
            bool keepGoing = true;
            List<string> mediaFiles = new List<string>();
            do
            {
                try
                {
                    Match match = mediaFileRegex.Match((string)control.Tag, iStart);
                    iStart = match.Index + match.Length;
                    string mediaFile = match.Result("${mediaFile}");
                    mediaFiles.Add(mediaFile);
                    if (iStart >= ((string)control.Tag).Length - 1)
                    {
                        keepGoing = false;
                    }
                }
                catch (Exception)
                {
                    keepGoing = false;
                    readOk = false;
                }
            } while (keepGoing);

            if (readOk)
            {
                return new MilestoneInfo(milestoneId, shortDescription, title, longDescription, mediaFiles.ToArray(), control);
            }
            else
            {
                return null;
            }
        }

        private MilestoneInfo MilestoneInfoByControl(Control control)
        {
            for (int iMilestoneInfo = 0; iMilestoneInfo < milestoneInfos.Count; iMilestoneInfo++)
            {
                if (milestoneInfos[iMilestoneInfo].Control == control)
                {
                    return milestoneInfos[iMilestoneInfo];
                }
            }
            return null;
        }

        private void ClickedMapMilestoneControl(MilestoneInfo milestoneInfo)
        {
            Control control = milestoneInfo.Control;
            if (lastClickedControl != control)
            {
                if (lastClickedControl != null)
                {
                    Storyboard storyboard = (Storyboard)lastClickedControl.Template.Resources["unselect"];
                    storyboard.Begin((Grid)lastClickedControl.Template.FindName("mainGrid", lastClickedControl));
                }

                {
                    Storyboard storyboard = (Storyboard)control.Template.Resources["select"];
                    storyboard.Begin((Grid)control.Template.FindName("mainGrid", control));
                }

                lastClickedControl = control;
            }
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

                //create MilestoneInfos
                ExtractMilestoneInfos(missionUserControl, milestoneInfos);
                
                //make sure we get called when the user clicks on a milestone in the map
                for (int iMilestoneInfo = 0; iMilestoneInfo < milestoneInfos.Count; iMilestoneInfo++)
                {
                    milestoneInfos[iMilestoneInfo].Control.PreviewMouseDown += new MouseButtonEventHandler(milestoneInfoControl_PreviewMouseDown);
                }

                //add mission timeline
                string[] timelineStrings = new string[milestoneInfos.Count];
                for (int iMilestoneInfo = 0; iMilestoneInfo < milestoneInfos.Count; iMilestoneInfo++)
                {
                    timelineStrings[iMilestoneInfo] = milestoneInfos[iMilestoneInfo].ShortDesctiption;
                }
                missionTimeline.Initialise(timelineStrings);
            }

            if (milestoneInfos.Count > 0)
            {
                //select the first milestone initially
                milestoneInfos[0].Control.ApplyTemplate();
                ClickedMapMilestoneControl(milestoneInfos[0]);
            }
        }

        //this gets called when the user clicks on an element representing a milestone
        void milestoneInfoControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() == typeof(Control) || sender.GetType().IsSubclassOf(typeof(Control)))
            {
                //if the user clicks on an element representing a milestone we need to select it in the timeline
                MilestoneInfo milestoneInfo = MilestoneInfoByControl((Control)sender);
                missionTimeline.SelectMilestone(milestoneInfo.Id);

                ClickedMapMilestoneControl(milestoneInfo);
            }
        }

        //this gets called when the user selects a milestone in the timeline
        void missionTimeline_ClickedMilestoneEvent(object sender, MissionTimelineUserControl.ClickedMilestoneEventArgs e)
        {
            MilestoneInfo milestoneInfo = milestoneInfos[e.milestoneId];
            ClickedMapMilestoneControl(milestoneInfo);
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

        #endregion

    }
}
