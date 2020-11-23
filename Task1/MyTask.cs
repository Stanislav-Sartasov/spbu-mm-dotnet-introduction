using System;
using System.Collections.Generic;
using System.Text;

namespace Task1
{
    public class MyTask<TResult> : IMyTask<TResult>
    {
        private TResult _result;
        public TResult Result
        {
            get
            {
                return _result;
            }
        }

        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Run()
        {
            throw new NotImplementedException();
        }
    }
}
