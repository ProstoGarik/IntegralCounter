using LiveCharts.Defaults;
using LiveCharts.Wpf;
using LiveCharts;
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
using IntegralCounterLibrary;
using System.Diagnostics;

namespace IntegralCounterWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int ProcCount = Environment.ProcessorCount;
        private CounterMethods counterMethods;
        public SeriesCollection SeriesCollection { get; set; }
        Stopwatch Stopwatch;
        double starterTolerance = 0.1;
        public Func<double, string> XAxisFormatter { get; set; }
        public Func<double, string> YAxisFormatter { get; set; }
        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            counterMethods = new CounterMethods();
            Stopwatch = new Stopwatch();
            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Обычный",
                    Values = new ChartValues<ObservablePoint>{ }
                },
                new LineSeries
                {
                    Title = "Сегментный",
                    Values = new ChartValues<ObservablePoint>{ }
                },
                new LineSeries
                {
                    Title = "Thread",
                    Values = new ChartValues<ObservablePoint>{ }
                },
                new LineSeries
                {
                    Title = "Task",
                    Values = new ChartValues<ObservablePoint>{ }
                }
            };

            initializeProcessors();

            Chart.Series = SeriesCollection;
            XAxisFormatter = value => $"{value:0.000}";
            YAxisFormatter = value => $"{value:0.##} ms";


        }
        private void initializeProcessors()
        {
            procCountComboBox.Items.Clear();
            for (int i = 1; i < ProcCount + 1; i++)
            {
                procCountComboBox.Items.Add(i);
            }
            procCountComboBox.SelectedItem = 1;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SeriesCollection[0].Values.Clear();
            SeriesCollection[1].Values.Clear();
            SeriesCollection[2].Values.Clear();
            SeriesCollection[3].Values.Clear();

            Stopwatch.Start();
            double start = Convert.ToDouble(LowerBorder.Text);
            double end = Convert.ToDouble(UpperBorder.Text);

            double tempTolerance = starterTolerance;

            for (double i = 1; i < 9; i++)
            {
                Stopwatch.Restart();
                counterMethods.Integrate(counterMethods.ParabolaGetY1, start, end, tempTolerance);
                double logTolerance = Math.Abs(Math.Log10(tempTolerance));
                SeriesCollection[0].Values.Add(new ObservablePoint(logTolerance, Stopwatch.Elapsed.TotalMilliseconds));

                Stopwatch.Restart();
                counterMethods.IntegrateWithSegments(start, end, ProcCount, tempTolerance);
                SeriesCollection[1].Values.Add(new ObservablePoint(logTolerance, Stopwatch.Elapsed.TotalMilliseconds));

                Stopwatch.Restart();
                counterMethods.IntegrateThreads(start, end, Convert.ToInt32(procCountComboBox.SelectedItem), tempTolerance);
                SeriesCollection[2].Values.Add(new ObservablePoint(logTolerance, Stopwatch.Elapsed.TotalMilliseconds));

                Stopwatch.Restart();
                counterMethods.IntegrateParallel(start, end, Convert.ToInt32(procCountComboBox.SelectedItem), tempTolerance);
                SeriesCollection[3].Values.Add(new ObservablePoint(logTolerance, Stopwatch.Elapsed.TotalMilliseconds));

                tempTolerance = starterTolerance * Math.Pow(10, -i);
            }


            
        }
    }
}