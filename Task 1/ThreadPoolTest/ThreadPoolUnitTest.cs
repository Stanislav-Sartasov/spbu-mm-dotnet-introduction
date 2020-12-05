using System;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThreadPool;
using ThreadPool.Tasks;

namespace ThreadPoolTest
{
    [TestClass]
    public class ThreadPoolUnitTest
    {
        private const int ThreadCount = 8;

        [TestMethod]
        public void SingleTaskTestMethod()
        {
            IMyTaskScheduler scheduler = new MyThreadPool(ThreadCount);

            int a = 40, b = 2;
            int function() => a + b;

            IMyTask<int> task = scheduler.Enqueue(function);

            Assert.AreEqual(42, task.Result);

            scheduler.Dispose();
            Assert.ThrowsException<ObjectDisposedException>(() => scheduler.Enqueue(function));
        }

        [TestMethod]
        public void MultipleTasksTestMethod()
        {
            IMyTaskScheduler scheduler = new MyThreadPool(ThreadCount);

            int a = 40, b = 2;

            int sum() => a + b;
            int subtraction() => a - b;
            int multiplication() => a * b;

            IMyTask<int> sumTask = scheduler.Enqueue(sum);
            IMyTask<int> subTask = scheduler.Enqueue(subtraction);
            IMyTask<int> multTask = scheduler.Enqueue(multiplication);

            Assert.AreEqual(42, sumTask.Result);
            Assert.AreEqual(38, subTask.Result);
            Assert.AreEqual(80, multTask.Result);

            scheduler.Dispose();
            Assert.ThrowsException<ObjectDisposedException>(() => scheduler.Enqueue(sum));
        }

        [TestMethod]
        public void CountinueWithTestMethod()
        {
            IMyTaskScheduler scheduler = new MyThreadPool(ThreadCount);

            int a = 5;

            int root() => a * 2;

            int sum(int r) => r + a;
            int sub(int r) => r - a;

            int mult(int r) => r * a;
            int div(int r) => r / a;

            // First level
            IMyTask<int> rootTask = scheduler.Enqueue(root);

            // Second level
            IMyTask<int> sumTask = rootTask.ContinueWith(sum);
            IMyTask<int> subTask = rootTask.ContinueWith(sub);

            // Third Level
            IMyTask<int> multTask1 = sumTask.ContinueWith(mult);
            IMyTask<int> multTask2 = subTask.ContinueWith(mult);
            IMyTask<int> divTask1 = sumTask.ContinueWith(div);
            IMyTask<int> divTask2 = subTask.ContinueWith(div);

            Assert.AreEqual(a * 2, rootTask.Result);

            Assert.AreEqual(a * 2 + a, sumTask.Result);
            Assert.AreEqual(a * 2 - a, subTask.Result);

            Assert.AreEqual((a * 2 + a) * a, multTask1.Result);
            Assert.AreEqual((a * 2 - a) * a, multTask2.Result);
            Assert.AreEqual((a * 2 + a) / a, divTask1.Result);
            Assert.AreEqual((a * 2 - a) / a, divTask2.Result);

            scheduler.Dispose();
            Assert.ThrowsException<ObjectDisposedException>(() => scheduler.Enqueue(root));
        }

        [TestMethod]
        public void ThreadCountTestMethod()
        {
            int expectedThreadCount = 3;
            int repetitions = 20;

            IMyTaskScheduler scheduler = new MyThreadPool(expectedThreadCount);

            int threadFunction() => Thread.CurrentThread.ManagedThreadId;

            int threadNumber = Enumerable.Range(0, repetitions)
                .Select(_ => scheduler.Enqueue(threadFunction))
                .Select(task => task.Result)
                .Distinct()
                .Count();

            Assert.AreEqual(expectedThreadCount, threadNumber);

            scheduler.Dispose();
            Assert.ThrowsException<ObjectDisposedException>(() => scheduler.Enqueue(threadFunction));
        }
    }
}
