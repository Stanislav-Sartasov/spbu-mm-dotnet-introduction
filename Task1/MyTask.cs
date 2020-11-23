using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Task1
{
    public class MyTask<TResult> : IMyTask<TResult>
    {
        private TResult _result;
        private readonly Func<TResult> _func;
        private Exception _exception;
        private readonly ManualResetEvent _mre = new ManualResetEvent(false);

        public TResult Result
        {
            get
            {
                _mre.WaitOne();
                if (_exception != null)
                {
                    throw new AggregateException("Task failed", _exception);
                }
                return _result;
            }
        }

        public bool IsCompleted
        {
            get;
            private set;
        }

        public MyTask(Func<TResult> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func), "Task cannot run null");
        }
        
        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation)
        {
            return new MyTask<TNewResult>(() =>
            {
                var oldResult = Result;
                return continuation.Invoke(oldResult);
            });
        }

        public void Dispose()
        {
            _mre?.Dispose();
        }

        public void Run()
        {
            if (IsCompleted)
                return;
            try
            {
                _result = _func.Invoke();
            }
            catch(Exception e)
            {
                _exception = e;
            }
            finally
            {
                IsCompleted = true;
                _mre.Set();

            }
        }
    }
}
