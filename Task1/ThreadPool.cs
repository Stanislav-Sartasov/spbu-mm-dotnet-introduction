using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Task1
{
    class ThreadPool : IDisposable
    {
        private const int _defaultSize = 10;
        private readonly List<Thread> _pool;
       
        private readonly BlockingCollection<Action> _waitingTasks = new BlockingCollection<Action>();
        public int Size { get; }

        public ThreadPool(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentException("Invalid ThreadPool size", nameof(size));
            }
            Size = size;
            _pool = new List<Thread>(size);

            for (var i = 0; i < size; i++)
            {
                var worker = new Thread(Work);
                worker.Start();
                _pool.Add(worker);
            }
        }

        public ThreadPool() : this(_defaultSize)
        {
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        private void Work()
        {
            while (true)
            {
                try
                {
                    var task = _waitingTasks.Take();
                    task.Invoke();
                }
                catch (InvalidOperationException e)
                {
                    return;
                }
                catch (Exception)
                {
                    //do nothing
                }
            }
        }
    }
}
