/*********************************************************************
 * CLR Version： 4.0.30319.18444
 * file version：  V1.0.0.0
 * creater：  wangxiaoying
 * email：wangxiaoying_op@163.com
 * create time：2015/4/12 14:04:01
 * description：
 * 
 * 
 * **********************************************************************/
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HandlingExceptionsDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Task<int> task;
            try
            {
                task = Task.Run(() => TaskMethod("Task 1", 2));
                //The Get part of the Result property makes the current thread wait until the completion
                //of the task and propagates the exception to the current thread.
                int result = task.Result;
                Console.WriteLine("Result:{0}", result);
            }
            catch (Exception ex)
            {
                //The exception is a wrapper exception called AggregateException
                Console.WriteLine("Exception caught:{0}", ex);
            }
            Console.WriteLine("--------------------------------------");
            Console.WriteLine();
            try
            {
                task = Task.Run(() => TaskMethod("Task2", 2));
                //It is mostly the same as the first example, but to access the task result we use the
                //GetAwaiter and GetResult methods. In this case, we do not have wrapper exception
                //because it is unwrapped by the TPL infrastructure.
                int result = task.GetAwaiter().GetResult();
                Console.WriteLine("Result: {0}", result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught:{0}", ex);
            }
            Console.WriteLine("--------------------------------------");
            Console.WriteLine();

            var t1 = new Task<int>(() => TaskMethod("Task3", 3));
            var t2 = new Task<int>(() => TaskMethod("Task4", 2));
            var complexTask = Task.WhenAll(t1, t2);
            
            var exceptionHandler = complexTask.ContinueWith(t => Console.WriteLine("Exception caught:{0}", t.Exception),
                TaskContinuationOptions.OnlyOnFaulted);
            t1.Start();
            t2.Start();
            Console.Read();
        }

        static int TaskMethod(string name, int seconds)
        {
            Console.WriteLine("Task {0} is running on a thread id{1}. Is thread pool thread:{2}",
                name, Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread);
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            throw new Exception("Boom!");
            return 42;
        }
    }
}
