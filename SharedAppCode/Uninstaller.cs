using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace SelfUpdatingApp
{
    static class Uninstaller
    {
        public static async Task UninstallAsync(string id, IProgress<ProgressData> progress = null)
        {
            if (Debugger.IsAttached)
                return;
#if DEBUG
            return;
#endif

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                progress?.Report(new ProgressData("Deleting registry entries"));
                RegistryEntries.UnregisterUninstallInfo(id);

                //Delete the desktop shortcut
                try
                {
                    XmlData data = await XmlData.ReadAsync(Path2.LocalManifest(id)).ConfigureAwait(false);
                    progress?.Report(new ProgressData("Deleting shortcuts"));
                    string shortcut = Path2.DesktopLinkPath(data.Name);
                    if (File.Exists(shortcut))
                        File.Delete(shortcut);

                    //Delete the Pinned To Taskbar shortcut
                    shortcut = Path2.TaskBarShortcutPath(data.Name);
                    if (File.Exists(shortcut))
                        File.Delete(shortcut);

                    //Delete the Pinned To Start Menu shortcut

                    //Delete Start Menu Shortcut
                }
                catch { }
            }

            await Task.Run(() =>
            {
                //Delete files
                DirectoryInfo rootDir = new DirectoryInfo(Path2.InstalledDirectory(id));
                if (rootDir.Exists)
                {
                    var files = new List<FileSystemInfo>(rootDir.GetFileSystemInfos("*", SearchOption.AllDirectories));
                    files.Sort((x, y) => x.FullName.CompareTo(y.FullName));
                    files.Reverse();

                    double total = files.Count;
                    double cur = 0;
                    int lastPerc = 0;
                    progress?.Report(new ProgressData("Deleting files", 0));
                    foreach (var file in files)
                    {
                        try { file.Delete(); }
                        catch { NativeMethods.DeleteAfterReboot(file.FullName); }

                        int perc = Math.Min(99, (int)Math.Floor((++cur / total) * 100));
                        if (perc != lastPerc)
                        {
                            lastPerc = perc;
                            progress?.Report(new ProgressData("Deleting files", perc));
                        }
                    }
                }

                string xmlPath = Path2.LocalManifest(id);
                if (File.Exists(xmlPath))
                    File.Delete(xmlPath);
            });

            progress?.Report(new ProgressData("Uninstall Complete", 100, true));
        }
    }
}
