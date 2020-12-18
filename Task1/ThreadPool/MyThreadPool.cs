using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ThreadPool;

namespace MyThreadPool
{
    public class MyThreadPool : IDisposable
    {
        private readonly List<Thread> myPool;
        private readonly object myDisposeLock = new object();
        private readonly CancellationTokenSource myCancelToken = new CancellationTokenSource();
        private readonly BlockingCollection<Action> myQueue = new BlockingCollection<Action>();

        public int Capacity { get; }

        public MyThreadPool(int capacity)
        {
            // Check input capacity arg
            if (capacity <= 0) throw new ArgumentException("Invalid capacity", nameof(capacity));

            Capacity = capacity;
            myPool = new List<Thread>(Capacity);

            // Then create and run threads
            for (var i = 0; i < capacity; i++)
            {
                var worker = new Thread(() => Worker(myCancelToken.Token))
                {
                    Name = $"worker {i}",
                    IsBackground = true
                };
                worker.Start();
                myPool.Add(worker);
            }
        }

        private void Worker(CancellationToken token)
        {
            try
            {
                // try to get and execute task from queue until token is canceled
                foreach (var task in myQueue.GetConsumingEnumerable(token))
                    task();
            }
            catch (OperationCanceledException)
            {
                // calculate remaining tasks in queue and stop thread
                foreach (var task in myQueue.GetConsumingEnumerable())
                    task();
            }
        }

        public void Enqueue<TResult>(IMyTask<TResult> task)
        {
            CheckDisposed();
            if (task == null) 
                throw new ArgumentNullException(nameof(task), "Task cannot be null");
            myQueue.Add(task.Execute);
        }

        public bool IsDisposed => myCancelToken.IsCancellationRequested;
        public void Dispose()
        {
            lock (myDisposeLock)
            {
                if (IsDisposed) return;

                myCancelToken.Cancel();
                myQueue.CompleteAdding();
                myPool.ForEach(thread =>
                {
                    try
                    {
                        thread.Join();
                    }
                    catch (Exception)
                    {
                        // skip
                    }
                });
                myPool.Clear();
                myQueue.Dispose();
                myCancelToken.Dispose();
            }
        }

        private void CheckDisposed()
        {
            lock (myDisposeLock)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(nameof(MyThreadPool), "The ThreadPool has been disposed.");
            }
        }
    }
}
