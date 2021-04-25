using System;
using System.Diagnostics;
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
                        if (!relaunchArgs[i].StartsWith("\""))
                            relaunchArgs[i] = '"' + relaunchArgs[i];
                        if (!relaunchArgs[i].EndsWith("\""))
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
    }
}
