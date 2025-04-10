using MyPCL.Modules;
using MyPCL.ViewModules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static MyPCL.Utils.BaseUtil;
using static MyPCL.Modules.ModBase;
using static MyPCL.ViewModules.HintModule;
using static MyPCL.ViewModules.ViewMMyMsgBox;
using System.Windows;

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

        private static object LogListLock = new object();

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
        public static void Log(string text, LogLevel level = LogLevel.Normal, string title = "出现错误")
        {
            try
            {
                // 输出日志
                string appendText = $"[{GetTimeNow()}] {text}{Environment.NewLine}";
                if (ModeDebug)
                {
                    lock (LogListLock)
                    {
                        LogList.Append(appendText);
                    }
                }
                else
                {
                    LogList.Append(appendText);
                }

#if DEBUG
                Console.Write(appendText);
#endif

                if (IsProgramEnded || level == LogLevel.Normal)
                {
                    return;
                }

                // 去除前缀
                text = Regex.Replace(text, @"\[[^\]]+?\] ", "");

                // 输出提示
                switch (level)
                {
#if DEBUG
                    case LogLevel.Developer:
                        Hint($"[开发者模式] {text}", HintType.Info, false);
                        break;
                    case LogLevel.Debug:
                        Hint($"[调试模式] {text}", HintType.Info, false);
                        break;
#else
                case LogLevel.Developer:
                    break;
                case LogLevel.Debug:
                    if (ModeDebug)
                    {
                        Hint($"[调试模式] {text}", HintType.Info, false);
                    }
                    break;
#endif
                    case LogLevel.Hint:
                        Hint(text, HintType.Critical, false);
                        break;
                    case LogLevel.Msgbox:
                        MyMsgBox(text, title, isWarn:true);
                        break;
                    case LogLevel.Feedback:
                        if (CanFeedback(false))
                        {
                            if (MyMsgBox($"{text}{Environment.NewLine}{Environment.NewLine}是否反馈此问题？如果不反馈，这个问题可能永远无法得到解决！", title, "反馈", "取消", true) == 1)
                            {
                                Feedback(false, true);
                            }
                        }
                        else
                        {
                            MyMsgBox($"{text}{Environment.NewLine}{Environment.NewLine}将 PCL 更新至最新版或许可以解决这个问题……", title, true);
                        }
                        break;
                    case LogLevel.Assert:
                        long time = GetTimeTick();
                        if (CanFeedback(false))
                        {
                            if (System.Windows.Forms.MessageBox.Show($"{text}{Environment.NewLine}{Environment.NewLine}是否反馈此问题？如果不反馈，这个问题可能永远无法得到解决！", title, System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Critical) == System.Windows.Forms.DialogResult.Yes)
                            {
                                Feedback(false, true);
                            }
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show($"{text}{Environment.NewLine}{Environment.NewLine}将 PCL 更新至最新版或许可以解决这个问题……", title, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Critical);
                        }
                        if (GetTimeTick() - time < 1500)
                        {
                            // 弹窗无法保留
                            Log($"[System] PCL 已崩溃：{Environment.NewLine}{text}");
                            FormMain.EndProgramForce(ProcessReturnValues.Exception);
                        }
                        else
                        {
                            FormMain.EndProgramForce(ProcessReturnValues.Fail);
                        }
                        break;
                }
            }
            catch
            {
                // 原代码使用 On Error Resume Next，这里简单忽略异常
            }
        }


        public static void Log(Exception Ex, string Desc, LogLevel Level = LogLevel.Debug, string Title = "出现错误")
        {
            try
            {
                string ExFull = Desc + "：" + GetExceptionDetail(Ex);

                string AppendText = "[" + GetTimeNow() + "] " + Desc + "：" + GetExceptionDetail(Ex, true) + Environment.NewLine;
                if (ModeDebug)
                {
                    lock (LogListLock)
                    {
                        LogList.Append(AppendText);
                    }
                }
                else
                {
                    LogList.Append(AppendText);
                }

#if DEBUG
                Console.Write(AppendText);
#endif

                if (IsProgramEnded)
                {
                    return;
                }

                switch (Level)
                {
                    case LogLevel.Normal:
#if DEBUG
                        break;
                    case LogLevel.Developer:
                        string ExLineDeveloper = Desc + "：" + GetExceptionSummary(Ex);
                        Hint("[开发者模式] " + ExLineDeveloper, HintType.Info, false);
                        break;
                    case LogLevel.Debug:
                        string ExLineDebug = Desc + "：" + GetExceptionSummary(Ex);
                        Hint("[调试模式] " + ExLineDebug, HintType.Info, false);
                        break;
#else
            case LogLevel.Developer:
            case LogLevel.Debug:
                string ExLine = Desc + "：" + GetExceptionSummary(Ex);
                if (ModeDebug)
                {
                    Hint("[调试模式] " + ExLine, HintType.Info, false);
                }
                break;
#endif
                    case LogLevel.Hint:
                        string ExLineHint = Desc + "：" + GetExceptionSummary(Ex);
                        Hint(ExLineHint, HintType.Critical, false);
                        break;
                    case LogLevel.Msgbox:
                        MyMsgBox(ExFull, Title, true);
                        break;
                    case LogLevel.Feedback:
                        if (CanFeedback(false))
                        {
                            if (MyMsgBox(ExFull + Environment.NewLine + Environment.NewLine + "是否反馈此问题？如果不反馈，这个问题可能永远无法得到解决！", Title, "反馈", "取消", true) == 1)
                            {
                                Feedback(false, true);
                            }
                        }
                        else
                        {
                            MyMsgBox(ExFull + Environment.NewLine + Environment.NewLine + "将 PCL 更新至最新版或许可以解决这个问题……", Title, true);
                        }
                        break;
                    case LogLevel.Assert:
                        long Time = GetTimeTick();
                        if (CanFeedback(false))
                        {
                            if (MessageBox.Show(ExFull + Environment.NewLine + Environment.NewLine + "是否反馈此问题？如果不反馈，这个问题可能永远无法得到解决！", Title, MessageBoxButtons.YesNo, MessageBoxIcon.Critical) == DialogResult.Yes)
                            {
                                Feedback(false, true);
                            }
                        }
                        else
                        {
                            MessageBox.Show(ExFull + Environment.NewLine + Environment.NewLine + "将 PCL 更新至最新版或许可以解决这个问题……", Title, MessageBoxButtons.OK, MessageBoxIcon.Critical);
                        }
                        if (GetTimeTick() - Time < 1500)
                        {
                            Log("[System] PCL 已崩溃：" + Environment.NewLine + ExFull);
                            FormMain.EndProgramForce(ProcessReturnValues.Exception);
                        }
                        else
                        {
                            FormMain.EndProgramForce(ProcessReturnValues.Fail);
                        }
                        break;
                }
            }
            catch
            {

            }
        }
    }
}
