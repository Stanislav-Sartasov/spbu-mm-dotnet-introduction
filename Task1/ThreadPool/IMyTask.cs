using System;
using System.Collections.Generic;
using System.Text;

namespace Task1
{
    public interface IMyTask<out TResult> : IDisposable
    {
        TResult Result { get; }
        bool IsCompleted { get; }

        void Run();

        IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation);
    }
}
