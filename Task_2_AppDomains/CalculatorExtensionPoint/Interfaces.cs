using System;

namespace CalculatorExtensionPoint
{
public interface ICalculator
{
    int Sum(int a, int b);
}

public interface IFileSystemAccessor
{
    bool TestWriteAccessToFileSystem(out Exception? reason);
    bool TestReadAccessToFileSystem(out Exception? reason);
}
}