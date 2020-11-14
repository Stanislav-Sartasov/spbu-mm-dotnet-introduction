using System;

namespace ThreadPool
{
    /// <summary>
    /// Thread-safe task interface. Represents wrapped lambda action,
    /// which can be executed in the background, and safely chained with
    /// continue function without blocking to wait for result.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public interface ITask<out TResult>
    {
        public abstract bool IsCompleted();
        public abstract TResult GetResult();
        public abstract Func<TResult> GetAction();
        public abstract ITask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> nextAction);
    }
}