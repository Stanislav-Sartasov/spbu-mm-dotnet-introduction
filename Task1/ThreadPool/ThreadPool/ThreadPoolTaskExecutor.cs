using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ThreadPool
{
    public class ThreadPoolTaskExecutor: ITaskExecutor
    {
        private static readonly uint MinWorkersCount = 1;

        private volatile bool _canEnqueueTasks;
        private readonly Action _actionOnDispose;
        private readonly List<Thread> _workers;
        private readonly ConcurrentQueue<IPoolWork> _workToProcess;
        private readonly CancellationTokenSource _cancellationTokenSource;
        
        public ThreadPoolTaskExecutor(uint workersCount)
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

        public ThreadPoolTaskExecutor(uint workersCount, Action actionOnDispose): this(workersCount)
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
                task.Abort(new Exception($"{this} was disposed. Enqueued tasks won't be processed"));
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
                throw new Exception($"{this} cannot enqueue tasks");
                
            _workToProcess.Enqueue(work);
        }
        
        private interface IPoolWork
        {
            public void Execute();
            public bool CanExecute();
            public void Abort(Exception cause);
        }

        private class PoolTask<TResult>: ITask<TResult>, IPoolWork
        {
            private volatile bool _isCompleted;
            private volatile bool _wasAborted;
            private volatile AggregateException _abortCause;
            private readonly Func<bool> _conditionToStart;
            private readonly Func<TResult> _action;
            private readonly ThreadPoolTaskExecutor _taskExecutor;
            private TResult _result;
    
            public PoolTask(Func<TResult> action, Func<bool> conditionToStart, ThreadPoolTaskExecutor taskExecutor)
            {
                _isCompleted = false;
                _wasAborted = false;
                _action = action;
                _conditionToStart = conditionToStart;
                _taskExecutor = taskExecutor;
            }

            public void Execute()
            {
                try
                {
                    _result = _action();
                    _isCompleted = true;
                }
                catch (Exception e)
                {
                    _abortCause = new AggregateException($"{this} execution was finished with exception", e);
                    _wasAborted = true;
                }   
            }

            public bool CanExecute()
            {
                return _conditionToStart?.Invoke() ?? true;
            }

            public void Abort(Exception cause)
            {
                _abortCause = new AggregateException($"{this} was explicitly aborted, because of", cause);
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
                        throw _abortCause;

                    Thread.Yield();
                }

                return _result;
            }

            public ITask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> nextAction)
            {
                TNewResult Action() => nextAction(GetResult());
                var poolTask = new PoolTask<TNewResult>(Action, IsCompleted, _taskExecutor);

                _taskExecutor.EnqueueWork(poolTask);
                
                return poolTask;
            }
            
            public override string ToString()
            {
                return $"PoolTask [action={_action}]";
            }
        }
    }
}