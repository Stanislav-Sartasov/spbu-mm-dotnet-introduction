using ThreadPool;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Xunit;

namespace Testing {

    public class Tests
    {

        [Fact]
        public void AddOneTaskTest() 
        {
            var pool = new ThreadPool.ThreadPool(2);

            var helloWorldTask = new ThreadPoolTask<String>(
                () => {
                   return "Hello World!";
                },
                pool
            );

            pool.Enqueue<String>(helloWorldTask);

            Assert.Equal("Hello World!", helloWorldTask.Result);
            
            var throwErrorTask = new ThreadPoolTask<String>(
                () => {
                    throw new Exception("Something wrong...");
                },
                pool
            );
            
            pool.Enqueue<String>(throwErrorTask);

            try 
            {
                var res = throwErrorTask.Result;
                Assert.True(false);
            }
            catch (AggregateException) 
            {
                Assert.True(true);
            }
            catch (Exception)
            {
                Assert.True(false);
            }

            pool.Dispose();
        }

        [Fact]
        public void AddSeveralTasksTest()
        {
            var pool = new ThreadPool.ThreadPool(4);
            var answers = new List<int>();
            var tasks = new List<ThreadPoolTask<int>>();
            var taskAmount = 8;
            
            for (int i = 0; i < taskAmount; i++) 
            {
                int ans = 0;
                for (int j = 0; j < (i + 1) * 10; j++) 
                    ans += j;
                answers.Add(ans);

                var currentI = i;
                var task = new ThreadPoolTask<int>(
                    () => {
                        int res = 0;
                        for (int j = 0; j < (currentI + 1) * 10; j++) 
                            res += j;    
                        return res;
                    },
                    pool
                );
                tasks.Add(task);
            }

            for (int i = 0; i < taskAmount; i++)
                pool.Enqueue<int>(tasks[i]);

            for (int i = 0; i < taskAmount; i++) 
                Assert.Equal(answers[i], tasks[i].Result);

            pool.Dispose();
        }

        [Fact]
        public void ContinueWithCheckTest() 
        {
            var pool = new ThreadPool.ThreadPool(4);
            var answers = new List<int>();
            var tasks = new List<ThreadPoolTask<int>>();
            var nextTasks = new List<IMyTask<int>>();
            var taskAmount = 8;
            
            for (int i = 0; i < taskAmount; i++) 
            {
                int ans = 0;
                for (int j = 0; j < (i + 1) * 10; j++) 
                    ans += j;
                answers.Add(ans * 2);

                var currentI = i;
                var task = new ThreadPoolTask<int>(
                    () => {
                        int res = 0;
                        for (int j = 0; j < (currentI + 1) * 10; j++) 
                            res += j;    
                        return res;
                    },
                    pool
                );
                tasks.Add(task);

                pool.Enqueue<int>(tasks[i]);

                var nextTask = task.ContinueWith<int>(
                    (i) => {
                        return i * 2;
                    }
                );
                nextTasks.Add(nextTask);
            }

            for (int i = 0; i < taskAmount; i++) 
                Assert.Equal(answers[i], nextTasks[i].Result);
        
            pool.Dispose();
        }

        [Fact]
        public void ThreadAmountCheckTest() 
        {
            var pool = new ThreadPool.ThreadPool(4);
            var threadIds = new ConcurrentDictionary<int, bool>();
            var tasks = new List<ThreadPoolTask<int>>();
            var taskAmount = 8;
            
            for (int i = 0; i < taskAmount; i++) 
            {
                var task = new ThreadPoolTask<int>(
                    () => {
                        var id = Thread.CurrentThread.ManagedThreadId;
                        threadIds[id] = true;
                        Thread.Sleep(5000);
                        return 0;
                    },
                    pool
                );
                tasks.Add(task);
            }

            for (int i = 0; i < taskAmount; i++) 
                pool.Enqueue<int>(tasks[i]);

            for (int i = 0; i < taskAmount; i++) 
            {
                int j = tasks[i].Result;
            }

            Assert.Equal(4, threadIds.Count);

            pool.Dispose();
        }
    }
}