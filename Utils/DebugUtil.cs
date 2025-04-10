using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPCL.Utils
{
    /// <summary>
    /// 调试工具
    /// </summary>
    public static class DebugUtil
    {
        /// <summary>
        /// 断言
        /// </summary>
        /// <param name="exp"></param>
        /// <exception cref="Exception"></exception>
        public static void DebugAssert(bool exp)
        {
            if (!exp)
            {
                throw new Exception("断言命中");
            }
        }
    }
}
