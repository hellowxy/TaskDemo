using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CancellationDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            //We pass a cancellation token to the underlying task once and then to the task constructor the 
            //second time, that's because if we cancel the task before it was actually started, its TPL infrastructure
            //is responsible for dealing with the cancellation, because our code will not execute at all. After a 
            //Task was started, we are now fully responsible for the cancellation process, and after we cancelled the task,
            //its status is still RanToCompletion, because from TPL's perspective, the task finished its job normally.
            //
            var longTask = new Task<int>(() => TaskMetohd("Task 1", 10, cts.Token), cts.Token);
            Console.WriteLine(longTask.Status);
            cts.Cancel();
            Console.WriteLine(longTask.Status);
            Console.WriteLine("First task has been cancelled before execution");
            cts = new CancellationTokenSource();
            longTask = new Task<int>(() => TaskMetohd("Task 2",10,cts.Token),cts.Token);
            longTask.Start();
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                Console.WriteLine(longTask.Status);
            }
            cts.Cancel();
            Console.WriteLine("-------------------------------");
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                Console.WriteLine(longTask.Status);
            }
            Console.WriteLine("A task has been completed with result {0}.",longTask.Result);
            Console.Read();
        }

        private static int TaskMetohd(string name, int seconds, CancellationToken token)
        {
            Console.WriteLine("Task {0} is running on a thread id{1}. Is thread pool thread:{2}",
                name,Thread.CurrentThread.ManagedThreadId,Thread.CurrentThread.IsThreadPoolThread);
            for (int i = 0; i < seconds; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                if (token.IsCancellationRequested)
                {
                    return -1;
                }
            }
            return 42*seconds;
        }
    }
}
