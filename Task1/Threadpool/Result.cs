using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Threadpool
{
    public class Result<TResult>
    {
        public readonly bool isCompleted;
        public readonly bool isFailed;
        public readonly TResult result;
        public readonly Exception exception;


        public Result(bool isCompleted = false, bool isFailed = false, TResult result = default, Exception exception = default)
        {
            this.isCompleted = isCompleted;
            this.isFailed = isFailed;
            this.result = result;
            this.exception = exception;
        }
    }
}
