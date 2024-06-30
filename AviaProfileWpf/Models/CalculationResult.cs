using AviaProfileWpf.Calculators;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AviaProfileWpf.Models
{
    /// <summary>
    /// MVVM класс, использующийся для отображения и автоматического обновления объектов в форме 
    /// </summary>
    public class CalculationResult : INotifyPropertyChanged
    {
        private ICalculator<double> calculator;
        private double result;
        private double[] dataSource;
        private string resultName;
        private string resultSuperscript;

        /// <summary>
        /// Конструктор принимающий на вход любой вычисляющий класс типа double
        /// </summary>
        public CalculationResult(ICalculator<double> calculator)
        {
            this.calculator = calculator;
        }

        public string ResultName
        {
            get
            {
                return resultName;
            }
            set
            {
                resultName = value;
                OnPropertyChanged("ResultName");
            }
        }

        public string ResultSuperscript
        {
            get
            {
                return resultSuperscript;
            }
            set
            {
                resultSuperscript = value;
                OnPropertyChanged("ResultSuperscript");
            }
        }

        public string Result 
        { 
            get
            {
                return Math.Round(result, 2).ToString();
            } 
        }

        public double[] DataSource
        {
            set
            {
                dataSource = value;
                result = calculator.Calculate(dataSource);
                OnPropertyChanged("Result");
            }
        }

        /// <summary>
        /// Обновление поля result с использованием объекта calculator
        /// </summary>
        public void Recalculate()
        {
            result = calculator.Calculate(dataSource);
            OnPropertyChanged("Result");
        }

        public ICalculator<double> Calculator { get { return calculator; } }

        /// <summary>
        /// Ивент необходимы для реализации интерфйса INotifyPropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>
        /// Функция для активации ивента
        /// </summary>
        /// <param name="prop">Имя объекта на форме</param>
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}