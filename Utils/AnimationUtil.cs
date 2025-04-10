using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static MyPCL.Utils.BaseUtil;
using static MyPCL.Utils.MathUtil;
using static MyPCL.Utils.ThreadUtil;
using static MyPCL.Utils.StringUtil;
using static MyPCL.Utils.LogUtil;
using static MyPCL.Modules.ModBase;
using static MyPCL.Utils.RandomUtil;
using static MyPCL.ViewModules.ViewMUI;
using System.Windows;
using MyPCL.ViewModules;
using System.Windows.Controls;
using System.Windows.Media;


namespace MyPCL.Utils
{
    /// <summary>
    /// 动画引擎模块 <br/>
    /// 使用 Ani 作为方法或属性的开头，使用 Aa 作为单个动画对象的开头（便于自动补全）
    /// </summary>
    public static class AnimationUtil
    {
        /// <summary>
        /// 动画速度。最大为 200。
        /// </summary>
        public static double AniSpeed = 1;

        /// <summary>
        /// 动画组列表。
        /// </summary>
        public static Dictionary<string, AniGroupEntry> AniGroups = new Dictionary<string, AniGroupEntry>();

        /// <summary>
        /// 上一次记刻的时间。
        /// </summary>
        private static long AniLastTick;

        /// <summary>
        /// 动画模块是否正在运行。。
        /// </summary>
        public static bool AniRunning = false;

        /// <summary>
        /// 动画控制是否启用。
        /// </summary>
        private static int _AniControlEnabled = 0;
        private static readonly object AniControlEnabledLock = new object();

        private static int AniCount = 0;
        private static int AniFPSCounter = 0;
        private static long AniFPSTimer = 0;

        /// <summary>
        /// 当前的动画 FPS。
        /// </summary>
        private static int AniFPS = 0;

        /// <summary>
        /// 控件动画执行是否开启。先 +1，再 -1。
        /// </summary>
        public static int AniControlEnabled
        {
            get
            {
                return _AniControlEnabled;
            }
            set
            {
                lock (AniControlEnabledLock)
                {
                    _AniControlEnabled = value;
                }
            }
        }

        /// <summary>
        /// 开始一个动画组。
        /// </summary>
        /// <param name="aniGroup">由 Aa 开头的函数初始化的 AniData 对象集合。</param>
        /// <param name="name">动画组的名称。如果重复会直接停止同名动画组。</param>
        /// <param name="refreshTime"></param>
        public static void AniStart(IList aniGroup,string name = "",bool refreshTime = false)
        {
            // 避免处理动画时已经造成了极大的延迟，导致动画突然结束
            if (refreshTime) AniLastTick = GetTimeTick();
            // 添加到正在执行的动画组
            var NewEntry = new AniGroupEntry()
            {
                Data = GetFullList<AniData>(aniGroup),
                StartTick = GetTimeTick(),
            };
            if (string.IsNullOrEmpty(name))
                name = NewEntry.Uuid.ToString();
            else
                AniStop(name);
            AniGroups.Add(name, NewEntry);
        }

        /// <summary>
        /// 开始一个动画组。
        /// </summary>
        /// <param name="aniGroup"></param>
        /// <param name="name"></param>
        /// <param name="refreshTime"></param>
        public static void AniStart(AniData aniGroup,string name = "",bool refreshTime = false)
        {
            AniStart(new List<AniData> { aniGroup }, name, refreshTime);
        }

        /// <summary>
        /// 直接停止一个动画组。
        /// </summary>
        /// <param name="name"></param>
        public static void AniStop(string name)
        {
            AniGroups.Remove(name);
        }

        /// <summary>
        /// 直接停止一个动画组。
        /// </summary>
        /// <param name="name">需要停止的动画组的名称。</param>
        /// <returns></returns>
        public static bool AniIsRun(string name)
        {
            return AniGroups.ContainsKey(name);
        }

        /// <summary>
        /// 开始动画执行。
        /// </summary>
        public static void AniStart()
        {
            // 初始化计时器
            AniFPSTimer = AniLastTick = GetTimeTick();
            RunInNewThread(() =>
            {
                try
                {
                    Log("[Animation] 动画线程开始");
                    while (true)
                    {
                        // 两帧之间的间隔时间
                        long DeltaTime = (long)MathClamp(GetTimeTick() - AniLastTick, 0, 10000);
                        if (DeltaTime < 3) goto Sleeper;
                        // 记录 FPS
                        if (ModeDebug)
                        {
                            if(MathClamp(AniLastTick - AniFPSTimer, 0, 100000) >= 500)
                            {
                                AniFPS = AniFPSCounter;
                                AniFPSCounter = 0;
                                AniFPSTimer = AniLastTick;
                            }
                            AniFPSCounter += 2;
                        }
                        // 执行动画
                        RunInUiWait(() => {
                            AniCount = 0;
                            AniTimer((int)(DeltaTime * AniSpeed));
                            if(RandomInteger(0,64 * (ModeDebug ? 5:30)) == 0 && ((AniFPS < 62 && AniFPS >0) || AniCount > 4))
                            {
                                // 下载动画
                                // Log("[Report] FPS " & AniFPS & ", 动画 " & AniCount & ", 下载中 " & NetManager.FileRemain & "（" & GetString(NetManager.Speed) & "/s）")
                            }
                        });



                    Sleeper:
                        // 控制 FPS
                        Thread.Sleep(1);
                    }
                }
                catch(Exception ex)
                {

                }
            }, "Animation", ThreadPriority.AboveNormal);
        }

        /// <summary>
        /// 动画定时器事件。
        /// </summary>
        /// <param name="deltaTick"></param>
        public static void AniTimer(int deltaTick)
        {
            try
            {
                if (deltaTick / AniSpeed > 200)
                {
                    Log(new Exception(), $"[Animation] 两个动画帧间隔 {deltaTick} ms", LogLevel.Developer);
                }

                int i = -1;
                while (i + 1 < AniGroups.Count)
                {
                    i++;
                    AniGroupEntry entry = AniGroups.Values.ElementAt(i);
                    if (entry.StartTick > AniLastTick)
                    {
                        continue; // 跳过本刻之后开始的动画
                    }

                    bool canRemoveAfter = true; // 是否应该去除“之后”标记
                    int ii = 0;

                    while (ii < entry.Data.Count)
                    {
                        AniData anim = entry.Data[ii];
                        if (!anim.IsAfter) // 之前
                        {
                            canRemoveAfter = false; // 取消“之后”标记 
                            anim.TimeFinished += deltaTick;
                            if (anim.TimeFinished > 0)
                            {
                                anim = AniRun(anim);
                                AniCount++;
                            }

                            if (anim.TimeFinished >= anim.TimeTotal)
                            {
                                if (anim.TypeMain == AniType.Color && ((dynamic)anim.Obj)[2] != "")
                                {
                                    ((dynamic)anim.Obj)[0].SetResourceReference(((dynamic)anim.Obj)[1], ((dynamic)anim.Obj)[2]);
                                }

                                entry.Data.RemoveAt(ii);
                                continue;
                            }

                            entry.Data[ii] = anim;
                        }
                        else // 之后
                        {
                            if (canRemoveAfter)
                            {
                                canRemoveAfter = false;
                                anim.IsAfter = false;
                                entry.Data[ii] = anim;
                                continue;
                            }
                            else
                            {
                                break; // 不能去除该“之后”标记，结束该动画组
                            }
                        }

                        ii++;
                    }

                    if (!entry.Data.Any())
                    {
                        // 为了避免新添加的动画影响顺序，不能 RemoveAt(i)
                        // 为了允许动画在执行中添加同名动画组，不能按名字移除
                        var currentEntry = AniGroups.ElementAtOrDefault(i);
                        if (currentEntry.Value != null && currentEntry.Value.Uuid == entry.Uuid)
                        {
                            AniGroups.Remove(currentEntry.Key);
                        }
                        i--;
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex, "动画刻执行失败", LogLevel.Hint);
            }
        }

        /// <summary>
        /// 执行一个动画。
        /// </summary>
        /// <param name="ani"></param>
        /// <returns></returns>
        public static AniData AniRun(AniData ani)
        {
            try
            {
                switch (ani.TypeMain)
                {
                    case AniType.Number:
                        double delta = MathPercent(0, (double)ani.Value, ani.Ease.GetDelta(ani.TimeFinished / (double)ani.TimeTotal, ani.TimePercent));
                        if (delta != 0)
                        {
                            switch (ani.TypeSub)
                            {
                                case AniTypeSub.X:
                                    DeltaLeft((FrameworkElement)ani.Obj, delta);
                                    break;
                                case AniTypeSub.Y:
                                    DeltaTop((FrameworkElement)ani.Obj, delta);
                                    break;
                                case AniTypeSub.Opacity:
                                    ((FrameworkElement)ani.Obj).Opacity = MathClamp(((FrameworkElement)ani.Obj).Opacity + delta, 0, 1);
                                    break;
                                case AniTypeSub.Width:
                                    FrameworkElement widthObj = ani.Obj as FrameworkElement;
                                    widthObj.Width = Math.Max(double.IsNaN(widthObj.Width) ? widthObj.ActualWidth : widthObj.Width + delta, 0);
                                    break;
                                case AniTypeSub.Height:
                                    FrameworkElement heightObj = ani.Obj as FrameworkElement;
                                    heightObj.Height = Math.Max(double.IsNaN(heightObj.Height) ? heightObj.ActualHeight : heightObj.Height + delta, 0);
                                    break;
                                case AniTypeSub.Value:
                                    ((dynamic)ani.Obj).Value += delta;
                                    break;
                                case AniTypeSub.Radius:
                                    ((dynamic)ani.Obj).Radius += delta;
                                    break;
                                case AniTypeSub.StrokeThickness:
                                    ((dynamic)ani.Obj).StrokeThickness = Math.Max(((dynamic)ani.Obj).StrokeThickness + delta, 0);
                                    break;
                                case AniTypeSub.BorderThickness:
                                    ((dynamic)ani.Obj).BorderThickness = new Thickness(((Thickness)((dynamic)ani.Obj).BorderThickness).Bottom + delta);
                                    break;
                                case AniTypeSub.TranslateX:
                                    if (((dynamic)ani.Obj).RenderTransform == null || !(((dynamic)ani.Obj).RenderTransform is TranslateTransform))
                                    {
                                        ((dynamic)ani.Obj).RenderTransform = new TranslateTransform(0, 0);
                                    }
                                    ((TranslateTransform)((dynamic)ani.Obj).RenderTransform).X += delta;
                                    break;
                                case AniTypeSub.TranslateY:
                                    if (((dynamic)ani.Obj).RenderTransform == null || !(((dynamic)ani.Obj).RenderTransform is TranslateTransform))
                                    {
                                        ((dynamic)ani.Obj).RenderTransform = new TranslateTransform(0, 0);
                                    }
                                    ((TranslateTransform)((dynamic)ani.Obj).RenderTransform).Y += delta;
                                    break;
                                case AniTypeSub.Double:
                                    ((dynamic)ani.Obj)[0].SetValue((DependencyProperty)((dynamic)ani.Obj)[1], (double)((dynamic)ani.Obj)[0].GetValue((DependencyProperty)((dynamic)ani.Obj)[1]) + delta);
                                    break;
                                case AniTypeSub.DoubleParam:
                                    ((ParameterizedThreadStart)ani.Obj)(delta);
                                    break;
                                case AniTypeSub.GridLengthWidth:
                                    ((dynamic)ani.Obj).Width = new GridLength(Math.Max(((dynamic)ani.Obj).Width.Value + delta, 0), GridUnitType.Star);
                                    break;
                            }
                        }
                        break;
                    case AniType.Color:
                        MyColor colorDelta = MathPercent(new MyColor(0, 0, 0, 0), (MyColor)ani.Value, ani.Ease.GetDelta(ani.TimeFinished / (double)ani.TimeTotal, ani.TimePercent)) + (MyColor)ani.ValueLast;
                        FrameworkElement colorObj = (FrameworkElement)((object[])ani.Obj)[0];
                        DependencyProperty colorProp = (DependencyProperty)((object[])ani.Obj)[1];
                        MyColor newColor = new MyColor(colorObj.GetValue(colorProp)) + colorDelta;
                        colorObj.SetValue(colorProp, (colorProp.PropertyType.Name == "Color" ? Convert.ChangeType(newColor,typeof(Color)) : Convert.ChangeType(newColor,typeof(SolidColorBrush))));
                        ani.ValueLast = newColor - new MyColor(colorObj.GetValue(colorProp));
                        break;
                    case AniType.Scale:
                        FrameworkElement scaleObj = ani.Obj as FrameworkElement;
                        double scaleDelta = ani.Ease.GetDelta(ani.TimeFinished / (double)ani.TimeTotal, ani.TimePercent);
                        scaleObj.Margin = new Thickness(
                            scaleObj.Margin.Left + MathPercent(0, ((dynamic)ani.Obj).Left, scaleDelta),
                            scaleObj.Margin.Top + MathPercent(0, ((dynamic)ani.Obj).Top, scaleDelta),
                            scaleObj.Margin.Right + MathPercent(0, ((dynamic)ani.Obj).Left, scaleDelta),
                            scaleObj.Margin.Bottom + MathPercent(0, ((dynamic)ani.Obj).Top, scaleDelta));
                        scaleObj.Width = Math.Max(scaleObj.Width + MathPercent(0, ((dynamic)ani.Obj).Width, scaleDelta), 0);
                        scaleObj.Height = Math.Max(scaleObj.Height + MathPercent(0, ((dynamic)ani.Obj).Height, scaleDelta), 0);
                        break;
                    case AniType.TextAppear:
                        int textCount = (bool)((dynamic)ani.Obj)[1] ? ((dynamic)ani.Obj)[0].ToString().Length : 0 +
                            (int)Math.Round(((dynamic)ani.Obj)[0].ToString().Length * ((bool)((dynamic)ani.Obj)[1] ? -1 : 1) * ani.Ease.GetDelta(ani.TimeFinished / (double)ani.TimeTotal, 0));
                        string newText = ((dynamic)ani.Obj)[0].ToString().Substring(0, textCount);
                        // 添加乱码
                        if (textCount < ((dynamic)ani.Obj)[0].ToString().Length)
                        {
                            string nextText = ((dynamic)ani.Obj)[0].ToString().Substring(textCount, 1);
                            if ((int)nextText[0] >= 128)
                            {
                                byte[] bytes = { (byte)RandomInteger(16 + 160, 87 + 160), (byte)RandomInteger(1 + 160, 89 + 160) };
                                newText += Encoding.GetEncoding("GB18030").GetString(bytes);
                            }
                            else
                            {
                                char[] charArray = "0123456789./*-+\\[]{};':/?,!@#$%^&*()_+-=qwwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM".ToCharArray();
                                newText += RandomOne(charArray);
                            }
                        }
                        // 设置文本
                        if (ani.Obj is TextBlock textBlock)
                        {
                            textBlock.Text = newText;
                        }
                        else
                        {
                            ((dynamic)ani.Obj).Context = newText;
                        }
                        break;
                    case AniType.Code:
                        ((ThreadStart)ani.Value)();
                        break;
                    case AniType.ScaleTransform:
                        FrameworkElement scaleTransformObj = ani.Obj as FrameworkElement;
                        if (!(scaleTransformObj.RenderTransform is ScaleTransform))
                        {
                            scaleTransformObj.RenderTransformOrigin = new Point(0.5, 0.5);
                            scaleTransformObj.RenderTransform = new ScaleTransform(1, 1);
                        }
                        double scaleTransformDelta = MathPercent(0, (double)ani.Obj, ani.Ease.GetDelta(ani.TimeFinished / (double)ani.TimeTotal, ani.TimePercent));
                        ((ScaleTransform)scaleTransformObj.RenderTransform).ScaleX = Math.Max(((ScaleTransform)scaleTransformObj.RenderTransform).ScaleX + scaleTransformDelta, 0);
                        ((ScaleTransform)scaleTransformObj.RenderTransform).ScaleY = Math.Max(((ScaleTransform)scaleTransformObj.RenderTransform).ScaleY + scaleTransformDelta, 0);
                        break;
                    case AniType.RotateTransform:
                        FrameworkElement rotateTransformObj = ani.Obj as FrameworkElement;
                        if (!(rotateTransformObj.RenderTransform is RotateTransform))
                        {
                            rotateTransformObj.RenderTransformOrigin = new Point(0.5, 0.5);
                            rotateTransformObj.RenderTransform = new RotateTransform(0);
                        }
                        double rotateTransformDelta = MathPercent(0, (double)ani.Value, ani.Ease.GetDelta(ani.TimeFinished / (double)ani.TimeTotal, ani.TimePercent));
                        ((RotateTransform)rotateTransformObj.RenderTransform).Angle = ((RotateTransform)rotateTransformObj.RenderTransform).Angle + rotateTransformDelta;
                        break;
                }
                ani.TimePercent = (double)ani.TimeFinished / ani.TimeTotal; // 修改执行百分比
            }
            catch (Exception ex)
            {
                Log(ex, "执行动画失败：" + ani.ToString(), LogLevel.Hint);
            }
            return ani;
        }

        #region 动画
        /// <summary>
        /// 移动X轴的动画。
        /// </summary>
        /// <param name="Obj">动画的对象。</param>
        /// <param name="Value">进行移动的值。</param>
        /// <param name="Time">动画长度（毫秒）。</param>
        /// <param name="Delay">动画延迟执行的时间（毫秒）。</param>
        /// <param name="Ease">插值器类型。</param>
        /// <param name="After">是否等到以前的动画完成后才继续本动画。</param>
        /// <returns></returns>
        public static AniData AaX(object Obj, double Value, int Time = 400, int Delay = 0, AniEase Ease = null, bool After = false)
        {
            return new AniData
            {
                TypeMain = AniType.Number,
                TypeSub = AniTypeSub.X,
                TimeTotal = Time,
                Ease = Ease ?? new AniEaseLinear(),
                Obj = Obj,
                Value = Value,
                IsAfter = After,
                TimeFinished = -Delay
            };
        }

        /// <summary>
        /// 移动Y轴的动画。
        /// </summary>
        /// <param name="Obj">动画的对象。</param>
        /// <param name="Value">进行移动的值。</param>
        /// <param name="Time">动画长度（毫秒）。</param>
        /// <param name="Delay">动画延迟执行的时间（毫秒）。</param>
        /// <param name="Ease">插值器类型。</param>
        /// <param name="After">是否等到以前的动画完成后才继续本动画。</param>
        /// <returns></returns>
        public static AniData AaY(object Obj, double Value, int Time = 400, int Delay = 0, AniEase Ease = null, bool After = false)
        {
            return new AniData
            {
                TypeMain = AniType.Number,
                TypeSub = AniTypeSub.Y,
                TimeTotal = Time,
                Ease = Ease ?? new AniEaseLinear(),
                Obj = Obj,
                Value = Value,
                IsAfter = After,
                TimeFinished = -Delay
            };
        }

        /// <summary>
        /// 改变宽度的动画。
        /// </summary>
        /// <param name="Obj">动画的对象。</param>
        /// <param name="Value">宽度改变的值。</param>
        /// <param name="Time">动画长度（毫秒）。</param>
        /// <param name="Delay">动画延迟执行的时间（毫秒）。</param>
        /// <param name="Ease">插值器类型。</param>
        /// <param name="After">是否等到以前的动画完成后才继续本动画。</param>
        /// <returns></returns>
        public static AniData AaWidth(object Obj, double Value, int Time = 400, int Delay = 0, AniEase Ease = null, bool After = false)
        {
            return new AniData
            {
                TypeMain = AniType.Number,
                TypeSub = AniTypeSub.Width,
                TimeTotal = Time,
                Ease = Ease ?? new AniEaseLinear(),
                Obj = Obj,
                Value = Value,
                IsAfter = After,
                TimeFinished = -Delay
            };
        }

        /// <summary>
        /// 改变高度的动画。
        /// </summary>
        /// <param name="obj">动画的对象。</param>
        /// <param name="value">高度改变的值。</param>
        /// <param name="time">动画长度（毫秒）。</param>
        /// <param name="delay">动画延迟执行的时间（毫秒）。</param>
        /// <param name="ease">插值器类型。</param>
        /// <param name="after">是否等到以前的动画完成后才继续本动画。</param>
        /// <returns>AniData 对象</returns>
        public static AniData AaHeight(object obj, double value, int time = 400, int delay = 0, AniEase ease = null, bool after = false)
        {
            return new AniData
            {
                TypeMain = AniType.Number,
                TypeSub = AniTypeSub.Height,
                TimeTotal = time,
                Ease = ease ?? new AniEaseLinear(),
                Obj = obj,
                Value = value,
                IsAfter = after,
                TimeFinished = -delay
            };
        }

        /// <summary>
        /// 改变透明度的动画。
        /// </summary>
        /// <param name="obj">动画的对象。</param>
        /// <param name="value">透明度改变的值。</param>
        /// <param name="time">动画长度（毫秒）。</param>
        /// <param name="delay">动画延迟执行的时间（毫秒）。</param>
        /// <param name="ease">插值器类型。</param>
        /// <param name="after">是否等到以前的动画完成后才继续本动画。</param>
        /// <returns>AniData 对象</returns>
        public static AniData AaOpacity(object obj, double value, int time = 400, int delay = 0, AniEase ease = null, bool after = false)
        {
            return new AniData
            {
                TypeMain = AniType.Number,
                TypeSub = AniTypeSub.Opacity,
                TimeTotal = time,
                Ease = ease ?? new AniEaseLinear(),
                Obj = obj,
                Value = value,
                IsAfter = after,
                TimeFinished = -delay
            };
        }

        /// <summary>
        /// 改变对象的Value属性的动画。
        /// </summary>
        /// <param name="Obj">动画的对象。</param>
        /// <param name="Value">Value属性改变的值。</param>
        /// <param name="Time">动画长度（毫秒）。</param>
        /// <param name="Delay">动画延迟执行的时间（毫秒）。</param>
        /// <param name="Ease">插值器类型。</param>
        /// <param name="After">是否等到以前的动画完成后才继续本动画。</param>
        /// <returns></returns>
        public static AniData AaValue(object Obj, double Value, int Time = 400, int Delay = 0, AniEase Ease = null, bool After = false)
        {
            return new AniData
            {
                TypeMain = AniType.Number,
                TypeSub = AniTypeSub.Value,
                TimeTotal = Time,
                Ease = Ease ?? new AniEaseLinear(),
                Obj = Obj,
                Value = Value,
                IsAfter = After,
                TimeFinished = -Delay
            };
        }

        /// <summary>
        /// 改变对象的Radius属性的动画。
        /// </summary>
        /// <param name="Obj">动画的对象。</param>
        /// <param name="Value">Radius属性改变的值。</param>
        /// <param name="Time">动画长度（毫秒）。</param>
        /// <param name="Delay">动画延迟执行的时间（毫秒）。</param>
        /// <param name="Ease">插值器类型。</param>
        /// <param name="After">是否等到以前的动画完成后才继续本动画。</param>
        /// <returns></returns>
        public static AniData AaRadius(object Obj, double Value, int Time = 400, int Delay = 0, AniEase Ease = null, bool After = false)
        {
            return new AniData
            {
                TypeMain = AniType.Number,
                TypeSub = AniTypeSub.Radius,
                TimeTotal = Time,
                Ease = Ease ?? new AniEaseLinear(),
                Obj = Obj,
                Value = Value,
                IsAfter = After,
                TimeFinished = -Delay
            };
        }

        /// <summary>
        /// 改变对象的BorderThickness属性的动画。
        /// </summary>
        /// <param name="Obj">动画的对象。</param>
        /// <param name="Value">BorderThickness属性改变的值。</param>
        /// <param name="Time">动画长度（毫秒）。</param>
        /// <param name="Delay">动画延迟执行的时间（毫秒）。</param>
        /// <param name="Ease">插值器类型。</param>
        /// <param name="After">是否等到以前的动画完成后才继续本动画。</param>
        /// <returns></returns>
        public static AniData AaBorderThickness(object Obj, double Value, int Time = 400, int Delay = 0, AniEase Ease = null, bool After = false)
        {
            return new AniData
            {
                TypeMain = AniType.Number,
                TypeSub = AniTypeSub.BorderThickness,
                TimeTotal = Time,
                Ease = Ease ?? new AniEaseLinear(),
                Obj = Obj,
                Value = Value,
                IsAfter = After,
                TimeFinished = -Delay
            };
        }

        /// <summary>
        /// 改变对象的StrokeThickness属性的动画。
        /// </summary>
        /// <param name="Obj">动画的对象。</param>
        /// <param name="Value">StrokeThickness属性改变的值。</param>
        /// <param name="Time">动画长度（毫秒）。</param>
        /// <param name="Delay">动画延迟执行的时间（毫秒）。</param>
        /// <param name="Ease">插值器类型。</param>
        /// <param name="After">是否等到以前的动画完成后才继续本动画。</param>
        /// <returns></returns>
        public static AniData AaStrokeThickness(object Obj, double Value, int Time = 400, int Delay = 0, AniEase Ease = null, bool After = false)
        {
            return new AniData
            {
                TypeMain = AniType.Number,
                TypeSub = AniTypeSub.StrokeThickness,
                TimeTotal = Time,
                Ease = Ease ?? new AniEaseLinear(),
                Obj = Obj,
                Value = Value,
                IsAfter = After,
                TimeFinished = -Delay
            };
        }

        /// <summary>
        /// 改变 Width 的 GridLength 属性的动画。必须为 Star。
        /// </summary>
        /// <param name="Obj">动画的对象。</param>
        /// <param name="Value">GridLength.Value 改变的值。</param>
        /// <param name="Time">动画长度（毫秒）。</param>
        /// <param name="Delay">动画延迟执行的时间（毫秒）。</param>
        /// <param name="Ease">插值器类型。</param>
        /// <param name="After">是否等到以前的动画完成后才继续本动画。</param>
        /// <returns></returns>
        public static AniData AaGridLengthWidth(object Obj, double Value, int Time = 400, int Delay = 0, AniEase Ease = null, bool After = false)
        {
            return new AniData
            {
                TypeMain = AniType.Number,
                TypeSub = AniTypeSub.GridLengthWidth,
                TimeTotal = Time,
                Ease = Ease ?? new AniEaseLinear(),
                Obj = Obj,
                Value = Value,
                IsAfter = After,
                TimeFinished = -Delay
            };
        }

        // DoubleAnimation（Obj, Prop, [Res]）

        /// <summary>
        /// 改变数字属性的动画。
        /// </summary>
        /// <param name="Obj">动画的对象。</param>
        /// <param name="Prop">动画的依赖属性。</param>
        /// <param name="Value">改变的值。</param>
        /// <param name="Time">动画长度（毫秒）。</param>
        /// <param name="Delay">动画延迟执行的时间（毫秒）。</param>
        /// <param name="Ease">插值器类型。</param>
        /// <param name="After">是否等到以前的动画完成后才继续本动画。</param>
        /// <returns></returns>
        public static AniData AaDouble(object Obj, DependencyProperty Prop, double Value, int Time = 400, int Delay = 0, AniEase Ease = null, bool After = false)
        {
            return new AniData
            {
                TypeMain = AniType.Number,
                TypeSub = AniTypeSub.Double,
                TimeTotal = Time,
                Ease = Ease ?? new AniEaseLinear(),
                Obj = new object[] { Obj, Prop, "" },
                Value = Value,
                IsAfter = After,
                TimeFinished = -Delay
            };
        }

        /// <summary>
        /// 获取数字动画值。
        /// </summary>
        /// <param name="Lambda">参数化线程启动委托。</param>
        /// <param name="Value">改变的值。</param>
        /// <param name="Time">动画长度（毫秒）。</param>
        /// <param name="Delay">动画延迟执行的时间（毫秒）。</param>
        /// <param name="Ease">插值器类型。</param>
        /// <param name="After">是否等到以前的动画完成后才继续本动画。</param>
        /// <returns></returns>
        public static AniData AaDouble(ParameterizedThreadStart Lambda, double Value, int Time = 400, int Delay = 0, AniEase Ease = null, bool After = false)
        {
            return new AniData
            {
                TypeMain = AniType.Number,
                TypeSub = AniTypeSub.DoubleParam,
                TimeTotal = Time,
                Ease = Ease ?? new AniEaseLinear(),
                Obj = Lambda,
                Value = Value,
                IsAfter = After,
                TimeFinished = -Delay
            };
        }


        // ColorAnimation（Obj, Prop, [Res]）

        /// <summary>
        /// 改变颜色属性的动画。
        /// </summary>
        /// <param name="Obj">动画的对象。</param>
        /// <param name="Prop">动画的依赖属性。</param>
        /// <param name="Value">颜色改变的值。以RGB加减法进行计算。不用担心超额。</param>
        /// <param name="Time">动画长度（毫秒）。</param>
        /// <param name="Delay">动画延迟执行的时间（毫秒）。</param>
        /// <param name="Ease">插值器类型。</param>
        /// <param name="After">是否等到以前的动画完成后才继续本动画。</param>
        /// <returns></returns>
        public static AniData AaColor(FrameworkElement Obj, DependencyProperty Prop, MyColor Value, int Time = 400, int Delay = 0, AniEase Ease = null, bool After = false)
        {
            return new AniData
            {
                TypeMain = AniType.Color,
                TimeTotal = Time,
                Ease = Ease ?? new AniEaseLinear(),
                Obj = new object[] { Obj, Prop, "" },
                Value = Value,
                IsAfter = After,
                TimeFinished = -Delay,
                ValueLast = new MyColor(0, 0, 0, 0)
            };
        }

        /// <summary>
        /// 改变颜色属性为一个资源的动画。
        /// </summary>
        /// <param name="Obj">动画的对象。</param>
        /// <param name="Prop">动画的依赖属性。</param>
        /// <param name="Res">要将颜色改变为该资源值。</param>
        /// <param name="Time">动画长度（毫秒）。</param>
        /// <param name="Delay">动画延迟执行的时间（毫秒）。</param>
        /// <param name="Ease">插值器类型。</param>
        /// <param name="After">是否等到以前的动画完成后才继续本动画。</param>
        /// <returns></returns>
        public static AniData AaColor(FrameworkElement Obj, DependencyProperty Prop, string Res, int Time = 400, int Delay = 0, AniEase Ease = null, bool After = false)
        {
            var targetColor = new MyColor(Application.Current.FindResource(Res));
            var currentColor = new MyColor(Obj.GetValue(Prop));
            return new AniData
            {
                TypeMain = AniType.Color,
                TimeTotal = Time,
                Ease = Ease ?? new AniEaseLinear(),
                Obj = new object[] { Obj, Prop, Res },
                Value = targetColor - currentColor,
                IsAfter = After,
                TimeFinished = -Delay,
                ValueLast = new MyColor(0, 0, 0, 0)
            };
        }

        // Scale

        /// <summary>
        /// 缩放控件的动画。
        /// </summary>
        /// <param name="Obj">动画的对象。</param>
        /// <param name="Value">大小改变的百分比（如-0.6）或值。</param>
        /// <param name="Time">动画长度（毫秒）。</param>
        /// <param name="Delay">动画延迟执行的时间（毫秒）。</param>
        /// <param name="Ease">插值器类型。</param>
        /// <param name="After">是否等到以前的动画完成后才继续本动画。</param>
        /// <param name="Absolute">大小改变是否为绝对值。若为 True 则为绝对像素，若为 False 则为相对百分比。</param>
        /// <returns></returns>
        public static AniData AaScale(object Obj, double Value, int Time = 400, int Delay = 0, AniEase Ease = null, bool After = false, bool Absolute = false)
        {
            MyRect ChangeRect;
            if (Absolute)
            {
                ChangeRect = new MyRect(-0.5 * Value, -0.5 * Value, Value, Value);
            }
            else
            {
                var fe = Obj as FrameworkElement;
                if (fe != null)
                {
                    ChangeRect = new MyRect(-0.5 * fe.ActualWidth * Value, -0.5 * fe.ActualHeight * Value, fe.ActualWidth * Value, fe.ActualHeight * Value);
                }
                else
                {
                    throw new ArgumentException("The object must be a FrameworkElement when using relative scaling.");
                }
            }
            return new AniData
            {
                TypeMain = AniType.Scale,
                TimeTotal = Time,
                Ease = Ease ?? new AniEaseLinear(),
                Obj = Obj,
                Value = ChangeRect,
                IsAfter = After,
                TimeFinished = -Delay
            };
        }

        // TextAppear

        /// <summary>
        /// 让一段文字一个个字出现或消失的动画。
        /// </summary>
        /// <param name="Obj">动画的对象。必须是Label或TextBlock。</param>
        /// <param name="Hide">是否为一个个字隐藏。默认为False（一个个字出现）。这些字必须已经存在了。</param>
        /// <param name="TimePerText">是否采用根据文本长度决定时间的方式。</param>
        /// <param name="Time">动画长度（毫秒）。若TimePerText为True，这代表每个字所占据的时间。</param>
        /// <param name="Delay">动画延迟执行的时间（毫秒）。</param>
        /// <param name="After">是否等到以前的动画完成后才继续本动画。</param>
        /// <returns></returns>
        public static AniData AaTextAppear(object Obj, bool Hide = false, bool TimePerText = true, int Time = 70, int Delay = 0, AniEase Ease = null, bool After = false)
        {
            string text = null;
            if (Obj is TextBlock textBlock)
            {
                text = textBlock.Text;
            }
            else if (Obj is Label label)
            {
                text = label.Content.ToString();
            }
            else
            {
                throw new ArgumentException("The object must be a Label or a TextBlock.");
            }

            int totalTime = TimePerText ? Time * text.Length : Time;
            return new AniData
            {
                TypeMain = AniType.TextAppear,
                Ease = Ease ?? new AniEaseLinear(),
                TimeTotal = totalTime,
                Obj = Obj,
                Value = new object[] { text, Hide },
                IsAfter = After,
                TimeFinished = -Delay
            };
        }

        // Code

        /// <summary>
        /// 执行代码。
        /// </summary>
        /// <param name="code">一个 ThreadStart。这将会在执行时在主线程调用。</param>
        /// <param name="delay">代码延迟执行的时间（毫秒）。</param>
        /// <param name="after">是否等到以前的动画完成后才执行。</param>
        /// <returns>AniData 对象</returns>
        public static AniData AaCode(ThreadStart code, int delay = 0, bool after = false)
        {
            return new AniData
            {
                TypeMain = AniType.Code,
                TimeTotal = 1,
                Value = code,
                IsAfter = after,
                TimeFinished = -delay
            };
        }

        // ScaleTransform

        /// <summary>
        /// 按照 WPF 方式缩放控件的动画。
        /// </summary>
        /// <param name="obj">动画的对象。它必须已经拥有了单一的 ScaleTransform 值。</param>
        /// <param name="value">大小改变的百分比（如-0.6）。</param>
        /// <param name="time">动画长度（毫秒）。</param>
        /// <param name="delay">动画延迟执行的时间（毫秒）。</param>
        /// <param name="ease">插值器类型。</param>
        /// <param name="after">是否等到以前的动画完成后才继续本动画。</param>
        /// <returns></returns>
        public static AniData AaScaleTransform(object obj, double value, int time = 400, int delay = 0, AniEase ease = null, bool after = false)
        {
            return new AniData()
            {
                TypeMain = AniType.ScaleTransform,
                TimeTotal = time,
                Ease = ease ?? new AniEaseLinear(),
                Obj = obj,
                Value = value,
                IsAfter = after,
                TimeFinished = -delay
            };
        }

        // RotateTransform

        /// <summary>
        /// 按照 WPF 方式旋转控件的动画。
        /// </summary>
        /// <param name="Obj">动画的对象。它必须已经拥有了单一的 ScaleTransform 值。</param>
        /// <param name="Value">大小改变的百分比（如-0.6）。</param>
        /// <param name="Time">动画长度（毫秒）。</param>
        /// <param name="Delay">动画延迟执行的时间（毫秒）。</param>
        /// <param name="Ease">插值器类型。</param>
        /// <param name="After">是否等到以前的动画完成后才继续本动画。</param>
        /// <returns></returns>
        public static AniData AaRotateTransform(object Obj, double Value, int Time = 400, int Delay = 0, AniEase Ease = null, bool After = false)
        {
            return new AniData
            {
                TypeMain = AniType.RotateTransform,
                TimeTotal = Time,
                Ease = Ease ?? new AniEaseLinear(),
                Obj = Obj,
                Value = Value,
                IsAfter = After,
                TimeFinished = -Delay
            };
        }

        // TranslateTransform

        /// <summary>
        /// 利用 TranslateTransform 移动 X 轴的动画，这不会造成布局更新。
        /// </summary>
        /// <param name="Obj">动画的对象。</param>
        /// <param name="Value">进行移动的值。</param>
        /// <param name="Time">动画长度（毫秒）。</param>
        /// <param name="Delay">动画延迟执行的时间（毫秒）。</param>
        /// <param name="Ease">插值器类型。</param>
        /// <param name="After">是否等到以前的动画完成后才继续本动画。</param>
        public static AniData AaTranslateX(object Obj, double Value, int Time = 400, int Delay = 0, AniEase Ease = null, bool After = false)
        {
            return new AniData
            {
                TypeMain = AniType.Number,
                TypeSub = AniTypeSub.TranslateX,
                TimeTotal = Time,
                Ease = Ease ?? new AniEaseLinear(),
                Obj = Obj,
                Value = Value,
                IsAfter = After,
                TimeFinished = -Delay
            };
        }

        /// <summary>
        /// 利用 TranslateTransform 移动 Y 轴的动画，这不会造成布局更新。
        /// </summary>
        /// <param name="Obj">动画的对象。</param>
        /// <param name="Value">进行移动的值。</param>
        /// <param name="Time">动画长度（毫秒）。</param>
        /// <param name="Delay">动画延迟执行的时间（毫秒）。</param>
        /// <param name="Ease">插值器类型。</param>
        /// <param name="After">是否等到以前的动画完成后才继续本动画。</param>
        public static AniData AaTranslateY(object Obj, double Value, int Time = 400, int Delay = 0, AniEase Ease = null, bool After = false)
        {
            return new AniData
            {
                TypeMain = AniType.Number,
                TypeSub = AniTypeSub.TranslateY,
                TimeTotal = Time,
                Ease = Ease ?? new AniEaseLinear(),
                Obj = Obj,
                Value = Value,
                IsAfter = After,
                TimeFinished = -Delay
            };
        }

        // 特殊

        /// <summary>
        /// 将一个 StackPanel 中的各个项目依次显示。
        /// </summary>
        /// <param name="Stack">StackPanel 对象</param>
        /// <param name="Time">动画时长（毫秒）</param>
        /// <param name="Delay">每个项目动画的延迟时间（毫秒）</param>
        /// <returns>包含每个项目动画数据的列表</returns>
        public static List<AniData> AaStack(StackPanel Stack, int Time = 100, int Delay = 25)
        {
            var result = new List<AniData>();
            int aniDelay = 0;
            foreach (var item in Stack.Children)
            {
                if (item is UIElement uiElement)
                {
                    uiElement.Opacity = 0;
                    result.Add(AaOpacity(uiElement, 1, Time, aniDelay));
                    aniDelay += Delay;
                }
            }
            return result;
        }
        #endregion


    }

    /// <summary>
    /// 动画数据
    /// </summary>
    public class AniGroupEntry
    {
        public List<AniData> Data;
        public long StartTick;
        public int Uuid = GetUuid();
    }

    /// <summary>
    /// 缓动函数基类。
    /// </summary>
    public abstract class AniEase
    {
        /// <summary>
        /// 获取函数值
        /// </summary>
        /// <param name="t">时间百分比</param>
        public abstract double GetValue(double t);

        /// <summary>
        /// 获取增量值
        /// </summary>
        /// <param name="t1">较大的 X</param>
        /// <param name="t0">较小的 X</param>
        public virtual double GetDelta(double t1, double t0)
        {
            return GetValue(t1) - GetValue(t0);
        }
    }

    /// <summary>
    /// 渐入渐出组合。
    /// </summary>
    public class AniEaseInout : AniEase
    {
        private readonly AniEase EaseIn;
        private readonly AniEase EaseOut;
        private readonly double EaseInPercent;

        public AniEaseInout(AniEase easeIn, AniEase easeOut, double easeInPercent = 0.5)
        {
            EaseIn = easeIn;
            EaseOut = easeOut;
            EaseInPercent = easeInPercent;
        }

        public override double GetValue(double t)
        {
            if (t < EaseInPercent)
            {
                return EaseInPercent * EaseIn.GetValue(t / EaseInPercent);
            }
            else
            {
                return (1 - EaseInPercent) * EaseOut.GetValue((t - EaseInPercent) / (1 - EaseInPercent)) + EaseInPercent;
            }
        }
    }

    /// <summary>
    /// 线性，无缓动。
    /// </summary>
    public class AniEaseLinear : AniEase
    {
        public override double GetValue(double t)
        {
            return MathUtil.MathClamp(t, 0, 1);
        }

        public override double GetDelta(double t1, double t0)
        {
            return MathClamp(t1,0,1) - MathClamp(t0,0,1);
        }
    }

    /// <summary>
    /// 平滑开始。
    /// </summary>
    public class AniEaseInFluent : AniEase
    {
        private readonly AniEasePower p;
        /// <summary>
        /// 平滑开始。
        /// </summary>
        /// <param name="power"></param>
        public AniEaseInFluent(AniEasePower power = AniEasePower.Middle)
        {
            p = power;
        }
        public override double GetValue(double t)
        {
            // 幂函数计算
            return Math.Pow(MathClamp(t, 0, 1), (double)p);
        }
    }

    /// <summary>
    /// 平滑结束。
    /// </summary>
    public class AniEaseOutFluent : AniEase
    {
        private readonly AniEasePower p;

        /// <summary>
        /// 平滑结束。
        /// </summary>
        /// <param name="power"></param>
        public AniEaseOutFluent(AniEasePower power = AniEasePower.Middle)
        {
            p = power;
        }

        public override double GetValue(double t)
        {
            // 幂函数计算
            return 1 - Math.Pow(MathClamp(1 - t, 0, 1), (double)p);
        }
    }

    /// <summary>
    /// 回弹结束。有效时间为 1/3。
    /// </summary>
    public class AniEaseOutBack : AniEase
    {
        private readonly double p;

        public AniEaseOutBack(AniEasePower power = AniEasePower.Middle)
        {
            p = 3 - (double)power * 0.5;
        }

        public override double GetValue(double t)
        {
            t = MathClamp(t, 0, 1);
            return 1 - Math.Pow((1 - t), p) * Math.Cos(1.5 * Math.PI * t);
        }
    }


    /// <summary>
    /// 平滑开始与结束。
    /// </summary>
    public class AniEaseInoutFluent : AniEase
    {
        private AniEaseInout ease;

        public AniEaseInoutFluent(AniEasePower power = AniEasePower.Middle, double middle = 0.5)
        {
            ease = new AniEaseInout(new AniEaseInFluent(power), new AniEaseOutFluent(power), middle);
        }

        public override double GetValue(double t)
        {
            return ease.GetValue(t);
        }
    }

    // Elastic / 弹簧
    /// <summary>
    /// 弹簧开始。约在 60% 到达最小值。
    /// </summary>
    public class AniEaseInElastic : AniEase
    {
        private readonly int p; // 6~9

        public AniEaseInElastic(AniEasePower Power = AniEasePower.Middle)
        {
            p = (int)Power + 4;
        }

        public override double GetValue(double t)
        {
            t = MathClamp(t, 0, 1);
            return Math.Pow(t, (p - 1) * 0.25) * Math.Cos((p - 3.5) * Math.PI * Math.Pow(1 - t, 1.5));
        }
    }

    /// <summary>
    /// 弹簧结束。约在 40% 到达最大值。
    /// </summary>
    public class AniEaseOutElastic : AniEase
    {
        private readonly int p;

        public AniEaseOutElastic(AniEasePower Power = AniEasePower.Middle)
        {
            p = (int)Power + 4;
        }

        public override double GetValue(double t)
        {
            t = 1 - MathClamp(t, 0, 1);
            return 1 - Math.Pow(t, (p - 1) * 0.25) * Math.Cos((p - 3.5) * Math.PI * Math.Pow(1 - t, 1.5));
        }
    }



    /// <summary>
    /// 单个动画对象。
    /// </summary>
    public struct AniData
    {
        /// <summary>
        /// 动画种类。
        /// </summary>
        public AniType TypeMain;

        /// <summary>
        /// 动画副种类。
        /// </summary>
        public AniTypeSub TypeSub;

        /// <summary>
        /// 动画总长度。
        /// </summary>
        public int TimeTotal;

        /// <summary>
        /// 已经执行的动画长度。如果为负数则为延迟。
        /// </summary>
        public int TimeFinished;

        /// <summary>
        /// 已经完成的百分比。
        /// </summary>
        public double TimePercent;

        /// <summary>
        ///  是否为“以后”。
        /// </summary>
        public bool IsAfter;

        /// <summary>
        /// 插值器类型。
        /// </summary>
        public AniEase Ease;

        /// <summary>
        /// 动画对象。
        /// </summary>
        public object Obj;

        /// <summary>
        /// 动画值。
        /// </summary>
        public object Value;

        /// <summary>
        /// 上次执行时的动画值。
        /// </summary>
        public object ValueLast;

        public override string ToString()
        {
            return $"{GetStringFromEnum(TypeMain)}|{TimeFinished}/{TimeTotal}({Math.Round(TimePercent * 100)}%){(Obj == null ? "" : $"|{Obj}({Obj.GetType().Name})")}";
        }

    }

    /// <summary>
    /// 动画基础种类。
    /// </summary>
    public enum AniType
    {
        /// <summary>
        /// 单个Double的动画，包括位置、长宽、透明度等。这需要附属类型。
        /// </summary>
        /// <remarks></remarks>
        Number,
        /// <summary>
        /// 颜色属性的动画。这需要附属类型。
        /// </summary>
        /// <remarks></remarks>
        Color,
        /// <summary>
        /// 缩放控件大小。比起4个DoubleAnimation来说效率更高。
        /// </summary>
        /// <remarks></remarks>
        Scale,
        /// <summary>
        /// 文字一个个出现。
        /// </summary>
        /// <remarks></remarks>
        TextAppear,
        /// <summary>
        /// 执行代码。
        /// </summary>
        /// <remarks></remarks>
        Code,
        /// <summary>
        /// 以 WPF 方式缩放控件。
        /// </summary>
        ScaleTransform,
        /// <summary>
        /// 以 WPF 方式旋转控件。
        /// </summary>
        RotateTransform
    }

    /// <summary>
    /// 动画扩展种类。
    /// </summary>
    public enum AniTypeSub
    {
        /// <summary>
        /// X 轴的位移。
        /// </summary>
        X,
        /// <summary>
        /// Y 轴的位移。
        /// </summary>
        Y,
        /// <summary>
        /// 宽度。
        /// </summary>
        Width,
        /// <summary>
        /// 高度。
        /// </summary>
        Height,
        /// <summary>
        /// 不透明度。
        /// </summary>
        Opacity,
        /// <summary>
        /// 值。
        /// </summary>
        Value,
        /// <summary>
        /// 圆角。
        /// </summary>
        Radius,
        /// <summary>
        /// 边框厚度。
        /// </summary>
        BorderThickness,
        /// <summary>
        /// 边框厚度。
        /// </summary>
        StrokeThickness,
        /// <summary>
        /// X 轴的位移。
        /// </summary>
        TranslateX,
        /// <summary>
        /// Y 轴的位移。
        /// </summary>
        TranslateY,
        /// <summary>
        /// Double 类型。
        /// </summary>
        Double,
        /// <summary>
        /// Double 类型，带参数。
        /// </summary>
        DoubleParam,
        /// <summary>
        /// GridLength 类型。
        /// </summary>
        GridLengthWidth
    }

    /// <summary>
    /// 动画缓动函数的强度。
    /// </summary>
    public enum AniEasePower : int
    {
        Weak = 2,
        Middle = 3,
        Strong = 4,
        ExtraStrong = 5
    }
}
