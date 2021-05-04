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
                progress?.Report(new ProgressData($"Preparing to install v{serverData.Version}"));
                
                //Unzip from the web                   
                string status = $"Installing v{serverData.Version}";
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
                }
                else
                {
                    Process.Start("chmod", $"+x \"{Path2.InstalledExe(serverData)}\"");
                }

                //Success
                serverData.Save(Path2.LocalManifest(serverData.Id));
                progress?.Report(new ProgressData("Done", 100, true));
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
                    entryName = entryName.Substring(1);

                string filename = Path.Combine(targetDir, entryName);
                Directory.CreateDirectory(Path.GetDirectoryName(filename));

                using var outputStream = File.Create(filename);
                int entryRead = 0;
                while (entryRead < entry.Length)
                {
                    int read = await entryStream.ReadAsync(buffer, 0, Constants.BUFFER_SIZE).ConfigureAwait(false);
                    await outputStream.WriteAsync(buffer, 0, read).ConfigureAwait(false);
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
