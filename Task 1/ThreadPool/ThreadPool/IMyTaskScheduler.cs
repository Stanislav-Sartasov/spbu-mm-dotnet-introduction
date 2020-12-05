using System;
using ThreadPool.Tasks;

namespace ThreadPool
{
    public interface IMyTaskScheduler : IDisposable
    {
        IMyTask Enqueue(Action action);

        IMyTask<TResult> Enqueue<TResult>(Func<TResult> function);
    }
}
