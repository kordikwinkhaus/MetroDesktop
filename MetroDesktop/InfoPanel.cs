using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MetroDesktop
{
    public class InfoPanel : ContentControl
    {
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(InfoPanel), new PropertyMetadata(null));

        public ImageSource TileImage
        {
            get { return (ImageSource)GetValue(TileImageProperty); }
            set { SetValue(TileImageProperty, value); }
        }

        public static readonly DependencyProperty TileImageProperty =
            DependencyProperty.Register("TileImage", typeof(ImageSource), typeof(InfoPanel), new PropertyMetadata(null));

        public bool Compact
        {
            get { return (bool)GetValue(CompactProperty); }
            set { SetValue(CompactProperty, value); }
        }

        public static readonly DependencyProperty CompactProperty =
            DependencyProperty.Register("Compact", typeof(bool), typeof(InfoPanel), new PropertyMetadata(false));
    }
}
