using System;
using System.Threading;

namespace ThreadPool 
{
    public class ThreadPoolTask<TResult>: IMyTask<TResult>
    {
        private Func<TResult> function;
        private ThreadPool threadPool;
        private Exception exception = null;

        public bool IsCompleted
        {
            get;
            private set;
        }

        private TResult result;

        public TResult Result 
        {
            get 
            {
                while (!IsCompleted)
                    Thread.Yield();

                if (exception != null)
                    throw exception;

                return result;
            }
            private set 
            {
                result = value;
            }
        }

        public ThreadPoolTask(Func<TResult> function, ThreadPool threadPool) 
        {
            this.function = function;
            this.threadPool = threadPool;
            this.IsCompleted = false;
        }

        public void Run() 
        {
            try 
            {
                Result = function.Invoke();
            }
            catch (Exception e)
            {
                exception = new AggregateException(e);
            }
            finally 
            {
                IsCompleted = true;
            }
        }

        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation)
        {
            IMyTask<TNewResult> newTask = new ThreadPoolTask<TNewResult>(
                () => {
                    return continuation(this.Result);
                },
                threadPool
            );
            threadPool.Enqueue(newTask);
            return newTask;
        }
    }
}