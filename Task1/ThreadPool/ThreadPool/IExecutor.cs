using System;

namespace ThreadPool
{
    /// <summary>
    /// Thread-safe executor interface for scheduling task for background processing.
    /// </summary>
    public interface IExecutor: IDisposable
    {
        public abstract ITask<TResult> Enqueue<TResult>(Func<TResult> action);
    }
}