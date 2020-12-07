using System;
using System.Collections.Generic;
using System.Threading;
using MTP = MyThreadPool;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class UnitTests
    {
        static int WaitAndReturn(int toReturn)
        {
            var random = new Random();
            int t = random.Next(10, 60);

            Thread.Sleep(t);

            return toReturn;
        }

        [TestMethod]
        public void TestOneTask()
        {
            MTP.IMyTask<int> task;

            using (var pool = new MTP.ThreadPool(8))
            {
                pool.Start();

                task = new MTP.MyTask<int>(pool, () => WaitAndReturn(0));
                pool.Enqueue(task);
            }

            Assert.AreEqual(0, task.Result);
            Assert.IsTrue(task.IsCompleted);
        }

        [TestMethod]
        public void TestMultipleTask()
        {
            bool done;
            int count = 32;

            using (var pool = new MTP.ThreadPool(8))
            {
                pool.Start();
                var tasks = new MTP.MyTask<int>[count];

                for (int i = 0; i < count; i++)
                {
                    int taskResult = i;

                    var task = new MTP.MyTask<int>(pool, () => WaitAndReturn(taskResult));
                    pool.Enqueue(task);

                    tasks[i] = task;
                }

                // wait until all tasks will be complete
                bool[] checkedTasks = new bool[count];
                do
                {
                    done = true;

                    for (int i = 0; i < count; i++)
                    {
                        var t = tasks[i];
                        done = done && t.IsCompleted;

                        if (t.IsCompleted && !checkedTasks[i])
                        {
                            Assert.AreEqual(i, t.Result);
                            checkedTasks[i] = true;
                        }
                    }

                } while (!done);
            }
        }

        [TestMethod]
        public void TestContinuation()
        {
            MTP.IMyTask<int> contTaskInt;
            MTP.IMyTask<string> contTaskStr;

            using (var pool = new MTP.ThreadPool(8))
            {
                pool.Start();

                var task = new MTP.MyTask<int>(pool, () => WaitAndReturn(1));
                pool.Enqueue(task);

                contTaskInt = task
                    .ContinueWith(a => a * 2);

                contTaskStr = task
                    .ContinueWith(a => "String" + a)
                    .ContinueWith(a => "Other" + a);
            }

            Assert.AreEqual(2, contTaskInt.Result);
            Assert.AreEqual("OtherString1", contTaskStr.Result);
        }

        [TestMethod]
        public void TestThreadCount()
        {
            int threadCount = 8;
            var threadIds = new HashSet<int>();
            object setLock = new object();

            using (var pool = new MTP.ThreadPool(threadCount))
            {
                pool.Start();

                for (int i = 0; i < threadCount * 2; i++)
                {
                    pool.Enqueue(new MTP.MyTask<int>(pool, () =>
                    {
                        lock (setLock)
                        {
                            threadIds.Add(Thread.CurrentThread.ManagedThreadId);
                            return 0;
                        }
                    }));
                }
            }

            Assert.AreEqual(threadCount, threadIds.Count);
        }
    }
}
