using System;
using System.Collections.Generic;
using ExpTrees;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class Test
    {
        private readonly List<Queue<SortedList<int, int>>> filledCollection;
        private readonly List<Queue<SortedList<int, int>>> nullCollection;
        private readonly List<Queue<SortedList<int, int>>> hasNullCollection;
        private readonly List<Queue<SortedList<int, int>>> queueHasNullCollection;

        private readonly Func<List<Queue<SortedList<int, int>>>, int?> lambda;

        public Test()
        {
            filledCollection = new List<Queue<SortedList<int, int>>>();
            var queue = new Queue<SortedList<int, int>>();
            var sl = new SortedList<int, int>();
            sl.Add(0, 42);
            queue.Enqueue(sl);
            filledCollection.Add(queue);

            nullCollection = null;

            hasNullCollection = new List<Queue<SortedList<int, int>>>();
            hasNullCollection.Add(null);

            queueHasNullCollection = new List<Queue<SortedList<int, int>>>();
            var queueWithNull = new Queue<SortedList<int, int>>();
            queue.Enqueue(null);
            queueHasNullCollection.Add(queueWithNull);

            lambda = ExpressionTreesGenerator.GenerateLambda();
        }

        [Test]
        public void FilledCollectionAccess()
        {
            int? res = lambda(filledCollection);
            Assert.AreEqual(42, res.Value);
        }

        [Test]
        public void NullCollectionAccess()
        {
            int? res = lambda(nullCollection);
            Assert.AreEqual(null, res.Value);
        }

        [Test]
        public void HasNullCollectionAccess()
        {
            int? res = lambda(hasNullCollection);
            Assert.AreEqual(null, res.Value);
        }

        [Test]
        public void QueueHasNullCollectionAccess()
        {
            int? res = lambda(queueHasNullCollection);
            Assert.AreEqual(null, res.Value);
        }

    }
}
