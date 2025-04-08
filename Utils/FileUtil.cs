using Microsoft.Win32;
using MyPCL.Modules;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using File = System.IO.File;
using static MyPCL.Modules.ModBase;
using static MyPCL.Utils.StringUtil;

namespace MyPCL.Utils
{
    /// <summary>
    /// 文件操作工具类
    /// </summary>
    public static class FileUtil
    {

        private static readonly SafeDictionary<string, SafeDictionary<string, string>> IniCache = new SafeDictionary<string, SafeDictionary<string, string>>();

        /// <summary>
        /// 还原文件路径。如果文件路径以 :\ 开头，则直接返回。否则，使用“ApplicationName\文件名.ini”作为路径。
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        public static string RestoreFilePath(string FilePath)
        {
            if (FilePath.Contains(":\\"))
            {
                return FilePath;
            }
            else
            {
                return $"{ModBase.Path}\\{FilePath}";
            }
        }

        /// ----------------------------------
        ///  Ini 文件操作
        /// ----------------------------------

        private static object WriteIniLock = new object();

        /// <summary>
        /// 清除某 ini 文件的运行时缓存。
        /// </summary>
        /// <param name="FileName">文件完整路径或简写文件名。简写将会使用“ApplicationName\文件名.ini”作为路径。</param>
        public static void IniClearCache(string FileName)
        {
            FileName = RestoreFilePath(FileName);
            if (IniCache.ContainsKey(FileName))
            {
                IniCache.Remove(FileName);
            }
        }

        /// <summary>
        /// 读取 ini 文件。这可能会使用到缓存。
        /// </summary>
        /// <param name="FileName">文件完整路径或简写文件名。简写将会使用“ApplicationName\文件名.ini”作为路径。</param>
        /// <param name="Key">键。</param>
        /// <param name="DefaultValue">没有找到键时返回的默认值。</param>
        /// <returns></returns>
        public static string ReadIni(string FileName,string Key,string DefaultValue = "")
        {
            var content = IniGetContent(FileName);
            if (content == null || !content.ContainsKey(Key))
            {
                return DefaultValue;
            }
            else
            {
                return content[Key];
            }
        }

        /// <summary>
        /// 获取 ini 文件缓存。如果没有，则新读取 ini 文件内容。<br/>
        /// 在文件不存在或读取失败时返回 Nothing。
        /// </summary>
        /// <param name="FileName">文件完整路径或简写文件名。简写将会使用“ApplicationName\文件名.ini”作为路径。</param>
        /// <returns></returns>
        private static SafeDictionary<string,string> IniGetContent(string FileName)
        {
            try
            {
                // 还原文件路径
                FileName = RestoreFilePath(FileName);
                // 检索缓存
                if (IniCache.ContainsKey(FileName))
                {
                    return IniCache[FileName];
                }
                // 读取文件
                if (!File.Exists(FileName))
                {
                    return null;
                }
                SafeDictionary<string, string> Ini = new SafeDictionary<string, string>();
                string[] lines = ReadFile(FileName).Split(Environment.NewLine.ToArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    int index = line.IndexOf(':');
                    if(index > 0)
                    {
                        // 可能会有重复键，见 #3616
                        Ini[line.Substring(0, index)] = line.Substring(index + 1);
                    }
                }
                IniCache[FileName] = Ini;
                return Ini;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        /// <summary>
        /// 写入 ini 文件，这会更新缓存。<br/>
        /// 若 Value 为 Nothing，则删除该键。
        /// </summary>
        /// <param name="fileName">文件完整路径或简写文件名。简写将会使用“ApplicationName\文件名.ini”作为路径。</param>
        /// <param name="key">键。</param>
        /// <param name="value">值。</param>
        public static void WriteIni(string fileName,string key,string value)
        {
            try
            {
                // 预处理 
                if (key.Contains(":")) throw new Exception($"尝试写入 ini 文件 {fileName} 的键名中包含了冒号：{key}");
                key = key.Replace("\r", "").Replace("\n", "");
                value = value?.Replace("\r", "").Replace("\n", "");
                // 防止争用
                lock (WriteIniLock)
                {
                    // 获取目前文件
                    var content = IniGetContent(fileName);
                    if (content == null) content = new SafeDictionary<string, string>();
                    // 更新值
                    if(value == null)
                    {
                        // 无需处理
                        if (!content.ContainsKey(key)) return;
                        content.Remove(key);
                    }
                    else
                    {
                        // 无需处理
                        if (content.ContainsKey(key) && content[key] == value) return;
                        content[key] = value;
                    }
                    // 写入文件
                    var fileContent = new StringBuilder();
                    foreach (var item in content)
                    {
                        fileContent.AppendLine($"{item.Key}:{item.Value}");
                    }
                    fileName = RestoreFilePath(fileName);
                    WriteFile(fileName, fileContent.ToString());
                }
            }
            catch(Exception ex)
            {
                // Log(ex, $"写入文件失败（{FileName} → {Key}:{Value}）", LogLevel.Hint)
            }
        }


        /// ------------------------------------
        /// 文件操作
        /// ------------------------------------


        /// <summary>
        /// 读取文件，如果失败则返回空字符串。
        /// </summary>
        /// <param name="FilePath"></param>
        /// <param name="Encoding"></param>
        /// <returns></returns>
        public static string ReadFile(string FilePath,Encoding Encoding = null)
        {
            string ReadFile = "";
            var fileBytes = ReadFileBytes(FilePath, Encoding);

            if(Encoding == null)
            {
                ReadFile = DecodeBytes(fileBytes);
            }
            else
            {
                Encoding.GetString(fileBytes);
            }
            return ReadFile;
        }

        /// <summary>
        /// 写入文件。
        /// </summary>
        /// <param name="filePath">文件完整或相对路径。</param>
        /// <param name="text">文件内容。</param>
        /// <param name="append">是否将文件内容追加到当前文件，而不是覆盖它。</param>
        /// <param name="encoding"></param>
        public static void WriteFile(string filePath,string text,bool append = false,Encoding encoding = null)
        {
            // 处理相对路径
            filePath = RestoreFilePath(filePath);
            // 确保目录存在
            Directory.CreateDirectory(GetPathFromFullPath(filePath)); // Path.GetDirectoryName(filePath)
            // 写入文件
            if (append)
            {
                // 追加目前文件
                using (var writer = new StreamWriter(filePath, true, encoding ?? GetEncoding(ReadFileBytes(filePath))))
                {
                    writer.Write(text);
                    writer.Flush();
                    writer.Close();
                }
            }
            else
            {
                // 直接写入字节
                System.IO.File.WriteAllBytes(filePath, (encoding == null ? new UTF8Encoding(false).GetBytes(text) : encoding.GetBytes(text)));
            }
        }


        /// <summary>
        /// 根据字节数组分析其编码。
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Encoding GetEncoding(byte[] bytes)
        {
            int length = bytes.Length;
            if(length < 3) return new UTF8Encoding(false); // 不带 BOM 的 UTF8
            // 根据 BOM 判断编码
            if (bytes[0] >= 0xEF)
            {
                // 有 BOM 类型
                if (bytes[0] == 0xEF && bytes[1] == 0xBB) return new UTF8Encoding(true); //带 BOM 的 UTF8
                else if(bytes[0] == 0xFE && bytes[1] == 0xFF) return Encoding.BigEndianUnicode;
                else if (bytes[0] == 0xFF && bytes[1] == 0xFE) return Encoding.Unicode;
            }
            // 无 BOM 文件：GB18030（ANSI）或 UTF8
            string utf8String = Encoding.UTF8.GetString(bytes);
            char errorChar = Encoding.UTF8.GetString(new byte[] { 239, 191, 189 })[0];
            if (utf8String.Contains(errorChar))
            {
                return Encoding.GetEncoding("GB18030");
            }
            else
            {
                return new UTF8Encoding(false); // 不带 BOM 的 UTF8
            }
        }


        /// <summary>
        /// 读取文件，如果失败则返回空数组。
        /// </summary>
        /// <param name="FilePath"></param>
        /// <param name="Encoding"></param>
        /// <returns></returns>
        public static byte[] ReadFileBytes(string FilePath,Encoding Encoding = null)
        {
            try
            {
                // 还原文件路径
                FilePath = RestoreFilePath(FilePath);
                if (File.Exists(FilePath))
                {
                    byte[] FileBytes;
                    // 支持读取使用中的文件
                    using (FileStream fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read,FileShare.ReadWrite))
                    {
                        FileBytes = new byte[fileStream.Length];
                        fileStream.Read(FileBytes, 0, (int)fileStream.Length);
                    }
                    return FileBytes;
                }
                // Log("[System] 欲读取的文件不存在，已返回空内容：" & FilePath)
                return new byte[0];
            }
            catch(Exception ex)
            {
                // Log(ex, "读取文件出错：" & FilePath)
            }
            return new byte[0];
        }

        /// <summary>
        /// 获取图片的缓存地址
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetTempPath(string url)
        {
            return $"{PathTemp}MyImage\\{GetHash(url)}.png";
        }

        /// <summary>   
        /// 解码 Bytes。
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string DecodeBytes(byte[] bytes)
        {
            int Length = bytes.Length;
            if(Length < 3)
            {
                return Encoding.UTF8.GetString(bytes);
            }
            // 根据 BOM 判断编码
            if (bytes[0] >= 0xEF)
            {
                // 有 BOM 类型
                if (bytes[0] == 0xEF && bytes[1] == 0xBB)
                {
                    return Encoding.UTF8.GetString(bytes, 3, Length - 3);
                }
                else if (bytes[0] == 0xFE && bytes[1] == 0xFF)
                {
                    return Encoding.BigEndianUnicode.GetString(bytes, 3, Length - 3);
                }
                else if (bytes[0] == 0xFF && bytes[1] == 0xFE)
                {
                    return Encoding.Unicode.GetString(bytes, 3, Length - 3);
                }
                else
                {
                    return Encoding.GetEncoding("GB18030").GetString(bytes, 3, Length - 3);
                }
            }
            // 无 BOM 文件：GB18030（ANSI）或 UTF8
            var UTF8 = Encoding.UTF8.GetString(bytes);
            var ErrorChar = Encoding.UTF8.GetString(new byte[] { 239, 191, 189 }).ToCharArray()[0];
            if (UTF8.Contains(ErrorChar))
            {
                return Encoding.GetEncoding("GB18030").GetString(bytes);
            }
            else
            {
                return UTF8;
            }
        }

        /// <summary>
        /// 读取、写入、复制文件
        /// </summary>
        /// <param name="FromPath"></param>
        /// <param name="ToPath"></param>
        public static void CopyFile(string FromPath,string ToPath)
        {
            try
            {
                // 还原文件路径
                if (!FromPath.Contains(":\\"))
                {
                    FromPath = ModBase.Path + FromPath;
                }
                if (!ToPath.Contains(":\\"))
                {
                    ToPath = ModBase.Path + ToPath;
                }
                // 如果复制同一个文件则跳过
                if (FromPath == ToPath) return;
                // 确保目录存在
                Directory.CreateDirectory(GetPathFromFullPath(ToPath));
                // 复制文件
                File.Copy(FromPath, ToPath, true);
            }
            catch (Exception ex) 
            { 
                throw new Exception($"复制文件出错:{FromPath}→{ToPath}",ex);
            }
        }

        /// <summary>
        /// 从文件路径或者 Url 获取不包含文件名的路径，或获取文件夹的父文件夹路径。<br/>
        /// 取决于原路径格式，路径以 / 或 \ 结尾。<br/>
        /// 不包含路径将会抛出异常。
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        public static string GetPathFromFullPath(string FilePath)
        {
            string returnFilePath = "";
            char[] fileSymbols = { '\\', '/' };
            if (!FilePath.Contains("\\") && !FilePath.Contains("/"))
                throw new Exception($"不包含路径：{FilePath}");
            if(FilePath.EndsWith("\\") || FilePath.EndsWith("/"))
            {
                // 是文件夹的路劲
                bool IsRight = FilePath.EndsWith("\\");
                FilePath = FilePath.Substring(0, FilePath.Length - 1);

                returnFilePath = FilePath.Substring(0, FilePath.LastIndexOfAny(fileSymbols)) + (IsRight ? fileSymbols[0] : fileSymbols[1]);
                if(returnFilePath.Length <= 1)
                {
                    throw new Exception($"不包含路径：{FilePath}");
                }
            }
            else
            {
                // 是文件路径
                returnFilePath = FilePath.Substring(0, FilePath.LastIndexOfAny(fileSymbols));
                if (returnFilePath == "")
                {
                    throw new Exception($"不包含路径：{FilePath}");
                }
            }
            return returnFilePath;
        }

        ///----------------------------
        /// 注册表相关
        ///----------------------------

        /// <summary>
        /// 读取注册表键。如果失败则返回默认值。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string ReadReg(string key,string defaultValue)
        {
            try
            {
                RegistryKey subKey = Registry.CurrentUser.OpenSubKey($"Software\\{SecretUtil.RegFolder}", false);
                object value = subKey?.GetValue(key);
                return value != null ? value.ToString() : defaultValue;
            }
            catch (Exception ex)
            {
                // Log(ex, $"读取注册表出错：{key}", LogLevel.Hint);
                return defaultValue;
            }
        }

        /// <summary>
        /// 写入注册表键。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="throwException"></param>
        public static void WriteReg(string key,string value,bool throwException = false)
        {
            try
            {
                RegistryKey subKey = Registry.CurrentUser.OpenSubKey($"Software\\{SecretUtil.RegFolder}", true);
                // 如果没有就创建
                if (subKey == null)
                {
                    subKey = Registry.CurrentUser.CreateSubKey($"Software\\{SecretUtil.RegFolder}");
                }
                subKey?.SetValue(key, value);
            }
            catch (Exception ex)
            {
                if (throwException)
                {
                    throw new Exception($"写入注册表出错：{key}→{value}", ex);
                }
                else
                {
                    LogUtil.Log(ex, $"写入注册表出错：{key}→{value}", LogLevel.Hint);
                }
            }
        }
    }
}
