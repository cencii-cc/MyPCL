using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPCL.Utils.Minecraft
{
    public static class ModDownload
    {

    }

    /// <summary>
    /// OptiFine 列表项。
    /// </summary>
    public class DlOptiFineListEntry
    {
        /// <summary>
        /// 显示名称，已去除 HD_U 字样，如“1.12.2 C8”。
        /// </summary>
        public string NameDisplay { set; get; }

        /// <summary>
        /// 原始文件名称，如“preview_OptiFine_1.11_HD_U_E1_pre.jar”。
        /// </summary>
        public string NameFile { set; get; }

        /// <summary>
        /// 对应的版本名称，如“1.13.2-OptiFine_HD_U_E6”。
        /// </summary>
        public string NameVersion { set; get;}

        /// <summary>
        /// 是否为测试版。
        /// </summary>
        public string IsPreview { set; get; }


        private string _inherit;
        /// <summary>
        /// 对应的 Minecraft 版本，如“1.12.2”。
        /// </summary>
        public string Inherit
        {
            get
            {
                return _inherit;
            }
            set
            {
                if (value.EndsWith(".0"))
                {
                    value = value.Substring(0, value.Length - 2);
                }
                _inherit = value;
            }
        }

        /// <summary>
        /// 发布时间，格式为“yyyy/mm/dd”。OptiFine 源无此数据。
        /// </summary>
        public string ReleaseTime { set; get;}

        /// <summary>
        /// 需要的最低 Forge 版本。空字符串为无限制，Nothing 为不兼容，“28.1.56” 表示版本号，“1161” 表示版本号的最后一位。
        /// </summary>
        public string RequiredForgeVersion { set; get; }
    }

    /// <summary>
    /// LiteLoader 列表项。
    /// </summary>
    public class DlLiteLoaderListEntry
    {
        /// <summary>
        /// 实际的文件名，如“liteloader-installer-1.12-00-SNAPSHOT.jar”。
        /// </summary>
        public string FileName { set; get; }

        /// <summary>
        /// 是否为测试版。
        /// </summary>
        public string IsPreview { set; get;}

        /// <summary>
        /// 对应的 Minecraft 版本，如“1.12.2”。
        /// </summary>
        public string Inherit { set; get; }

        /// <summary>
        /// 是否为 1.7 及更早的远古版。
        /// </summary>
        public string IsLegacy { set; get; }

        /// <summary>
        /// 发布时间，格式为“yyyy/mm/dd HH:mm”。
        /// </summary>
        public string ReleaseTime { set; get; }

        /// <summary>
        /// 文件的 MD5。
        /// </summary>
        public string MD5 { set; get;}

        /// <summary>
        /// 对应的 Json 项。
        /// </summary>
        public JToken JsonToken { set; get; }
    }

    public abstract class DlForgelikeEntry
    {
        public bool IsNeoForge;

        /// <summary>
        /// 加载器名称。Forge 或 NeoForge。
        /// </summary>
        public string LoaderName
        {
            get
            {
                return IsNeoForge ? "NeoForge" : "Forge";
            }
        }

        /// <summary>
        /// 文件扩展名。不以小数点开头。
        /// </summary>
        public string FileExtension
        {
            get
            {
                if (IsNeoForge)
                {
                    return "jar";
                }
                else
                {
                    return ((DlForgeVersionEntry)this).Category == "installer" ? "jar" : "zip";
                }
            }
        }

        /// <summary>
        /// 标准化后的版本号，仅可用于比较与排序。
        /// <br/>
        /// 格式：Major.Minor.Build.Revision
        /// <br/>
        /// Forge：如 “50.1.9.0”（最后一位固定为 0）、“14.22.1.2478”（Legacy）。
        /// <br/>
        /// NeoForge：如 “20.4.30.0”（最后一位固定为 0）、“19.47.1.99”（Legacy：第一位固定为 19）。
        /// </summary>
        public Version Version;

        /// <summary>
        /// 可对玩家显示的非格式化版本名。
        /// <br/>
        /// Forge：如 “50.1.9”、“14.22.1.2478”（Legacy）。
        /// <br/>
        /// NeoForge：如 “20.4.30-beta”、“47.1.99”（Legacy）。
        /// </summary>
        public string VersionName;

        /// <summary>
        ///  对应的 Minecraft 版本，如“1.12.2”。
        /// </summary>
        public string Inherit;
    }

    /// <summary>
    /// Forge 版本列表项。
    /// </summary>
    public class DlForgeVersionEntry : DlForgelikeEntry
    {
        /// <summary>
        /// 发布时间，格式为“yyyy/MM/dd HH:mm”。
        /// </summary>
        public string ReleaseTime { set; get;}

        /// <summary>
        /// 文件的 MD5 或 SHA1（BMCLAPI 的老版本是 MD5，新版本是 SHA1；官方源总是 MD5）。
        /// </summary>
        public string Hash { set; get; } = null;

        /// <summary>
        /// 是否为推荐版本。
        /// </summary>
        public bool IsRecommended { set; get; }

        /// <summary>
        /// 安装类型。有 installer、client、universal 三种。
        /// </summary>
        public string Category {  set; get; }

        /// <summary>
        /// 用于下载的文件版本名。可能在 Version 的基础上添加了分支。
        /// </summary>
        public string FileVersion { set; get; }

        public DlForgeVersionEntry(string Version,string Branch,string Inherit)
        {
            // 司马版本的特殊处理
            if(Version == "11.15.1.2318" || Version == "11.15.1.1902" || Version == "11.15.1.1890")
            {
                Branch = "1.8.9";
            }
            if (Branch == null && Inherit == "1.7.10" && Convert.ToInt16(Version.Split('.')[3]) > 1300)
            {
                Branch = "1.7.10";
            }
            IsNeoForge = false;
            VersionName = Version;
            this.Version = new Version(Version);
            this.Inherit = Inherit;
            FileVersion = $"{Version}{(string.IsNullOrEmpty(Branch) ? "" : $"-{Branch}")}";
        }
    }
}
