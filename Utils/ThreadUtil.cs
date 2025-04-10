using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace MyPCL.Utils
{
    /// <summary>
    /// 线程工具
    /// </summary>
    public static class ThreadUtil
    {

        private static readonly int UiThreadId = Thread.CurrentThread.ManagedThreadId;

        /// <summary>
        /// 当前线程是否为主线程。
        /// </summary>
        /// <returns></returns>
        public static bool RunInUi()
        {
            return Thread.CurrentThread.ManagedThreadId == UiThreadId;
        }

        /// <summary>
        /// 确保在 UI 线程中执行代码，代码按触发顺序执行。<br/>
        /// 如果当前并非 UI 线程，也不阻断当前线程的执行。
        /// </summary>
        /// <param name="action"></param>
        /// <param name="forceWaitUntilLoaded"></param>
        public static void RunInUi(Action action,bool forceWaitUntilLoaded = false)
        {
            if (forceWaitUntilLoaded) Application.Current.Dispatcher.InvokeAsync(action, System.Windows.Threading.DispatcherPriority.Loaded);
            else if (RunInUi()) action();
            else Application.Current.Dispatcher.InvokeAsync(action);
        }


        /// <summary>
        /// 确保在 UI 线程中执行代码。<br/>
        /// 如果当前并非 UI 线程，则会阻断当前线程，直至 UI 线程执行完毕。<br/>
        /// 为防止线程互锁，请仅在开始加载动画、从 UI 获取输入时使用！
        /// </summary>
        /// <param name="action"></param>
        public static void RunInUiWait(Action action)
        {
            if (RunInUi())
                action();
            else
                Application.Current.Dispatcher.Invoke(action);
        }

        /// <summary>
        /// 确保在 UI 线程中执行代码。<br/>
        /// 如果当前并非 UI 线程，则会阻断当前线程，直至 UI 线程执行完毕。<br/>
        /// 为防止线程互锁，请仅在开始加载动画、从 UI 获取输入时使用！
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public static T RunInUiWait<T>(Func<T> action)
        {
            if (RunInUi())
            {
                return action();
            }
            else
            {
                return Application.Current.Dispatcher.Invoke(action);
            }
        }

        /// <summary>
        ///  在新的工作线程中执行代码。
        /// </summary>
        /// <param name="Action"></param>
        /// <param name="Name"></param>
        /// <param name="Priority"></param>
        /// <returns></returns>
        public static Thread RunInNewThread(Action Action,string Name = null,ThreadPriority Priority = ThreadPriority.Normal)
        {
            Thread th = new Thread(() =>
            {
                try
                {
                    Action();
                }
                catch (ThreadInterruptedException)
                {
                    //Log($"{Name}：线程已中止");
                }
                catch (Exception ex)
                {
                    //Log(ex, $"{Name}：线程执行失败", LogLevel.Feedback);
                }
            });
            th.Name = Name ?? $"Runtime New Invoke {BaseUtil.GetUuid()}#";
            th.Priority = Priority;
            th.Start();
            return th;
        }

        /// <summary>
        /// 确保在工作线程中执行代码。
        /// </summary>
        /// <param name="action"></param>
        public static void RunInThread(Action action)
        {
            if (RunInUi()) RunInNewThread(action, $"Runtime Invoke {BaseUtil.GetUuid()} #");
            else action();
        }
    }
}
