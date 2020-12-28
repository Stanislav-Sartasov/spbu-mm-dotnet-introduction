using System;
using System.Threading;

namespace Homework1
{
    internal sealed class MyTask<T> : IMyTask<T>, IMyTaskExistentialWrapper
    {
        private MyThreadPool Pool { get; }
        private ManualResetEventSlim TaskCompletedAwaitPoint { get; } = new();
        internal event Action TaskCompletedEvent;
        public TaskState State { get; internal set; } = TaskState.Executing;
        private T MyResult { get; set; }
        private Exception CrashReason { get; set; }
        private Func<T> Delegate { get; }

        public MyTask(MyThreadPool pool, Func<T> myDelegate)
        {
            Pool = pool;
            Delegate = myDelegate;
        }

        public bool IsCompleted => State != TaskState.Executing;

        public T Result
        {
            get
            {
                switch (State)
                {
                    case TaskState.WaitingForDependency:
                    case TaskState.Ready:
                    case TaskState.Executing:
                        TaskCompletedAwaitPoint.Wait();
                        return Result;
                    case TaskState.Finished:
                        return MyResult!;
                    case TaskState.Crashed:
                        throw CrashReason!;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<T, TNewResult> continuation) =>
            Pool.EnqueueDependent(continuation, this);

        public void Execute()
        {
            try
            {
                MyResult = Delegate();
                State = TaskState.Finished;
            }
            catch (Exception e)
            {
                CrashReason = new AggregateException(e);
                State = TaskState.Crashed;
            }

            TaskCompletedAwaitPoint.Set();
            TaskCompletedEvent?.Invoke();
        }
    }
}
