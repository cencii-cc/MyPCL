using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPCL.Modules.Minecraft
{
    public enum CompType
    {
        /// <summary>
        /// Mod。
        /// </summary>
        Mod = 0,
        /// <summary>
        /// 整合包。
        /// </summary>
        ModPack = 1,
        /// <summary>
        /// 资源包。
        /// </summary>
        ResourcePack = 2,
    }

    /// <summary>
    /// https://docs.curseforge.com/?http#tocS_ModLoaderType
    /// </summary>
    public enum CompModLoaderType
    {
        Any = 0,
        Forge = 1,
        LiteLoader = 3,
        Fabric = 4,
        Quilt = 5,
        NeoForge = 6
    }


    #region CompFile | 文件信息

    // 类定义

    public enum ComFileStatus
    {
        //枚举值来源：https://docs.curseforge.com/#tocS_FileReleaseType
        Release = 1, 
        Beta = 2,
        Alpha = 3
    }


    public class ModComp
    {
        // 源信息

        /// <summary>
        /// 文件的种类。
        /// </summary>
        public readonly CompType Type;

        /// <summary>
        /// 该文件来自 CurseForge 还是 Modrinth。
        /// </summary>
        public readonly bool FromCurseForge;

        /// <summary>
        /// 用于唯一性鉴别该文件的 ID。CurseForge 中为 123456 的大整数，Modrinth 中为英文乱码的 Version 字段。
        /// </summary>
        public readonly string Id;

        // 描述性信息

        public string DisplayName;

        /// <summary>
        /// 发布时间。
        /// </summary>
        public readonly DateTime ReleaseDate;

        /// <summary>
        /// 下载量计数。注意，该计数仅为一个来源，无法反应两边加起来的下载量，且 CurseForge 可能错误地返回 0。
        /// </summary>
        public readonly int DownloadCount;

        /// <summary>
        /// 支持的 Mod 加载器列表。可能为空。
        /// </summary>
        public readonly List<CompModLoaderType> ModLoaders;

        /// <summary>
        /// 支持的游戏版本列表。类型包括："1.18.5"，"1.18"，"1.18 预览版"，"21w15a"，"未知版本"。
        /// </summary>
        public readonly List<string> GameVersions;

        /// <summary>
        /// 发布状态：Release/Beta/Alpha。
        /// </summary>
        public readonly ComFileStatus Status;

        /// <summary>
        /// 发布状态的友好描述。例如："正式版"，"Beta 版"。
        /// </summary>
        public string StatusDescription
        {
            get
            {
                switch (Status)
                {
                    case ComFileStatus.Release:
                        return "正式版";
                    case ComFileStatus.Beta:
                        return ModBase.ModeDebug ? "Beta 版" : "测试版";
                    case ComFileStatus.Alpha:
                        return ModBase.ModeDebug ? "Alpha 版" : "测试版";
                    default:
                        return "未知版本";
                }
            }
        }

        /// <summary>
        /// 下载信息是否可用。
        /// </summary>
        public bool Available
        {
            get
            {
                return !string.IsNullOrEmpty(FileName) && DownloadUrls != null;
            }
        }

        /// <summary>
        /// 下载的文件名。
        /// </summary>
        public readonly string FileName = null;

        /// <summary>
        /// 文件所有可能的下载源。
        /// </summary>
        public List<string> DownloadUrls;

        /// <summary>
        /// 文件的 SHA1 或 MD5。
        /// </summary>
        public string Hash = null;

        /// <summary>
        /// 该文件的所有依赖工程的原始 ID。
        /// <br/>
        /// 这些 ID 可能没有加载，在加载后会添加到 Dependencies 中（主要是因为 Modrinth 返回的是字符串 ID 而非 Slug，导致 Project.Id 查询不到）。
        /// </summary>
        public readonly List<string> RawDependencies = new List<string>();

        /// <summary>
        /// 该文件的所有依赖工程的 Project.Id。
        /// </summary>
        public readonly List<string> Dependencies = new List<string>();
    }
    #endregion

}
