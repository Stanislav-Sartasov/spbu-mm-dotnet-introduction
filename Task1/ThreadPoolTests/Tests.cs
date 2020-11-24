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

        [Test]
        public void ContinueWithTest()
        {
            using (var threadPool = new ThreadPool(threadPoolSize))
            {
                using (var task1 = new MyTask<int>(() => 2))
                using (var task2 = task1.ContinueWith(first =>
                {
                    Thread.Sleep(1000);
                    return first * first;
                }))
                using (var task3 = task2.ContinueWith(second =>
                {
                    Thread.Sleep(1000);
                    return second * second;
                }))
                using (var task4 = task2.ContinueWith(second =>
                {
                    Thread.Sleep(1000);
                    return second * second * second;
                }))
                using (var task5 = task3.ContinueWith(third =>
                {
                    Thread.Sleep(1000);
                    return third * third;
                }))
                {
                    threadPool.Enqueue(task5);
                    threadPool.Enqueue(task4);
                    threadPool.Enqueue(task3);
                    threadPool.Enqueue(task2);
                    threadPool.Enqueue(task1);
                    Assert.AreEqual(256, task5.Result);
                    Assert.AreEqual(64, task4.Result);
                }
            }
        }

        [Test]
        public void AddMoreTasksThanThreadPoolSizeTest()
        {
            const int tasksCount = threadPoolSize * 2;
            var threadPool = new ThreadPool(threadPoolSize);
            var tasks = new List<MyTask<int>>();
            for (var i = 0; i < tasksCount; ++i)
            {
                var task = new MyTask<int>(() =>
                {
                    Thread.Sleep(420);
                    return 42 + 42;
                });
                threadPool.Enqueue(task);
                tasks.Add(task);
            }

            tasks.ForEach(task =>
            {
                Assert.AreEqual(84, task.Result);
                task.Dispose();
            });
        }

        [Test]
        public void AddTasksInParallelTest()
        {
            const int parallelThreadsCount = 20;
            var threadPool = new ThreadPool(threadPoolSize);
            var threads = new List<Thread>();

            for (var i = 0; i < parallelThreadsCount; i++)
            {
                var thread = new Thread(() =>
                {
                    var j = i;
                    var task = new MyTask<int>(() => j);
                    threadPool.Enqueue(task);
                    Assert.AreEqual(j, task.Result);
                });
                threads.Add(thread);
                thread.Start();
            }
            threads.ForEach(thread => thread.Join());
        }

        [Test]
        public void EnqueueIntoDisposedThreadPoolTest()
        {
            var threadPool = new ThreadPool(threadPoolSize);
            var task1 = new MyTask<int>(() =>
            {
                Thread.Sleep(500);
                return 42 + 42;
            });
            threadPool.Enqueue(task1);
            threadPool.Dispose();
            using (var task2 = new MyTask<int>(() =>
            {
                Thread.Sleep(500);
                return 42 + 42;
            }))
            {
                Assert.Throws<ObjectDisposedException>(() => threadPool.Enqueue(task2));
            }
            task1.Dispose();
        }

    }
}
