using System;

namespace ThreadPool.Sources
{
public interface IMyTask<out TResult>
{
    bool IsCompleted { get; }

    TResult Result { get; }

    IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation);
}
}