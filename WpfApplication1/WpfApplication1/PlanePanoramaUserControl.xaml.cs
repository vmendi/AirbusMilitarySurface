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
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for PlanePanoramaUserControl.xaml
    /// </summary>
    public partial class PlanePanoramaUserControl : UserControl
    {

        #region Data

        private DateTime lastTick;

        private bool isMouseDown = false;
        private Point lastMousePos;

        Quaternion cameraAngularVelocity = new Quaternion(new Vector3D(0.0f, 1.0f, 0.0f), 0.0f);
        private readonly float cameraAngularVelocityDamp = 0.95f;
        private readonly float cameraAngularVelocityMax = 1.0f;
        private readonly float cameraRotationInputMultiplier = 1.0f;

        #endregion

        #region Properties

        public static readonly DependencyProperty PanoramaImageProperty = DependencyProperty.Register("PanoramaImageProperty", typeof(ImageSource), typeof(PlanePanoramaUserControl));

        public ImageSource PanoramaImage
        {
            get { return (ImageSource)GetValue(PanoramaImageProperty); }
            set
            {
                SetValue(PanoramaImageProperty, value);

                Model3D a = ((Model3DGroup)((ModelVisual3D)world.Children[0]).Content).Children[3];
                GeometryModel3D g = (GeometryModel3D)((Model3DGroup)a).Children[0];
                MaterialGroup materialGroup = (MaterialGroup)g.BackMaterial;
                DiffuseMaterial mat = (DiffuseMaterial)materialGroup.Children[0];
                ImageBrush brush = (ImageBrush)mat.Brush;
                brush.ImageSource = value;
            }
        }

        #endregion

        #region Construction

        public PlanePanoramaUserControl()
        {
            InitializeComponent();

            //read settings
            cameraRotationInputMultiplier = WpfApplication1.Properties.Settings.Default.PanoramaCameraRotationInputMultiplier;
            cameraAngularVelocityMax = WpfApplication1.Properties.Settings.Default.PanoramaCameraAngularVelocityMax;

            //attach ourselves to rendering event
            System.Windows.Media.CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }

        #endregion

        #region Methods

        private Quaternion ClampAngle(Quaternion value, float min, float max)
        {
            if (value.Angle > max) return new Quaternion(value.Axis, max);
            if (value.Angle < min) return new Quaternion(value.Axis, min);
            return value;
        }

        #endregion

        #region Method handlers

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            Point p = e.GetPosition(viewport);
            double mouseDeltaX = p.X - lastMousePos.X;
            lastMousePos = p;

            if (isMouseDown)
            {
                Quaternion delta = new Quaternion(new Vector3D(0.0f, 1.0f, 0.0f), mouseDeltaX);

                //apply delta to camera angular velocity
                cameraAngularVelocity = Quaternion.Slerp(cameraAngularVelocity, delta, 0.1f);

                //clamp camera angular velocity
                float cameraAngularVelocityMaxScale = 5.0f;
                cameraAngularVelocity = ClampAngle(cameraAngularVelocity, -cameraAngularVelocityMax * cameraAngularVelocityMaxScale, cameraAngularVelocityMax * cameraAngularVelocityMaxScale);
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            viewport.CaptureMouse();

            isMouseDown = true;

            Point p = e.GetPosition(viewport);
            lastMousePos = p;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            isMouseDown = false;

            viewport.ReleaseMouseCapture();
        }

        #endregion

        #region Event handlers

        //this gets called once per frame
        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            TimeSpan elapsed = now - lastTick;
            lastTick = now;

            //get the current orientantion from the RotateTransform3D
            RotateTransform3D rt = (RotateTransform3D)myPerspectiveCamera.Transform;
            AxisAngleRotation3D r = (AxisAngleRotation3D)rt.Rotation;

            // Apply angular velocity to camera
            Quaternion q = new Quaternion(r.Axis, r.Angle);

            //calculate velocity quaternion given passed time
            Quaternion v = new Quaternion(cameraAngularVelocity.Axis, cameraAngularVelocity.Angle * elapsed.Milliseconds * 0.05f * cameraRotationInputMultiplier);

            //apply velocity quaternion
            q *= v;
            r.Axis = q.Axis;
            r.Angle = q.Angle;

            //damp camera angular velocity
            Quaternion zeroVelocity = new Quaternion(cameraAngularVelocity.Axis, 0.0f);
            cameraAngularVelocity = Quaternion.Slerp(cameraAngularVelocity, zeroVelocity, elapsed.Milliseconds * 0.01f * cameraAngularVelocityDamp);
        }

        #endregion

    }
}
