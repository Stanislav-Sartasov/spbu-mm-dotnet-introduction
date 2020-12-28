using System;
using System.Collections.Concurrent;
using System.Threading;
using JetBrains.Annotations;

namespace Homework1
{
    public sealed class MyThreadPool : IDisposable
    {
        [NotNull, ItemNotNull]
        private Thread[] Workers { get; }

        [NotNull]
        private CancellationTokenSource CancellationTokenSource { get; }

        private CancellationToken CancellationToken => CancellationTokenSource.Token;

        [NotNull]
        private BlockingCollection<IMyTaskExistentialWrapper> Queue { get; }

        [NotNull]
        private object Locker { get; } = new object();

        private bool IsDisposed { get; set; }

        public MyThreadPool(int threadCount)
        {
            CancellationTokenSource = new CancellationTokenSource();
            Queue = new BlockingCollection<IMyTaskExistentialWrapper>(new ConcurrentQueue<IMyTaskExistentialWrapper>());
            Workers = new Thread[threadCount];
            for (int index = 0; index < Workers.Length; index++)
            {
                Workers[index] = new Thread(() =>
                {
                    var myToken = CancellationTokenSource.Token;
                    while (true)
                    {
                        IMyTaskExistentialWrapper myTaskExistentialWrapper;
                        try
                        {
                            myTaskExistentialWrapper = Queue.Take(myToken);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }

                        if (myTaskExistentialWrapper.State == TaskState.WaitingForDependency)
                        {
                            Queue.Add(myTaskExistentialWrapper);
                            continue;
                        }

                        myTaskExistentialWrapper.Execute();
                    }
                });
                Workers[index].Start();
            }
        }

        [NotNull]
        public IMyTask<T> Enqueue<T>([NotNull] Func<T> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (CancellationToken.IsCancellationRequested)
                throw new OperationCanceledException();

            var task = new MyTask<T>(this, action);
            Queue.Add(task, CancellationToken);
            return task;
        }

        [NotNull]
        internal IMyTask<U> EnqueueDependent<T, U>([NotNull] Func<T, U> continuation, [NotNull] MyTask<T> dependency)
        {
            if (dependency == null) throw new ArgumentNullException(nameof(dependency));
            if (CancellationToken.IsCancellationRequested)
                throw new OperationCanceledException();

            var task = new MyTask<U>(this, () => continuation(dependency.Result))
            {
                State = TaskState.WaitingForDependency
            };

            void MarkTaskAsReady()
            {
                task.State = TaskState.Ready;
                dependency.TaskCompletedEvent -= MarkTaskAsReady;
            }

            dependency.TaskCompletedEvent += MarkTaskAsReady;
            Queue.Add(task, CancellationToken);
            return task;
        }

        public void Dispose()
        {
            lock (Locker)
            {
                if (IsDisposed) throw new ObjectDisposedException("");
                CancellationTokenSource.Cancel();
                foreach (var worker in Workers) worker.Join();
                CancellationTokenSource.Dispose();
                IsDisposed = true;
            }
        }
    }
}
