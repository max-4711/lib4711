using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lib4711.Desktop.Windows.Controls.Charts.LineChart
{
    /// <summary>
    /// Interaction logic for YAxisLabels.xaml
    /// </summary>
    internal partial class YAxisLabels : UserControl
    {
        public YAxisLabels()
        {
            InitializeComponent();
        }

        public SolidColorBrush LineColor
        {
            get { return (SolidColorBrush)GetValue(LineColorProperty); }
            set { SetValue(LineColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineColorProperty =
            DependencyProperty.Register("LineColor", typeof(SolidColorBrush), typeof(YAxisLabels), new PropertyMetadata(Brushes.Black));

        public double YLocation
        {
            get { return (double)GetValue(YLocationProperty); }
            set { SetValue(YLocationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for YLocation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty YLocationProperty =
            DependencyProperty.Register("YLocation", typeof(double), typeof(YAxisLabels), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string YLabel
        {
            get { return (string)GetValue(YLabelProperty); }
            set { SetValue(YLabelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for YLabel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty YLabelProperty =
            DependencyProperty.Register("YLabel", typeof(string), typeof(YAxisLabels), new PropertyMetadata("test"));

        public bool YLabelVisible
        {
            get { return (bool)GetValue(YLabelVisibleProperty); }
            set { SetValue(YLabelVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for YLabelVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty YLabelVisibleProperty =
            DependencyProperty.Register("YLabelVisible", typeof(bool), typeof(YAxisLabels), new PropertyMetadata(true));

    }
}
