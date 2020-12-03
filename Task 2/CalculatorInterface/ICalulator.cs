namespace CalculatorInterface
{
    public interface ICalculator
    {
        string CalculatorName { get; }

        int Sum(int a, int b);
    }
}
