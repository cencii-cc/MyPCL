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
    /// 模块加载状态枚举。
    /// </summary>
    public enum LoadState
    {
        Waiting,
        Loading,
        Finished,
        Failed,
        Aborted
    }

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

        /// <summary>
        /// 在加载器目标事件执行完成，加载器状态即将变为 Finish 时调用。可以视为扩展加载器目标事件。
        /// </summary>
        public event Action<LoaderBase> PreviewFinish;

        protected void RaisePreviewFinish()
        {
            PreviewFinish?.Invoke(this);
        }

        /// <summary>
        /// 当状态改变时，在工作线程触发代码。在添加事件后，必须将 HasOnStateChangedThread 设为 True。
        /// </summary>
        public event Action<LoaderBase, LoadState, LoadState> OnStateChangedThread;
        public bool HasOnStateChangedThread = false;


        /// <summary>
        /// 当状态改变时，在工作线程触发代码。在添加事件后，必须将 HasOnStateChangedThread 设为 True。
        /// </summary>
        public event Action<LoaderBase, LoadState, LoadState> OnStateChangedUi;


        public MyLoadingState LoadingState { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event ILoadingTriggerEvenHandler.LoadingStateChangedHandler LoadingStateChanged;
        public event ILoadingTriggerEvenHandler.ProgressChangedHandler ProgressChanged;
    }
}
