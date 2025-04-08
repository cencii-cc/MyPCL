using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPCL.ViewModules
{
    /// <summary>
    /// 提示信息的种类。
    /// </summary>
    public enum HintType
    {
        /// <summary>
        /// 信息，通常是蓝色的“i”。
        /// </summary>
        Info,
        /// <summary>
        /// 已完成，通常是绿色的“√”。
        /// </summary>
        Finish,
        /// <summary>
        /// 错误，通常是红色的“×”。
        /// </summary>
        Critical,
    }

    /// <summary>
    /// 弹出提示
    /// </summary>
    public static class HintModule
    {
        private struct HintMessage
        {
            public string Text;
            public HintType Type;
            public bool Log;
        }

        /// <summary>
        /// 等待弹出的提示列表。以 {String, HintType, Log As Boolean} 形式存储为数组。
        /// </summary>
        private static List<HintMessage> HintWaiting = new List<HintMessage>();

        /// <summary>
        /// 在窗口左下角弹出提示文本。
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="type"></param>
        /// <param name="Log"></param>
        public static void Hint(string Text,HintType type = HintType.Info,bool Log = true)
        {
            if(HintWaiting == null)
            {
                HintWaiting = new List<HintMessage>();
            }
            HintWaiting.Add(new HintMessage()
            {
                Text = string.IsNullOrEmpty(Text) ? "":Text,
                Type = type,
                Log = Log
            });
        }
    }
}
