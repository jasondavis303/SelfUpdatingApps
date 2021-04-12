using Microsoft.Win32;
using System;

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
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey($@"SOFTWARE\Classes\{ThisApp.Extension}"))
            {
                key.SetValue("", $"{ThisApp.Handler}_file");
            }

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey($@"SOFTWARE\Classes\{ThisApp.Handler}_file"))
            {
                key.SetValue("", "SelfUpdatingApp Manifest");
            }

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey($@"SOFTWARE\Classes\{ThisApp.Handler}_file\DefaultIcon"))
            {
                key.SetValue("", $"\"{Path2.SelfUpdatingExe}\",0");
            }

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey($@"SOFTWARE\Classes\{ThisApp.Handler}_file\Shell\open\command"))
            {
                key.SetValue("", $"\"{Path2.SelfUpdatingExe}\" install --package \"%1\"");
            }

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey($@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\{ThisApp.Extension}"))
            {
                try { key.DeleteSubKeyTree("UserChoice"); }
                catch { }
            }


            NativeMethods.SHChangeNotify(NativeMethods.HChangeNotifyEventID.SHCNE_ASSOCCHANGED, NativeMethods.HChangeNotifyFlags.SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);

        }
    }
}