using MyPCL.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using static MyPCL.Utils.AnimationUtil;
using static MyPCL.ViewModules.ViewBase;
using static MyPCL.Utils.BaseUtil;
using static MyPCL.Utils.MathUtil;
using static MyPCL.Utils.LogUtil;

namespace MyPCL.ViewModules
{
    /// <summary>
    /// 提示信息的种类。
    /// </summary>
    public enum HintType
    {
        /// <summary>
        /// 信息，通常是蓝色的“i”。
        /// </summary>
        Info,
        /// <summary>
        /// 已完成，通常是绿色的“√”。
        /// </summary>
        Finish,
        /// <summary>
        /// 错误，通常是红色的“×”。
        /// </summary>
        Critical,
    }

    /// <summary>
    /// 弹出提示
    /// </summary>
    public static class HintModule
    {
        private struct HintMessage
        {
            public string Text;
            public HintType Type;
            public bool Log;
        }

        /// <summary>
        /// 等待弹出的提示列表。以 {String, HintType, Log As Boolean} 形式存储为数组。
        /// </summary>
        private static List<HintMessage> HintWaiting = new List<HintMessage>();

        /// <summary>
        /// 在窗口左下角弹出提示文本。
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="type"></param>
        /// <param name="Log"></param>
        public static void Hint(string Text,HintType type = HintType.Info,bool Log = true)
        {
            if(HintWaiting == null) HintWaiting = new List<HintMessage>();
            HintWaiting.Add(new HintMessage()
            {
                Text = string.IsNullOrEmpty(Text) ? "":Text,
                Type = type,
                Log = Log
            });
        }

        /// <summary>
        /// 循环检测并弹出提示。
        /// </summary>
        public static void HintTick()
        {
            try
            {
                // Tag 存储了：{ 是否可以重用, Uuid }
                if (!HintWaiting.Any()) return;

                while (HintWaiting.Count > 0)
                {
                    var currentHint = HintWaiting[0];
                    //去回车
                    currentHint.Text = currentHint.Text.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");

                    // 超量提示直接忽略
                    if (FrmMain.PanHint.Children.Count >= 20) goto EndHint;

                    // 检查是否有重复提示
                    Border doubleStack = null;
                    foreach (Border stack in FrmMain.PanHint.Children)
                    {
                        if ( (bool)(((object[])stack.Tag)[0]) && ((TextBlock)stack.Child).Text == currentHint.Text)
                        {
                            doubleStack = stack;
                            break;
                        }
                    }

                    // 获取渐变颜色
                    MyColor targetColor0, targetColor1;
                    double percent = 0.3;
                    switch (currentHint.Type)
                    {
                        case HintType.Info:
                            targetColor0 = new MyColor(215, 37, 155, 252);
                            targetColor1 = new MyColor(215, 10, 142, 252);
                            break;
                        case HintType.Finish:
                            targetColor0 = new MyColor(215, 33, 177, 33);
                            targetColor1 = new MyColor(215, 29, 160, 29);
                            break;
                        default: // HintType.Critical
                            targetColor0 = new MyColor(215, 255, 53, 11);
                            targetColor1 = new MyColor(215, 255, 43, 0);
                            break;
                    }

                    if (doubleStack != null)
                    {
                        if (!AniIsRun($"Hint Show {(((object[])doubleStack.Tag)[1])}"))
                        {
                            AniStop($"Hint Hide {((object[])doubleStack.Tag)[1]}");
                            double delay = (800 + MathClamp(currentHint.Text.Length, 5, 23) * 180) * AniSpeed;

                            AniStart(new List<AniData>
                        {
                            AaX(doubleStack, -12 - doubleStack.Margin.Left, 50, 0, new AniEaseOutFluent()),
                            AaX(doubleStack, -8, 50, 50, new AniEaseInFluent()),
                            AaX(doubleStack, 8, 50, 100, new AniEaseOutFluent()),
                            AaX(doubleStack, -8, 50, 150, new AniEaseInFluent()),
                            AaDouble(i =>
                            {
                                percent += (double)i;
                                var gradient = (LinearGradientBrush)doubleStack.Background;
                                gradient.GradientStops[0].Color = targetColor0 * percent + new MyColor(255, 255, 255) * (1 - percent);
                                gradient.GradientStops[1].Color = targetColor1 * percent + new MyColor(255, 255, 255) * (1 - percent);
                            }, 0.7, 250),
                            AaX(doubleStack, -50, 200, (int)delay, new AniEaseInFluent()),
                            AaOpacity(doubleStack, -1, 150, (int) delay),
                            AaCode(() => ((object[])(doubleStack.Tag))[0] = false, (int) delay),
                            AaHeight(doubleStack, -26, 100, 0, new AniEaseOutFluent(), true),
                            AaCode(() => FrmMain.PanHint.Children.Remove(doubleStack), 0, true)
                        }, $"Hint Hide {((object[])doubleStack.Tag)[1]}");
                        }
                    }
                    else
                    {
                        var newHintControl = new Border
                        {
                            Tag = new object[] { true, GetUuid() },
                            Margin = new Thickness(-70, 0, 20, 0),
                            Opacity = 0,
                            Height = 0,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            CornerRadius = new CornerRadius(0, 6, 6, 0)
                        };

                        newHintControl.Background = new LinearGradientBrush(
                            new GradientStopCollection(new List<GradientStop>
                            {
                            new GradientStop(targetColor0 * percent + new MyColor(255, 255, 255) * (1 - percent), 0),
                            new GradientStop(targetColor1 * percent + new MyColor(255, 255, 255) * (1 - percent), 1)
                            }), 90);

                        newHintControl.Child = new TextBlock
                        {
                            TextTrimming = TextTrimming.CharacterEllipsis,
                            FontSize = 13,
                            Text = currentHint.Text,
                            Foreground = new MyColor(255, 255, 255),
                            Margin = new Thickness(33, 5, 8, 5)
                        };
                        // newHintControl.MouseLeftButtonDown += HideAllHint;  // 需根据实际添加事件处理
                        FrmMain.PanHint.Children.Add(newHintControl);

                        var animations = new List<AniData>();
                        if (FrmMain.PanHint.Children.Count > 1)
                        {
                            animations.Add(AaHeight(newHintControl, 26, 150, null, new AniEaseOutFluent()));
                        }
                        else
                        {
                            newHintControl.Height = 26;
                        }

                        animations.AddRange(new List<AniData>
                    {
                        AaX(newHintControl, 30, 400, 0, new AniEaseOutElastic(AniEasePower.Weak)),
                        AaX(newHintControl, 20, 200, 0, new AniEaseOutFluent()),
                        AaOpacity(newHintControl, 1, 100),
                        AaDouble(i =>
                        {
                            percent += (double)i;
                            var gradient = (LinearGradientBrush)newHintControl.Background;
                            gradient.GradientStops[0].Color = targetColor0 * percent + new MyColor(255, 255, 255) * (1 - percent);
                            gradient.GradientStops[1].Color = targetColor1 * percent + new MyColor(255, 255, 255) * (1 - percent);
                        }, 0.7, 250, 100)
                    });

                        AniStart(animations, $"Hint Show {((object[])(newHintControl.Tag))[1]}");

                        double delay = (800 + MathClamp(currentHint.Text.Length, 5, 23) * 180) * AniSpeed;
                        AniStart(new List<AniData>
                    {
                        AaX(newHintControl, -50, 200, (int) delay, new AniEaseInFluent()),
                        AaOpacity(newHintControl, -1, 150, (int) delay),
                        AaCode(() =>((object[])(newHintControl.Tag))[0] = false,(int) delay),
                        AaHeight(newHintControl, -26, 100, 0, new AniEaseOutFluent(), true),
                        AaCode(() => FrmMain.PanHint.Children.Remove(newHintControl), 0, true)
                    }, $"Hint Hide {((object[])newHintControl.Tag)[1]}");
                    }

                EndHint:
                    if (currentHint.Log)
                    {
                        Log("[UI] 弹出提示：" + currentHint.Text);
                    }
                    HintWaiting.RemoveAt(0);
                }
            }
            catch (Exception ex)
            {
                Log(ex, "显示弹出提示失败", LogLevel.Normal);
            }
        }

    }
}
