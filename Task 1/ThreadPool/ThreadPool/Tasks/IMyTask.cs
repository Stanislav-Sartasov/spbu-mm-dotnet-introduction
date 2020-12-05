using System;

namespace ThreadPool.Tasks
{
    public interface IMyTask<TResult> : IMyTask
    {
        TResult Result { get; }

        IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> transformer);
    }
}
