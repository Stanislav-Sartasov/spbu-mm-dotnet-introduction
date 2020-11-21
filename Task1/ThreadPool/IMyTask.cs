using System;

namespace MyThreadPool
{
    public interface IMyTaskBase
    {
        void Invoke();
    }

    public interface IMyTask<TResult> : IMyTaskBase
    {
        bool IsCompleted { get; }
        TResult Result { get; }
        IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> f);
    }
}
