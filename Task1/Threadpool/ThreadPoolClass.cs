using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Threadpool
{
    //Using class to make shure that either all information is available or none of it  
    public class CustomTask<TResult> : IMyTask<TResult>
    {
        private volatile Result<TResult> _result;

        private readonly CustomThreadPool _threadpool;

        private readonly Func<TResult> _function;

        public CustomTask(CustomThreadPool threadpool, Func<TResult> function)
        {
            _threadpool = threadpool;
            _function = function;
            _result = new Result<TResult>();
        }

        public bool IsCompleted => this._result.isCompleted;

        public TResult Result
        {
            get
            {
                while (!(this._result.isCompleted | this._result.isFailed))
                {
                    Thread.Yield();
                }

                if (this._result.isCompleted)
                {
                    return this._result.result;
                }
                else
                {
                    throw new AggregateException(this._result.exception);
                }
            }
        }

        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> new_task)
        {
            return this._threadpool.Enqueue<TNewResult>(() => new_task(this.Result));

        }


        public void Calculate()
        {
            try
            {
                TResult result = _function.Invoke();

                this._result = new Result<TResult>(isCompleted: true, result: result);
            }
            catch (Exception e)
            {
                this._result = new Result<TResult>(isFailed: true, exception: new AggregateException(e));
            }
        }
    }


    


    public class CustomThreadPool : IDisposable
    {
        private readonly List<Thread> ThreadsList = new List<Thread>();

        private readonly ConcurrentQueue<IMyTask> TaskQueue = new ConcurrentQueue<IMyTask>();

        private readonly CancellationTokenSource source;
        private CancellationToken token;

        public int ThreadsNum
        {
            get
            {
                return this.ThreadsList.Count();
            }
        }


        private void BaseThreadFunction()
        {
            while (true)
            {

                IMyTask task;
                if (this.TaskQueue.TryDequeue(out task))
                {
                    task.Calculate();
                }
                else
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }

        public CustomThreadPool(int threadsNum)
        {
            this.source = new CancellationTokenSource();
            this.token = source.Token;

            for (int i = 0; i < threadsNum; i++)
            {
                Thread new_thread = new Thread(new ThreadStart(this.BaseThreadFunction));
                new_thread.Start();
                this.ThreadsList.Add(new_thread);
            }
        }



        public IMyTask<TResult> Enqueue<TResult>(Func<TResult> function)
        {
            if (token.IsCancellationRequested)
            {
                throw new ObjectDisposedException("ThreadPool is disposed");
            }
            var task = new CustomTask<TResult>(function: function, threadpool: this);
            this.TaskQueue.Enqueue(task);
            return task;
        }

        public void Dispose()
        {
            source.Cancel();
            foreach (Thread thread in this.ThreadsList)
            {
                thread.Join();
            }
            source.Dispose();
        }


    }




}
