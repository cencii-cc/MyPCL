using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPCL.Modules.Minecraft
{
    public enum McFolderType
    {
        Original,
        RenamedOriginal,
        Custom,
    }

    public class McFolder
    {
        public string Name;

        public string Path;

        public McFolderType Type;

        public override bool Equals(object obj)
        {
            if (!(obj is McFolder)) return false;
            McFolder folder = obj as McFolder;
            return (Name == folder.Name && Path == folder.Path && Type == folder.Type);
        }

        public override string ToString()
        {
            return Path;
        }
    }

    public static class ModMinecraft
    {
        /// <summary>
        /// 当前的 Minecraft 文件夹路径，以“\”结尾。
        /// </summary>
        public static string PathMcFolder;

        /// <summary>
        /// 当前的 Minecraft 文件夹列表。
        /// </summary>
        public static List<McFolder> McFolderList = new List<McFolder>(); 
    }
}
