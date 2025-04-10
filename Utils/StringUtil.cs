using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPCL.Utils
{
    public static class StringUtil
    {

        public static char vbLQ = Convert.ToChar(8220);

        /// <summary>
        /// 提取 Exception 的具体描述与堆栈。
        /// </summary>
        /// <param name="ShowAllStacks">是否必须显示所有堆栈。通常用于判定堆栈信息</param>
        public static string GetExceptionDetail(Exception ex, bool showAllStacks = false)
        {
            if (ex == null)
            {
                return "无可用错误信息！";
            }

            // 获取最底层的异常（Do Until 转换：循环直到 InnerEx.InnerException 为 Nothing，即 C# 中 InnerEx.InnerException == null 时停止）
            Exception innerEx = ex;
            do // VB 的 Do Until 是先执行再检查条件，C# 用 do-while 确保至少执行一次
            {
                innerEx = innerEx.InnerException;
            } while (innerEx != null); // 当 InnerEx.InnerException 不为 null 时继续循环，直到为 null 停止

            var descList = new List<string>();
            bool isInner = false;
            Exception currentEx = ex;
            do // 处理异常链，Do Until Ex Is Nothing 转换为 do-while (currentEx != null)
            {
                // 处理消息中的换行符（VB 的 vbLf=LF, vbCr=CR，转换为 CRLF）
                string message = currentEx.Message
                    .Replace("\n", "\r")       // 将 LF 转换为 CR
                    .Replace("\r\r", "\r")     // 合并连续 CR
                    .Replace("\r", "\r\n");    // 最终转换为 CRLF

                descList.Add(isInner ? "→ " : "" + message);

                // 处理堆栈跟踪（修正 ContainsF 逻辑：VB 中 ContainsF 是忽略大小写的包含检查）
                if (currentEx.StackTrace != null)
                {
                    foreach (string stackLine in currentEx.StackTrace
                        .Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        // 实现类似 VB ContainsF("pcl", True) 的忽略大小写检查
                        bool containsPcl = stackLine.IndexOf("pcl", StringComparison.OrdinalIgnoreCase) >= 0;
                        if (showAllStacks || containsPcl)
                        {
                            // 移除所有换行符（VB 中 Replace(vbCr, "") 和 Replace(vbLf, "")）
                            string cleanedStack = stackLine.Replace("\r", "").Replace("\n", "");
                            descList.Add(cleanedStack);
                        }
                    }
                }

                // 添加错误类型（非 System.Exception 时）
                if (currentEx.GetType().FullName != "System.Exception")
                {
                    descList.Add("   错误类型：" + currentEx.GetType().FullName);
                }

                isInner = true;
                currentEx = currentEx.InnerException; // 移动到下一个内层异常
            } while (currentEx != null); // 循环直到 currentEx 为 Nothing

            // 常见错误处理（逻辑不变）
            string commonReason = null;
            if (innerEx is TypeLoadException ||
                innerEx is BadImageFormatException ||
                innerEx is MissingMethodException ||
                innerEx is NotImplementedException ||
                innerEx is TypeInitializationException)
            {
                commonReason = "PCL 的运行环境存在问题。请尝试重新安装 .NET Framework 4.6.2 然后再试。若无法安装，请先卸载较新版本的 .NET Framework，然后再尝试安装。";
            }
            else if (innerEx is UnauthorizedAccessException)
            {
                commonReason = "PCL 的权限不足。请尝试右键 PCL，选择以管理员身份运行。";
            }
            else if (innerEx is OutOfMemoryException)
            {
                commonReason = "你的电脑运行内存不足，导致 PCL 无法继续运行。请在关闭一部分不需要的程序后再试。";
            }
            else if (innerEx is System.Runtime.InteropServices.COMException)
            {
                commonReason = "由于操作系统或显卡存在问题，导致出现错误。请尝试重启 PCL。";
            }
            else if (new[] {
            "远程主机强迫关闭了", "远程方已关闭传输流", "未能解析此远程名称", "由于目标计算机积极拒绝",
            "操作已超时", "操作超时", "服务器超时", "连接超时"
        }.Any(s => descList.Any(l => l.Contains(s)))) // 此处原 VB 未忽略大小写，保持原始逻辑
            {
                commonReason = "你的网络环境不佳，导致难以连接到服务器。请检查网络，多重试几次，或尝试使用 VPN。";
            }

            // 构造最终输出
            string detail = string.Join("\r\n", descList);
            return commonReason == null ? detail :
                $"{commonReason}\r\n\r\n————————————\r\n详细错误信息：\r\n{detail}";
        }


        /// <summary>
        /// 获取字符串哈希值。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static ulong GetHash(string str)
        {
            ulong hash = 5381;
            for (int i = 0; i < str.Length; i++)
            {
                hash = ((hash << 5) ^ hash) ^ (ulong)str[i];
            }
            return hash ^ 0xA98F501BC684032FUL;
        }
    }
}
