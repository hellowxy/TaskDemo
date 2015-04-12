using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskMethod("Main Thread Task");
            var task = CreateTask("Task1");
            task.Start();
            int result = task.Result;
            Console.WriteLine("Result is:{0}",result);
            task = CreateTask("Task2");
            task.RunSynchronously();
            result = task.Result;
            Console.WriteLine("Result is:{0}",result);

            task = CreateTask("Task3");
            task.Start();
            while (!task.IsCompleted)
            {
                Console.WriteLine(task.Status);
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            Console.WriteLine(task.Status);
            result = task.Result;
            Console.WriteLine("Result is:{0}",result);
            Console.Read();
        }

        static Task<int> CreateTask(string name)
        {
            return new Task<int>(() => TaskMethod(name));
        }

        static int TaskMethod(string name)
        {
            Console.WriteLine("Task {0} is running on a thread id {1}. Is threadpool thread:{2}",
                name,Thread.CurrentThread.ManagedThreadId,Thread.CurrentThread.IsThreadPoolThread);
            Thread.Sleep(TimeSpan.FromSeconds(2));
            return 42;
        }
    }
}
