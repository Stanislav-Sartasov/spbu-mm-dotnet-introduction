using System;
using System.Collections.Generic;
using System.Threading;

namespace ThreadPool.Tasks
{
    internal class MyTask<TResult> : MyTask, IMyTask<TResult>
    {
        private TResult _result;
        private readonly Func<TResult> _storedFunction;

        public TResult Result
        {
            get
            {
                while (!IsCompleted)
                {
                    _cancellationToken.ThrowIfCancellationRequested();
                    Thread.Yield();
                }

                return _result;
            }

            set
            {
                _result = value;
            }
        }

        public MyTask(Func<TResult> function, CancellationToken token, IMyTaskScheduler scheduler)
        {
            _storedFunction = function;

            _completedLock = new object();
            _children = new List<IMyTask>();
            _isCompleted = false;

            _cancellationToken = token;
            _scheduler = scheduler;
        }

        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> function)
        {
            // If current task is not ready yet, delay scheduling child task
            lock (_completedLock)
            {
                if (!IsCompleted)
                {
                    MyTask<TNewResult> nextTask = new MyTask<TNewResult>(() => function(Result), _cancellationToken, _scheduler);
                    _children.Add(nextTask);

                    return nextTask;
                }
            }

            // Otherwise, just schedule it right away
            IMyTask<TNewResult> scheduledTask = _scheduler.Enqueue(() => function(Result));
            _children.Add(scheduledTask);

            return scheduledTask;
        }

        public override void Run()
        {
            _result = _storedFunction();
            IsCompleted = true;
        }
    }
}
