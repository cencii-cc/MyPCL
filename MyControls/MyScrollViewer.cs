using MyPCL.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using static MyPCL.Utils.AnimationUtil;
using static MyPCL.Utils.BaseUtil;
using static MyPCL.Utils.MathUtil;
using static MyPCL.ViewModules.ViewBase;

namespace MyPCL.MyControls
{
    /// <summary>
    ///  自定义 ScrollViewer 控件。
    /// </summary>
    public class MyScrollViewer : ScrollViewer
    {
        public double DeltaMult { get; set; } = 1;

        private double RealOffset;

        public MyScrollViewer()
        {
            PreviewMouseWheel += MyScrollViewer_PreviewMouseWheel;
            ScrollChanged += MyScrollViewer_ScrollChanged;
            IsVisibleChanged += MyScrollViewer_IsVisibleChanged;
            PreviewGotKeyboardFocus += MyScrollViewer_PreviewGotKeyboardFocus;
            Loaded += MyScrollViewer_Loaded;
        }

        private void MyScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta == 0 || ActualHeight == 0 || ScrollableHeight == 0) return;

            var sourceType = e.Source.GetType();
            if (((dynamic)Content).TemplatedParent == null && (
                (typeof(ComboBox).IsAssignableFrom(sourceType) && ((ComboBox)e.Source).IsDropDownOpen) ||
                (typeof(TextBox).IsAssignableFrom(sourceType) && ((TextBox)e.Source).AcceptsReturn) ||
                typeof(ComboBoxItem).IsAssignableFrom(sourceType) ||
                e.Source is CheckBox))
            {
                // 如果当前是在对有滚动条的下拉框或文本框执行，则不接管操作
                return;
            }

            e.Handled = true;
            PerformVerticalOffsetDelta(-e.Delta);

            // 关闭 Tooltip (#2552)
            foreach (var tooltipBorder in App.ShowingTooltips)
            {
                AniStart(AaOpacity(tooltipBorder, -1, 100), $"Hide Tooltip {GetUuid()}");
            }
        }

        public void PerformVerticalOffsetDelta(double delta)
        {
            AniStart(
                AaDouble(
                (animDelta) =>
                {
                    RealOffset = MathClamp(RealOffset + (double)animDelta, 0, ExtentHeight - ActualHeight);
                    ScrollToVerticalOffset(RealOffset);
                },
                delta * DeltaMult, 300, 0, new AniEaseOutFluent(AniEasePower.ExtraStrong)));
        }

        private void MyScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            RealOffset = VerticalOffset;
            if (FrmMain != null && (e.VerticalChange != 0 || e.ViewportHeightChange != 0))
            {
                FrmMain.BtnExtraBack.ShowRefresh();
            }
        }

        private void MyScrollViewer_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            FrmMain.BtnExtraBack.ShowRefresh();
        }

        public MyScrollBar ScrollBar { get; private set; }

        private void MyScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollBar = GetTemplateChild("PART_VerticalScrollBar") as MyScrollBar;
        }

        private void MyScrollViewer_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (e.NewFocus != null && e.NewFocus is MySlider)
            {
                e.Handled = true; // #3854，阻止获得焦点时自动滚动
            }
        }


    }
}
