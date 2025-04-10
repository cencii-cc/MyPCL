using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using static MyPCL.Utils.LogUtil;
using static MyPCL.Utils.BaseUtil;
using static MyPCL.ViewModules.ViewMUI;
using static MyPCL.Modules.Minecraft.ModMinecraft;
using static MyPCL.Modules.ModBase;
using static MyPCL.ViewModules.ViewMMyMsgBox;
using static MyPCL.Utils.FileUtil;

namespace MyPCL.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public static class FeedbackUtil
    {
        public static void Feedback(bool showMsgbox = true,bool forceOpenLog = false)
        {
            try
            {
                FeedbackInfo();
                if (forceOpenLog || (showMsgbox && MyMsgBox("若你在汇报一个 Bug，请点击 打开文件夹 按钮，并上传 Log(1~5).txt 中包含错误信息的文件。" + Environment.NewLine + "游戏崩溃一般与启动器无关，请不要因为游戏崩溃而提交反馈。", "反馈提交提醒", "打开文件夹", "不需要") == 1))
                    OpenExplorer($"{Path}PCL\\Log1.txt");
                else OpenWebsite("https://github.com/Hex-Dragon/PCL2/issues/")
            }
            catch
            {

            }
        }

        public static void CanFeedback(bool showHint)
        {

        }

        /// <summary>
        /// 在日志中输出系统诊断信息。
        /// </summary>
        public static void FeedbackInfo()
        {
            try
            {
                // 操作系统信息
                OperatingSystem os = Environment.OSVersion;
                string osFullName = $"{os.VersionString} ({os.Platform})";
                bool is64BitSystem = Environment.Is64BitOperatingSystem;
                string osInfo = $"操作系统：{osFullName}（32 位：{!is64BitSystem}）";

                // 物理内存信息（通过 ManagementObject 获取）
                long availableMemoryMB = 0;
                long totalMemoryMB = 0;
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        availableMemoryMB = (long)obj["FreePhysicalMemory"] / 1024; // 单位 KB 转 MB
                        totalMemoryMB = (long)obj["TotalVisibleMemorySize"] / 1024;
                    }
                }
                string memoryInfo = $"剩余内存：{availableMemoryMB} M / {totalMemoryMB} M";

                // DPI（WinForms 示例，WPF 需用其他方式）
                // 注意：需添加 System.Windows.Forms 引用
                string dpiInfo = $"DPI：{DPI}({Math.Round((double)(DPI / 96), 2) * 100}%)";

                // 组合日志信息（同上）
                string logMessage = $"[System] 诊断信息：{Environment.NewLine}" +
                                    $"{osInfo}{Environment.NewLine}" +
                                    $"{memoryInfo}{Environment.NewLine}" +
                                    $"{dpiInfo}{Environment.NewLine}" +
                                    $"MC 文件夹：{(string.IsNullOrEmpty(PathMcFolder) ? "Nothing" : PathMcFolder)}{Environment.NewLine}" +
                                    $"文件位置：{Path}";

                Log(logMessage);
            }
            catch
            {

            }
        }
    }
}
