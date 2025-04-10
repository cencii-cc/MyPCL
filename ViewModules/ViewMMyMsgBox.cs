using MyPCL.Modules.Base;
using MyPCL.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Threading;
using static MyPCL.Utils.ThreadUtil;
using static MyPCL.ViewModules.ViewBase;
using static MyPCL.Utils.LogUtil;

namespace MyPCL.ViewModules
{
    public enum MyMsgBoxType
    {
        Text,
        Select,
        Input,
        Login
    }

    /// <summary>
    /// 存储弹窗信息的转换器。
    /// </summary>
    public class MyMsgBoxConverter
    {
        public MyMsgBoxType Type { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        /// <summary>
        /// 输入模式：文本框的文本。
        /// 选择模式：需要放进去的 List<IMyRadio>。
        /// 登录模式：登录步骤 1 中返回的 JSON。
        /// </summary>
        public object Content { get; set; }
        /// <summary>
        /// 输入模式：输入验证规则。
        /// </summary>
        public System.Collections.ObjectModel.Collection<ValidateBase> ValidateRules { get; set; }
        /// <summary>
        /// 输入模式：提示文本。
        /// </summary>
        public string HintText { get; set; } = "";
        /// <summary>
        /// 有多个按钮时，是否给第一个按钮加高亮。
        /// </summary>
        public bool HighLight { get; set; }
        public string Button1 { get; set; } = "确定";
        public string Button2 { get; set; } = "";
        public string Button3 { get; set; } = "";
        /// <summary>
        /// 点击第一个按钮将执行该方法，不关闭弹窗。
        /// </summary>
        public Action Button1Action { get; set; } = null;
        /// <summary>
        /// 点击第二个按钮将执行该方法，不关闭弹窗。
        /// </summary>
        public Action Button2Action { get; set; } = null;
        /// <summary>
        /// 点击第三个按钮将执行该方法，不关闭弹窗。
        /// </summary>
        public Action Button3Action { get; set; } = null;
        public bool IsWarn { get; set; } = false;
        public bool ForceWait { get; set; } = false;
        public DispatcherFrame WaitFrame { get; set; } = new DispatcherFrame(true);
        /// <summary>
        /// 弹窗是否已经关闭。
        /// </summary>
        public bool IsExited { get; set; } = false;
        /// <summary>
        /// 输入模式：输入的文本。若点击了非第一个按钮，则为 null。
        /// 选择模式：点击的按钮编号，从 1 开始。
        /// 登录模式：字符串数组 {AccessToken, RefreshToken} 或一个 Exception。
        /// </summary>
        public object Result { get; set; }
    }


    public static class ViewMMyMsgBox
    {

        public static List<MyMsgBoxConverter> WaitingMyMsgBox = WaitingMyMsgBox ?? new List<MyMsgBoxConverter>();


        /// <summary>
        /// 显示弹窗，返回点击按钮的编号（从 1 开始）。
        /// </summary>
        /// <param name="caption">弹窗的内容。</param>
        /// <param name="title">弹窗的标题。</param>
        /// <param name="button1">显示的第一个按钮，默认为“确定”。</param>
        /// <param name="button2">显示的第二个按钮，默认为空。</param>
        /// <param name="button3">显示的第三个按钮，默认为空。</param>
        /// <param name="isWarn">是否为警告弹窗，若为 True，弹窗配色和背景会变为红色。</param>
        /// <param name="highLight">是否高亮显示。</param>
        /// <param name="forceWait">是否强制等待。</param>
        /// <param name="button1Action">点击第一个按钮将执行该方法，不关闭弹窗。</param>
        /// <param name="button2Action">点击第二个按钮将执行该方法，不关闭弹窗。</param>
        /// <param name="button3Action">点击第三个按钮将执行该方法，不关闭弹窗。</param>
        public static int MyMsgBox(string caption, string title = "提示",
                                   string button1 = "确定", string button2 = "", string button3 = "",
                                   bool isWarn = false, bool highLight = true, bool forceWait = false,
                                   Action button1Action = null, Action button2Action = null, Action button3Action = null)
        {
            // 将弹窗列入队列
            var converter = new MyMsgBoxConverter
            {
                Type = MyMsgBoxType.Text,
                Button1 = button1,
                Button2 = button2,
                Button3 = button3,
                Text = caption,
                IsWarn = isWarn,
                Title = title,
                HighLight = highLight,
                ForceWait = true,
                Button1Action = button1Action,
                Button2Action = button2Action,
                Button3Action = button3Action
            };
            WaitingMyMsgBox.Add(converter);

            if (RunInUi())
            {
                // 若为 UI 线程，立即执行弹窗刻， 避免快速（连点器）点击时多次弹窗
                MyMsgBoxTick();
            }

            if (button2.Length > 0 || forceWait)
            {
                // 若有多个按钮则开始等待
                if (FrmMain == null || FrmMain.PanMsg == null && RunInUi())
                {
                    // 主窗体尚未加载，用老土的弹窗来替代
                    WaitingMyMsgBox.Remove(converter);
                    if (button2.Length > 0)
                    {
                        MessageBoxResult rawResult;
                        if (button3.Length > 0)
                        {
                            rawResult = MessageBox.Show(caption, title, MessageBoxButton.YesNoCancel,
                                                        isWarn ? MessageBoxImage.Error : MessageBoxImage.Question);
                        }
                        else
                        {
                            rawResult = MessageBox.Show(caption, title, MessageBoxButton.YesNo,
                                                        isWarn ? MessageBoxImage.Error : MessageBoxImage.Question);
                        }

                        switch (rawResult)
                        {
                            case MessageBoxResult.Yes:
                                converter.Result = 1;
                                break;
                            case MessageBoxResult.No:
                                converter.Result = 2;
                                break;
                            case MessageBoxResult.Cancel:
                                converter.Result = 3;
                                break;
                        }
                    }
                    else
                    {
                        MessageBox.Show(caption, title, MessageBoxButton.OK,
                                        isWarn ? MessageBoxImage.Error : MessageBoxImage.Question);
                        converter.Result = 1;
                    }

                    Log(new Exception(), "[Control] 主窗体加载完成前出现意料外的等待弹窗：" + button1 + "," + button2 + "," + button3, LogLevel.Debug);
                }
                else
                {
                    try
                    {
                        FrmMain.DragStop();
                        ComponentDispatcher.PushModal();
                        Dispatcher.PushFrame(converter.WaitFrame);
                    }
                    finally
                    {
                        ComponentDispatcher.PopModal();
                    }
                }

                Log("[Control] 普通弹框返回：" + (converter.Result ?? "null"));
                return (int)converter.Result;
            }
            else
            {
                // 不进行等待，直接返回
                return 1;
            }
        }


        public static void MyMsgBoxTick()
        {
            try
            {
                if (FrmMain == null || FrmMain.PanMsg == null || FrmMain.WindowState == WindowState.Minimized) return;
                if(FrmMain.PanMsg.Children.Count > 0)
                {
                    // 弹窗中
                    FrmMain.PanMsg.Visibility = Visibility.Visible;
                }
                else if (WaitingMyMsgBox.Any())
                {
                    // 没有弹窗，显示一个等待的弹窗
                    FrmMain.PanMsg.Visibility = Visibility.Visible;
                    switch ((WaitingMyMsgBox[0]).Type)
                    {
                        case MyMsgBoxType.Text:
                            break;
                        case MyMsgBoxType.Select:
                            break;
                        case MyMsgBoxType.Input:
                            break;
                        case MyMsgBoxType.Login:
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex, "处理等待中的弹窗失败", LogLevel.Feedback);
            }
        }
    }
}
