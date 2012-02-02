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

        private bool isMouseDown = false;
        private Point lastMousePos;

        Quaternion cameraAngularVelocity = new Quaternion(new Vector3D(0.0f, 1.0f, 0.0f), 0.0f);
        private readonly float cameraAngularVelocityDamp = 0.95f;
        private readonly float cameraAngularVelocityMax = 1.0f;

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

            //attach ourselves to rendering event
            System.Windows.Media.CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }

        #endregion

        //this gets called once per frame
        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            //get the current orientantion from the RotateTransform3D
            RotateTransform3D rt = (RotateTransform3D)myPerspectiveCamera.Transform;
            AxisAngleRotation3D r = (AxisAngleRotation3D)rt.Rotation;

            r.Angle = (DateTime.Now.Second * 1000.0f + DateTime.Now.Millisecond) * 0.01f;

        }
    }
}
