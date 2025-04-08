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
using static MyPCL.Utils.BaseUtil;
using static MyPCL.Utils.ThreadUtil;

namespace MyPCL.MyControls
{
    /// <summary>
    /// MyExtraButton.xaml 的交互逻辑
    /// </summary>
    public partial class MyExtraButton : Grid
    {
        // 自定义事件
        public event EventHandler<MouseButtonEventArgs> Click;

        public event EventHandler<MouseButtonEventArgs> RightClick;

        /// <summary>
        /// 进度条
        /// </summary>
        private double _Progress = 0;
        public double Progress
        {
            get => _Progress;
            set
            {
                if (Math.Abs(_Progress - value) < 0.0001) return;
                _Progress = value;

                var panProgress = GetTemplateChild("PanProgress") as Border;
                var rectProgress = GetTemplateChild("RectProgress") as RectangleGeometry;

                if (value < 0.0001)
                {
                    panProgress.Visibility = Visibility.Collapsed;
                }
                else
                {
                    panProgress.Visibility = Visibility.Visible;
                    rectProgress.Rect = new Rect(0, 40 * (1 - value), 40, 40 * value);
                }
            }
        }

        // 自定义属性
        public int Uuid = GetUuid();

        /// <summary>
        /// 图标
        /// </summary>
        private string _Logo = "";
        public string Logo
        {
            get => _Logo;
            set
            {
                if (_Logo == value) return;
                _Logo = value;
                var path = GetTemplateChild("Path") as Path;
                path.Data = Geometry.Parse(value);
            }
        }

        /// <summary>
        /// 图标缩放
        /// </summary>
        private double _LogoScale = 1;
        public double LogoScale
        {
            get => _LogoScale;
            set
            {
                _LogoScale = value;
                var path = GetTemplateChild("Path") as Path;
                if (path != null)
                    path.RenderTransform = new ScaleTransform(value, value);
            }
        }

        private bool _Show = false;
        public bool Show
        {
            get => _Show;
            set
            {
                if (_Show = value) return;
                _Show = value;
                RunInUi(() => {
                    if (value)
                    {
                        // 有了 // 显示动画
                        Visibility = Visibility.Visible;
                    }
                
                });
            }
        }

        public MyExtraButton()
        {
            InitializeComponent();
        }
    }
}
