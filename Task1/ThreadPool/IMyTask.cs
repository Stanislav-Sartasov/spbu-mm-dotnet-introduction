using System;

namespace ThreadPool {
    public interface IMyTask<TResult> {
        bool IsCompeted { get; }
        TResult Result { get; }
        
        IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> func);
    }
}