using System;
using System.Collections.Generic;

namespace ThreadPool.Tasks
{
    public interface IMyTask
    {
        bool IsCompleted { get; }

        /// <summary>
        /// Tasks which were produced as a result of ContinueWith invokation on current task
        /// </summary>
        IEnumerable<IMyTask> Children { get; }

        void Run();

        void Await();

        IMyTask ContinueWith(Action action);
    }
}
