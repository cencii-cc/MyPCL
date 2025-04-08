using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPCL.Utils
{
    /// <summary>
    /// 数学工具
    /// </summary>
    public static class MathUtil
    {
        /// <summary>
        /// 将数值限定在某个范围内。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static double MathClamp(double value,double min,double max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
}
