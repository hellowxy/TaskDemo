/*********************************************************************
 * CLR Version： 4.0.30319.18444
 * file version：  V1.0.0.0
 * creater：  wangxiaoying
 * email：wangxiaoying_op@163.com
 * create time：2015/4/12 13:24:36
 * description：
 * 
 * 
 * **********************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConvertingAPMAndEAPDemo
{
    //A delegate using the out parameter is incompatible with the standard TPL API for 
    //converting the APM pattern to tasks.
    internal delegate string IncompatibleAsynchronousTask(out int threadId);

    internal delegate string AsynchronousTask(string threadName);
    class Program
    {
        static void Main(string[] args)
        {
            int threadId;
            AsynchronousTask compatiable = Test;
            IncompatibleAsynchronousTask incompatible = Test;
            Console.WriteLine("Option 1");
            //The key point for converting APM to TPL is a Task<T>.Factory.FromAsync method, where T 
            //is the asychronous operation result.
            Task<string> task =
                Task<string>.Factory.FromAsync(
                    compatiable.BeginInvoke("AsyncTaskThread", Callback, "a delegateasynchronous call"),
                    compatiable.EndInvoke);
            task.ContinueWith(
                t => Console.WriteLine("Callback is finished, now running a continuation! Result:{0}", t.Result));

            while (!task.IsCompleted)
            {
                Console.WriteLine(task.Status);
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            Console.WriteLine(task.Status);
            Thread.Sleep(TimeSpan.FromSeconds(1));

            Console.WriteLine("--------------------------------------------");
            Console.WriteLine();
            Console.WriteLine("Option 2");

            //The overload of FromAsync method doesn't allow specifying a callback that will be executed after
            //the asynchronous delegate call completes. We are able to replace this with continuation, but if 
            //the callback is important, we can use the first example.
            task = Task<string>.Factory.FromAsync(compatiable.BeginInvoke, compatiable.EndInvoke, "AsyncTaskThread",
                "adelegateasynchronous call");
            task.ContinueWith(
                t => Console.WriteLine("Task is completed, now running a continuation! Result:{0}", t.Result));
            while (!task.IsCompleted)
            {
                Console.WriteLine(task.Status); 
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            Console.WriteLine(task.Status);
            Thread.Sleep(TimeSpan.FromSeconds(1));

            Console.WriteLine("--------------------------------------------");
            Console.WriteLine();
            Console.WriteLine("Option 3");

            //EndMethod of the IncompatibleAsynchronousTask delegate uses the out parameter, and is 
            //not compatible with any FromAsync method overload.
            IAsyncResult ar = incompatible.BeginInvoke(out threadId, Callback, "adelegate asynchronous call");
            task = Task<string>.Factory.FromAsync(ar, _ => incompatible.EndInvoke(out threadId, ar));
            task.ContinueWith(
                t =>
                    Console.WriteLine("Task is completed, now running a continuation! Result:{0}, ThreadId:{1}",
                        t.Result, threadId));
            while (!task.IsCompleted)
            {
                Console.WriteLine(task.Status);
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }

            Console.WriteLine(task.Status);
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Console.WriteLine("---------------------------------------");
            Console.WriteLine();
            //To see what is going on with the underlying task, we are printing task.Status while waiting
            //for the asynchronous operation's result. We can see that the output is WaitingForActivation,
            //which means that the task was not actually started yet by the TPL infrastructure.
            /***********Converting from EAP Pattern*********************************/
            var tcs = new TaskCompletionSource<int>();
            var worker = new BackgroundWorker();
            worker.DoWork += (sender, eventArgs) =>
            {
                eventArgs.Result = TaskMethod("Background worker", 5);
            };
            worker.RunWorkerCompleted += (sender, eventArgs) =>
            {
                if (eventArgs.Error != null)
                {
                    tcs.SetException(eventArgs.Error);
                }
                else if (eventArgs.Cancelled)
                {
                    tcs.SetCanceled();
                }
                else
                {
                    tcs.SetResult((int) eventArgs.Result);
                }
            };
            worker.RunWorkerAsync();
            int result = tcs.Task.Result;
            Console.WriteLine("Result is:{0}",result);

            Console.Read();
        }

        private static void Callback(IAsyncResult ar)
        {
            Console.WriteLine("Starting a callback...");
            Console.WriteLine("State passed to a callback:{0}",ar.AsyncState);
            Console.WriteLine("Is thread pool thread:{0}",Thread.CurrentThread.IsThreadPoolThread);
            Console.WriteLine("Thread pool worker thread id:{0}",Thread.CurrentThread.ManagedThreadId);
        }

        private static string Test(string threadName)
        {
            Console.WriteLine("Starting...");
            Console.WriteLine("Is thread pool thread:{0}",Thread.CurrentThread.IsThreadPoolThread);
            Thread.Sleep(TimeSpan.FromSeconds(2));
            Thread.CurrentThread.Name = threadName;
            return string.Format("Thread name:{0}", Thread.CurrentThread.Name);
        }

        private static string Test(out int threadId)
        {
            Console.WriteLine("Starting...");
            Console.WriteLine("Is thread pool thread:{0}",Thread.CurrentThread.IsThreadPoolThread);
            Thread.Sleep(TimeSpan.FromSeconds(2));
            threadId = Thread.CurrentThread.ManagedThreadId;
            return string.Format("Thread pool worker thread id was:{0}", threadId);
        }

        private static int TaskMethod(string name, int seconds)
        {
            Console.WriteLine("Task {0} is running on a thread id{1}. Is thread pool thread:{2}",
                name,Thread.CurrentThread.ManagedThreadId,Thread.CurrentThread.IsThreadPoolThread);
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            return 42*seconds;
        }
    }
}
