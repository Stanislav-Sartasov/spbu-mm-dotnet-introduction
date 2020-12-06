using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using ThreadPool.Sources;

namespace ThreadPool.Tests
{
public class Tests
{
    [Test]
    public void TestAddOneSuccessfulTaskAndWait()
    {
        using var threadPool = new MyThreadPool(4);
        var task = threadPool.Enqueue(() => 0);
        for (int i = 0; i < 2; i++)
        {
            Assert.AreEqual(0, task.Result);
            Assert.IsTrue(task.IsCompleted);
        }
    }

    [Test]
    public void TestAddOneFailingTaskAndWait()
    {
        using var threadPool = new MyThreadPool(4);
        var task = threadPool.Enqueue<int>(() => throw new InvalidOperationException());
        for (int i = 0; i < 2; i++)
        {
            AssertFailsWithAggregateException(typeof(InvalidOperationException), null, () =>
            {
                var unused = task.Result;
            });
            Assert.IsTrue(task.IsCompleted);
        }
    }

    [Test]
    public void TestAddManySuccessfulTasksAndWait()
    {
        using var threadPool = new MyThreadPool(4);
        var tasks = new IMyTask<int>[10];
        for (int i = 0; i < 10; i++)
        {
            var i1 = i;
            tasks[i] = threadPool.Enqueue(() => i1);
        }

        for (int i = 0; i < 2; i++)
        {
            for (var j = 0; j < tasks.Length; j++)
            {
                Assert.AreEqual(j, tasks[j].Result);
                Assert.IsTrue(tasks[j].IsCompleted);
            }
        }
    }

    [Test]
    public void TestAddManyFailingTasksAndWait()
    {
        using var threadPool = new MyThreadPool(4);
        var tasks = new IMyTask<int>[20];
        for (int i = 0; i < 20; i++)
        {
            var i1 = i;
            tasks[i] = threadPool.Enqueue<int>(() => throw new InvalidOperationException(i1.ToString()));
        }

        for (int i = 0; i < 2; i++)
        {
            for (var j = 0; j < tasks.Length; j++)
            {
                var j1 = j;
                AssertFailsWithAggregateException(typeof(InvalidOperationException), j.ToString(), () =>
                {
                    var unused = tasks[j1].Result;
                });
                Assert.IsTrue(tasks[j].IsCompleted);
            }
        }
    }

    [Test]
    public void TestEnqueueAfterDisposing()
    {
        var threadPool = new MyThreadPool(4);
        threadPool.Dispose();
        Assert.Throws<InvalidOperationException>(() => threadPool.Enqueue(() => 0), "ThreadPool has been disposed");
        Assert.Throws<ObjectDisposedException>(() => threadPool.Dispose(), "Thread pool has been disposed");
    }

    [Test]
    public void TestContinueWithSuccessful()
    {
        using var threadPool = new MyThreadPool(4);
        var task1 = threadPool.Enqueue(() => 0);
        var task2 = task1.ContinueWith(it =>
        {
            Assert.AreEqual(0, it);
            Assert.IsTrue(task1.IsCompleted);
            return 42;
        });
        for (int i = 0; i < 2; i++)
        {
            Assert.AreEqual(42, task2.Result);
            Assert.IsTrue(task2.IsCompleted);
            Assert.AreEqual(0, task1.Result);
            Assert.IsTrue(task1.IsCompleted);
        }
    }

    [Test]
    public void TestContinueWithFailing()
    {
        using var threadPool = new MyThreadPool(4);
        var task1 = threadPool.Enqueue<int>(() => throw new InvalidOperationException());
        var task2 = task1.ContinueWith(_ =>
        {
            Assert.True(false);
            return 42;
        });
        for (int i = 0; i < 2; i++)
        {
            AssertFailsWithAggregateException(typeof(AggregateException), null, () =>
            {
                var unused = task2.Result;
            });
            Assert.IsTrue(task2.IsCompleted);
            AssertFailsWithAggregateException(typeof(InvalidOperationException), null, () =>
            {
                var unused = task1.Result;
            });

            Assert.IsTrue(task1.IsCompleted);
        }
    }

    [Test]
    public void TestRealThreadsAmount()
    {
        using var threadPool = new MyThreadPool(4);
        var tasks = new IMyTask<int>[4];
        var threadsSet = new HashSet<int>();
        for (int i = 0; i < 4; i++)
        {
            var i1 = i;
            tasks[i] = threadPool.Enqueue(() =>
            {
                Thread.Sleep(5);
                threadsSet.Add(Thread.CurrentThread.ManagedThreadId);
                return i1;
            });
        }

        for (int i = 0; i < 2; i++)
        {
            for (var j = 0; j < tasks.Length; j++)
            {
                Assert.AreEqual(j, tasks[j].Result);
                Assert.IsTrue(tasks[j].IsCompleted);
            }
        }

        Assert.AreEqual(4, threadsSet.Count);
    }

    private void AssertFailsWithAggregateException(Type innerExceptionType, string? message, Action action)
    {
        try
        {
            action();
        }
        catch (AggregateException ae)
        {
            Assert.AreEqual(innerExceptionType, ae.InnerException?.GetType());
            if (message != null) Assert.AreEqual(message, ae.InnerException?.Message);
        }
    }
}
}