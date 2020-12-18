using System;
using System.Threading;

namespace ThreadPool
{
    public class MyTask<TResult> : IMyTask<TResult>
    {
        private readonly Func<TResult> myFunction;
        private TResult myResult;
        private Exception myException;
        private bool isDisposed;

        private readonly object myLock = new object();
        private readonly object myDisposeLock = new object();
        private readonly ManualResetEvent mySynchEvent = new ManualResetEvent(false);

        private MyTask(Func<TResult> function)
        {
            if (function == null)
            {
                throw new ArgumentNullException(nameof(function), "Function cannot be null");
            }

            myFunction = function;
        }

        public static IMyTask<TResult> New(Func<TResult> func) => new MyTask<TResult>(func);

        public bool IsCompleted { private set; get; }

        public TResult Result
        {
            get
            {
                // check if task is finished
                lock (myDisposeLock)
                {
                    // wait for result
                    if (!isDisposed) 
                        mySynchEvent.WaitOne(); 
                }

                // check task result
                lock (myLock)
                {
                    if (myException != null) 
                        throw new AggregateException("Task failed", myException);

                    CheckDisposedBeforeCompleted("Unable to get task result");
                    return myResult;
                }
            }
        }

        public void Execute()
        {
            if (IsCompleted) return;

            lock (myLock)
            {
                try
                {
                    CheckDisposedBeforeCompleted("Unable to execute task");
                    myResult = myFunction();
                }
                catch (Exception e)
                {
                    myException = e;
                }
                finally
                {
                    IsCompleted = true;
                    // send event for allow to get result or exception of finished task
                    mySynchEvent.Set(); 
                }
            }
        }

        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> function)
        {
            if (function == null) 
                throw new ArgumentNullException(nameof(function), "Function cannot be null");

            CheckDisposedBeforeCompleted("Unable to create task continuation");

            return new MyTask<TNewResult>(() => function(Result));
        }

        public void Dispose()
        {
            lock (myLock)
                lock (myDisposeLock)
                {
                    if (isDisposed) return;
                    mySynchEvent.Dispose();
                    isDisposed = true;
                }
        }

        private void CheckDisposedBeforeCompleted(string message)
        {
            //can't get result of execution if task has been disposed before completion
            if (!IsCompleted && isDisposed)
                throw new ObjectDisposedException(nameof(mySynchEvent), $"Task has been disposed but not completed - {message}");
        }
    }
}
