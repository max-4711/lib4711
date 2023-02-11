using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Lib4711.Desktop.Windows.Controls.Charts.LineChart
{
    public partial class LineChart : UserControl
    {
        private static List<SolidColorBrush> DistinctColorList = new List<SolidColorBrush>();

        public LineChart()
        {
            InitializeComponent();

            ColourGenerator generator = new ColourGenerator();
            for (int i = 0; i < 20; i++)
            {
                DistinctColorList.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#" + generator.NextColour())));
            }
        }
        private ObservableCollection<YAxisLabels> YItems
        {
            get { return (ObservableCollection<YAxisLabels>)GetValue(YItemsProperty); }
            set { SetValue(YItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for YItems.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty YItemsProperty =
            DependencyProperty.Register("YItems", typeof(ObservableCollection<YAxisLabels>),
                typeof(BasicChart), new PropertyMetadata(new ObservableCollection<YAxisLabels>()));

        private ObservableCollection<GraphVisibility> CurveVisibility
        {
            get { return (ObservableCollection<GraphVisibility>)GetValue(CurveVisibilityProperty); }
            set { SetValue(CurveVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyCheckBoxes.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty CurveVisibilityProperty =
            DependencyProperty.Register("CurveVisibility", typeof(ObservableCollection<GraphVisibility>),
                typeof(BasicChart), new PropertyMetadata(new ObservableCollection<GraphVisibility>()));

        private static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        public static void Redraw(BasicChart basicChart)
        {
            basicChart.Dispatcher.Invoke(() =>
            {
                SetUpYAxis(basicChart);
                SetUpGraph(basicChart, basicChart.ItemsSource);
            });
        }

        public static void SetUpYAxis(BasicChart d)
        {
            var MyClass = (BasicChart)d;

            // Only calcualte the min and max values if AutoScale is true
            if (MyClass.AutoScale)
            {
                double TempYMax, TempYMin;
                TempYMax = double.MinValue;
                TempYMin = double.MaxValue;

                if (MyClass.ItemsSource == null) return;
                foreach (var ClassItem in MyClass.ItemsSource)
                {

                    IEnumerable MyCollectionItem = (IEnumerable)GetPropValue(ClassItem, MyClass.DataCollectionName);
                    foreach (var item in MyCollectionItem)
                    {
                        double value = (double)GetPropValue(item, MyClass.DisplayMemberValues);
                        if (value < TempYMin)
                            TempYMin = value;

                        if (value > TempYMax)
                            TempYMax = value;
                    }

                }

                MyClass.YMax = TempYMax + (TempYMax - TempYMin) * 0.05d;
                MyClass.YMin = TempYMin - (TempYMax - TempYMin) * 0.05d;
            }

            MyClass.YItems = new ObservableCollection<YAxisLabels>();

            double deltaY = (MyClass.YMax - MyClass.YMin) / (MyClass.NumberOfYSteps);

            for (int j = 0; j <= MyClass.NumberOfYSteps; j++)
            {
                var YLabelObject = new YAxisLabels
                {
                    YLabel = (MyClass.YMin + (double)j * deltaY).ToString(MyClass.DoubleToString),
                    YLocation = ((double)j) * (double)MyClass.PlotHeight / (double)MyClass.NumberOfYSteps
                };
                MyClass.YItems.Add(YLabelObject);
            }
        }

        public static void SetUpGraph(BasicChart sender, IEnumerable ItemsSource)
        {
            List<PointCollection> ListOfChartCurves = new List<PointCollection>();
            List<double> origianlValues = new List<double>();

            if (ItemsSource == null) return;

            //Loop trough all the sources
            foreach (var ClassItem in ItemsSource)
            {
                PointCollection PointsOnChart = new PointCollection();
                int X = 0;

                // Get the Collection of dataitems from the current source
                IEnumerable MyCollectionItem = (IEnumerable)GetPropValue(ClassItem, sender.DataCollectionName);

                // For all the chart points, get the relevant Y values
                foreach (var item in MyCollectionItem)
                {
                    var YValues = GetPropValue(item, sender.DisplayMemberValues);

                    // No X value filters are applied
                    if (sender.XMin == sender.XMax)
                    {

                        if (YValues is double)
                        {
                            origianlValues.Add((double)YValues);
                            if (double.IsInfinity((double)YValues) || double.IsNaN((double)YValues) || double.IsNegativeInfinity((double)YValues) || double.IsPositiveInfinity((double)YValues))
                            {
                                PointsOnChart.Add(new Point(0, double.NaN));

                            }
                            else
                            {
                                double YValue = ((double)YValues - sender.YMin) / (sender.YMax - sender.YMin) * sender.PlotHeight;
                                PointsOnChart.Add(new Point(0, YValue));
                            }
                        }
                    }
                    else if (sender.XMin <= X && X <= sender.XMax)
                    {

                        if (YValues is double)
                        {
                            origianlValues.Add((double)YValues);
                            if (double.IsInfinity((double)YValues) || double.IsNaN((double)YValues) || double.IsNegativeInfinity((double)YValues) || double.IsPositiveInfinity((double)YValues))
                            {
                                PointsOnChart.Add(new Point(0, double.NaN));

                            }
                            else
                            {
                                double YValue = ((double)YValues - sender.YMin) / (sender.YMax - sender.YMin) * sender.PlotHeight;
                                PointsOnChart.Add(new Point(0, YValue));
                            }
                        }
                    }


                    X++;
                }
                ListOfChartCurves.Add(PointsOnChart);
            }

            ObservableCollection<FrameworkElement> items = new ObservableCollection<FrameworkElement>();

            for (int k = 0; k < ListOfChartCurves.Count; k++)
            {
                for (int i = 0; i < ListOfChartCurves[k].Count; i++)
                {
                    double pos = (double)i * sender.PlotWidth / (double)ListOfChartCurves[k].Count;
                    ListOfChartCurves[k][i] = new Point(pos, ListOfChartCurves[k][i].Y);
                }
            }

            for (int k = 0; k < ListOfChartCurves.Count; k++)
            {
                PointCollection PointsOnChart = ListOfChartCurves[k];
                List<PointCollection> MyLines = new List<PointCollection>();
                PointCollection CurrentCollection = new PointCollection();

                // Create lines even if points are disjoint/missing
                for (int i = 0; i < PointsOnChart.Count; i++)
                {
                    // Current point is mission
                    if (double.IsNaN(PointsOnChart[i].Y))
                    {
                        // Any values that should be stored in previous points?
                        if (CurrentCollection.Count != 0)
                            MyLines.Add(CurrentCollection.Clone());

                        // Create a new line to store any new valid points found
                        CurrentCollection = new PointCollection();
                    }
                    else
                    {
                        // It's a valid point, add it to the current pointcollection
                        CurrentCollection.Add(PointsOnChart[i]);
                    }
                }

                // Add the last pontcollection, if it has any points in it.
                if (CurrentCollection.Count != 0)
                    MyLines.Add(CurrentCollection.Clone());

                //Draw all the lines found in the curve
                foreach (PointCollection item in MyLines)
                {
                    Polyline Curve = new Polyline
                    {
                        Points = item,
                        StrokeThickness = 4,
                        Stroke = DistinctColorList[k]
                    };
                    items.Add(Curve);
                }                
            }

            sender.PlotArea.ItemsSource = items;
        }

        private static void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e, IEnumerable eNewValue)
        {
            var MyClass = (BasicChart)sender;

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                MyClass.CurveVisibility.Add(new GraphVisibility()
                {
                    BackColor = DistinctColorList[MyClass.CurveVisibility.Count],
                });
                ((INotifyPropertyChanged)MyClass.CurveVisibility[MyClass.CurveVisibility.Count - 1]).PropertyChanged
                    += (s, ee) => OnCurveVisibilityChanged(MyClass, eNewValue);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                ((INotifyPropertyChanged)MyClass.CurveVisibility[e.OldStartingIndex]).PropertyChanged
                    -= (s, ee) => OnCurveVisibilityChanged(MyClass, eNewValue);
                MyClass.CurveVisibility.RemoveAt(e.OldStartingIndex);
            }


            if (MyClass.DisplayMemberValues != "" && MyClass.DataCollectionName != "")
            {
                SetUpYAxis(MyClass);
                SetUpGraph(MyClass, eNewValue);
            }
        }

        private static void OnCurveVisibilityChanged(BasicChart sender, IEnumerable NewValues)
        {
            SetUpGraph(sender, NewValues);
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var MyBasicChart = (BasicChart)d;

            foreach (var item in MyBasicChart.ItemsSource)
            {
                int i = MyBasicChart.CurveVisibility.Count;
                // Set up a Notification if the IsChecked property is changed
                MyBasicChart.CurveVisibility.Add(new GraphVisibility() { BackColor = DistinctColorList[i] });

                ((INotifyPropertyChanged)MyBasicChart.CurveVisibility[MyBasicChart.CurveVisibility.Count - 1]).PropertyChanged +=
                    (s, ee) => OnCurveVisibilityChanged(MyBasicChart, (IEnumerable)e.NewValue);
            }

            if (e.NewValue != null)
            {
                // Assuming that the curves are binded using an ObservableCollection, 
                // it needs to update the Layout if items are added, removed etc.
                if (e.NewValue is INotifyCollectionChanged)
                    ((INotifyCollectionChanged)e.NewValue).CollectionChanged += (s, ee) =>
                    ItemsSource_CollectionChanged(MyBasicChart, ee, (IEnumerable)e.NewValue);
            }

            if (e.OldValue != null)
            {
                // Unhook the Event
                if (e.OldValue is INotifyCollectionChanged)
                    ((INotifyCollectionChanged)e.OldValue).CollectionChanged -=
                        (s, ee) => ItemsSource_CollectionChanged(MyBasicChart, ee, (IEnumerable)e.OldValue);

            }

            // Check that the properties to bind to is set
            if (MyBasicChart.DisplayMemberValues != "" && MyBasicChart.DataCollectionName != "")
            {
                SetUpYAxis(MyBasicChart);
                SetUpGraph(MyBasicChart, (IEnumerable)e.NewValue);
            }
            else
            {
                MessageBox.Show("Values that indicate the X value and the resulting Y value must be given, as well as the name of the Collection");
            }
        }

        private void PlotAreaBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.PlotWidth = PlotAreaBorder.ActualWidth - 40 - 40;
            this.PlotHeight = PlotAreaBorder.ActualHeight - 40 - 40;
            PlotArea.Height = this.PlotHeight;
            PlotArea.Width = this.PlotWidth;
            XAxisLine.Points[1] = new Point(PlotWidth, 0);
            YAxisLine.Points[1] = new Point(0, PlotHeight);

            //Curves.Width = PlotAreaBorder.Width;
            if (this.DisplayMemberValues != "" && this.DataCollectionName != "")
            {
                SetUpYAxis(this);
                SetUpGraph(this, this.ItemsSource);
            }
        }

        #region "DependencyProperties"


        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(BasicChart),
                new FrameworkPropertyMetadata(null,
                    new PropertyChangedCallback(OnItemsSourceChanged)));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public bool AutoScale
        {
            get { return (bool)GetValue(AutoScaleProperty); }
            set { SetValue(AutoScaleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AutoScale.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoScaleProperty =
            DependencyProperty.Register("AutoScale", typeof(bool),
                typeof(BasicChart), new PropertyMetadata(true));

        public string DataCollectionName
        {
            get { return (string)GetValue(DataCollectionNameProperty); }
            set { SetValue(DataCollectionNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DataCollectionName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataCollectionNameProperty =
            DependencyProperty.Register("DataCollectionName", typeof(string),
                typeof(BasicChart), new PropertyMetadata(""));

        public string DisplayMemberValues
        {
            get { return (string)GetValue(DisplayMemberValuesProperty); }
            set { SetValue(DisplayMemberValuesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayMemberValues.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayMemberValuesProperty =
            DependencyProperty.Register("DisplayMemberValues", typeof(string),
                typeof(BasicChart), new PropertyMetadata(""));

        public double XMin
        {
            get { return (double)GetValue(XMinProperty); }
            set { SetValue(XMinProperty, value); }
        }

        // Using a DependencyProperty as the backing store for XMin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty XMinProperty =
            DependencyProperty.Register("XMin", typeof(double),
                typeof(BasicChart), new PropertyMetadata(0d));

        public double XMax
        {
            get { return (double)GetValue(XMaxProperty); }
            set { SetValue(XMaxProperty, value); }
        }

        // Using a DependencyProperty as the backing store for XMax.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty XMaxProperty =
            DependencyProperty.Register("XMax", typeof(double),
                typeof(BasicChart), new PropertyMetadata(0d));

        public double YMin
        {
            get { return (double)GetValue(YMinProperty); }
            set { SetValue(YMinProperty, value); }
        }

        public int NumberOfYSteps
        {
            get { return (int)GetValue(NumberOfYStepsProperty); }
            set { SetValue(NumberOfYStepsProperty, value); }
        }

        private static void YAxisValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SetUpYAxis((BasicChart)d);
        }

        public string DoubleToString
        {
            get { return (string)GetValue(DoubleToStringProperty); }
            set { SetValue(DoubleToStringProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DoubleToString.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DoubleToStringProperty =
            DependencyProperty.Register("DoubleToString", typeof(string),
                typeof(BasicChart), new PropertyMetadata("N2",
                    new PropertyChangedCallback(YAxisValuesChanged)));

        // Using a DependencyProperty as the backing store for NumberOfYSteps.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NumberOfYStepsProperty =
            DependencyProperty.Register("NumberOfYSteps", typeof(int),
                typeof(BasicChart), new PropertyMetadata(5,
                    new PropertyChangedCallback(YAxisValuesChanged)));

        // Using a DependencyProperty as the backing store for YMin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty YMinProperty =
            DependencyProperty.Register("YMin", typeof(double),
                typeof(BasicChart), new PropertyMetadata(0d,
                    new PropertyChangedCallback(YAxisValuesChanged)));

        public double YMax
        {
            get { return (double)GetValue(YMaxProperty); }
            set { SetValue(YMaxProperty, value); }
        }

        // Using a DependencyProperty as the backing store for YMax.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty YMaxProperty =
            DependencyProperty.Register("YMax", typeof(double),
                typeof(BasicChart), new PropertyMetadata(5d,
                    new PropertyChangedCallback(YAxisValuesChanged)));

        public string ChartTitle
        {
            get { return (string)GetValue(ChartTitleProperty); }
            set { SetValue(ChartTitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ChartTitle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChartTitleProperty =
            DependencyProperty.Register("ChartTitle", typeof(string),
                typeof(BasicChart), new PropertyMetadata(""));

        public double PlotWidth
        {
            get { return (double)GetValue(PlotWidthProperty); }
            set { SetValue(PlotWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PlotWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlotWidthProperty =
            DependencyProperty.Register("PlotWidth", typeof(double), typeof(BasicChart), new PropertyMetadata(400d));

        public double PlotHeight
        {
            get { return (double)GetValue(PlotHeightProperty); }
            set { SetValue(PlotHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PlotHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlotHeightProperty =
            DependencyProperty.Register("PlotHeight", typeof(double),
                typeof(BasicChart), new PropertyMetadata(170d));
        #endregion

        #region "Color generator"
        /// <summary>
        /// http://stackoverflow.com/questions/309149/generate-distinctly-different-rgb-colors-in-graphs/309193#309193
        /// </summary>
        public class ColourGenerator
        {
            private int index = 0;
            private IntensityGenerator intensityGenerator = new IntensityGenerator();

            public string NextColour()
            {
                string colour = string.Format(PatternGenerator.NextPattern(index),
                    intensityGenerator.NextIntensity(index));
                index++;
                return colour;
            }
        }

        public class PatternGenerator
        {
            public static string NextPattern(int index)
            {
                switch (index % 7)
                {
                    case 0: return "{0}0000";
                    case 1: return "00{0}00";
                    case 2: return "0000{0}";
                    case 3: return "{0}{0}00";
                    case 4: return "{0}00{0}";
                    case 5: return "00{0}{0}";
                    case 6: return "{0}{0}{0}";
                    default: throw new Exception("Math error");
                }
            }
        }

        public class IntensityGenerator
        {
            private IntensityValueWalker walker;
            private int current;

            public string NextIntensity(int index)
            {
                if (index == 0)
                {
                    current = 255;
                }
                else if (index % 7 == 0)
                {
                    if (walker == null)
                    {
                        walker = new IntensityValueWalker();
                    }
                    else
                    {
                        walker.MoveNext();
                    }
                    current = walker.Current.Value;
                }
                string currentText = current.ToString("X");
                if (currentText.Length == 1) currentText = "0" + currentText;
                return currentText;
            }
        }

        public class IntensityValue
        {

            private IntensityValue mChildA;
            private IntensityValue mChildB;

            public IntensityValue(IntensityValue parent, int value, int level)
            {
                if (level > 7) throw new Exception("There are no more colours left");
                Value = value;
                Parent = parent;
                Level = level;
            }

            public int Level { get; set; }
            public int Value { get; set; }
            public IntensityValue Parent { get; set; }

            public IntensityValue ChildA
            {
                get
                {
                    return mChildA ?? (mChildA = new IntensityValue(this, this.Value - (1 << (7 - Level)), Level + 1));
                }
            }

            public IntensityValue ChildB
            {
                get
                {
                    return mChildB ?? (mChildB = new IntensityValue(this, Value + (1 << (7 - Level)), Level + 1));
                }
            }
        }

        public class IntensityValueWalker
        {

            public IntensityValueWalker()
            {
                Current = new IntensityValue(null, 1 << 7, 1);
            }

            public IntensityValue Current { get; set; }

            public void MoveNext()
            {
                if (Current.Parent == null)
                {
                    Current = Current.ChildA;
                }
                else if (Current.Parent.ChildA == Current)
                {
                    Current = Current.Parent.ChildB;
                }
                else
                {
                    int levelsUp = 1;
                    Current = Current.Parent;
                    while (Current.Parent != null && Current == Current.Parent.ChildB)
                    {
                        Current = Current.Parent;
                        levelsUp++;
                    }
                    if (Current.Parent != null)
                    {
                        Current = Current.Parent.ChildB;
                    }
                    else
                    {
                        levelsUp++;
                    }
                    for (int i = 0; i < levelsUp; i++)
                    {
                        Current = Current.ChildA;
                    }

                }
            }
        }


        #endregion
    }
}