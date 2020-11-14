using System;

namespace ThreadPool
{
    public interface ITask<out TResult>
    {
        public abstract bool IsCompleted();
        public abstract TResult GetResult();
        public abstract Func<TResult> GetAction();
        public abstract ITask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> nextAction);
    }
}