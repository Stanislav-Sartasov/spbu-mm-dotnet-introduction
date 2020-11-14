using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using ThreadPool;

namespace Tests
{
    public class ThreadPoolTest
    {
        private static readonly uint ThreadsCount = 4;
        
        [Test]
        public void TestThreadsCount()
        {
            var executor = new ThreadPoolExecutor(ThreadsCount);
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
            
            executor.Dispose();
        }

        [Test]
        public void TestTaskEnqueue()
        {
            var maxNum = 100;
            var executor = new ThreadPoolExecutor(ThreadsCount);

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
            
            executor.Dispose();
        }

        [Test]
        public void TestTaskContinueWith()
        {
            
        }

        [Test]
        public void TestTaskAbort()
        {
            
        }
    }
}