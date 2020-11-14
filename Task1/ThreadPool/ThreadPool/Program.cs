using System;

namespace ThreadPool
{
    static class Program
    {
        static void Main(string[] args)
        {
            var initial = 0;
            var tasksCount = 1000u;
            var threads = 4u;
            var executor = new ThreadPoolExecutor(threads);

            var task = executor.Enqueue(() => initial);

            for (int i = 0; i < tasksCount; i++)
            {
                task = task.ContinueWith((prev => prev + 1));
            }

            Console.Out.WriteLine($"Result {task.GetResult()}");
            
            executor.Dispose();
        }
    }
}