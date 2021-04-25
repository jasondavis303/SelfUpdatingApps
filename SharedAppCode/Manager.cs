using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace SelfUpdatingApp
{
    public static partial class Manager
    {
        internal static bool InstallMe(CLOptions.InstallMeOptions opts)
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
                if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                    Process.Start("chmod", $"+x \"{tmpFile}\"");
                string args = $"install-me --process-id {Process.GetCurrentProcess().Id}";
                if (opts.NoGui)
                    args += " --no-gui";
                Process.Start(tmpFile, args);
                return false;
            }

            InstallNewest();

            //No errors means true
            return true;
        }
    }
}
