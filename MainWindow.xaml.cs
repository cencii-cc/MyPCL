using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static MyPCL.Utils.ThreadUtil;
using static MyPCL.ViewModules.ViewBase;

namespace MyPCL
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PanBack.MouseMove += (send,e) => DragDoing();
        }

        #region 控件拖拽
        /// <summary>
        /// 在时钟中调用，使得即使鼠标在窗口外松开，也可以释放控件
        /// </summary>
        public void DragTick()
        {
            if (DragControl == null) return;
            if(Mouse.LeftButton != MouseButtonState.Pressed)
            {

            }
        }

        /// <summary>
        /// 在鼠标移动时调用，以改变 Slider 位置
        /// </summary>
        public void DragDoing()
        {
            if (DragControl == null) return;
            if (Mouse.LeftButton == MouseButtonState.Pressed) ((dynamic)DragControl).DragDoing();
            else DragStop();
        }

        /// <summary>
        /// 停止拖拽
        /// </summary>
        public void DragStop()
        {
            // 存在其他线程调用的可能性，因此需要确保在 UI 线程运行
            RunInUiWait(() =>
            {
                if(DragControl == null) return;
                object contorl = DragControl;
                DragControl = null;
                // 控件会在该事件中判断 DragControl，所以得放在后面
                ((dynamic)contorl).DragStop();
            });
        }


        #endregion
    }
}
