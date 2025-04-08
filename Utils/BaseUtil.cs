using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
