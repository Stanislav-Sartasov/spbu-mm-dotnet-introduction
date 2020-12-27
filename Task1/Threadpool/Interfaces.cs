using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Threadpool
{
    //Interface for storage and internal usage in threads
    public interface IMyTask
    {
        void Calculate();
        bool IsCompleted { get; }
    }
    //Interface for external work with tasks
    public interface IMyTask<TResult> : IMyTask
    {
        TResult Result { get; }
        IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> new_task);
    }
}
