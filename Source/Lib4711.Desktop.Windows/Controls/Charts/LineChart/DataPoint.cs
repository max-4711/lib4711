namespace Lib4711.Desktop.Windows.Controls.Charts.LineChart
{
    public class DataPoint:NotifierBase
    {
        private double m_Value = new double();
        public double Value
        {
            get { return m_Value; }
            set
            {
                SetProperty(ref m_Value, value);
            }
        }
    }
}
