using System;
using System.Collections.Generic;
using System.Threading;
using MTP = MyThreadPool;

namespace Tests
{
    class Program
    {
        #region example methods
        static int CalculateValue()
        {
            var random = new Random();
            int t = random.Next(50, 300);

            Thread.Sleep(t);

            return t;
        }

        static string CreateString(int a)
        {
            return "Created string with " + a;
        }

        static string CreateOtherString(string a)
        {
            return "Other " + a;
        }

        static float ProcessInt(int a)
        {
            return a / 7.0f;
        }
        #endregion

        static void TestOneTask()
        {
            MTP.IMyTask<int> task;

            using (var pool = new MTP.ThreadPool(8))
            {
                pool.Start();

                task = new MTP.MyTask<int>(pool, CalculateValue);
                pool.Enqueue(task);
            }

            Console.WriteLine("Completed task: " + task.Result);

            Console.WriteLine();
        }

        static void TestMultipleTask()
        {
            bool done;
            int count = 32;

            using (var pool = new MTP.ThreadPool(8))
            {
                pool.Start();
                var tasks = new MTP.MyTask<int>[count];

                for (int i = 0; i < count; i++)
                {
                    var task = new MTP.MyTask<int>(pool, CalculateValue);
                    pool.Enqueue(task);

                    tasks[i] = task;
                }

                // wait until all tasks will be complete
                bool[] written = new bool[count];
                do
                {
                    done = true;

                    for (int i = 0; i < count; i++) 
                    {
                        var t = tasks[i];
                        done = done && t.IsCompleted;

                        if (t.IsCompleted && !written[i])
                        {
                            Console.WriteLine($"Completed task #{i}: {t.Result}");
                            written[i] = true;
                        }
                    }

                } while (!done);
            }

            Console.WriteLine();
        }

        static void TestContinuation()
        {
            MTP.IMyTask<float> contTaskFloat;
            MTP.IMyTask<string> contTaskStr;

            using (var pool = new MTP.ThreadPool(8))
            {
                pool.Start();

                var task = new MTP.MyTask<int>(pool, CalculateValue);
                pool.Enqueue(task);

                contTaskFloat = task
                    .ContinueWith(ProcessInt);

                contTaskStr = task
                    .ContinueWith(CreateString)
                    .ContinueWith(CreateOtherString);
            }

            Console.WriteLine("Completed continued task #0: " + contTaskFloat.Result);
            Console.WriteLine("Completed continued task #1: " + contTaskStr.Result);

            Console.WriteLine();
        }

        static void TestThreadCount()
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

            Console.WriteLine("Actual thread number: " + threadCount);
            Console.WriteLine("Counted thread number: " + threadIds.Count);
            Console.WriteLine();
        }

        static int Main(string[] args)
        {
            TestOneTask();
            TestMultipleTask();
            TestContinuation();
            TestThreadCount();

            return 0;
        }
    }
}
