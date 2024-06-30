using AviaProfileWpf.Claculators;
using AviaProfileWpf.Models;
using AviaProfileWpf.ViewModels;
using LSGL.Core.Maths.Splines.Interpolation;
using System.Collections.ObjectModel;
using System.DirectoryServices.ActiveDirectory;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AviaProfileWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Coords? airpalneProfile;
        private ObservableCollection<CalculationResult> collectionС, collectionF;
        private List<CalculationResultView> context;
        private CubicSpline? spl_y, spl_x;

        public MainWindow()
        {
            InitializeComponent();
            InitializeData();
        }
        /// <summary>
        /// функция создание объектов
        /// </summary>
        private void InitializeData()
        {
            collectionF = new ObservableCollection<CalculationResult>()
            {
                new CalculationResult(new MaxDataCalculator()){
                    ResultName = "f",
                    ResultSuperscript="max"
                },
                new CalculationResult(new MaxDataCalculator()){
                    ResultName = "f__x",
                    ResultSuperscript="max"
                }
            };
            collectionС = new ObservableCollection<CalculationResult>()
            {
                new CalculationResult(new MaxDataCalculator()){
                    ResultName = "с",
                    ResultSuperscript="max"
                },
                new CalculationResult(new MaxDataCalculator()){
                    ResultName = "с__x",
                    ResultSuperscript="max"
                }
            };

            context = new List<CalculationResultView>() {
                new CalculationResultView("Максимальная толщина:", collectionС),
                new CalculationResultView("Максимальная кривизна:", collectionF),
            };
            lvCalcData.DataContext = context;
            
        }
 
        private void btnNewFile_Click(object sender, RoutedEventArgs e)
        {
            ClearAll();
            InitializeData();
        }

        /// <summary>
        /// Функция очистки данных
        /// </summary>
        private void ClearAll()
        {
            airpalneProfile = null;
            spl_y = spl_x = null;
            //pfMain.Segments.Clear();
            paintSurface.Children.Clear();
        }

        /// <summary>
        /// Функция чтения файла
        /// </summary>
        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Document"; 
            dialog.DefaultExt = ".dat"; 
            dialog.Filter = "DAT documents (.dat)|*.dat"; 

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                DatFileLoader datFileLoader = new DatFileLoader() { Source = dialog.FileName };
                airpalneProfile = datFileLoader.Load();
                if (airpalneProfile == null) { return; }
                UpdateCalculatedData();
                spl_x = CubicSpline.InterpolateNaturalSorted(airpalneProfile.T, airpalneProfile.X);
                spl_y = CubicSpline.InterpolateNaturalSorted(airpalneProfile.T, airpalneProfile.Y);
                airpalneProfile.Count = (int)slDensityChanger.Value;
                UpdateGraficalData();
            }
        }

        /// <summary>
        /// Функция обносления рисунка
        /// </summary>
        private void UpdateGraficalData()
        {
            paintSurface.Children.Clear();
            //pfMain.Segments.Clear(); убрал path surface так как возникает баг при изменении количества точек с отрисовкой не существующих линий
            if (airpalneProfile == null || spl_x == null || spl_y == null) return;
            PointCollection points = new PointCollection();
            double x_beg = 0, y_beg = 0, x_prev = 0, y_prev = 0;
            for (int i = 0; i < airpalneProfile.T.Length-1; i++)
            {
                //вычисляем значения и центрируем относительно решётки
                if (i == 0) 
                {
                    x_beg = x_prev = spl_x.Interpolate(airpalneProfile.T[i]) * 200 + contentGrid.ColumnDefinitions[1].ActualWidth / 2;
                    y_beg = y_prev = spl_x.Interpolate(airpalneProfile.T[i]) * 200 + contentGrid.RowDefinitions[0].ActualHeight / 2;
                    continue;
                }
                Line line = new Line();
                line.X1 = x_prev;
                line.Y1 = y_prev;
                x_prev = spl_x.Interpolate(airpalneProfile.T[i]) * 200 + contentGrid.ColumnDefinitions[1].ActualWidth / 2;
                y_prev = spl_x.Interpolate(airpalneProfile.T[i]) * 200 + contentGrid.RowDefinitions[0].ActualHeight / 2;
                line.X2 = x_prev;
                line.Y2 = y_prev;
                line.Stroke = Brushes.LightSteelBlue;
                line.StrokeThickness = 5;
                paintSurface.Children.Add(line);
                //points.Add(new Point(x_prev, y_prev);
            }
            Line lastLine = new Line();
            lastLine.X1 = x_prev;
            lastLine.Y1 = y_prev;
            lastLine.X2 = x_beg;
            lastLine.Y2 = y_beg;
            lastLine.Stroke = Brushes.LightSteelBlue;
            lastLine.StrokeThickness = 5;
            paintSurface.Children.Add(lastLine);
            //pfMain.StartPoint = points[0];
            //points.RemoveAt(0);
            //PolyBezierSegment pbs = new PolyBezierSegment(points, true);
            //pfMain.Segments.RemoveAt(0);
            //pfMain.Segments.Add(pbs);

        }
       
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateGraficalData();
        }

        

        private void slDensityChanger_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (airpalneProfile == null) return;
            airpalneProfile.Count = (int)slDensityChanger.Value;
            UpdateGraficalData();  
        }

        /// <summary>
        /// Функция обновлени вычисялемых данных на форме
        /// </summary>
        private void UpdateCalculatedData()
        {
            if (airpalneProfile == null) return;
            airpalneProfile.CalculateCurveAndThin();
            context[0].Results[0].DataSource = airpalneProfile.C_y;
            context[0].Results[1].DataSource = airpalneProfile.C_x;
            context[1].Results[0].DataSource = airpalneProfile.F_y;
            context[1].Results[1].DataSource = airpalneProfile.F_x;
        }
    }
     
}