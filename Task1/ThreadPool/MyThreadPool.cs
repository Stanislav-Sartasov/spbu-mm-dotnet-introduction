using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ThreadPool
{
    public class MyThreadPool : IDisposable
    {
        private volatile bool _isActive;
        private List<Thread> _threadWorkers;
        private ConcurrentQueue<Action> _actions;
        private CancellationTokenSource _cancellationTokenSource;

        public MyThreadPool(int numberOfThreads)
        {
            if (numberOfThreads <= 0)
            {
                throw new ArgumentException("Illegal number of threads");
            }

            _isActive = true;
            _threadWorkers = new List<Thread>(numberOfThreads);
            _actions = new ConcurrentQueue<Action>();
            _cancellationTokenSource = new CancellationTokenSource();

            for (int i = 0; i < numberOfThreads; i++)
            {
                Thread thread = new Thread(() =>
                {
                    CancellationToken cancellationToken = _cancellationTokenSource.Token;

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        Action action;

                        if (_actions.TryDequeue(out action))
                        {
                            action();
                        }
                    }
                });
                _threadWorkers.Add(thread);
                thread.Start();
            }
        }

        public IMyTask<TResult> Enqueue<TResult>(Func<TResult> func)
        {
            MyTask<TResult> task = new MyTask<TResult>(func, this);
            if (!_isActive)
            {
                throw new Exception("Tread pool is not active");
            }

            _actions.Enqueue(task.ActionToPerform);
            return task;
        }

        public void Dispose()
        {
            _isActive = false;

            while (!_actions.IsEmpty) { }

            _cancellationTokenSource.Cancel();

            foreach (var threadWorker in _threadWorkers)
            {
                threadWorker.Join();
            }

            _cancellationTokenSource.Dispose();
        }
    }
}