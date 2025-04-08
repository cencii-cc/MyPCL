using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPCL.Utils
{
    public static class StringUtil
    {
        /// <summary>
        /// 获取字符串哈希值。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static ulong GetHash(string str)
        {
            ulong hash = 5381;
            for (int i = 0; i < str.Length; i++)
            {
                hash = ((hash << 5) ^ hash) ^ (ulong)str[i];
            }
            return hash ^ 0xA98F501BC684032FUL;
        }
    }
}
