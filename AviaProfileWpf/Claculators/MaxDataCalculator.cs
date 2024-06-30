using AviaProfileWpf.Calculators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviaProfileWpf.Claculators
{
    /// <summary>
    /// Вычисляющий класс для определения максимального значения серди объектов
    /// </summary>
    public class MaxDataCalculator : ICalculator<double>
    {
        /// <summary>
        /// Вычисляем в массиве data максимальное значение с использованием linq
        /// </summary>
        /// <param name="data">массив элементов типа double</param>
        /// <returns>максимальное значение этого массива</returns>
        public double Calculate(double[] data)
        {
            return data.Max();
        }
    }
}
