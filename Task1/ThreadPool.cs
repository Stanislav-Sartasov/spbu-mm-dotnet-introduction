using System;
using System.Collections.Generic;
using System.Text;

namespace Task1
{
    class ThreadPool : IDisposable
    {
        private volatile int _size = 10;

        public int Size => _size;

        public ThreadPool(int poolSize)
        {
            _size = poolSize;
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
