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
    }
}
