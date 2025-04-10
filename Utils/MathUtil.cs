using MyPCL.ViewModules;
using Newtonsoft.Json.Linq;
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

        /// <summary>
        /// 提供 MyColor 类型支持的 Math.Round。
        /// </summary>
        /// <param name="col"></param>
        /// <param name="w"></param>
        /// <returns></returns>
        public static MyColor MathRound(MyColor col,int w = 0)
        {
            return new MyColor(
                Math.Round(col.A, w),
                Math.Round(col.R, w),
                Math.Round(col.G, w),
                Math.Round(col.B, w)
            );
        }

        /// <summary>
        /// 获取两数间的百分比。小数点精确到 6 位。
        /// </summary>
        /// <param name="valueA"></param>
        /// <param name="valueB"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        public static double MathPercent(double valueA,double valueB,double percent)
        {
            return Math.Round(valueA * (1 - percent) + valueB * percent, 6);
        }

        /// <summary>
        /// 获取两颜色间的百分比，根据 RGB 计算。小数点精确到 6 位。
        /// </summary>
        /// <param name="valueA"></param>
        /// <param name="valueB"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        public static MyColor MathPercent(MyColor valueA, MyColor valueB, double percent)
        {
            return MathRound(valueA * (1 - percent) + valueB * percent, 6);
        }
    }
}
