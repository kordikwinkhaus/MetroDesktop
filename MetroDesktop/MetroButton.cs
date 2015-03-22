using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MetroDesktop
{
    public class MetroButton : Button
    {
        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(MetroButton), new PropertyMetadata(null));

        public bool Compact
        {
            get { return (bool)GetValue(CompactProperty); }
            set { SetValue(CompactProperty, value); }
        }

        public static readonly DependencyProperty CompactProperty =
            DependencyProperty.Register("Compact", typeof(bool), typeof(MetroButton), new PropertyMetadata(false));
    }
}
