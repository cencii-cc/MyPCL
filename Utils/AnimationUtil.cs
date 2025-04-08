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
        public static AniData AaScaleTransform(object obj, double value,int time = 400,int delay = 0,AniEase ease = null,bool after = false)
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

        /// <summary>
        /// 直接停止一个动画组。
        /// </summary>
        /// <param name="name"></param>
        public static void AniStop(string name)
        {
            AniGroups.Remove(name);
        }
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
