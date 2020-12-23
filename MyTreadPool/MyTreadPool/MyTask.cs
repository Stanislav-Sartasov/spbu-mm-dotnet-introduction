using System;
using System.Threading;

namespace MyThreadPool
{
    public class MyTask<TResult> : IMyTask<TResult>
    {

        private TResult result;
        public TResult Result 
        { 
            private set
            {
                result = value;
            }
            get
            {
                while(!IsComplete)
                {
                    Thread.Sleep(50);
                }

                return result;
            }
        }

        private AggregateException exception;
        public AggregateException Exception 
        {
            private set
            {
                exception = value;
            }
            get
            {
                while (!IsComplete)
                {
                    Thread.Sleep(50);
                }

                return exception;
            }
        }

        public bool IsComplete { get; private set;}

        private Func<TResult> func;

        private MyThreadPool ThreadPool;

        public MyTask(Func<TResult> function, MyThreadPool pool)
        {
            IsComplete = false;
            func = function;
            ThreadPool = pool;
        }

        public void Start()
        {
            try
            {
                Result = func.Invoke();
            } 
            catch(Exception e)
            {
                Exception = new AggregateException(e);
            }
            finally
            {
                IsComplete = true;
            }
        }

        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> moreCalculations)
        {
            var task = new MyTask<TNewResult>(() => moreCalculations(Result), ThreadPool);

            if (!ThreadPool.IsDisposed)
            {
                ThreadPool.Enqueue(task);
            } else
            {
                throw new ObjectDisposedException("Threadpool has been disposed");
            }

            return task;
        }
    }
}
