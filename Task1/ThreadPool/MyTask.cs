using System;
using System.Diagnostics;

namespace MyThreadPool
{
    public class MyTask<TResult> : IMyTask<TResult>
    {
        ThreadPool              pool;
        Func<TResult>           func;
        AggregateException      funcException;
        TResult                 result;

        public bool IsCompleted { get; private set; }

        public TResult Result
        {
            get
            {
                while (!IsCompleted) { }

                if (funcException != null)
                {
                    throw funcException;
                }

                return result;
            }
        }

        public MyTask(ThreadPool pool, Func<TResult> func)
        {
            Debug.Assert(func != null && pool != null);

            this.pool = pool;
            this.func = func;
            this.funcException = null;
            this.IsCompleted = false;
        }

        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> f)
        {
            var nextTask = new MyTask<TNewResult>(pool, () =>
            {
                // if this task is not ready, this.Result will wait for completion
                // and then new Func will be called
                return f(this.Result);
            });

            // newly created task must be started;
            // if pool was disposed, it will throw exception about it
            pool.Enqueue(nextTask);

            return nextTask;
        }

        public void Invoke()
        {
            Debug.Assert(!IsCompleted);

            try
            {
                result = func();
            }
            catch (Exception e)
            {
                funcException = new AggregateException(e);
            }
            finally
            {
                IsCompleted = true;
            }
        }
    }
}
