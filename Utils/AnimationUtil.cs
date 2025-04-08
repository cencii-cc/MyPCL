using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MyPCL.Utils.BaseUtil;

namespace MyPCL.Utils
{
    /// <summary>
    /// 动画引擎模块 <br/>
    /// 使用 Ani 作为方法或属性的开头，使用 Aa 作为单个动画对象的开头（便于自动补全）
    /// </summary>
    public static class AnimationUtil
    {
        /// <summary>
        /// 上一次记刻的时间。
        /// </summary>
        private static long AniLastTick;


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

        }
    }

    /// <summary>
    /// 动画数据
    /// </summary>
    public class AniGroupEntry
    {

    }

    /// <summary>
    /// 单个动画对象。
    /// </summary>
    public struct AniData
    {
        public 
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
}
