using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ThreadPool;

namespace Tests
{
    [TestClass]
    public class Tests
    {
        private readonly Random myRandom = new Random(0);

        private int myCapacity = 5;
        private MyThreadPool.MyThreadPool myThreadPool;

        private void CreateThreadPool(int capacity)
        {
            Debug.WriteLine($"initialize threadpool with capacity = {capacity}");
            myThreadPool = new MyThreadPool.MyThreadPool(capacity);
        }


        private IMyTask<string> CreateTask(int number)
        {
            return MyTask<string>.New(() =>
            {
                Debug.WriteLine($"start task {number}");
                Thread.Sleep(myRandom.Next(777));
                Debug.WriteLine($"finish task {number}");
                return $"task {number}";
            });
        }

        [TestMethod]
        public void ThreadCountTest()
        {
            Debug.WriteLine("--- CheckThreadsCount ---");
            CreateThreadPool(myCapacity);
            Debug.WriteLine($"real number of threads - {myThreadPool.Capacity}");
            Assert.AreEqual(myCapacity, myThreadPool.Capacity);
        }

        [TestMethod]
        public void EnqueueTaskTest()
        {
            Debug.WriteLine("--- EnqueueTaskTest ---");
            myCapacity = 1;
            CreateThreadPool(myCapacity);
            var task = CreateTask(0);
            Debug.WriteLine("enqueue one task");
            myThreadPool.Enqueue(task);
            Debug.WriteLine($"result = {task.Result}");
        }

        [TestMethod]
        public void EnqueueManyTasksTest()
        {
            Debug.WriteLine("--- EnqueueManyTasksTest ---");
            CreateThreadPool(myCapacity);
            var taskList = new List<IMyTask<string>>();
            for (var i = 0; i < myCapacity << 1; i++)
            {
                taskList.Add(CreateTask(i));
                Debug.WriteLine($"enqueue task {i}");
                myThreadPool.Enqueue(taskList.Last());
            }

            for (var i = 0; i < myCapacity << 1; i++)
            {
                Debug.WriteLine($"wait for result task {i}");
                Debug.WriteLine($"result {i} = {taskList[i].Result}");
            }
        }

        [TestMethod]
        public void EnqueueTasksAndDisposeThreadPoolTest()
        {
            Debug.WriteLine("--- EnqueueTasksAndDisposeThreadPoolTest ---");
            myCapacity = 1;
            CreateThreadPool(myCapacity);
            var task1 = MyTask<string>.New(() =>
            {
                Debug.WriteLine("in task 1");
                Thread.Sleep(2000);
                Debug.WriteLine("return from task 1");
                return "task 1";
            });
            var task2 = task1.ContinueWith(result =>
            {
                Debug.WriteLine("in task 2");
                Thread.Sleep(100);
                Debug.WriteLine("return from task 2");
                return $"task 2 after {result}";
            });
            Debug.WriteLine("enqueue task 1");
            myThreadPool.Enqueue(task1);
            Debug.WriteLine("enqueue task 2");
            myThreadPool.Enqueue(task2);

            Debug.WriteLine($"result 1 = {task1.Result}");

            Debug.WriteLine("dispose threadpool");
            myThreadPool.Dispose();
            
            Debug.WriteLine($"result 2 = {task2.Result}");
        }

        [TestMethod]
        public void ExceptionTaskTest()
        {
            Debug.WriteLine("--- ExceptionTaskTest ---");
            myCapacity = 1;
            CreateThreadPool(myCapacity);
            var task = MyTask<string>.New(() =>
            {
                var list = new List<string> { "task 0" };
                // IndexOutofRange
                return list[5];
            });

            myThreadPool.Enqueue(task);

            var exception = Assert.ThrowsException<AggregateException>(() =>
            {
                Debug.WriteLine($"result = {task.Result}");
            });

            Debug.WriteLine($"task exception result = {exception.Message}");
            Assert.IsNotNull(exception.InnerException);
            Assert.IsTrue(exception.InnerException is ArgumentOutOfRangeException);
            Debug.WriteLine($"task inner exception = {exception.InnerException.Message}");
        }

        [TestMethod]
        public void ContinueWithTest()
        {
            Debug.WriteLine("--- ContinueWith ---");
            myCapacity = 3;
            CreateThreadPool(myCapacity);
            var task = CreateTask(0);
            Debug.WriteLine("enqueue task 0");
            myThreadPool.Enqueue(task);
            var tasks = new List<IMyTask<string>> { task };
            for (var i = 1; i < myCapacity << 1; i++)
            {
                var number = i;
                task = task.ContinueWith(result =>
                {
                    Thread.Sleep(myRandom.Next(500));
                    return $"task {number} after {result}";
                });
                tasks.Add(task);
                Debug.WriteLine($"enqueue task {i}");
                myThreadPool.Enqueue(tasks.Last());
            }

            
            for (var i = 0; i < myCapacity << 1; i++)
            {
                Debug.WriteLine($"result task {i} = {tasks[i].Result}");
            }
        }

        [TestCleanup]
        public void Dispose()
        {
            myCapacity = 5;
            if (!myThreadPool.IsDisposed) myThreadPool.Dispose();
        }
    }
}
