/*********************************************************************
 * CLR Version： 4.0.30319.18444
 * file version：  V1.0.0.0
 * creater：  wangxiaoying
 * email：wangxiaoying_op@163.com
 * create time：2015/4/10 22:02:58
 * description：
 * The Task.Run method is just a shortcut to Task.Factory.StartNew, but the latter method
 * has additional options 
 * **********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CombiningTasksDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var firstTask = new Task<int>(() => TaskMethod("First Task", 3));
            var secondTask = new Task<int>(() => TaskMethod("Second Task", 2));
            firstTask.ContinueWith(
                t => Console.WriteLine("The first answer is {0}. Thread id {1}, is thread pool thread:{2}",
                    t.Result, Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread),
                TaskContinuationOptions.OnlyOnRanToCompletion);
            firstTask.Start();
            secondTask.Start();
            Thread.Sleep(TimeSpan.FromSeconds(4));

            //We run a continuation to the secondTask and try to execute it synchronously by specifying a 
            //TaskContinuationOptions.ExecuteSynchronously option. This is useful technique when the continuation
            //is very short-lived, and it will be faster to run it on the main thread than to put it on a thread pool.
            //We are able to achieve this because the second task is completed by that moment.
            Task continuation = secondTask.ContinueWith(
                t => Console.WriteLine("The second answer is {0}. Thread id {1}, is thread pool thread:{2}",
                    t.Result, Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread),
                TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);

            continuation.GetAwaiter().OnCompleted(()=>
                Console.WriteLine("Continuation Task Completed! Thread id {0}, is thread pool thread:{1}",Thread.CurrentThread.ManagedThreadId,
                Thread.CurrentThread.IsThreadPoolThread));
            Thread.Sleep(TimeSpan.FromSeconds(2));
            Console.WriteLine();

            //The child task must be created while running a parent task to attach to the parent properly.
            //The parent task will not complete until all child tasks finish its work.
            firstTask = new Task<int>(() =>
            {
                var innerTask = Task.Factory.StartNew(() => TaskMethod("Second Task", 5),TaskCreationOptions.AttachedToParent);
                innerTask.ContinueWith(t => TaskMethod("Third Task", 2));
                return TaskMethod("First Task", 2);
            });

            firstTask.Start();

            while (!firstTask.IsCompleted)
            {
                Console.WriteLine(firstTask.Status);
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            Console.WriteLine(firstTask.Status);
           
            Console.Read();
        }

        static int TaskMethod(string name, int seconds)
        {
            Console.WriteLine("Task {0} is running on a thread id {1}. Is thread pool thread:{2}",
                name,Thread.CurrentThread.ManagedThreadId,Thread.CurrentThread.IsThreadPoolThread);
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            return 42*seconds;
        }
    }
}
