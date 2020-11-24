using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Task1
{
    class ThreadPool : IDisposable
    {
        private const int _defaultSize = 10;
        private readonly List<Thread> _pool;
        private readonly object _lock = new object();
        private bool _isDisposed = false;

        private readonly BlockingCollection<Action> _waitingTasks = new BlockingCollection<Action>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        public int Size { get; }

        public ThreadPool(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentException("Invalid ThreadPool size", nameof(size));
            }
            Size = size;
            _pool = new List<Thread>(size);

            for (var i = 0; i < size; i++)
            {
                var worker = new Thread(Work);
                worker.Start();
                _pool.Add(worker);
            }
        }

        public ThreadPool() : this(_defaultSize)
        {
        }

        public void Enqueue<TResult>(IMyTask<TResult> task)
        {
            CheckDisposed();
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task), "Cannot run null in ThreadPool");
            }
            _waitingTasks.Add(task.Run);
        }

        public void CheckDisposed()
        {
            lock (_lock)
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(ThreadPool), "The ThreadPool has been disposed.");
                }
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_isDisposed)
                    return;

                _isDisposed = true;
                _waitingTasks.CompleteAdding();
                foreach(var thread in _pool)
                {
                    thread.Join();
                }
                _waitingTasks.Dispose();
            }
        }

        private void Work()
        {
            while (true)
            {
                try
                {
                    var task = _waitingTasks.Take();
                    task.Invoke();
                }
                catch (InvalidOperationException e)
                {
                    return;
                }
                catch (Exception)
                {
                    //do nothing
                }
            }
        }
    }
}
