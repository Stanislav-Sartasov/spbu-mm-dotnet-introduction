using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace ThreadPool.Sources
{
public class MyThreadPool : IDisposable
{
    private readonly Thread[] _workers;
    private readonly BlockingCollection<IExecutable> _taskQueue;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly CancellationToken _myCancellationToken;
    private bool _disposed;
    private readonly object _lock = new();

    public MyThreadPool(int numOfThreads)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _myCancellationToken = _cancellationTokenSource.Token;
        _taskQueue = new BlockingCollection<IExecutable>(new ConcurrentQueue<IExecutable>());
        _workers = new Thread[numOfThreads];
        for (var i = 0; i < _workers.Length; i++)
        {
            _workers[i] = new Thread(CreateWorker());
            _workers[i].Start();
        }
    }

    public IMyTask<TResult> Enqueue<TResult>(Func<TResult> action)
    {
        if (_myCancellationToken.IsCancellationRequested)
            throw new InvalidOperationException("ThreadPool has been disposed");

        var task = new MyTask<TResult>(this, action);
        Debug.Print($"MyThreadPool: Task {task} has been created");
        _taskQueue.Add(task);
        Debug.Print($"MyThreadPool: Task {task} has been added");
        return task;
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed)
                throw new ObjectDisposedException("Thread pool has been disposed");

            _cancellationTokenSource.Cancel();
            Debug.Print("MyThreadPool: Thread pool waits for workers to join...");
            foreach (var worker in _workers) worker.Join();
            _cancellationTokenSource.Dispose();
            Debug.Print("MyThreadPool: Thread pool has been disposed");
            _disposed = true;
        }
    }

    private ThreadStart CreateWorker() =>
        () =>
        {
            Debug.Print($"MyThreadPool: Thread {Thread.CurrentThread.ManagedThreadId} started");
            var myToken = _cancellationTokenSource.Token;
            while (true)
            {
                IExecutable executable;
                try
                {
                    executable = _taskQueue.Take(myToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                Debug.Print($"MyThreadPool: Task {executable} was taken by {Thread.CurrentThread.ManagedThreadId}");
                executable.Execute();
            }

            Debug.Print($"MyThreadPool: Thread {Thread.CurrentThread.ManagedThreadId} exited");
        };

    private interface IExecutable
    {
        void Execute();
    }

    private class MyTask<TResult> : IMyTask<TResult>, IExecutable
    {
        private readonly MyThreadPool _myPool;
        private readonly ManualResetEventSlim _completedEvent = new();
        private volatile State _state = State.Waiting;
        private TResult? _result;
        private AggregateException? _failure;

        private readonly Func<TResult> _delegate;

        private enum State : byte
        {
            Waiting = 0,
            Success = 1,
            Failure = 2
        }

        public MyTask(MyThreadPool myPool, Func<TResult> myDelegate)
        {
            _myPool = myPool;
            _delegate = myDelegate;
        }

        public bool IsCompleted => _state != State.Waiting;

        public TResult Result
        {
            get
            {
                switch (_state)
                {
                    case State.Waiting:
                        _completedEvent.Wait();
                        return Result;
                    case State.Success:
                        return _result!;
                    case State.Failure:
                        throw _failure!;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation) =>
            _myPool.Enqueue(() => continuation(Result));

        public void Execute()
        {
            try
            {
                _result = _delegate.Invoke();
                _state = State.Success;
            }
            catch (Exception e)
            {
                _failure = new AggregateException(e);
                _state = State.Failure;
            }

            _completedEvent.Set();
        }
    }
}
}