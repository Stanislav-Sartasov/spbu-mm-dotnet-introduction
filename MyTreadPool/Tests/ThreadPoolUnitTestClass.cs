using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyThreadPool;
using System;
using System.Threading;

namespace Tests
{
    [TestClass]
    public class ThreadPoolUnitTestClass
    {

        private MyThreadPool.MyThreadPool pool;
        private Random random;

        static int SimpleTestFun(int a, int b, int time)
        {
            Thread.Sleep(time);
            return a + b;
        }

        private void init(int threadscount)
        {
            pool = new MyThreadPool.MyThreadPool(threadscount);
            random = new Random();
        }

        [TestMethod]
        public void SimpleTest()
        {

            init(4);

            int a = 7, b = 4;

            IMyTask<int> myTask = new MyTask<int>(() => SimpleTestFun(a, b, random.Next(50, 70)), pool);

            pool.Enqueue(myTask);

            pool.Dispose();

            Assert.AreEqual(a + b, myTask.Result);
            Assert.IsTrue(myTask.IsComplete);
        }

        [TestMethod]
        public void MultiplyThreadsTest()
        {
            var threadscount = 4;
            init(threadscount);

            int a = 7, b = 4;

            for(int i = 0; i < threadscount * 4; i++)
            {
                IMyTask<int> myTask = new MyTask<int>(() => SimpleTestFun(a + i, b * i, random.Next(50, 70)), pool);

                pool.Enqueue(myTask);

                if (threadscount % random.Next(1, 4) == 0)
                {
                    Assert.AreEqual(a + i + b * i, myTask.Result);
                    Assert.IsTrue(myTask.IsComplete);
                }
            }

            pool.Dispose();
        }

        [TestMethod]
        public void ContinueWithTest()
        {
            var threadscount = 4;
            init(threadscount);

            int a = 7, b = 4;

            IMyTask<int> myTask = new MyTask<int>(() => SimpleTestFun(a, b, random.Next(50, 70)), pool);
            
            pool.Enqueue(myTask);

            var multyplyTask = myTask.ContinueWith(i => i * 2);

            Assert.AreEqual((a + b) * 2, multyplyTask.Result);

            var tribleMultyplyTask = myTask.ContinueWith(i => i * 2)
                .ContinueWith(i => i * 2)
                .ContinueWith(i => i * 2);

            Assert.AreEqual((a + b) * 8, tribleMultyplyTask.Result);

            pool.Dispose();
        }
       
    }
}
