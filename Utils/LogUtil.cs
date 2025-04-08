using MyPCL.Modules;
using MyPCL.ViewModules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyPCL.Utils
{
    /// <summary>
    /// 日志等级
    /// </summary>
    public enum LogLevel
    {
        ///<summary>
        ///不提示，只记录日志。
        ///</summary>
        Normal = 0,
        ///<summary>
        ///只提示开发者。
        ///</summary>
        Developer = 1,
        ///<summary>
        ///只提示开发者与调试模式用户。
        ///</summary>
        Debug = 2,
        ///<summary>
        ///弹出提示所有用户。
        ///</summary>
        Hint = 3,
        ///<summary>
        ///弹窗，不要求反馈。
        ///</summary>
        Msgbox = 4,
        ///<summary>
        ///弹窗，要求反馈。
        ///</summary>
        Feedback = 5,
        ///<summary>
        ///弹窗，结束程序。
        ///</summary>
        Assert = 6,
    }
    /// <summary>
    /// 日志工具
    /// </summary>
    public static class LogUtil
    {
        /// <summary>
        /// 日志列表
        /// </summary>
        private static StringBuilder LogList;

        /// <summary>
        /// 日志文件
        /// </summary>
        private static StreamWriter LogWritter;

        public static void LogStart()
        {
            ThreadUtil.RunInNewThread(() => {
                bool IsInitSuccess = true;
                try
                {
                    for(int i = 4;i >=1; i--)
                    {
                        if (File.Exists($"{ModBase.Path}PCL\\Log{i}.txt"))
                        {
                            if (File.Exists($"{ModBase.Path}PCL\\Log{i + 1}.txt"))
                            {
                                File.Delete($"{ModBase.Path}PCL\\Log{i + 1}.txt");
                            }
                            FileUtil.CopyFile($"{ModBase.Path}\\PCL\\Log{i}.txt", $"{ModBase.Path}\\PCL\\Log{i + 1}.txt");
                        }
                    }
                    File.Create(ModBase.Path + "PCL\\Log1.txt").Dispose();
                }
                catch (IOException ex)
                {
                    IsInitSuccess = false;
                    HintModule.Hint("可能同时开启了多个 PCL，程序可能会出现未知问题！", HintType.Critical);
                }


            }, "Log Writer", ThreadPriority.Lowest);
        }

        /// <summary>
        /// 输出错误信息。
        /// </summary>
        /// <param name="Ex"></param>
        /// <param name="Desc">错误描述。会在处理时在末尾加入冒号。</param>
        /// <param name="Level"></param>
        /// <param name="Title"></param>
        public static void Log(Exception Ex,string Desc,LogLevel Level = LogLevel.Debug,string Title = "出现错误")
        {
            if (Ex is ThreadInterruptedException)
            {
                return;
            }
            // 获取错误信息
            //string ExFull = $"{Desc}:"
        }

    }
}
