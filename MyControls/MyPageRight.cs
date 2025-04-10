using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using static MyPCL.Utils.BaseUtil;
using static MyPCL.Utils.AnimationUtil;
using static MyPCL.Utils.ThreadUtil;
using static MyPCL.Utils.LogUtil;
using static MyPCL.Modules.ModBase;
using System.Windows;
using MyPCL.Utils;
using System.Xml;
using System.Threading;

namespace MyPCL.MyControls
{
    /// <summary>
    /// 当前状态
    /// </summary>
    public enum PageStates
    {
        /// <summary>
        /// 空闲状态，页面全空
        /// </summary>
        Empty,
        /// <summary>
        /// 加载环初始等待
        /// </summary>
        LoaderWait,
        /// <summary>
        /// 加载环进入动画
        /// </summary>
        LoaderEnter,
        /// <summary>
        /// 加载环正常显示（强制等待）
        /// </summary>
        LoaderStayForce,
        /// <summary>
        ///  加载环正常显示
        /// </summary>
        LoaderStay,
        /// <summary>
        /// 加载环退出动画
        /// </summary>
        LoaderExit,
        /// <summary>
        /// 内容进入动画
        /// </summary>
        ContentEnter,
        /// <summary>
        /// 内容正常显示
        /// </summary>
        ContentStay,
        /// <summary>
        /// 刷新导致的全部退出动画，或页面内容退出（子页面更改）导致的全部退出动画
        /// </summary>
        ContentExit,
        /// <summary>
        /// 切换页面导致的全部退出动画
        /// </summary>
        PageExit 
    }

    public class MyPageRight : AdornerDecorator
    {
        public int PageUuid = GetUuid();

        public MyScrollViewer PanScroll
        {
            get { return (MyScrollViewer)GetValue(PanScrollProperty); }
            set { SetValue(PanScrollProperty, value); }
        }
        public static readonly DependencyProperty PanScrollProperty =
            DependencyProperty.Register("PanScroll", typeof(MyScrollViewer), typeof(MyPageRight), new PropertyMetadata(null));

        private PageStates _pageState = PageStates.Empty;
        public PageStates PageState
        {
            get { return _pageState; }
            set
            {
                if (_pageState == value) return;
                _pageState = value;
                if (ModeDebug) Log($"[UI] 页面状态切换为 {GetStringFromEnum(value)}");
            }
        }

        #region 加载器

        private LoaderBase PageLoader;
        private object PageLoaderInputInvoke;
        private MyLoading PageLoaderUi;
        private FrameworkElement PanLoader;
        private FrameworkElement PanContent;
        private FrameworkElement PanAlways;
        private bool PageLoaderAutoRun;

        public void PageLoaderInit(MyLoading loaderUi, FrameworkElement panLoader, FrameworkElement panContent,
                                 FrameworkElement panAlways, LoaderBase realLoader,
                                 Action<LoaderBase> finishedInvoke = null, Func<object> inputInvoke = null,
                                 bool autoRun = true)
        {
            PanLoader = panLoader;
            PanContent = panContent;
            PanAlways = panAlways;
            PageLoader = realLoader;
            PageLoaderUi = loaderUi;
            PageLoaderInputInvoke = inputInvoke;
            PageLoaderAutoRun = autoRun;

            if (finishedInvoke != null)
            {
                realLoader.PreviewFinish += (e) =>
                {
                    while (PageState == PageStates.PageExit || PageState == PageStates.ContentExit)
                    {
                        Thread.Sleep(10);
                    }
                    RunInUiWait(() => finishedInvoke(realLoader));
                    Thread.Sleep(20);
                };
            }

            realLoader.OnStateChangedUi += (loader, newState, oldState) =>
                RunInUi(() => PageLoaderState(loader, newState, oldState));

            panLoader.Visibility = Visibility.Collapsed;
            panContent.Visibility = Visibility.Collapsed;
            if (panAlways != null) panAlways.Visibility = Visibility.Collapsed;

            if (PageLoaderAutoRun)
            {
                if (PageLoader.GetType().Name.StartsWith("LoaderTask"))
                {
                    dynamic loader = PageLoader;
                    PageLoader.Start(loader.StartGetInput(null, PageLoaderInputInvoke));
                }
                else
                {
                    object input = PageLoaderInputInvoke?.Invoke();
                    PageLoader.Start(input);
                }
            }

            if (PageLoader.State == LoadState.Finished && finishedInvoke != null)
            {
                RunInUiWait(() => finishedInvoke(realLoader));
            }

            PageLoaderUi.State = realLoader;
            PageLoaderUi.Click += () =>
            {
                if (realLoader.State == LoadState.Failed) PageLoaderRestart();
            };
        }

        public void PageLoaderRestart(object input = null, bool isForceRestart = true)
        {
            if (!PageLoaderAutoRun) return;

            if (PageLoader.GetType().Name.StartsWith("LoaderTask"))
            {
                dynamic loader = PageLoader;
                PageLoader.Start(loader.StartGetInput(input, PageLoaderInputInvoke), isForceRestart);
            }
            else
            {
                if (input == null && PageLoaderInputInvoke != null)
                    input = PageLoaderInputInvoke();
                PageLoader.Start(input, isForceRestart);
            }
        }

        #endregion

    }
}
