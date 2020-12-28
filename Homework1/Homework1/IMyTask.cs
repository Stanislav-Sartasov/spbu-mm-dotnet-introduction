using System;
using JetBrains.Annotations;

namespace Homework1
{
    public interface IMyTask<out T>
    {
        bool IsCompleted { get; }
        T Result { get; }

        [NotNull]
        IMyTask<U> ContinueWith<U>(Func<T, U> continuation);
    }
}
