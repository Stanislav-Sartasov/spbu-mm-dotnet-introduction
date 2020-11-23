using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using ThreadPool;

namespace ThreadPoolTests {
    [TestFixture]
    public class Tests {
        private const int ThreadCount = 4;
        private const int TaskCount = ThreadCount + 1;
        private MyThreadPool _threadPool;

        [SetUp]
        public void SetupExecutor() {
            _threadPool = new MyThreadPool(ThreadCount);
        }

        [TearDown]
        public void DisposeExecutor() {
            _threadPool.Dispose();
        }

        [Test]
        public void TestOneTaskAdding() {
            IMyTask<bool> task = _threadPool.Enqueue(BoolJob);
            Assert.AreEqual(BoolJob(), task.Result);
        }

        [Test]
        public void TestMultipleTasksAdding() {
            var intTasks = new List<IMyTask<int>>();

            for (int i = 0; i < TaskCount; i++) {
                intTasks.Add(_threadPool.Enqueue(OneMoreIntJob));
            }

            foreach (var task in intTasks) {
                Assert.AreEqual(OneMoreIntJob(), task.Result);
            }
        }

        [Test]
        public void TestContinueWith() {
            var stringTasks = new List<IMyTask<String>>();
            var greetingTasks = new List<IMyTask<String>>();

            for (int i = 0; i < TaskCount; i++) {
                int value = i;
                stringTasks.Add(_threadPool.Enqueue(() => {
                    Thread.Sleep(100);
                    return StringJob() + value;
                }));
            }

            for (int i = 0; i < TaskCount; i++) {
                greetingTasks.Add(stringTasks[i].ContinueWith(ConstructGreeting));
            }

            for (int i = 0; i < TaskCount; i++) {
                Assert.AreEqual(ConstructGreeting(StringJob() + i), greetingTasks[i].Result);
            }
        }

        [Test]
        public void TestThreadCount() {
            var threadIdsSet = new HashSet<int>();

            void BlockUntilSuccess() {
                while (true) {
                    lock (threadIdsSet) {
                        if (threadIdsSet.Count == ThreadCount) {
                            break;
                        }
                    }
                }
            }


            for (int i = 0; i < ThreadCount; i++) {
                _threadPool.Enqueue(() => {
                    lock (threadIdsSet) {
                        threadIdsSet.Add(Thread.CurrentThread.ManagedThreadId);
                    }

                    BlockUntilSuccess();

                    return 0;
                });
            }

            BlockUntilSuccess();

            Assert.AreEqual(threadIdsSet.Count, ThreadCount);
        }

        private int IntJob() {
            return 1 + 1;
        }

        private int OneMoreIntJob() {
            return 13 / 11;
        }

        private bool BoolJob() {
            int intJobResult = IntJob();
            bool isDraculaTheBestStory = intJobResult > 1;
            return isDraculaTheBestStory;
        }

        private String StringJob() {
            return "meow";
        }

        private String ConstructGreeting(String name) {
            return "Hello, " + name;
        }
    }
}