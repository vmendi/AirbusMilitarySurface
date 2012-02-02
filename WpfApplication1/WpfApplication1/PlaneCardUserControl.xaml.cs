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
    /// Interaction logic for PlaneCardUserControl.xaml
    /// </summary>
    public partial class PlaneCardUserControl : UserControl
    {
        public static readonly DependencyProperty CardImageProperty = DependencyProperty.Register("CardImageProperty", typeof(ImageSource), typeof(PlaneCardUserControl));

        public ImageSource CardImage
        {
            get { return (ImageSource)GetValue(CardImageProperty); }
            set
            {
                SetValue(CardImageProperty, value);
                cardImage.Source = value;
            }
        }

        public PlaneCardUserControl()
        {
            InitializeComponent();
        }
    }
}
