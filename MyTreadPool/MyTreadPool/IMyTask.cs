using System;


namespace MyThreadPool
{
    public interface IMyTask<TResult>
    {
        bool IsComplete { get; }

        TResult Result { get; }

        AggregateException Exception { get; }

        void Start();

        IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> moreCalculations);
    }
}
