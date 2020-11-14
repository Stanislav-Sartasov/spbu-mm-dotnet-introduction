using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ThreadPool
{
    public class ThreadPoolExecutor: IExecutor
    {
        private static readonly uint MinWorkersCount = 1;

        private volatile bool _canEnqueueTasks;
        private readonly Action _actionOnDispose;
        private readonly List<Thread> _workers;
        private readonly ConcurrentQueue<IPoolWork> _workToProcess;
        private readonly CancellationTokenSource _cancellationTokenSource;
        
        public ThreadPoolExecutor(uint workersCount)
        {
            if (workersCount < MinWorkersCount)
                throw new ArgumentException($"Workers count must be >= {MinWorkersCount}");

            _canEnqueueTasks = true;
            _workers = new List<Thread>();
            _workToProcess = new ConcurrentQueue<IPoolWork>();
            _cancellationTokenSource = new CancellationTokenSource();

            for (var i = 0; i < workersCount; i++)
            {
                var worker = new Thread(WorkerJob);
                worker.Start();
                _workers.Add(worker);
            }
        }

        public ThreadPoolExecutor(uint workersCount, Action actionOnDispose): this(workersCount)
        {
            _actionOnDispose = actionOnDispose;
        }

        public ITask<TResult> Enqueue<TResult>(Func<TResult> action)
        {
            var poolTask = new PoolTask<TResult>(action, null, this);

            EnqueueWork(poolTask);
            
            return poolTask;
        }

        public void Dispose()
        {
            _canEnqueueTasks = false;
            _cancellationTokenSource.Cancel();
            _actionOnDispose?.Invoke();             // Cool hack to test abort feature

            foreach (var worker in _workers)
            {
                worker.Join();
            }

            foreach (var task in _workToProcess)
            {
                task.Abort();
            }
            
            _cancellationTokenSource.Dispose();
        }
        
        public override string ToString()
        {
            return $"ThreadPoolExecutor [workers={_workers}]";
        }

        private void WorkerJob()
        {
            var token = _cancellationTokenSource; 
            
            while (!token.IsCancellationRequested)
            {
                if (_workToProcess.TryDequeue(out var work))
                {
                    if (work.CanExecute())
                    {
                        work.Execute();
                    }
                    else
                    {
                        _workToProcess.Enqueue(work);
                    }
                }
                else
                {
                    Thread.Yield();
                }
            }   
            
            Debug.Print($"Safely terminate thread: {Thread.CurrentThread.ManagedThreadId}");
        }

        private void EnqueueWork(IPoolWork work)
        {
            if (!_canEnqueueTasks)
                throw new Exception($"ThreadPool {this} cannot enqueue tasks");
                
            _workToProcess.Enqueue(work);
        }
        
        private interface IPoolWork
        {
            public abstract void Execute();
            public abstract bool CanExecute();
            public abstract void Abort();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        private class PoolTask<TResult>: ITask<TResult>, IPoolWork
        {
            private volatile bool _isCompleted;
            private volatile bool _wasAborted;
            private readonly Func<bool> _conditionToStart;
            private readonly Func<TResult> _action;
            private readonly ThreadPoolExecutor _executor;
            private TResult _result;
    
            public PoolTask(Func<TResult> action, Func<bool> conditionToStart, ThreadPoolExecutor executor)
            {
                _isCompleted = false;
                _wasAborted = false;
                _action = action;
                _conditionToStart = conditionToStart;
                _executor = executor;
            }

            public void Execute()
            {
                _result = _action();
                _isCompleted = true;
            }

            public bool CanExecute()
            {
                return _conditionToStart?.Invoke() ?? true;
            }

            public void Abort()
            {
                _wasAborted = true;
            }

            public bool IsCompleted()
            {
                return _isCompleted;
            }

            public TResult GetResult()
            {
                while (!_isCompleted)
                {
                    if (_wasAborted)
                        throw new AggregateException($"Task {ToString()} was aborted");

                    Thread.Yield();
                }

                return _result;
            }

            public Func<TResult> GetAction()
            {
                return _action;
            }

            public ITask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> nextAction)
            {
                TNewResult Action() => nextAction(GetResult());
                var poolTask = new PoolTask<TNewResult>(Action, IsCompleted, _executor);

                _executor.EnqueueWork(poolTask);
                
                return poolTask;
            }
            
            public override string ToString()
            {
                return $"PoolTask [action={_action}]";
            }
        }
    }
}