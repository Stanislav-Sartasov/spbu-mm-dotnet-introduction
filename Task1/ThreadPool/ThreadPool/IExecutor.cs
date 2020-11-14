using System;

namespace ThreadPool
{
    public interface IExecutor: IDisposable
    {
        public abstract ITask<TResult> Enqueue<TResult>(Func<TResult> action);
    }
}