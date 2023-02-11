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
        private static readonly List<SolidColorBrush> distinctColorList = new();

        public LineChart()
        {
            InitializeComponent();

            ColourGenerator generator = new();
            for (int i = 0; i < 20; i++)
            {
                distinctColorList.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#" + generator.NextColour())));
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
                typeof(LineChart), new PropertyMetadata(new ObservableCollection<YAxisLabels>()));

        private ObservableCollection<GraphVisibility> CurveVisibility
        {
            get { return (ObservableCollection<GraphVisibility>)GetValue(CurveVisibilityProperty); }
            set { SetValue(CurveVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyCheckBoxes.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty CurveVisibilityProperty =
            DependencyProperty.Register("CurveVisibility", typeof(ObservableCollection<GraphVisibility>),
                typeof(LineChart), new PropertyMetadata(new ObservableCollection<GraphVisibility>()));

        private static object? GetPropValue(object src, string propName)
        {
            return src.GetType()?.GetProperty(propName)?.GetValue(src, null);
        }

        public static void Redraw(LineChart basicChart)
        {
            basicChart.Dispatcher.Invoke(() =>
            {
                SetUpYAxis(basicChart);
                SetUpGraph(basicChart, basicChart.ItemsSource);
            });
        }

        public static void SetUpYAxis(LineChart d)
        {
            var MyClass = (LineChart)d;

            // Only calcualte the min and max values if AutoScale is true
            if (MyClass.AutoScale)
            {
                double TempYMax, TempYMin;
                TempYMax = double.MinValue;
                TempYMin = double.MaxValue;

                if (MyClass.ItemsSource == null) return;
                foreach (var ClassItem in MyClass.ItemsSource)
                {
                    IEnumerable collectionItem = GetPropValue(ClassItem, MyClass.DataCollectionName) as IEnumerable ?? Array.Empty<object>();
                    foreach (var item in collectionItem)
                    {
                        double value = GetPropValue(item, MyClass.DisplayMemberValues) as double? ?? 0.0d;
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

        public static void SetUpGraph(LineChart sender, IEnumerable? itemsSource)
        {
            List<PointCollection> listOfChartCurves = new();
            List<double> originalValues = new();

            if (itemsSource == null) return;

            //Loop trough all the sources
            foreach (object? classItem in itemsSource)
            {
                PointCollection pointsOnChart = new();
                int x = 0;

                // Get the Collection of dataitems from the current source
                IEnumerable collectionItems = GetPropValue(classItem, sender.DataCollectionName) as IEnumerable ?? Array.Empty<object>();

                // For all the chart points, get the relevant Y values
                foreach (object? item in collectionItems)
                {
                    object? yValues = GetPropValue(item, sender.DisplayMemberValues);

                    // No X value filters are applied
                    if (sender.XMin == sender.XMax)
                    {

                        if (yValues is double @double)
                        {
                            originalValues.Add(@double);
                            if (double.IsInfinity(@double) || double.IsNaN(@double) || double.IsNegativeInfinity(@double) || double.IsPositiveInfinity(@double))
                            {
                                pointsOnChart.Add(new Point(0, double.NaN));

                            }
                            else
                            {
                                double YValue = (@double - sender.YMin) / (sender.YMax - sender.YMin) * sender.PlotHeight;
                                pointsOnChart.Add(new Point(0, YValue));
                            }
                        }
                    }
                    else if (sender.XMin <= x && x <= sender.XMax)
                    {

                        if (yValues is double @double)
                        {
                            originalValues.Add(@double);
                            if (double.IsInfinity(@double) || double.IsNaN(@double) || double.IsNegativeInfinity(@double) || double.IsPositiveInfinity(@double))
                            {
                                pointsOnChart.Add(new Point(0, double.NaN));

                            }
                            else
                            {
                                double YValue = (@double - sender.YMin) / (sender.YMax - sender.YMin) * sender.PlotHeight;
                                pointsOnChart.Add(new Point(0, YValue));
                            }
                        }
                    }


                    x++;
                }
                listOfChartCurves.Add(pointsOnChart);
            }

            ObservableCollection<FrameworkElement> items = new();

            for (int k = 0; k < listOfChartCurves.Count; k++)
            {
                for (int i = 0; i < listOfChartCurves[k].Count; i++)
                {
                    double pos = (double)i * sender.PlotWidth / (double)listOfChartCurves[k].Count;
                    listOfChartCurves[k][i] = new Point(pos, listOfChartCurves[k][i].Y);
                }
            }

            for (int k = 0; k < listOfChartCurves.Count; k++)
            {
                PointCollection PointsOnChart = listOfChartCurves[k];
                List<PointCollection> MyLines = new();
                PointCollection CurrentCollection = new();

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
                    Polyline Curve = new()
                    {
                        Points = item,
                        StrokeThickness = 4,
                        Stroke = distinctColorList[k]
                    };
                    items.Add(Curve);
                }                
            }

            sender.PlotArea.ItemsSource = items;
        }

        private static void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e, IEnumerable eNewValue)
        {
            var myClass = (LineChart)sender;

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                myClass.CurveVisibility.Add(new GraphVisibility()
                {
                    BackColor = distinctColorList[myClass.CurveVisibility.Count],
                });
                ((INotifyPropertyChanged)myClass.CurveVisibility[^1]).PropertyChanged
                    += (s, ee) => OnCurveVisibilityChanged(myClass, eNewValue);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                ((INotifyPropertyChanged)myClass.CurveVisibility[e.OldStartingIndex]).PropertyChanged
                    -= (s, ee) => OnCurveVisibilityChanged(myClass, eNewValue);
                myClass.CurveVisibility.RemoveAt(e.OldStartingIndex);
            }


            if (myClass.DisplayMemberValues != "" && myClass.DataCollectionName != "")
            {
                SetUpYAxis(myClass);
                SetUpGraph(myClass, eNewValue);
            }
        }

        private static void OnCurveVisibilityChanged(LineChart sender, IEnumerable NewValues)
        {
            SetUpGraph(sender, NewValues);
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var MyBasicChart = (LineChart)d;

            foreach (var item in MyBasicChart.ItemsSource)
            {
                int i = MyBasicChart.CurveVisibility.Count;
                // Set up a Notification if the IsChecked property is changed
                MyBasicChart.CurveVisibility.Add(new GraphVisibility() { BackColor = distinctColorList[i] });

                ((INotifyPropertyChanged)MyBasicChart.CurveVisibility[^1]).PropertyChanged +=
                    (s, ee) => OnCurveVisibilityChanged(MyBasicChart, (IEnumerable)e.NewValue);
            }

            if (e.NewValue != null)
            {
                // Assuming that the curves are binded using an ObservableCollection, 
                // it needs to update the Layout if items are added, removed etc.
                if (e.NewValue is INotifyCollectionChanged changed)
                    changed.CollectionChanged += (s, ee) =>
                    ItemsSource_CollectionChanged(MyBasicChart, ee, (IEnumerable)e.NewValue);
            }

            if (e.OldValue != null)
            {
                // Unhook the Event
                if (e.OldValue is INotifyCollectionChanged changed)
                    changed.CollectionChanged -=
                        (s, ee) => ItemsSource_CollectionChanged(MyBasicChart, ee, (IEnumerable)e.OldValue);

            }

            // Check that the properties to bind to is set
            if (MyBasicChart.DisplayMemberValues != "" && MyBasicChart.DataCollectionName != "")
            {
                SetUpYAxis(MyBasicChart);
                SetUpGraph(MyBasicChart, e.NewValue as IEnumerable);
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
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(LineChart),
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
                typeof(LineChart), new PropertyMetadata(true));

        public string DataCollectionName
        {
            get { return (string)GetValue(DataCollectionNameProperty); }
            set { SetValue(DataCollectionNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DataCollectionName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataCollectionNameProperty =
            DependencyProperty.Register("DataCollectionName", typeof(string),
                typeof(LineChart), new PropertyMetadata(""));

        public string DisplayMemberValues
        {
            get { return (string)GetValue(DisplayMemberValuesProperty); }
            set { SetValue(DisplayMemberValuesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayMemberValues.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayMemberValuesProperty =
            DependencyProperty.Register("DisplayMemberValues", typeof(string),
                typeof(LineChart), new PropertyMetadata(""));

        public double XMin
        {
            get { return (double)GetValue(XMinProperty); }
            set { SetValue(XMinProperty, value); }
        }

        // Using a DependencyProperty as the backing store for XMin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty XMinProperty =
            DependencyProperty.Register("XMin", typeof(double),
                typeof(LineChart), new PropertyMetadata(0d));

        public double XMax
        {
            get { return (double)GetValue(XMaxProperty); }
            set { SetValue(XMaxProperty, value); }
        }

        // Using a DependencyProperty as the backing store for XMax.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty XMaxProperty =
            DependencyProperty.Register("XMax", typeof(double),
                typeof(LineChart), new PropertyMetadata(0d));

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
            SetUpYAxis((LineChart)d);
        }

        public string DoubleToString
        {
            get { return (string)GetValue(DoubleToStringProperty); }
            set { SetValue(DoubleToStringProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DoubleToString.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DoubleToStringProperty =
            DependencyProperty.Register("DoubleToString", typeof(string),
                typeof(LineChart), new PropertyMetadata("N2",
                    new PropertyChangedCallback(YAxisValuesChanged)));

        // Using a DependencyProperty as the backing store for NumberOfYSteps.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NumberOfYStepsProperty =
            DependencyProperty.Register("NumberOfYSteps", typeof(int),
                typeof(LineChart), new PropertyMetadata(5,
                    new PropertyChangedCallback(YAxisValuesChanged)));

        // Using a DependencyProperty as the backing store for YMin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty YMinProperty =
            DependencyProperty.Register("YMin", typeof(double),
                typeof(LineChart), new PropertyMetadata(0d,
                    new PropertyChangedCallback(YAxisValuesChanged)));

        public double YMax
        {
            get { return (double)GetValue(YMaxProperty); }
            set { SetValue(YMaxProperty, value); }
        }

        // Using a DependencyProperty as the backing store for YMax.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty YMaxProperty =
            DependencyProperty.Register("YMax", typeof(double),
                typeof(LineChart), new PropertyMetadata(5d,
                    new PropertyChangedCallback(YAxisValuesChanged)));

        public string ChartTitle
        {
            get { return (string)GetValue(ChartTitleProperty); }
            set { SetValue(ChartTitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ChartTitle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChartTitleProperty =
            DependencyProperty.Register("ChartTitle", typeof(string),
                typeof(LineChart), new PropertyMetadata(""));

        public double PlotWidth
        {
            get { return (double)GetValue(PlotWidthProperty); }
            set { SetValue(PlotWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PlotWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlotWidthProperty =
            DependencyProperty.Register("PlotWidth", typeof(double), typeof(LineChart), new PropertyMetadata(400d));

        public double PlotHeight
        {
            get { return (double)GetValue(PlotHeightProperty); }
            set { SetValue(PlotHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PlotHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlotHeightProperty =
            DependencyProperty.Register("PlotHeight", typeof(double),
                typeof(LineChart), new PropertyMetadata(170d));
        #endregion

        #region "Color generator"
        /// <summary>
        /// http://stackoverflow.com/questions/309149/generate-distinctly-different-rgb-colors-in-graphs/309193#309193
        /// </summary>
        public class ColourGenerator
        {
            private int index = 0;
            private readonly IntensityGenerator intensityGenerator = new();

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
                return (index % 7) switch
                {
                    0 => "{0}0000",
                    1 => "00{0}00",
                    2 => "0000{0}",
                    3 => "{0}{0}00",
                    4 => "{0}00{0}",
                    5 => "00{0}{0}",
                    6 => "{0}{0}{0}",
                    _ => throw new Exception("Math error"),
                };
            }
        }

        public class IntensityGenerator
        {
            private IntensityValueWalker? walker;
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

            private IntensityValue? mChildA;
            private IntensityValue? mChildB;

            public IntensityValue(IntensityValue? parent, int value, int level)
            {
                if (level > 7) throw new Exception("There are no more colours left");
                Value = value;
                Parent = parent;
                Level = level;
            }

            public int Level { get; set; }
            public int Value { get; set; }
            public IntensityValue? Parent { get; set; }

            public IntensityValue ChildA
            {
                get
                {
                    return mChildA ??= new IntensityValue(this, this.Value - (1 << (7 - Level)), Level + 1);
                }
            }

            public IntensityValue ChildB
            {
                get
                {
                    return mChildB ??= new IntensityValue(this, Value + (1 << (7 - Level)), Level + 1);
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