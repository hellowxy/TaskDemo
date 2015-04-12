/*********************************************************************
 * CLR Version： 4.0.30319.18444
 * file version：  V1.0.0.0
 * creater：  wangxiaoying
 * email：wangxiaoying_op@163.com
 * create time：2015/4/12 14:22:16
 * description：
 * 
 * 
 * **********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RunningTasksInParalledDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var firstTask = new Task<int>(() => TaskMethod("First Task", 3));
            var secondTask = new Task<int>(() => TaskMethod("Second Task", 2));
            var whenAllTask = Task.WhenAll(firstTask, secondTask);
            whenAllTask.ContinueWith(t => Console.WriteLine("The first answer is {0}, the second is {1}",
                t.Result[0], t.Result[1]), TaskContinuationOptions.OnlyOnRanToCompletion);
            firstTask.Start();
            secondTask.Start();
            Thread.Sleep(TimeSpan.FromSeconds(4));

            var tasks = new List<Task<int>>();
            for (int i = 0; i < 4; i++)
            {
                int counter = i;
                var t = new Task<int>(() => TaskMethod(string.Format("Task{0}", counter), counter));
                tasks.Add(t);
                t.Start();
            }
            while (tasks.Count > 0)
            {
                //This method is useful to get a number of tasks' completion progress or to use timeout
                //while running the tasks. For example, we wait for a number of tasks and one of those
                //tasks is counting a timeout. If this task completes first, we just cancel those tasks
                //that are not completed yet.
                var finishedTask = Task.WhenAny(tasks).Result;
                tasks.Remove(finishedTask);
                Console.WriteLine("A task has been completed with result{0}",finishedTask.Result);
            }
            Console.Read();
        }
        static int TaskMethod(string name, int seconds)
        {
            Console.WriteLine("Task {0} is running on a thread id{1}. Is thread pool thread:{2}",
                name, Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread);
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            return 42 * seconds;
        }
    }
}
