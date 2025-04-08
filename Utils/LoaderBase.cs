using MyPCL.Utils.Enum;
using MyPCL.Utils.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPCL.Utils
{
    /// <summary>
    /// 加载器的统一基类。
    /// </summary>
    public abstract class LoaderBase : ILoadingTrigger
    {
        public bool IsLoader => true;

        // 基础属性
        /// <summary>
        ///  加载器的标识编号。
        /// </summary>
        public int Uuid = BaseUtil.GetUuid();

        /// <summary>
        /// 加载器的名称。
        /// </summary>
        public string Name = "未命名任务#";

        /// <summary>
        /// 用于状态改变检测的同步锁。
        /// </summary>
        public object LockState => new object();

        /// <summary>
        /// 父加载器
        /// </summary>
        public LoaderBase Parent = null;

        /// <summary>
        /// 最上级的加载器。
        /// </summary>
        public LoaderBase RealParent
        {
            get
            {
                try
                {
                    LoaderBase RealParent = Parent;
                    while (RealParent != null && RealParent.Parent != null)
                    {
                        RealParent = RealParent.Parent;
                    }
                    return RealParent;

                }
                catch(Exception ex)
                {
                    return null;
                }
            }
        }

        public MyLoadingState LoadingState { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event ILoadingTriggerEvenHandler.LoadingStateChangedHandler LoadingStateChanged;
        public event ILoadingTriggerEvenHandler.ProgressChangedHandler ProgressChanged;
    }
}
