namespace AviaProfileWpf.Calculators
{
    /// <summary>
    /// Интерфейс для вычисляющих классов, используется для иньекции зависимости
    /// </summary>
    /// <typeparam name="T">T любой объект который будет поступать в качестве массива данных в функцию calculate</typeparam>
    public interface ICalculator<T>
    {
        public T Calculate(T[] data);
    }
}