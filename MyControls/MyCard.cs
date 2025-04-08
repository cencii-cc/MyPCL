using MyPCL.Utils;
using MyPCL.Utils.Minecraft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace MyPCL.MyControls
{
    /// <summary>
    /// 自定义卡片
    /// </summary>
    public class MyCard : Grid
    {
        /// <summary>
        /// 主网格
        /// </summary>
        private readonly Grid MainGrid;

        /// <summary>
        /// 主阴影
        /// </summary>
        public SystemDropShadowChrome MainChrome { get; }

        /// <summary>
        /// 主边框
        /// </summary>
        private readonly Border MainBorder;

        /// <summary>
        /// 主边框内容
        /// </summary>
        public UIElement BorderChild
        {
            get
            {
                return MainBorder.Child;
            }
            set
            {
                if(value as UIElement != null)
                {
                    MainBorder.Child = value;
                }
            }
        }


        /// <summary>
        /// 主文本
        /// </summary>
        private TextBlock _MainTextBlock;

        /// <summary>
        /// 主文本
        /// </summary>
        public TextBlock MainTextBlock
        {
            get
            {
                //当父级触发 Loaded 时，本卡片可能尚未触发 Loaded（该事件从父级向子级调用），因此这会是 null。手动触发以确保控件已加载。
                return _MainTextBlock;
            }
            set
            {
                _MainTextBlock = value;
            }
        }

        /// <summary>
        /// 主图标
        /// </summary>
        private System.Windows.Shapes.Path _MainSwap;

        // 属性

        public int Uuid = BaseUtil.GetUuid();

        /// <summary>
        /// 文本
        /// </summary>
        public InlineCollection Inlines
        {
            get
            {
                return MainTextBlock.Inlines;
            }
        }

        /// <summary>
        /// 卡片圆角
        /// </summary>
        public CornerRadius CornerRadius
        {
            get
            {
                return MainChrome.CornerRadius;
            }
            set
            {
                MainChrome.CornerRadius = value;
                MainBorder.CornerRadius = value;
            }
        }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            get
            {
                return GetValue(TitleProperty).ToString();
            }
            set
            {
                SetValue(TitleProperty, value);
                if(_MainTextBlock != null)
                {
                    // 设置文本
                    MainTextBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// 标题属性
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
            "Title",
            typeof(string),
            typeof(MyCard),
            new PropertyMetadata(""));

        public MyCard()
        {
            MainChrome = new SystemDropShadowChrome
            {
                Margin = new Thickness(-9.5, -9, 0.5, -0.5),
                Opacity = 0.1,
                CornerRadius = new CornerRadius(6)
            };
            MainChrome.SetResourceReference(SystemDropShadowChrome.ColorProperty, "ColorObject1");
            Children.Insert(0, MainChrome);
            MainBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(205, 255, 255, 255)),
                CornerRadius = new CornerRadius(6),
                IsHitTestVisible = false
            };
            Children.Insert(1, MainBorder);
            MainGrid = new Grid();
            Children.Add(MainGrid);
            // 绑定事件
            this.Loaded += Init;
        }

        private bool IsLoad = false;

        #region 折叠相关

        /**
         * 若设置了 CanSwap，或 SwapControl 不为空，则判定为会进行折叠
           这是因为不能直接在 XAML 中设置 SwapControl
         */


        public object SwapControl { get; set;}

        public bool CanSwap { set; get; } = false;

        /// <summary>
        /// 被折叠的种类，用于控件虚拟化
        /// </summary>
        public int SwapType { set; get; }

        /// <summary>
        /// 是否折叠
        /// </summary>
        private bool _IsSwaped = false;

        public bool IsSwaped
        {
            get
            {
                return _IsSwaped;
            }
            set
            {
                if(_IsSwaped == value)
                {
                    return;
                }
                _IsSwaped = value;
                if(SwapControl == null)
                {
                    return;
                }
                if(!_IsSwaped && SwapControl is StackPanel)
                {
                    
                }
            }
        }

        #endregion



        private void Init(object sender, RoutedEventArgs e)
        {
            // 首次加载
            if (IsLoad) return;
            IsLoad = true;
            // 初次加载限定
            // 文本初始化
            if(MainTextBlock != null)
            {
                MainTextBlock = new TextBlock
                {
                    // 水平左侧对齐
                    HorizontalAlignment = HorizontalAlignment.Left,
                    // 垂直顶部对齐
                    VerticalAlignment = VerticalAlignment.Top,
                    // 外边间距
                    Margin = new Thickness(15, 12, 0, 0),
                    // 字体粗细
                    FontWeight = FontWeights.Bold,
                    // 字体大小
                    FontSize = 13,
                    // 不可点击
                    IsHitTestVisible = false
                };
                // 设置前景色
                MainTextBlock.SetResourceReference(TextBlock.ForegroundProperty, "ColorBrush1");
                MainTextBlock.SetBinding(TextBlock.TextProperty, new Binding("Title")
                {
                    Source = this,
                    Mode = BindingMode.OneWay
                });
                // 添加
                MainGrid.Children.Add(MainTextBlock);
            }
        }

        public void StackInstall()
        {
            //StackInstall(SwapControl, SwapType, Title);
            //TriggerForceResize();
        }

        public static void StackInstall(ref StackPanel Stack,int Type,string CardTitle  = "")
        {
            // 这一部分的代码是好几年前留下的究极屎坑，当时还不知道该咋正确调用这种方法，就写了这么一坨屎
            // 但是现在……反正勉强能用……懒得改了就这样吧.jpg
            // 别骂了别骂了.jpg
            if (Stack.Tag == null) return;
            // 排序
            switch (Type)
            {
                case 3:
                    Stack.Tag = ((List<DlOptiFineListEntry>)Stack.Tag).OrderBy(a => a.NameDisplay).ToList();
                    break;
                case 4: case 10:
                    break;
                case 6:
                    break;
                case 8: case 9:
                    break;
                        
            }
        }  
    }
}
