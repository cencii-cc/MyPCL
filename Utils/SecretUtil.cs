using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MyPCL.Utils
{
    public class SecretUtil
    {
        /// <summary>
        /// 在开源版的注册表与常规版的注册表隔离，以防数据冲突
        /// </summary>
        public const string RegFolder = "PCLDebug";

        public const string OAuthClientId = "";

        /// <summary>
        /// 获取八位密钥。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string SecretKeyGet(string key)
        {
            return "00000000";
        }

        /// <summary>
        /// 加密字符串。
        /// </summary>
        /// <param name="SourceString"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static string SecretEncrypt(string SourceString,string Key = "")
        {
            Key = SecretKeyGet(Key);
            var btKey = Encoding.UTF8.GetBytes(Key);
            var btIV = Encoding.UTF8.GetBytes("87160295");
            var des = new DESCryptoServiceProvider();
            using (MemoryStream MS = new MemoryStream())
            {
                var inData = Encoding.UTF8.GetBytes(SourceString);
                using (CryptoStream cs = new CryptoStream(MS, des.CreateEncryptor(btKey, btIV), CryptoStreamMode.Write))
                {
                    cs.Write(inData, 0, inData.Length);
                    cs.FlushFinalBlock();
                    return Convert.ToBase64String(MS.ToArray());
                }
            }
        }

        /// <summary>
        /// 获取设备标识码。
        /// </summary>
        /// <returns></returns>
        public static string SecretGetUniqueAddress()
        {
            return "0000-0000-0000-0000";
        }
    }
}
