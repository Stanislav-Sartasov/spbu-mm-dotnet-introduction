using System;
using System.IO;
using CalculatorExtensionPoint;

namespace CalculatorExtension
{
public abstract class BaseCalculator : MarshalByRefObject, ICalculator, IFileSystemAccessor
{
    public int Sum(int a, int b)
    {
        return a + b;
    }

    public bool TestReadAccessToFileSystem(out Exception? reason)
    {
        try
        {
            new DirectoryInfo(Directory.GetCurrentDirectory()).GetFiles();
            reason = null;
            return true;
        }
        catch (Exception e)
        {
            reason = e;
            return false;
        }
    }

    public bool TestWriteAccessToFileSystem(out Exception? reason)
    {
        try
        {
            var handler = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid().ToString()));
            var t = handler.Create();
            t.Dispose();
            handler.Delete();
            reason = null;
            return true;
        }
        catch (Exception e)
        {
            reason = e;
            return false;
        }
    }
}

public class Calculator1 : BaseCalculator
{
}

public class Calculator2 : BaseCalculator
{
}
}