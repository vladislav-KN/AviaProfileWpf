using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviaProfileWpf
{
 
    /// <summary>
    /// Класс загрузки данных
    /// </summary>
    public class DatFileLoader  
    {
        
        public required string Source{ get; set; } //источник (файл)
        /// <summary>
        /// Функция чтения данных из файла
        /// </summary>
        /// <returns>возвращает новый объект типа  Coords либо null в случаях возникновения ошибок</returns>
        public Coords? Load()
        {
            List<double>? x = null, y = null, x_up = null, y_up = null, x_low = null , y_low = null;
            if (File.Exists(Source))
            {
                string[] lines;
                try
                {
                    lines = File.ReadAllLines(Source);
                }
                catch (Exception ex) 
                { 
                    Console.WriteLine(ex.Message);
                    return null;
                }
                x_up = new List<double>();
                y_up = new List<double>();
                x_low = new List<double>();
                y_low = new List<double>();
                x = new List<double>();
                y = new List<double>();
                //наполняем массивы данными, чтобы неповторять цикл несколько раз делаем всё сразу посчле загрузки
                for (var i = 1; i < lines.Length; i++)
                {
                    var line = lines[i];
                    var numbers = line.Split(' ');
                    double xval, yval;
                    if(double.TryParse(numbers[0].Replace("-","-0").Replace(".",","), out xval) && double.TryParse(numbers[1].Replace("-", "-0").Replace(".", ","), out yval))
                    {
                        if (x_low.Count == 0)
                        {
                            x_up.Add(xval);
                            y_up.Add(yval);
                        }
                        else
                        {
                            x_low.Add(xval);
                            y_low.Add(yval);
                        }
                        if (xval == 0)
                        {
                            x_low.Add(xval);
                            y_low.Add(yval);
                        }
                        x.Add(xval);
                        y.Add(yval);
                    }
                }
            }
            else
            {
                return null;
            }
            if (x is null || y is null || x_up is null || y_up is null || x_low is null || y_low is null)
            {
                return null;
            }
            //переворачиваем первые массивы
            x_up.Reverse();
            y_up.Reverse();
            return new Coords(x.ToArray(),y.ToArray(), x_up.ToArray(), y_up.ToArray(), x_low.ToArray(), y_low.ToArray());
        }
    }
}
