using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MyPCL.Utils.DebugUtil;
using System.Windows;

namespace MyPCL.ViewModules
{
    /// <summary>
    /// UI模块
    /// </summary>
    public static class ViewMUI
    {

        public static int DPI = (int)System.Drawing.Graphics.FromHwnd(IntPtr.Zero).DpiX;


        /// <summary>
        /// 相对增减控件的左边距。
        /// </summary>
        /// <param name="control"></param>
        /// <param name="newValue"></param>
        public static void DeltaLeft(FrameworkElement control,double newValue)
        {
            // 安全性检查
            DebugAssert(!double.IsNaN(newValue));
            DebugAssert(!double.IsInfinity(newValue));

            if (control is Window window)
            {
                // 窗口改变
                window.Left += newValue;
            }
            else
            {
                // 根据 HorizontalAlignment 改变数值
                switch (control.HorizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                    case HorizontalAlignment.Stretch:
                        control.Margin = new Thickness(control.Margin.Left + newValue, control.Margin.Top, control.Margin.Right, control.Margin.Bottom);
                        break;
                    case HorizontalAlignment.Right:
                        control.Margin = new Thickness(control.Margin.Left, control.Margin.Top, control.Margin.Right - newValue, control.Margin.Bottom);
                        //control.Margin = new Thickness(control.Margin.Left, control.Margin.Top, ((Object)control.Parent).ActualWidth - control.ActualWidth - newValue, control.Margin.Bottom);
                        break;
                    default:
                        DebugAssert(false);
                        break;
                }
            }
        }

        /// <summary>
        /// 相对增减控件的上边距。
        /// </summary>
        /// <param name="control"></param>
        /// <param name="newValue"></param>
        public static void DeltaTop(FrameworkElement control, double newValue)
        {
            // 安全性检查
            DebugAssert(!double.IsNaN(newValue));
            DebugAssert(!double.IsInfinity(newValue));

            if (control is Window window)
            {
                // 窗口改变
                window.Top += newValue;
            }
            else
            {
                // 根据 VerticalAlignment 改变数值
                switch (control.VerticalAlignment)
                {
                    case VerticalAlignment.Top:
                        control.Margin = new Thickness(control.Margin.Left, control.Margin.Top + newValue, control.Margin.Right, control.Margin.Bottom);
                        break;
                    case VerticalAlignment.Bottom:
                        control.Margin = new Thickness(control.Margin.Left, control.Margin.Top, control.Margin.Right, control.Margin.Bottom - newValue);
                        break;
                    default:
                        DebugAssert(false);
                        break;
                }
            }
        }
    }
}
