using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace SelfUpdatingApp
{
    public static partial class Manager
    {
       /// <summary>
        /// Installs the newest SUAG or SUAC executable. This does not update an app package
        /// </summary>
        public static void InstallNewest()
        {
            string tmpFile = Path.GetTempFileName();

            //Download the newest version
            using (var wc = new WebClient())
            {
                wc.Proxy.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                wc.DownloadFile(ThisApp.SourceExe, tmpFile);
            }

            try { File.Delete(Path2.SelfUpdatingExe); }
            catch { }
            File.Move(tmpFile, Path2.SelfUpdatingExe);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                RegistryEntries.RegisterSUAInfo();
            else
                Process.Start("chmod", $"+x \"{Path2.SelfUpdatingExe}\"");
        }
    }
}
