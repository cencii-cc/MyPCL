using MyPCL.Utils;
using MyPCL.ViewModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MyPCL.Utils.StringUtil;

namespace MyPCL.Modules
{
    public static class ModBase
    {
        #region 声明
        //下列版本信息由更新器自动修改
        public const string VersionBaseName = "2.9.2"; //不含分支前缀的显示用版本名
        /// <summary>
        /// 标准格式的四段式版本号
        /// </summary>
        public static string VersionStandardCode = $"2.9.2.{VersionBranchCode}";
        /// <summary>
        /// Commit Hash，由 GitHub Workflow 自动替换
        /// </summary>
        public const string CommitHash = "";

#if BETA
        public const int VersionCode = 352;
#else
        public const int VersionCode = 353;
#endif

#if RELEASE
        public const string VersionBranchName = "Snapshot";
        public const string VersionBranchCode = "0";
#elif BETA
        public const string VersionBranchName = "Release";
        public const string VersionBranchCode = "50";
#else
        public const string VersionBranchName = "Debug";
        public const string VersionBranchCode = "0";
#endif
        // 自动生成的版本信息
        public static string VersionDisplayName = $"{VersionBranchName} {VersionBaseName}";
        #endregion

        /// <summary>
        /// 主窗口句柄。
        /// </summary>
        public static IntPtr Handle;

        /// <summary>
        /// 程序的启动路径，以“\”结尾。
        /// </summary>
        public static string Path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

        /// <summary>
        /// 包含程序名的完整路径。
        /// </summary>
        public static string PathWithName = $"{Path}{AppDomain.CurrentDomain.SetupInformation.ApplicationName}";

        /// <summary>
        /// 程序内嵌图片文件夹路径，以“/”结尾。
        /// </summary>
        public static string PathImage = "pack://application:,,,/MyPCL;component/Images/";

        /// <summary>
        /// 程序的缓存文件夹路径，以 \ 结尾。
        /// </summary>
        public static string PathTemp
        {
            get
            {
                if (string.IsNullOrEmpty(Setup.Get("SystemSystemCache").ToString()))
                {
                    return System.IO.Path.GetTempPath() + "PCL\\";
                }
                else
                {
                    return Setup.Get("SystemSystemCache").ToString().Replace("/", "\\").TrimEnd('\\') + "\\";
                }
            }
        }

        /// <summary>
        /// 当前程序的语言。
        /// </summary>
        public static string Lang = "zh_CN";

        /// <summary>
        /// 设置对象。
        /// </summary>
        public static ModSetup Setup = new ModSetup();

        /// <summary>
        /// 程序打开时的时间。
        /// </summary>
        public static DateTime ApplicationOpenTime = DateTime.Now;

        /// <summary>
        /// 识别码。
        /// </summary>
        public static string UniqueAddress = SecretUtil.SecretGetUniqueAddress();

        /// <summary>
        /// 是否为 32 位系统。
        /// </summary>
        public static bool Is32BitSystem = !Environment.Is64BitOperatingSystem;


        public static bool ModeDebug = false;

        /// <summary>
        /// 程序是否已结束。
        /// </summary>
        public static bool IsProgramEnded = false;
    }
}
