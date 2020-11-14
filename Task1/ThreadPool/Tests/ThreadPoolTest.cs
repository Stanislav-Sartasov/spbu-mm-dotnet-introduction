using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using ThreadPool;

namespace Tests
{
    public class ThreadPoolTest
    {
        private static readonly uint ThreadsCount = 4; 
        private IExecutor executor;

        [SetUp]
        public void SetupExecutor()
        {
            executor = new ThreadPoolExecutor(ThreadsCount);
        }

        [TearDown]
        public void DisposeExecutor()
        {
            executor.Dispose();
        }
        
        [Test]
        public void TestThreadsCount()
        {
            var tokenSource = new CancellationTokenSource();
            var tasks = new List<ITask<int>>();
            var ids = new HashSet<int>();
            var started = 0;
            
            for (uint i = 0; i < 4; i++)
            {
                var task = executor.Enqueue((() =>
                {
                    Interlocked.Increment(ref started);
                    tokenSource.Token.WaitHandle.WaitOne();                    
                    return Thread.CurrentThread.ManagedThreadId;
                }));
                
                tasks.Add(task);
            }

            while (started != ThreadsCount)
                Thread.Yield();

            tokenSource.Cancel();

            foreach (var task in tasks)
                ids.Add(task.GetResult());

            Assert.AreEqual( ThreadsCount, ids.Count);
            
            tokenSource.Dispose();
        }

        [Test]
        public void TestTaskEnqueue()
        {
            var maxNum = 100;

            int Evaluate()
            {
                var sum = 0;

                for (int i = 1; i < maxNum; i++)
                {
                    sum += i * i;
                }

                return sum;
            }

            var task = executor.Enqueue(Evaluate);
            
            Assert.AreEqual(task.GetResult(), Evaluate());
        }

        [Test]
        public void TestLargeTasksAmount()
        {
            var tasksCount = ThreadsCount * 1000;
            var tasks = new List<ITask<long>>();

            for (int i = 0; i < tasksCount; i++)
            {
                var local = i;
                var task = executor.Enqueue(() =>
                {
                    Thread.Sleep(1);
                    return local % ThreadsCount;
                });
                tasks.Add(task);
            }
            
            for (int i = 0; i < tasksCount; i++)
            {
                Assert.AreEqual(tasks[i].GetResult(), i % ThreadsCount);   
            }
        }

        [Test]
        public void TestTaskContinueWith()
        {
            var tasksCount = ThreadsCount * 1000;
            var firstTasks = new List<ITask<int>>();
            var secondTasks = new List<ITask<String>>();

            for (int i = 0; i < tasksCount; i++)
            {
                var local = i;
                var task = executor.Enqueue(() =>
                {
                    Thread.Sleep(2);
                    return local + local;
                });
                
                firstTasks.Add(task);
            }

            foreach (var task in firstTasks)
            {
                var newTask = task.ContinueWith((i => (i * i).ToString()));
                secondTasks.Add(newTask);
            }

            for (int i = 0; i < tasksCount; i++)
            {
                Assert.AreEqual(secondTasks[i].GetResult(), ((i + i) * (i + i)).ToString());
            }
        }

        [Test]
        public void TestTaskAbort()
        {
            var waitTokenSource = new CancellationTokenSource();
            var tasksToAbort = new List<ITask<int>>();
            var tasksToAbortCount = ThreadsCount * 100;
            var executorToAbort = new ThreadPoolExecutor(ThreadsCount, () => waitTokenSource.Cancel());
            
            for (int i = 0; i < ThreadsCount; i++)
            {
                executorToAbort.Enqueue(() =>
                {
                    waitTokenSource.Token.WaitHandle.WaitOne();
                    return 0;
                });
            }

            for (int i = 0; i < tasksToAbortCount; i++)
            {
                var task = executorToAbort.Enqueue(() => 0);
                tasksToAbort.Add(task);
            }

            var abortWaitTask = executor.Enqueue(() =>
            {
                foreach (var task in tasksToAbort)
                {
                    Assert.Throws<AggregateException>(() =>
                    {
                        task.GetResult();
                    });
                }

                return 0;
            });
            
            executorToAbort.Dispose();
            waitTokenSource.Dispose();

            Assert.AreEqual(abortWaitTask.GetResult(), 0);
        }
    }
}