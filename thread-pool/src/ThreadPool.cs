using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace ThreadPool 
{
    public class ThreadPool: IDisposable
    {
        public readonly int threadAmount;
        private List<Thread> threads = new List<Thread>();
        private ConcurrentQueue<IRunnable> tasks = new ConcurrentQueue<IRunnable>();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public ThreadPool(int threadAmount) 
        {
            if (threadAmount <= 0) throw new Exception("Thread amount must be positive number");
            
            this.threadAmount = threadAmount;

            for (int i = 0; i < threadAmount; i++)
            {
                Thread thread = new Thread(ThreadLoop);
                thread.IsBackground = true;
                threads.Add(thread);
            }

            for (int i = 0; i < threadAmount; i++)
                threads[i].Start();
        }

        public IMyTask<TResult> Enqueue<TResult>(Func<TResult> f)
        {
            if (!cancellationTokenSource.IsCancellationRequested) 
            {
                var task = new ThreadPoolTask<TResult>(f, this);
                tasks.Enqueue(task);
                return task;
            }
            throw new Exception("Unable to enqueue task after disposing");
        }

        public void Dispose() 
        {
            cancellationTokenSource.Cancel();

            for (int i = 0; i < threadAmount; i++)
                threads[i].Join();
            
            cancellationTokenSource.Dispose();
        }

        private void ThreadLoop() 
        {
            // loop termination condition: Dispose() was invoked AND tast queue is empty 
            while (!cancellationTokenSource.IsCancellationRequested || !tasks.IsEmpty)
            {
                IRunnable currentTask;
                bool wasTaskExtracted = tasks.TryDequeue(out currentTask);
                if (wasTaskExtracted)
                    currentTask.Run();
                else Thread.Yield();
            }
        }
    }
}