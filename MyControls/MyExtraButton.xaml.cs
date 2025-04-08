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
using static MyPCL.Utils.AnimationUtil;
using MyPCL.Utils;

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
                        AniStart(new[] 
                        { 
                            AaScaleTransform(this, 0.3 - ((ScaleTransform)RenderTransform).ScaleX, 500, 60, new AniEaseOutFluent(AniEasePower.Weak)),
                            AaScaleTransform(this,0.7,500,60,new AniEaseOutBack(AniEasePower.Weak)),
                            AaHeight(this,50 - Height,200,0,new AniEaseOutFluent(AniEasePower.Weak))
                        }, $"MyExtraButton MainScale {Uuid}");
                    }
                    else
                    {
                        // 没了
                        AniStart(new[]
                        {
                            AaScaleTransform(this,-((ScaleTransform)RenderTransform).ScaleX, 100,0, new AniEaseInFluent(AniEasePower.Weak)),
                            AaHeight(this,-Height,400,100,new AniEaseOutFluent()),
                            AaCode(()=>{ Visibility = Visibility.Collapsed; },0,true)
                        }, $"MyExtraButton MainScale {Uuid}");
                    }
                    // 防止缩放动画中依然可以点进去
                    IsHitTestVisible = value;
                });
            }
        }

        public delegate bool ShowCheckDelegate();
        public ShowCheckDelegate ShowCheck = null;

        /// <summary>
        /// 刷新显示
        /// </summary>
        public void ShowRefresh()
        {
            if(ShowCheck != null) Show = ShowCheck();
        }

        public MyExtraButton()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 发出一圈波浪效果提示。
        /// </summary>
        public void Ribble()
        {
            RunInUi(() =>
            {
                Border shape = new Border
                {
                    CornerRadius = new CornerRadius(1000),
                    BorderThickness = new Thickness(0.001),
                    Opacity = 0.5,
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    RenderTransform = new ScaleTransform()
                };
                shape.SetResourceReference(Border.BackgroundProperty, "ColorBrush5");
                PanScale.Children.Insert(0, shape);

                AniData[] animations = new AniData[]
                {
                    AaScaleTransform(shape, 13, 1000, ease : new AniEaseInoutFluent(AniEasePower.Strong, 0.3)),
                    AaOpacity(shape, -shape.Opacity, 1000),
                    AaCode(() => PanScale.Children.Remove(shape), after : true)
                };
                AniStart(animations, "ExtraButton Ribble " + GetUuid());
            });
        }

    }
}
