using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Homework1;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void TestThreadCount()
        {
            const int threadCount = 8;
            using var pool = new MyThreadPool(threadCount);
            var tasks = new List<IMyTask<int>>();
            for (int i = 0; i < threadCount; i += 1)
            {
                tasks.Add(pool.Enqueue(() =>
                {
                    Thread.Sleep(1000);
                    return 0;
                }));
            }

            Thread.Sleep(1200);
            Assert.That(tasks.All(it => it.IsCompleted));
        }

        [Test]
        public void TestContinuationOrder()
        {
            using var pool = new MyThreadPool(4);
            var task = pool.Enqueue(() =>
            {
                Thread.Sleep(1000);
                return DateTime.Now;
            }).ContinueWith(arg => new {FirstTaskEnd = arg, SecondTaskStart = DateTime.Now});
            Assert.That(task.Result.FirstTaskEnd <= task.Result.SecondTaskStart);
        }

        [Test]
        public void TestThatThreadsAreNotBlocked()
        {
            using var pool = new MyThreadPool(4);

            static int Wait()
            {
                Thread.Sleep(1000);
                return 0;
            }

            static int WaitContinuation(int arg)
            {
                Thread.Sleep(1000);
                return 0;
            }

            var tasks = new List<IMyTask<int>>();
            for (int i = 0; i < 4; i += 1)
            {
                tasks.Add(pool.Enqueue(Wait)
                    .ContinueWith(WaitContinuation)
                    .ContinueWith(WaitContinuation)
                    .ContinueWith(WaitContinuation));
            }
            // If threads got blocked, full completion of the last task would take at least 16000 ms
            Thread.Sleep(4200);
            Assert.That(tasks.All(it => it.IsCompleted));
        }

        [Test]
        public void TestFailingTask()
        {
            using var pool = new MyThreadPool(4);
            var task = pool.Enqueue<int>(() => throw new NotImplementedException());
            Thread.Sleep(100);
            Assert.That(task.IsCompleted);
            Assert.Throws<AggregateException>(() =>
            {
                int result = task.Result;
            });
        }

        [Test]
        public void TestTaskWithFailedDependency()
        {
            using var pool = new MyThreadPool(4);
            var continuation = pool.Enqueue<int>(() => throw new NotImplementedException()).ContinueWith(arg => arg);
            Thread.Sleep(100);
            Assert.That(continuation.IsCompleted);
            Assert.Throws<AggregateException>(() =>
            {
                int result = continuation.Result;
            });
        }

        [Test]
        public void TestThatExecutionResultIsActuallyPassedToContinuation()
        {
            using var pool = new MyThreadPool(4);
            const string data = "Absolutely unique string that exists nowhere else";
            var continuation = pool.Enqueue(() => data).ContinueWith(x => x);
            Thread.Sleep(200);
            Assert.That(continuation.Result == data);
        }

        [Test]
        public void TestThatTaskIsNotCompletedUntilItIs()
        {
            using var pool = new MyThreadPool(4);
            var task = pool.Enqueue(() =>
            {
                Thread.Sleep(1000);
                return 0;
            });
            Assert.False(task.IsCompleted);
            Thread.Sleep(1200);
            Assert.True(task.IsCompleted);
        }
    }
}
