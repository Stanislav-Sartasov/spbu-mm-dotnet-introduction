using System;
using System.Collections.Concurrent;
using System.Threading;

namespace MyThreadPool
{
    public class MyThreadPool:IDisposable
    {

        private Thread[] workingPool;
        private CancellationTokenSource source;
        private ConcurrentQueue<Action> queue;

        public bool IsDisposed { private set; get; }

        public MyThreadPool(int threadsCount)
        {
            Console.WriteLine("Start with " + threadsCount + " threads");

            if (threadsCount <= 0)
            {
                throw new ArgumentException("Threads count must be positive");
            }
            IsDisposed = false;

            source = new CancellationTokenSource();
            workingPool = new Thread[threadsCount];
            queue = new ConcurrentQueue<Action>();

            for(int i = 0; i < threadsCount; i++)
            {
                workingPool[i] = new Thread(ProcessTasks);
                workingPool[i].Start(source.Token);
            }
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException("Threadpool has been disposed");
            }

            IsDisposed = true;
            source.Cancel();

            for(int i = 0; i < workingPool.Length; i++)
            {
                workingPool[i].Join();
            }

            source.Dispose();
            queue.Clear();
            queue = null;
            workingPool = null;
        }

        public void Enqueue<TResult>(IMyTask<TResult> task)
        {
            if (!IsDisposed)
            {
                queue.Enqueue(task.Start);
            } else
            {
                throw new ObjectDisposedException("Threadpool has been disposed");
            }
            
        }

        private void ProcessTasks(object token)
        {
            CancellationToken cancellationToken = (CancellationToken) token;

            while(!cancellationToken.IsCancellationRequested || !queue.IsEmpty)
            {
                Action task;

                if (queue.TryDequeue(out task))
                {
                    task.Invoke();
                }

            }
        }
    }
}
