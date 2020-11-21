using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace MyThreadPool
{
    public class ThreadPool : IDisposable
    {
        int                 threadCount;
        bool                isRunning;
        Thread[]            threads;

        Queue<IMyTaskBase>  taskQueue;
        readonly object     queueLock = new object();

        CancellationTokenSource cancelSource;

        public bool HasStopped => cancelSource.IsCancellationRequested;

        public ThreadPool(int threadCount)
        {
            Debug.Assert(threadCount > 0);
            this.threadCount = threadCount;
            this.isRunning = false;
            this.taskQueue = new Queue<IMyTaskBase>();
            this.cancelSource = new CancellationTokenSource();
        }

        public void Start()
        {
            Debug.Assert(!isRunning);

            isRunning = true;
            threads = new Thread[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                var t = new Thread(ProcessThread);
                t.IsBackground = true;
                t.Start(cancelSource.Token);

                threads[i] = t;
            }
        }

        public void Dispose()
        {
            Debug.Assert(!HasStopped);

            if (isRunning)
            {
                cancelSource.Cancel();

                foreach (var t in threads)
                {
                    t.Join();
                }
            }

            taskQueue.Clear();
            taskQueue = null;
            threads = null;
            cancelSource = null;
        }

        public void Enqueue<TResult>(IMyTask<TResult> a)
        {
            lock (queueLock)
            {
                if (HasStopped)
                {
                    throw new Exception("Trying to enqueue a task to a disposed pool");
                }

                taskQueue.Enqueue(a);
            }
        }

        void ProcessThread(object cancelTokenObj)
        {
            CancellationToken cancelToken = (CancellationToken)cancelTokenObj;

            while (true)
            {
                IMyTaskBase task = null;

                lock (queueLock)
                {
                    if (taskQueue.Count > 0)
                    {
                        task = taskQueue.Dequeue();
                    }
                    else if (cancelToken.IsCancellationRequested)
                    {
                        // if need to stop thread pool 
                        // and there are no tasks left
                        break;
                    }
                }

                if (task != null)
                {
                    task.Invoke();
                }
            }
        }
    }
}
