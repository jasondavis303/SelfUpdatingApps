using System;
using System.IO;

namespace SelfUpdatingApp
{
    static class SelfInstaller
    {
        public static void InstallMe()
        {
            try
            {
                string srcFile = Environment.GetCommandLineArgs()[0];
                File.Copy(Environment.GetCommandLineArgs()[0], Path2.SelfUpdatingExe, true);
            }
            catch { }

            RegistryEntries.RegisterSUAInfo();
        }
    }
}
