using MyPCL.Modules;
using MyPCL.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static MyPCL.Utils.LogUtil;
using static MyPCL.Utils.FileUtil;
using static MyPCL.Utils.ThreadUtil;
using static MyPCL.Utils.BaseUtil;
using static MyPCL.Utils.RandomUtil;
using System.Windows.Shell;
using System.Net;
using Microsoft.VisualBasic;
using System.Threading;

namespace MyPCL.MyControls
{
    /// <summary>
    /// 自定义图片控件
    /// </summary>
    public class MyImage : Image
    {
        /// <summary>
        /// 网络图片的缓存有效期。--7天<br/>
        /// 在这个时间后，才会重新尝试下载图片。
        /// </summary>
        public TimeSpan FileCacheExpiredTime = new TimeSpan(7, 0, 0, 0);

        /// <summary>
        /// 是否允许将网络图片存储到本地用作缓存。
        /// </summary>
        public bool EnableCache
        {
            get
            {
                return (bool)GetValue(EnableCacheProperty);
            }
            set
            {
                SetValue(EnableCacheProperty, value);
            }
        }
        public static new readonly DependencyProperty EnableCacheProperty = DependencyProperty.Register(
        "EnableCache", typeof(bool), typeof(MyImage), new PropertyMetadata(true));

        /// <summary>
        /// 与 Image 的 Source 类似。<br/>
        /// 若输入以 http 开头的字符串，则会尝试下载图片然后显示，图片会保存为本地缓存。<br/>
        /// 支持 WebP 格式的图片。
        /// </summary>
        public new string Source
        {
            get
            {
                return _Source;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = null;
                }
                if (_Source == value) return;
                _Source = value;
                // 属性读取顺序修正：在完成 XAML 属性读取后再触发图片加载（#4868）
                if (!IsInitialized) return;

            }
        }
        private string _Source = null;

        /// <summary>
        /// 当 Source 首次下载失败时，会从该备用地址加载图片。
        /// </summary>
        public string FallbackSource
        {
            get
            {
                return _FallbackSource;
            }
            set
            {
                _FallbackSource = value;
            }
        }
        private string _FallbackSource = null;

        public string LoadingSource
        {
            get
            {
                return _LoadingSource;
            }
            set
            {
                _LoadingSource = value;
            }
        }
        private string _LoadingSource = "pack://application:,,,/images/Icons/NoIcon.png";


        /// <summary>
        /// 实际被呈现的图片地址。
        /// </summary>
        public string ActualSource
        {
            get
            {
                return _ActualSource;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = null;
                }
                if (_ActualSource == value) return;
                _ActualSource = value;
                try
                {
                    // 在这里先触发可能的文件读取，尽量避免在 UI 线程中读取文件
                    var Bitmap = value == null ? null : new BitmapUtils(value);
                    RunInUiWait(() =>
                    {
                        base.Source = Bitmap;
                    });
                }
                catch (Exception ex) 
                {
                    Log(ex, $"加载图片失败（{value}）");
                    // 异常了删除图片
                    try
                    {
                        if(value.StartsWith(ModBase.PathTemp) && File.Exists(value)) File.Delete(value);
                    }
                    catch
                    {
                        return;
                    }
                }
            }
        }
        private string _ActualSource = null;


        public MyImage()
        {
            // 属性读取顺序修正：在完成 XAML 属性读取后再触发图片加载（#4868）
            this.Initialized += (sender, e) => Load();
        }

        private void Load()
        {
            // 空
            if (Source == null)
            {
                ActualSource = null;
                return;
            }

            // 本地图片
            if (!Source.StartsWith("http"))
            {
                ActualSource = Source;
                return;
            }

            // 从缓存加载网络图片
            string url = Source;
            bool retried = false;
            string tempPath = GetTempPath(url);
            FileInfo tempFile = new FileInfo(tempPath);
            bool enableCache = EnableCache;
            if (enableCache && tempFile.Exists)
            {
                ActualSource = tempPath;
                if (DateTime.Now - tempFile.LastWriteTime < FileCacheExpiredTime)
                {
                    return; // 无需刷新缓存
                }
            }

            RunInNewThread(() =>
            {
                string tempDownloadingPath = null;
            retryStart:
                try
                {
                    // 下载
                    ActualSource = LoadingSource; // 显示加载中图片
                    tempDownloadingPath = tempPath + RandomInteger(0, 10000000);
                    Directory.CreateDirectory(Path.GetDirectoryName(tempPath)); // 重新实现下载，以避免携带 Header（#5072）
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(url, tempDownloadingPath);
                    }

                    if (url != Source && url != FallbackSource)
                    {
                        // 已经更换了地址
                        File.Delete(tempDownloadingPath);
                    }
                    else if (enableCache)
                    {
                        // 保存缓存并显示
                        if (File.Exists(tempPath))
                        {
                            File.Delete(tempPath);
                        }
                        File.Move(tempDownloadingPath, tempPath);
                        RunInUi(() => ActualSource = tempPath);
                    }
                    else
                    {
                        // 直接显示
                        RunInUiWait(() => ActualSource = tempDownloadingPath);
                        File.Delete(tempDownloadingPath);
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        if (tempPath != null)
                        {
                            File.Delete(tempPath);
                        }
                        if (tempDownloadingPath != null)
                        {
                            File.Delete(tempDownloadingPath);
                        }
                    }
                    catch
                    {
                        // 忽略异常
                    }

                    if (!retried)
                    {
                        // 更换备用地址
                        Log(ex, $"下载图片可重试地失败（{url}）", LogLevel.Developer);
                        retried = true;
                        url = FallbackSource ?? Source;
                        // 空
                        if (url == null)
                        {
                            ActualSource = null;
                            return;
                        }

                        // 本地图片
                        if (!url.StartsWith("http"))
                        {
                            ActualSource = url;
                            return;
                        }

                        // 从缓存加载网络图片
                        tempPath = GetTempPath(url);
                        tempFile = new FileInfo(tempPath);
                        if (enableCache && tempFile.Exists)
                        {
                            ActualSource = tempPath;
                            if (DateTime.Now - tempFile.CreationTime < FileCacheExpiredTime)
                            {
                                return; // 无需刷新缓存
                            }
                        }

                        // 下载
                        if (Source == url)
                        {
                            Thread.Sleep(1000); // 延迟 1s 重试
                        }
                        goto retryStart;
                    }
                    else
                    {
                        Log(ex, $"下载图片失败（{url}）", LogLevel.Hint);
                    }
                }
            }, "MyImage PicLoader " + GetUuid() + "#", ThreadPriority.BelowNormal);
        }
    }
}
