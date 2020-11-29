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
            var que = new Queue<SortedList<int, int>>();
            var sl = new SortedList<int, int>
            {
                { 0, 42 }
            };
            que.Enqueue(sl);
            filledCollection.Add(que);

            nullCollection = null;

            hasNullCollection = new List<Queue<SortedList<int, int>>>
            {
                null
            };

            queueHasNullCollection = new List<Queue<SortedList<int, int>>>();
            var queueWithNull = new Queue<SortedList<int, int>>();
            queueWithNull.Enqueue(null);
            queueHasNullCollection.Add(queueWithNull);

            lambda = ExpressionTreesGenerator.GenerateLambda();
        }

        [Test]
        public void FilledCollectionAccess()
        {
            Assert.AreEqual(42, lambda(filledCollection));
        }

        [Test]
        public void NullCollectionAccess()
        {
            Assert.AreEqual(null, lambda(nullCollection));
        }

        [Test]
        public void HasNullCollectionAccess()
        {
            Assert.AreEqual(null, lambda(hasNullCollection));
        }

        [Test]
        public void QueueHasNullCollectionAccess()
        {
            Assert.AreEqual(null, lambda(queueHasNullCollection));
        }

    }
}
