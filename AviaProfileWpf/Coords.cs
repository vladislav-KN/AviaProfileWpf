using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviaProfileWpf
{
    /// <summary>
    /// Класс хранения данных о точках
    /// </summary>
    public class Coords
    {
        private double[] x;
        private double[] y;
        private double[] xLo;
        private double[] yLo;
        private double[] xUp;
        private double[] yUp;
        private double[] f_y;
        private double[] f_x;
        private double[] c_y;
        private double[] c_x;
        private double[] t;
        private int count;

        public Coords(double[] _x, double[] _y, double[] _xUp, double[] _yUp, double[] _xLow, double[] _yLow) 
        {
            x= _x;
            y= _y;
            xUp = _xUp;
            yUp = _yUp;
            xLo = _xLow;
            yLo = _yLow;
            f_y = new double[_xLow.Length];
            c_y = new double[_xLow.Length];
            f_x = new double[_xLow.Length];
            c_x = new double[_xLow.Length];
            t = new double[x.Length];
            for (int i = 0; i < t.Length - 1; i++, t[i] = i);
        }

        public double[] XLo { get => xLo; }
        public double[] YLo { get => yLo; }
        public double[] XUp { get => xUp; }
        public double[] YUp { get => yUp; }
        public double[] F_y { get => f_y; }
        public double[] F_x { get => f_x; }
        public double[] C_y { get => c_y; }
        public double[] C_x { get => c_x; }
        public double[] X { get => x; }
        public double[] Y { get => y; }
        public double[] T { get => t; }

        /// <summary>
        /// Изменение массива T для увеличения количества точек
        /// </summary>
        public int Count 
        {
            get
            {
                return count;
            }
            set
            {
                if (value >= 10)
                {
                    count = value;
                    double meanDistance = (t.Last() - t.First()) / (count - 1);
                    t = Enumerable.Range(0, count).Select(x => x * meanDistance).ToArray();
                }
            }
        }
        /// <summary>
        /// Вычисление массивов c и f
        /// </summary>
        public void CalculateCurveAndThin()
        {
            for(int i = 0; i<xLo.Length; i++)
            {
                f_x[i] = 0.5 * (xLo[i] + xUp[i]);
                f_y[i] = 0.5 * (yLo[i] + yUp[i]);
                c_y[i] = yUp[i] - yLo[i];
                c_x[i] = xUp[i] - xLo[i];
            }
        }
    }
}
