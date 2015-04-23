/*********************************************************************
 * CLR Version： 4.0.30319.18444
 * file version：  V1.0.0.0
 * creater：  wangxiaoying
 * email：wangxiaoying_op@163.com
 * create time：2015/4/12 15:48:25
 * description：
 * The TaskScheduler is responsible for how the task will be executed. The default task 
 * scheduler puts tasks on a thread pool worker thread.
 * 
 * **********************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskSchedulerDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonSync_Click(object sender, EventArgs e)
        {
            textBox1.Text = string.Empty;
            try
            {
                //If we uncomment the following code, which gets the result with the UI thread task
                //scheduler provided, we will never get any result. This is a classical deadlock situation: we are
                //dispatching an operation in the queue of the UI thread and the UI thread waits for this
                //operation to complete, but as it waits, it cannot run the operation. never use the synchronous
                //operations on task scheduled to the UI thread.
                //var result = TaskMethod(TaskScheduler.FromCurrentSynchronizationContext()).Result;
                var result = TaskMethod().Result;
            }
            catch (Exception ex)
            {
                textBox1.Text = ex.InnerException.Message; 
            }
        }

        private void buttonAsync_Click(object sender, EventArgs e)
        {
            textBox1.Text = string.Empty;
            Cursor.Current = Cursors.WaitCursor;
            var task = TaskMethod();
            //TaskScheduler.FromCurrentSynchronizationContext() instructs the TPL infrastructure to put
            // a code inside the conituation on current synchronization context in this example, it is
            //the UI thread, then run it asynchronously with a help of
            //the UI thread message loop. This resolves the problem with accessing UI controls from 
            //another thread, but still keeps our UI responsive.
            task.ContinueWith(t =>
            {
                textBox1.Text = t.Exception.InnerException.Message;
                Cursor.Current = Cursors.Arrow;
            }, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.FromCurrentSynchronizationContext());

        }

        private void buttonAsyncOk_Click(object sender, EventArgs e)
        {
            textBox1.Text = string.Empty;
            Cursor.Current = Cursors.WaitCursor;
            var task = TaskMethod(TaskScheduler.FromCurrentSynchronizationContext());
            task.ContinueWith(t =>
            {
                Cursor.Current = Cursors.Arrow;
            }, CancellationToken.None, TaskContinuationOptions.None,
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        Task<string> TaskMethod()
        {
            
            return TaskMethod(TaskScheduler.Default);
        }

        Task<string> TaskMethod(TaskScheduler scheduler)
        {
            var delay = Task.Delay(TimeSpan.FromSeconds(5));
            return delay.ContinueWith(t =>
            {
                var str = string.Format("Task is running on a thread id {0}. Is thread pool thread:{1}",
                    Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread);
                textBox1.Text = str;
                return str;
            },scheduler);
        }
    }
}
