using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPCL.ViewModules
{
    /// <summary>
    /// 支持负数与浮点数的矩形。
    /// </summary>
    public class MyRect
    {
        // 属性
        public double Width { get; set; } = 0;
        public double Height { get; set; } = 0;
        public double Left { get; set; } = 0;
        public double Top { get; set; } = 0;

        // 构造函数
        public MyRect()
        {
        }

        public MyRect(double left, double top, double width, double height)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }
    }
}
