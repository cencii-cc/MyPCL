using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static MyPCL.Utils.LogUtil;
using static MyPCL.ViewModules.HintModule;
using static MyPCL.Utils.ThreadUtil;
using System.Threading;
using MyPCL.ViewModules;

namespace MyPCL.Utils
{
    public static class BaseUtil
    {
        private static int Uuid = 1;
        private static object UuidLock;

        /// <summary>
        /// 返回一个ID，从1开始自增
        /// </summary>
        /// <returns></returns>
        public static int GetUuid()
        {
            if(UuidLock == null)
            {
                UuidLock = new object();
            }
            lock (UuidLock)
            {
                Uuid++;
                return Uuid;
            }
        }

        /// <summary>
        ///  获取系统运行时间（毫秒），保证为正 Long 且大于 1，但可能突变减小。
        /// </summary>
        /// <returns></returns>
        public static long GetTimeTick()
        {
            return Environment.TickCount + 2147483651L;
        }

        /// <summary>
        /// 返回一个枚举对应的字符串。
        /// </summary>
        /// <param name="EnumData">一个已经实例化的枚举类型。</param>
        /// <returns></returns>
        public static string GetStringFromEnum(System.Enum EnumData)
        {
            return System.Enum.GetName(EnumData.GetType(), EnumData);
        }


        // <summary>
        /// 将元素与 List 的混合体拆分为元素组。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="data">包含元素和列表混合的 IList</param>
        /// <returns>仅包含指定类型元素的 List</returns>
        public static List<T> GetFullList<T>(IList data)
        {
            List<T> result = new List<T>();
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i] is ICollection collection)
                {
                    foreach (var item in collection)
                    {
                        result.Add((T)item);
                    }
                }
                else
                {
                    result.Add((T)data[i]);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取格式类似于“11:08:52.037”的当前时间的字符串。
        /// </summary>
        /// <returns></returns>
        public static string GetTimeNow()
        {
            return DateTime.Now.ToString("HH:mm:ss.fff");
        }

        /// <summary>
        /// 打开网页。
        /// </summary>
        public static void OpenWebsite(string url)
        {
            try
            {
                if (url.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) && !url.StartsWith("minecraft://", StringComparison.InvariantCultureIgnoreCase))
                    throw new Exception($"{url}不是一个有效的网址，它必须以 http 开头！");
                Log("[System] 正在打开网页：" + url);
                Process.Start(url);
            }
            catch(Exception ex)
            {
                Log(ex, $"无法打开网页({url})");

            }
        }

        public static void ClipboardSet(string text,bool ShowSuccessHint = true)
        {
            RunInThread(() => 
            {
                int retryCount = 0;
            Retry:
                try
                {
                    RunInUi(() => 
                    {
                        Clipboard.Clear();
                        Clipboard.SetText(text);
                    });
                }
                catch(Exception ex)
                {
                    retryCount++;
                    if(retryCount <= 5)
                    {
                        Thread.Sleep(20);
                        goto Retry;
                    }
                    else
                    {
                        Log(ex, "可能由于剪贴板被其他程序占用，文本复制失败", LogLevel.Hint);
                    }
                }
                if (ShowSuccessHint) Hint("已成功复制！", HintType.Finish);
            });
        }

    }


    #region 系统

    /// <summary>
    /// 线程安全的，可以直接使用 For Each 的 List。
    /// <br/>
    /// 在使用 For Each 循环时，列表的结果可能并非最新，但不会抛出异常。
    /// </summary>
    public class SafeList<T> : SynchronizedCollection<T>,IEnumerable<T>, IEnumerable
    {
        // 构造方法
        public SafeList() : base()
        {
            
        }

        public SafeList(IEnumerable<T> Data) : base(new object(), Data)
        {

        }

        public static implicit operator SafeList<T>(List<T> data)
        {
            return new SafeList<T>(data);
        }

        public static implicit operator List<T>(SafeList<T> data)
        {
            return data.ToList();
        }
        // 基于 SyncLock 覆写
        public new IEnumerator<T> GetEnumerator()
        {
            lock (SyncRoot)
            {
                return Items.ToList().GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (SyncRoot)
            {
                return Items.ToList().GetEnumerator();
            }
        }
    }

    #endregion
}
