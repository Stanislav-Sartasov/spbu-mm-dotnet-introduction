using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ThreadPool.Tasks;

namespace ThreadPool
{
    public class MyThreadPool : IMyTaskScheduler
    {
        private readonly ConcurrentQueue<IMyTask> _taskQueue;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;

        private readonly List<Thread> _workingThreads;

        public MyThreadPool(int capacity = 4)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            _workingThreads = new List<Thread>();
            _taskQueue = new ConcurrentQueue<IMyTask>();

            for (int i = 0; i < capacity; i++)
            {
                Thread currentThread = new Thread(ProcessQueue);
                currentThread.Start();

                _workingThreads.Add(currentThread);
            }
        }

        public IMyTask Enqueue(Action action)
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                throw new ObjectDisposedException(typeof(MyThreadPool).Name);
            }

            if (action == null)
            {
                throw new ArgumentNullException();
            }

            IMyTask task = new MyTask(action, _cancellationToken, this);
            _taskQueue.Enqueue(task);

            return task;
        }

        public IMyTask<TResult> Enqueue<TResult>(Func<TResult> function)
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                throw new ObjectDisposedException(typeof(MyThreadPool).Name);
            }

            if (function == null)
            {
                throw new ArgumentNullException();
            }

            IMyTask<TResult> task = new MyTask<TResult>(function, _cancellationToken, this);
            _taskQueue.Enqueue(task);

            return task;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _workingThreads.ForEach(thread => thread.Join());
        }

        private void ProcessQueue()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                if (!_taskQueue.TryDequeue(out IMyTask taskToProcess))
                {
                    Thread.Yield();
                    continue;
                }

                taskToProcess.Run();
                foreach (IMyTask child in taskToProcess.Children)
                {
                    _taskQueue.Enqueue(child);
                }
            }
        }
    }
}
