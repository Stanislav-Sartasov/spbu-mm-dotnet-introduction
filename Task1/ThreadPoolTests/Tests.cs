using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Task1.ThreadPoolTests
{
    [TestFixture]
    public class Tests
    {
        private const int threadPoolSize = 5;

        [Test]
        public void AddOneTaskTest()
        {
            using (var threadPool = new ThreadPool(threadPoolSize))
            {
                using (var task = new MyTask<int>(() => 42 + 42))
                {
                    threadPool.Enqueue(task);
                    Assert.AreEqual(84, task.Result);
                }
            }
        }

        
    }
}
