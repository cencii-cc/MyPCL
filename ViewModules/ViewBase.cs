using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MyPCL.ViewModules
{
    public static class ViewBase
    {
        // 窗体声明
        public static MainWindow FrmMain;
        public static SplashScreen FrmStart;
        // 拖拽控件
        public static object DragControl = null;
    }
}
