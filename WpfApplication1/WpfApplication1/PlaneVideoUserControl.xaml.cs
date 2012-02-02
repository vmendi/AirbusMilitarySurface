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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for PlaneVideoUserControl.xaml
    /// </summary>
    public partial class PlaneVideoUserControl : UserControl
    {

        public static readonly DependencyProperty MediaFileProperty = DependencyProperty.Register("MediaFileProperty", typeof(Uri), typeof(PlaneVideoUserControl));

        public Uri MediaFile
        {
            get { return (Uri)GetValue(MediaFileProperty); }
            set
            {
                SetValue(MediaFileProperty, value);
                mediaElement.Source = value;
            }
        }

        public PlaneVideoUserControl()
        {
            InitializeComponent();
        }
    }
}
