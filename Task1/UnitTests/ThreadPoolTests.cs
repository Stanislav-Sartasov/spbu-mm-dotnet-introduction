using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using Threadpool;

namespace UnitTests
{
    [TestClass]
    public class ThreadPoolTests
    {

        private CustomThreadPool createThreadPool(int threadsNum)
        {
            return new CustomThreadPool(threadsNum);
        }

        private Func<string> createSampleFunc(string result_value, int sleepTimeMS)
        {
            Func<string, string> test_funk = (string i) =>
            {
                Thread.Sleep(sleepTimeMS);
                return i;
            };

            return () => test_funk(result_value);
        }


        [TestMethod]
        public void TestThreadsNum()
        {
            var threadsNum = 2;

            var threadpool = this.createThreadPool(threadsNum);

            Assert.IsTrue(threadpool.ThreadsNum == threadsNum);
        }

        [TestMethod]
        public void TestEnqueueForOneTask()
        {

            var threadpool = this.createThreadPool(2);
            string sample_string = "Hello";
            Func<string> test_funk = this.createSampleFunc(sample_string, 100);

            var task = threadpool.Enqueue(test_funk);

            Assert.IsTrue(task.IsCompleted == false);

            Assert.IsTrue(task.Result == sample_string);

            Assert.IsTrue(task.IsCompleted == true);

            threadpool.Dispose();
        }

        public void TestEnqueueForManyTasks()
        {

            var threadpool = this.createThreadPool(2);
            string sample_string = "Hello";
            Func<string> test_funk = this.createSampleFunc(sample_string, 100);
            var tasks_num = 1000;

            List<IMyTask> tasks = new List<IMyTask>();
            for (int i = 0; i < tasks_num; i++)
            {
                tasks.Add(threadpool.Enqueue(test_funk));
            }

            foreach (IMyTask<String> task in tasks)
            {
                Assert.IsTrue(task.Result == sample_string);
                Assert.IsTrue(task.IsCompleted == true);
            }
            threadpool.Dispose();
        }

        [TestMethod]
        public void TestContinueWith()
        {
            var threadpool = this.createThreadPool(2);
            string sample_string = "Hello";
            Func<string> test_funk = this.createSampleFunc(sample_string, 100);

            var task = threadpool.Enqueue(test_funk);

            Assert.IsTrue(task.IsCompleted == false);

            Assert.IsTrue(task.Result == sample_string);

            Assert.IsTrue(task.IsCompleted == true);
            threadpool.Dispose();
        }

        [TestMethod]
        public void TestContinueWithForManyTasks()
        {
            var threadpool = this.createThreadPool(2);
            string sample_string = "Hello";

            Func<string, string> fast_funk = (string i) =>
            {
                Thread.Sleep(1000);
                return i;
            };

            Func<string, string> long_funk = (string i) =>
            {
                Thread.Sleep(2000);
                return i;
            };

            var first_task = threadpool.Enqueue(() => long_funk(sample_string));
            var next_task = first_task.ContinueWith(fast_funk);
            var next_task2 = first_task.ContinueWith(fast_funk);

            Assert.IsTrue(first_task.Result == sample_string && next_task.IsCompleted == false);

            Assert.IsTrue(next_task.Result == next_task2.Result & next_task.Result == sample_string);
            threadpool.Dispose();
        }
        [TestMethod]
        public void TestDispose()
        {
            var threadpool = this.createThreadPool(2);
            string sample_string = "Hello";

            Func<string, string> fast_funk = (string i) =>
            {
                Thread.Sleep(1000);
                return i;
            };

            Func<string, string> long_funk = (string i) =>
            {
                Thread.Sleep(2000);
                return i;
            };

            var first_task = threadpool.Enqueue(() => long_funk(sample_string));
            var next_task = first_task.ContinueWith(fast_funk);
            var next_task2 = first_task.ContinueWith(fast_funk);

            threadpool.Dispose();
            Assert.IsTrue(next_task.Result == next_task2.Result & next_task.Result == sample_string);
            Assert.ThrowsException<ObjectDisposedException>(() => threadpool.Enqueue(() => long_funk(sample_string)));
        }

        [TestMethod]
        public void TestAggregateError()
        {
            var threadpool = this.createThreadPool(2);
            string sample_string = "Hello";
            var exception = new OverflowException("Overflow");
            Func<string, string> faulty_func = (string i) =>
            {
                throw exception;
                return i;
            };

            var first_task = threadpool.Enqueue(() => faulty_func(sample_string));
            Assert.ThrowsException<AggregateException>(() => first_task.Result);
            try
            {
                var a = first_task.Result;
            }
            catch (AggregateException e)
            {
                Assert.IsTrue(e.Flatten().InnerExceptions.Contains(exception));
            }

            threadpool.Dispose();
        }
    }
}
