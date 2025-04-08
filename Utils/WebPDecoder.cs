using Imazen.WebP;
using MyPCL.Modules;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPCL.Utils
{
    public class WebPDecoder
    {
        public static Bitmap DecodeFromBytes(byte[] bytes)
        {
            if (ModBase.Is32BitSystem)
            {
                throw new Exception("不支持在 32 位系统下加载 WebP 图片。");
            }
            SimpleDecoder deCoder = new SimpleDecoder();
            return deCoder.DecodeFromBytes(bytes, bytes.Length);
        }
    }
}
