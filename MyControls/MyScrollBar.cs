using MyPCL.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using static MyPCL.Utils.BaseUtil;
using static MyPCL.Utils.AnimationUtil;
using static MyPCL.Utils.LogUtil;


namespace MyPCL.MyControls
{
    /// <summary>
    /// 自定义 ScrollBar 控件。
    /// </summary>
    public class MyScrollBar : ScrollBar
    {
        // 基础
        public int Uuid { get; } = GetUuid();

        // 指向动画
        private void RefreshColor()
        {
            try
            {
                // 判断当前颜色
                double newOpacity;
                string newColor;
                int time;

                if (!IsVisible)
                {
                    newOpacity = 0;
                    time = 20; // 防止错误的尺寸判断导致闪烁
                    newColor = "ColorBrush4";
                }
                else if (IsMouseCaptureWithin)
                {
                    newOpacity = 1;
                    newColor = "ColorBrush4";
                    time = 50;
                }
                else if (IsMouseOver)
                {
                    newOpacity = 0.9;
                    newColor = "ColorBrush3";
                    time = 130;
                }
                else
                {
                    newOpacity = 0.5;
                    newColor = "ColorBrush4";
                    time = 180;
                }

                // 触发颜色动画
                if (IsLoaded && AniControlEnabled == 0) // 防止默认属性变更触发动画
                {
                    // 有动画
                    AniStart(new AniData[] {
                    AaColor(this, ForegroundProperty, newColor, time),
                    AaOpacity(this, newOpacity - Opacity, time)
                }, $"MyScrollBar Color {Uuid}");
                }
                else
                {
                    // 无动画
                    AniStop($"MyScrollBar Color {Uuid}");
                    SetResourceReference(ForegroundProperty, newColor);
                    Opacity = newOpacity;
                }
            }
            catch (Exception ex)
            {
                Log(ex, "滚动条颜色改变出错");
            }
        }

        public MyScrollBar()
        {
            IsEnabledChanged +=(s, e) => RefreshColor();
            GotMouseCapture += (s, e) => RefreshColor();
            LostMouseCapture += (s, e) => RefreshColor();
            MouseEnter += (s, e) => RefreshColor();
            MouseLeave += (s, e) => RefreshColor();
            IsVisibleChanged += (s,e) => RefreshColor();
        }
    }
}
