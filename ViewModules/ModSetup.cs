using MyPCL.Modules;
using MyPCL.Modules.Minecraft;
using MyPCL.Utils;
using System;
using System.Collections.Generic;
using static MyPCL.Utils.SecretUtil;

namespace MyPCL.ViewModules
{
    public enum SetupSource
    {
        Normal,
        Registry,
        Version,
    }

    public class ModSetup
    {
        /// <summary>
        /// 设置的更新号。
        /// </summary>
        public const int VersionSetup = 1;

        private readonly Dictionary<string, SetupEntry> SetupDict = new Dictionary<string, SetupEntry>{
    {"Identify", new SetupEntry("", source: SetupSource.Registry)},
    {"WindowHeight", new SetupEntry(550)},
    {"WindowWidth", new SetupEntry(900)},
    {"HintDownloadThread", new SetupEntry(false, source: SetupSource.Registry)},
    {"HintNotice", new SetupEntry(0, source: SetupSource.Registry)},
    {"HintDownload", new SetupEntry(0, source: SetupSource.Registry)},
    {"HintInstallBack", new SetupEntry(false, source: SetupSource.Registry)},
    {"HintHide", new SetupEntry(false, source: SetupSource.Registry)},
    {"HintHandInstall", new SetupEntry(false, source: SetupSource.Registry)},
    {"HintBuy", new SetupEntry(false, source: SetupSource.Registry)},
    {"HintClearRubbish", new SetupEntry(0, source: SetupSource.Registry)},
    {"HintUpdateMod", new SetupEntry(false, source: SetupSource.Registry)},
    {"HintCustomCommand", new SetupEntry(false, source: SetupSource.Registry)},
    {"HintMoreAdvancedSetup", new SetupEntry(false, source: SetupSource.Registry)},
    {"HintIndieSetup", new SetupEntry(false, source: SetupSource.Registry)},
    {"HintExportConfig", new SetupEntry(false, source: SetupSource.Registry)},
    {"SystemEula", new SetupEntry(false, source: SetupSource.Registry)},
    {"SystemCount", new SetupEntry(0, source: SetupSource.Registry, encoded: true)},
    {"SystemLaunchCount", new SetupEntry(0, source: SetupSource.Registry, encoded: true)},
    {"SystemLastVersionReg", new SetupEntry(0, source: SetupSource.Registry, encoded: true)},
    {"SystemHighestSavedBetaVersionReg", new SetupEntry(0, source: SetupSource.Registry, encoded: true)},
    {"SystemHighestBetaVersionReg", new SetupEntry(0, source: SetupSource.Registry, encoded: true)},
    {"SystemHighestAlphaVersionReg", new SetupEntry(0, source: SetupSource.Registry, encoded: true)},
    {"SystemSetupVersionReg", new SetupEntry(VersionSetup, source: SetupSource.Registry)},
    {"SystemSetupVersionIni", new SetupEntry(VersionSetup)},
    {"SystemHelpVersion", new SetupEntry(0, source: SetupSource.Registry)},
    {"SystemDebugMode", new SetupEntry(false, source: SetupSource.Registry)},
    {"SystemDebugAnim", new SetupEntry(9, source: SetupSource.Registry)},
    {"SystemDebugDelay", new SetupEntry(false, source: SetupSource.Registry)},
    {"SystemDebugSkipCopy", new SetupEntry(false, source: SetupSource.Registry)},
    {"SystemSystemCache", new SetupEntry("", source: SetupSource.Registry)},
    {"SystemSystemUpdate", new SetupEntry(0)},
    {"SystemSystemActivity", new SetupEntry(0)},
    {"CacheExportConfig", new SetupEntry("", source: SetupSource.Registry)},
    {"CacheSavedPageUrl", new SetupEntry("", source: SetupSource.Registry)},
    {"CacheSavedPageVersion", new SetupEntry("", source: SetupSource.Registry)},
    {"CacheMsOAuthRefresh", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"CacheMsAccess", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"CacheMsProfileJson", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"CacheMsUuid", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"CacheMsName", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"CacheMsV2Migrated", new SetupEntry(false, source: SetupSource.Registry)},
    {"CacheMsV2OAuthRefresh", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"CacheMsV2Access", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"CacheMsV2ProfileJson", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"CacheMsV2Uuid", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"CacheMsV2Name", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"CacheNideAccess", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"CacheNideClient", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"CacheNideUuid", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"CacheNideName", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"CacheNideUsername", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"CacheNidePass", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"CacheNideServer", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"CacheAuthAccess", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"CacheAuthClient", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"CacheAuthUuid", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"CacheAuthName", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"CacheAuthUsername", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"CacheAuthPass", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"CacheAuthServerServer", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"CacheAuthServerName", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"CacheAuthServerRegister", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"CacheDownloadFolder", new SetupEntry("", source: SetupSource.Registry)},
    {"CacheJavaListVersion", new SetupEntry(0, source: SetupSource.Registry)},
    {"LoginRemember", new SetupEntry(true, source: SetupSource.Registry, encoded: true)},
    {"LoginLegacyName", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"LoginMsJson", new SetupEntry("{}", source: SetupSource.Registry, encoded: true)}, // '{UserName: OAuthToken, ...}
    {"LoginNideEmail", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"LoginNidePass", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"LoginAuthEmail", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"LoginAuthPass", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"LoginType", new SetupEntry(McLoginType.Legacy, source: SetupSource.Registry)},
    {"LoginPageType", new SetupEntry(0)},
    {"LaunchSkinID", new SetupEntry("", source: SetupSource.Registry)},
    {"LaunchSkinType", new SetupEntry(0, source: SetupSource.Registry)},
    {"LaunchSkinSlim", new SetupEntry(false, source: SetupSource.Registry)},
    {"LaunchVersionSelect", new SetupEntry("")},
    {"LaunchFolderSelect", new SetupEntry("")},
    {"LaunchFolders", new SetupEntry("", source: SetupSource.Registry)},
    {"LaunchArgumentTitle", new SetupEntry("")},
    {"LaunchArgumentInfo", new SetupEntry("PCL")},
    {"LaunchArgumentJavaSelect", new SetupEntry("", source: SetupSource.Registry)},
    {"LaunchArgumentJavaAll", new SetupEntry("[]", source: SetupSource.Registry)},
    {"LaunchArgumentIndie", new SetupEntry(0)},
    {"LaunchArgumentIndieV2", new SetupEntry(4)},
    {"LaunchArgumentVisible", new SetupEntry(5, source: SetupSource.Registry)},
    {"LaunchArgumentPriority", new SetupEntry(1, source: SetupSource.Registry)},
    {"LaunchArgumentWindowWidth", new SetupEntry(854)},
    {"LaunchArgumentWindowHeight", new SetupEntry(480)},
    {"LaunchArgumentWindowType", new SetupEntry(1)},
    {"LaunchArgumentRam", new SetupEntry(false, source: SetupSource.Registry)},
    {"LaunchAdvanceJvm", new SetupEntry("-Dfile.encoding=UTF-8 -Dstdout.encoding=UTF-8 -Dstderr.encoding=UTF-8 -XX:+UseG1GC -XX:-UseAdaptiveSizePolicy -XX:-OmitStackTraceInFastThrow -Djdk.lang.Process.allowAmbiguousCommands=true -Dfml.ignoreInvalidMinecraftCertificates=True -Dfml.ignorePatchDiscrepancies=True -Dlog4j2.formatMsgNoLookups=true")},
    {"LaunchAdvanceGame", new SetupEntry("")},
    {"LaunchAdvanceRun", new SetupEntry("")},
    {"LaunchAdvanceRunWait", new SetupEntry(true)},
    {"LaunchAdvanceDisableJLW", new SetupEntry(false)},
    {"LaunchAdvanceGraphicCard", new SetupEntry(true, source: SetupSource.Registry)},
    {"LaunchRamType", new SetupEntry(0)},
    {"LaunchRamCustom", new SetupEntry(15)},
    {"LinkEula", new SetupEntry(false, source: SetupSource.Registry)},
    {"LinkName", new SetupEntry("", source: SetupSource.Registry)},
    {"LinkHiperCertLast", new SetupEntry("", source: SetupSource.Registry)},
    {"LinkHiperCertTime", new SetupEntry("", source: SetupSource.Registry)},
    {"LinkHiperCertWarn", new SetupEntry(true, source: SetupSource.Registry)},
    {"LinkIoiVersion", new SetupEntry(0, source: SetupSource.Registry, encoded: true)},
    {"ToolHelpChinese", new SetupEntry(true, source: SetupSource.Registry)},
    {"ToolDownloadThread", new SetupEntry(63, source: SetupSource.Registry)},
    {"ToolDownloadSpeed", new SetupEntry(42, source: SetupSource.Registry)},
    {"ToolDownloadVersion", new SetupEntry(0, source: SetupSource.Registry)},
    {"ToolDownloadTranslate", new SetupEntry(0, source: SetupSource.Registry)},
    {"ToolDownloadTranslateV2", new SetupEntry(1, source: SetupSource.Registry)},
    {"ToolDownloadIgnoreQuilt", new SetupEntry(true, source: SetupSource.Registry)},
    {"ToolDownloadCert", new SetupEntry(false, source: SetupSource.Registry)},
    {"ToolDownloadMod", new SetupEntry(1, source: SetupSource.Registry)},
    {"ToolModLocalNameStyle", new SetupEntry(0, source: SetupSource.Registry)},
    {"ToolUpdateAlpha", new SetupEntry(0, source: SetupSource.Registry, encoded: true)},
    {"ToolUpdateRelease", new SetupEntry(false, source: SetupSource.Registry)},
    {"ToolUpdateSnapshot", new SetupEntry(false, source: SetupSource.Registry)},
    {"ToolUpdateReleaseLast", new SetupEntry("", source: SetupSource.Registry)},
    {"ToolUpdateSnapshotLast", new SetupEntry("", source: SetupSource.Registry)},
    {"UiLauncherTransparent", new SetupEntry(600)}, // 避免与 PCL1 设置冲突（UiLauncherOpacity）
    {"UiLauncherHue", new SetupEntry(180)},
    {"UiLauncherSat", new SetupEntry(80)},
    {"UiLauncherDelta", new SetupEntry(90)},
    {"UiLauncherLight", new SetupEntry(20)},
    {"UiLauncherTheme", new SetupEntry(0)},
    {"UiLauncherThemeGold", new SetupEntry("", source: SetupSource.Registry, encoded: true)},
    {"UiLauncherThemeHide", new SetupEntry("0|1|2|3|4", source: SetupSource.Registry, encoded: true)},
    {"UiLauncherThemeHide2", new SetupEntry("0|1|2|3|4", source: SetupSource.Registry, encoded: true)},
    {"UiLauncherLogo", new SetupEntry(true)},
    {"UiLauncherEmail", new SetupEntry(false)},
    {"UiBackgroundColorful", new SetupEntry(true)},
    {"UiBackgroundOpacity", new SetupEntry(1000)},
    {"UiBackgroundBlur", new SetupEntry(0)},
    {"UiBackgroundSuit", new SetupEntry(0)},
    {"UiCustomType", new SetupEntry(0)},
    {"UiCustomPreset", new SetupEntry(0)},
    {"UiCustomNet", new SetupEntry("")},
    {"UiLogoType", new SetupEntry(1)},
    {"UiLogoText", new SetupEntry("")},
    {"UiLogoLeft", new SetupEntry(false)},
    {"UiMusicVolume", new SetupEntry(500)},
    {"UiMusicStop", new SetupEntry(false)},
    {"UiMusicStart", new SetupEntry(false)},
    {"UiMusicRandom", new SetupEntry(true)},
    {"UiMusicAuto", new SetupEntry(true)},
    {"UiHiddenPageDownload", new SetupEntry(false)},
    {"UiHiddenPageLink", new SetupEntry(false)},
    {"UiHiddenPageSetup", new SetupEntry(false)},
    {"UiHiddenPageOther", new SetupEntry(false)},
    {"UiHiddenFunctionSelect", new SetupEntry(false)},
    {"UiHiddenFunctionModUpdate", new SetupEntry(false)},
    {"UiHiddenFunctionHidden", new SetupEntry(false)},
    {"UiHiddenSetupLaunch", new SetupEntry(false)},
    {"UiHiddenSetupUi", new SetupEntry(false)},
    {"UiHiddenSetupLink", new SetupEntry(false)},
    {"UiHiddenSetupSystem", new SetupEntry(false)},
    {"UiHiddenOtherHelp", new SetupEntry(false)},
    {"UiHiddenOtherFeedback", new SetupEntry(false)},
    {"UiHiddenOtherVote", new SetupEntry(false)},
    {"UiHiddenOtherAbout", new SetupEntry(false)},
    {"UiHiddenOtherTest", new SetupEntry(false)},
    {"VersionAdvanceJvm", new SetupEntry("", source: SetupSource.Version)},
    {"VersionAdvanceGame", new SetupEntry("", source: SetupSource.Version)},
    {"VersionAdvanceAssets", new SetupEntry(0, source: SetupSource.Version)},
    {"VersionAdvanceAssetsV2", new SetupEntry(false, source: SetupSource.Version)},
    {"VersionAdvanceJava", new SetupEntry(false, source: SetupSource.Version)},
    {"VersionAdvanceRun", new SetupEntry("", source: SetupSource.Version)},
    {"VersionAdvanceRunWait", new SetupEntry(true, source: SetupSource.Version)},
    {"VersionAdvanceDisableJLW", new SetupEntry(false, source: SetupSource.Version)},
    {"VersionRamType", new SetupEntry(2, source: SetupSource.Version)},
    {"VersionRamCustom", new SetupEntry(15, source: SetupSource.Version)},
    {"VersionRamOptimize", new SetupEntry(0, source: SetupSource.Version)},
    {"VersionArgumentTitle", new SetupEntry("", source: SetupSource.Version)},
    {"VersionArgumentInfo", new SetupEntry("", source: SetupSource.Version)},
    {"VersionArgumentIndie", new SetupEntry(-1, source: SetupSource.Version)},
    {"VersionArgumentIndieV2", new SetupEntry(false, source: SetupSource.Version)},
    {"VersionArgumentJavaSelect", new SetupEntry("使用全局设置", source: SetupSource.Version)},
    {"VersionServerEnter", new SetupEntry("", source: SetupSource.Version)},
    {"VersionServerLogin", new SetupEntry(0, source: SetupSource.Version)},
    {"VersionServerNide", new SetupEntry("", source: SetupSource.Version)},
    {"VersionServerAuthRegister", new SetupEntry("", source: SetupSource.Version)},
    {"VersionServerAuthName", new SetupEntry("", source: SetupSource.Version)},
    {"VersionServerAuthServer", new SetupEntry("", source: SetupSource.Version)}
};

        /// <summary>
        /// 改变某个设置项的值。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="forceReload"></param>
        /// <param name="version"></param>
        public void Set(string key,object value, bool forceReload = false, McVersion version = null)
        {
            Set(key, value, SetupDict[key], forceReload, version);
        }

        public void Set(string key, object value, SetupEntry E, bool forceReload, McVersion version)
        {
            try
            {
                value = Convert.ChangeType(value, E.Type);
                if(E.State == 2)
                {
                    // 如果已应用，且值相同，则无需再次更改
                    if (E.Value == value && !forceReload) return;
                }
                else
                {
                    // 如果未应用，则直接更改并应用
                    if (E.Source != SetupSource.Version) E.State = 2;
                }
                // 设置新值
                E.Value = value;
                // 写入值
                if (E.Encoded)
                {
                    try
                    {
                        if (value == null) value = "";
                        // 加密
                        value = SecretUtil.SecretEncrypt((string)value, ModBase.UniqueAddress);
                    }
                    catch (Exception ex)
                    {
                        // Log(ex, "加密设置失败：" & Key, LogLevel.Developer)
                    }
                }
                switch (E.Source)
                {
                    case SetupSource.Normal:
                        FileUtil.WriteIni("Setup", key, value.ToString());
                        break;
                    case SetupSource.Registry:
                        FileUtil.WriteReg(key, value.ToString());
                        break;
                    case SetupSource.Version:
                        if (version == null) throw new Exception($"更改版本设置 {key} 时未提供目标版本");
                        FileUtil.WriteIni($"{version.Path}PCL\\Setup.ini", key, value.ToString());
                        break;
                }
                // 应用
                // 例如 VersionServerLogin 要求在设置之后再引发事件
                var method = GetType().GetMethod(key);
                if (method != null)
                {
                    method.Invoke(this, new object[] { value });
                }
            }
            catch (Exception ex)
            {
                LogUtil.Log(ex, $"设置设置项时出错({key},{value})", LogLevel.Feedback);
            }
        }

        /// <summary>
        /// 获取某个设置项的值。
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Version"></param>
        /// <returns></returns>
        public object Get(string Key,McVersion Version = null)
        {
            if (!SetupDict.ContainsKey(Key))
            {
                var ex =  new KeyNotFoundException($"未找到设置项：{Key}");
                ex.Source = Key;
                throw ex;
            }
            return Get(Key, SetupDict[Key], Version);
        }

        public object Get(string Key,SetupEntry E,McVersion Version)
        {
            // 获取强制值
            var Force = ForceValue(Key);
            if (!string.IsNullOrEmpty(Force))
            {
                E.Value = Convert.ChangeType(Force,E.Type);
                E.State = 1;
            }
            // 如果尚未读取过，则读取
            if(E.State == 0)
            {
                Read(Key, ref E, Version);
                if (E.Source != SetupSource.Version) E.State = 1;
            }
            return E.Value;
        }

        /// <summary>
        /// 读取设置。
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="E"></param>
        /// <param name="Version"></param>
        private void Read(string Key, ref SetupEntry E, McVersion Version)
        {
            try
            {
                if(E.State == 0) return;
                //先用 String 储存，避免类型转换
                string SourceValue = null;
                switch (E.Source)
                {
                    case SetupSource.Normal:
                        SourceValue = FileUtil.ReadIni("Setup", Key, E.DefaultValueEncoded.ToString());
                        break;
                    case SetupSource.Registry:
                        SourceValue = FileUtil.ReadReg(Key, E.DefaultValueEncoded.ToString());
                        break;
                    case SetupSource.Version:
                        if(Version == null)
                        {
                            throw new Exception($"读取版本设置 {Key} 时未提供目标版本");
                        }
                        SourceValue = FileUtil.ReadIni($"{Version.Path}PCL\\Setup.ini", Key, E.DefaultValueEncoded.ToString());
                        break;
                }
                if (E.Encoded)
                {
                    if (SourceValue.Equals(E.DefaultValueEncoded))
                    {
                        SourceValue = E.DefaultValue.ToString();
                    }
                    else
                    {
                        try
                        {
                            SourceValue = SecretEncrypt(SourceValue,$"PCL{ModBase.UniqueAddress}");
                        }
                        catch (Exception ex)
                        {
                            // Log(ex, "解密设置失败：" & Key, LogLevel.Developer)
                            SourceValue = (string)E.DefaultValue;
                            ModBase.Setup.Set(Key, E.DefaultValue, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.Log(ex, $"读取设置项时出错({Key})", LogLevel.Feedback);
                E.Value = Convert.ChangeType(E.DefaultValue, E.Type);
            }
        }

        /// <summary>
        /// 对部分设置强制赋值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string ForceValue(string key)
        {
#if BETA
        if (key == "UiLauncherTheme")
        {
            return "0";
        }
#endif
            if (key == "UiHiddenPageLink")
            {
                // 原代码返回布尔值，此处可能存在类型不匹配问题，应调整为返回字符串
                return "true";
            }
            if (key == "UiHiddenSetupLink")
            {
                return "true";
            }
            return null;
        }

    }

    public class SetupEntry
    {
        public bool Encoded;
        public object DefaultValue;
        public object DefaultValueEncoded;
        public object Value;
        public SetupSource Source;

        /// <summary>
        /// 加载状态：0/未读取  1/已读取未处理  2/已处理
        /// </summary>
        public byte State = 0;

        public Type Type;


        public SetupEntry(object value,SetupSource source = SetupSource.Normal,bool encoded = false)
        {
            try
            {
                this.DefaultValue = value;
                this.Encoded = encoded;
                this.Value = value;
                this.Source = source;
                this.Type = value == null ? typeof(object) : value.GetType();
                this.DefaultValueEncoded = encoded ? SecretUtil.SecretEncrypt(value.ToString(), $"PLC{ModBase.UniqueAddress}") : value;
            }
            catch (Exception ex)
            {
                //Log(ex, "初始化 SetupEntry 失败", LogLevel.Feedback) '#5095 的 fallback
            }
        }
    }
}
