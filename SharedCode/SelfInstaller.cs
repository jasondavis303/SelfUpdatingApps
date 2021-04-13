using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;

namespace SelfUpdatingApp
{
    static class SelfInstaller
    {
        public static bool InstallMe(CLOptions.InstallMeOptions opts)
        {
            try
            {
                if (opts.ProcessId > 0)
                    Process.GetProcessById(opts.ProcessId).WaitForExit();
            }
            catch { }

            string srcFile = Environment.GetCommandLineArgs()[0];
            if (srcFile.Equals(Path2.SelfUpdatingExe, StringComparison.CurrentCultureIgnoreCase))
            {
                //This file is in the dest, so copy it to a temp file and relaunch
                string tmpFile = Path.Combine(Path.GetTempPath(), ThisApp.DestExe);
                File.Copy(srcFile, tmpFile, true);
                if(Environment.OSVersion.Platform != PlatformID.Win32NT)
                    Process.Start("chmod", $"+x \"{tmpFile}\"");
                Process.Start(tmpFile, $"install-me --process-id {Process.GetCurrentProcess().Id}");
                return false;
            }

            InstallNewest();

            //No errors means true
            return true;
        }

        static void InstallNewest()
        {
            string tmpFile = Path.GetTempFileName();

            //Download the newest version
            using (var wc = new WebClient())
            {
                wc.DownloadFile(ThisApp.SourceExe, tmpFile);
            }

            File.Move(tmpFile, Path2.SelfUpdatingExe, true);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                RegistryEntries.RegisterSUAInfo();
            else
                Process.Start("chmod", $"+x \"{Path2.SelfUpdatingExe}\"");
        }
    }
}
