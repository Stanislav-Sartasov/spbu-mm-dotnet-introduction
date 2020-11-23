using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

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
                using (var task = new MyTask<int>(() => 
                {
                    Thread.Sleep(1000);
                    return 42 + 42;
                }))
                {
                    threadPool.Enqueue(task);
                    Assert.AreEqual(84, task.Result);
                }
            }
        }

        [Test]
        public void AddManyTasksCheckCountTest()
        {
            using (var threadPool = new ThreadPool(threadPoolSize))
            {
                using (var task1 = new MyTask<int>(() =>
                {
                    Thread.Sleep(1000);
                    return 42 + 42;
                }))
                using (var task2 = new MyTask<int>(() =>
                {
                    Thread.Sleep(1000);
                    return 42 + 42;
                }))
                using (var task3 = new MyTask<int>(() =>
                {
                    Thread.Sleep(1000);
                    return 42 + 42;
                }))
                {
                    threadPool.Enqueue(task1);
                    threadPool.Enqueue(task2);
                    threadPool.Enqueue(task3);
                    Assert.AreEqual(threadPoolSize, threadPool.Size);
                }
            }

        }

        [Test]
        public void AddTasksAndDisposeThreadPoolTest()
        {
            using (var threadPool = new ThreadPool(threadPoolSize))
            {
                var task1 = new MyTask<int>(() =>
                {
                    Thread.Sleep(1000);
                    return 42 + 42;
                });
                var task2 = new MyTask<int>(() =>
                {
                    Thread.Sleep(1000);
                    return 43 + 41;
                });
                threadPool.Enqueue(task1);
                threadPool.Enqueue(task2);
                Assert.AreEqual(84, task1.Result);
                Assert.AreEqual(84, task2.Result);
                task1.Dispose();
                task2.Dispose();
            }
        }

        
    }
}
