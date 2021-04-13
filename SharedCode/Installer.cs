using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfUpdatingApp
{
    public static partial class Installer
    {
        public static async Task<bool> IsUpdateAvailableAsync(string id)
        {
            try
            {
                Console.WriteLine("Checking for updates");
                var localData = XmlData.Read(Path2.LocalManifest(id));
                var serverData = await XmlData.ReadAsync(Path2.DepoManifest(localData)).ConfigureAwait(false);
                return serverData.Version > localData.Version;
            }
            catch
            {
                throw new Exception("Unable to check for update");
            }
        }

        public static void Launch(string id, string[] relaunchArgs = null)
        {
            string args = $"update --app-id \"{id}\" --process-id {Process.GetCurrentProcess().Id}";
            if(relaunchArgs != null && relaunchArgs.Length > 0)
            {
                string unencoded = null;
                for(int i = 0; i < relaunchArgs.Length; i++)
                {
                    if(relaunchArgs[i].Contains(' '))
                    {
                        if (!relaunchArgs[i].StartsWith('"'))
                            relaunchArgs[i] = '"' + relaunchArgs[i];
                        if (!relaunchArgs[i].EndsWith('"'))
                            relaunchArgs[i] += '"';
                    }

                    unencoded += relaunchArgs[i] + " ";
                }
                unencoded = unencoded.Trim();
                string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(unencoded), Base64FormattingOptions.None);
                args += $" --relaunch-args \"{encoded}\"";
            }

            Process.Start(Path2.SelfUpdatingExe, args);
            return;
        }

        public static Version GetInstalledVersion(string id)
        {
            try
            {
                return XmlData.Read(Path2.LocalManifest(id)).Version;
            }
            catch
            {
                return new Version();
            }
        }






        internal static async Task<InstallResult> InstallAsync(string url, IProgress<ProgressData> progress, bool createShortcuts)
        {

            InstallResult ret = new InstallResult { Success = true };

            if (Debugger.IsAttached)
                return ret;

#if DEBUG
            return ret;
#endif

            try
            {
                //Read the package
                progress?.Report(new ProgressData("Reading package"));
                XmlData serverData = await XmlData.ReadAsync(url).ConfigureAwait(false);
                ret.Id = serverData.Id;
                ret.Version = serverData.Version;
                
                //If installed, check installed version
                try
                {
                    var localVersion = GetInstalledVersion(ret.Id);
                    if (localVersion >= serverData.Version)
                        return ret;
                }
                catch { }

                //Install!
                string status = $"Installing v{serverData.Version}";
                progress?.Report(new ProgressData(status));

                //Unzip from the web                   
                string zipSrc = Path2.DepoPackage(serverData);
                if (StreamHelper.IsWebUrl(zipSrc))
                {
                    using var response = await StreamHelper.GetWebResponseAsync(zipSrc).ConfigureAwait(false);
                    using var source = response.GetResponseStream();
                    await UnzipPackage(source, Path2.InstalledDirectory(serverData.Id), status, progress).ConfigureAwait(false);
                }
                else
                {
                    using var source = StreamHelper.OpenAsyncRead(zipSrc);
                    await UnzipPackage(source, Path2.InstalledDirectory(serverData.Id), status, progress).ConfigureAwait(false);
                }
                


                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    RegistryEntries.RegisterUninstallInfo(serverData);

                    //Create desktop shortcut
                    string shortcutFile = Path2.DesktopLinkPath(serverData.Name);
                    if (createShortcuts || File.Exists(shortcutFile))
                        Shortcut.Create(shortcutFile, Path2.InstalledExe(serverData));

                    //Create start menu shortcut

                    //Create pinned to start shortcut


                    //Update if pinned to taskbar
                    shortcutFile = Path2.TaskBarShortcutPath(serverData.Name);
                    if (File.Exists(shortcutFile))
                        Shortcut.Create(shortcutFile, Path2.InstalledExe(serverData));
                }

                //Success
                serverData.Save(Path2.LocalManifest(serverData.Id));
                progress?.Report(new ProgressData("Done", 100));
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.Error = ex;
            }

            return ret;
        }

        private static async Task UnzipPackage(Stream source, string targetDir, string status, IProgress<ProgressData> progress)
        {
            double totalRead = 0;
            int lastPerc = 0;

            ZipArchive archive = new ZipArchive(source, ZipArchiveMode.Read);
            double totalLength = archive.Entries.Sum(item => item.Length);

            byte[] buffer = new byte[Constants.BUFFER_SIZE];

            foreach (var entry in archive.Entries)
            {
                using var entryStream = entry.Open();
                string entryName = entry.FullName;
                if (entryName.StartsWith("\\") || entryName.StartsWith("/"))
                    entryName = entryName[1..];

                string filename = Path.Combine(targetDir, entryName);
                Directory.CreateDirectory(Path.GetDirectoryName(filename));

                using var outputStream = File.Create(filename);
                int entryRead = 0;
                while (entryRead < entry.Length)
                {
                    int read = await entryStream.ReadAsync(buffer.AsMemory(0, Constants.BUFFER_SIZE)).ConfigureAwait(false);
                    await outputStream.WriteAsync(buffer.AsMemory(0, read)).ConfigureAwait(false);
                    entryRead += read;
                    totalRead += read;

                    int perc = Math.Min(99, (int)Math.Floor((totalRead / totalLength) * 100));
                    if (perc != lastPerc)
                    {
                        lastPerc = perc;
                        progress?.Report(new ProgressData(status, perc));
                    }
                }
            }
        }
     }
}
