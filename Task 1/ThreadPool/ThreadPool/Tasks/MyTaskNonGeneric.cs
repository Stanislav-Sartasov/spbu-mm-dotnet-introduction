using System;
using System.Collections.Generic;
using System.Threading;

namespace ThreadPool.Tasks
{
    internal class MyTask : IMyTask
    {
        private readonly Action _storedAction;

        protected object _completedLock;
        protected bool _isCompleted;

        protected CancellationToken _cancellationToken;
        protected IMyTaskScheduler _scheduler;
        protected List<IMyTask> _children;

        public IEnumerable<IMyTask> Children => _children;

        public bool IsCompleted
        {
            get
            {
                return _isCompleted;
            }

            protected set
            {
                lock (_completedLock)
                {
                    _isCompleted = value;
                }
            }
        }

        protected MyTask() { }

        public MyTask(Action action, CancellationToken token, IMyTaskScheduler scheduler)
        {
            _storedAction = action;

            _completedLock = new object();
            _children = new List<IMyTask>();
            _isCompleted = false;

            _cancellationToken = token;
            _scheduler = scheduler;
        }

        public virtual void Run()
        {
            _storedAction();
            IsCompleted = true;
        }

        public IMyTask ContinueWith(Action action)
        {
            // If current task is not ready yet, delay scheduling child task
            lock (_completedLock)
            {
                if (!IsCompleted)
                {
                    IMyTask nextTask = new MyTask(action, _cancellationToken, _scheduler);
                    _children.Add(nextTask);

                    return nextTask;
                }
            }

            // Otherwise, just schedule it right away
            IMyTask scheduledTask = _scheduler.Enqueue(action);
            _children.Add(scheduledTask);

            return scheduledTask;
        }

        public void Await()
        {
            while (!IsCompleted)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                Thread.Yield();
            }
        }
    }
}
