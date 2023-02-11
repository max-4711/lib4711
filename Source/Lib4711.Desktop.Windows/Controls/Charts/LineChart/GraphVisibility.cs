using System.Windows.Media;

namespace Lib4711.Desktop.Windows.Controls.Charts.LineChart
{
    public class GraphVisibility: NotifierBase
    {
        private SolidColorBrush m_BackColor = Brushes.Red;
        public SolidColorBrush BackColor
        {
            get { return m_BackColor; }
            set
            {
                SetProperty(ref m_BackColor, value);
            }
        }
    }
}
