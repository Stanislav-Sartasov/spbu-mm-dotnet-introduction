using Homework2;

namespace CalculatorImplementation
{
    public sealed class DeeplyBrokenCalculator : ICalculator
    {
        public int Sum(int a, int b) => Sum(a, b);
    }
}
