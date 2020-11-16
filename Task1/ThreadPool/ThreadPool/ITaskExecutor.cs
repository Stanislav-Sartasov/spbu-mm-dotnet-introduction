using System;

namespace ThreadPool
{
    /// <summary>
    /// Thread-safe executor interface for tasks scheduling for background processing.
    /// </summary>
    public interface ITaskExecutor: IDisposable
    {
        public abstract ITask<TResult> Enqueue<TResult>(Func<TResult> action);
    }
}