using Microsoft.Win32;
using System;
using System.Linq;

namespace SelfUpdatingApp
{
    static class RegistryEntries
    {
        public static void RegisterUninstallInfo(XmlData xmlData)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                return;

            using RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\" + xmlData.Id);
            key.SetValue("DisplayName", xmlData.Name);
            key.SetValue("ApplicationVersion", xmlData.Version.ToString());
            key.SetValue("DisplayIcon", Path2.InstalledExe(xmlData));
            key.SetValue("DisplayVersion", xmlData.Version.ToString());
            key.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
            key.SetValue("UninstallString", $"\"{Path2.SelfUpdatingExe}\" uninstall --app-id {xmlData.Id}");
        }

        public static void UnregisterUninstallInfo(string id)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                return;

            using RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            key.DeleteSubKeyTree(id, false);
        }

        public static void RegisterSUAInfo()
        {
            bool changed = UpdateRegistryIfNeeded($@"SOFTWARE\Classes\{ThisApp.Extension}", string.Empty, $"{ThisApp.Handler}_file");
            changed |= UpdateRegistryIfNeeded($@"SOFTWARE\Classes\{ThisApp.Handler}_file", string.Empty, "SelfUpdatingApp Manifest");
            changed |= UpdateRegistryIfNeeded($@"SOFTWARE\Classes\{ThisApp.Handler}_file\DefaultIcon", string.Empty, $"\"{Path2.SelfUpdatingExe}\",0");
            changed |= UpdateRegistryIfNeeded($@"SOFTWARE\Classes\{ThisApp.Handler}_file\Shell\install", string.Empty, "Install");
            changed |= UpdateRegistryIfNeeded($@"SOFTWARE\Classes\{ThisApp.Handler}_file\Shell\install\command", string.Empty, $"\"{Path2.SelfUpdatingExe}\" install --package \"%1\"");

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey($@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\{ThisApp.Extension}"))
            {
                try
                {
                    if (key.GetSubKeyNames().Contains("UserChoice"))
                    {
                        key.DeleteSubKeyTree("UserChoice");
                        changed = true;
                    }
                }
                catch { }
            }

            if (changed)
                NativeMethods.SHChangeNotify(NativeMethods.HChangeNotifyEventID.SHCNE_ASSOCCHANGED, NativeMethods.HChangeNotifyFlags.SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }
        
        private static bool UpdateRegistryIfNeeded(string keyPath, string name, string value)
        {
            using var key = Registry.CurrentUser.CreateSubKey(keyPath);
            
            try
            {
                if ((string)key.GetValue(name, string.Empty) == value)
                    return false;
            }
            catch { }
    
            key.SetValue(name, value, RegistryValueKind.String);
            return true;
        }
    
    }
}