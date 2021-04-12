using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SelfUpdatingApp
{
    class Program
    {
        static readonly object _locker = new object();

        static int Main(string[] args)
        {
            CLOptions opts = CLOptions.Parse(args);
            if (opts.Errors.Count > 0)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var ex in opts.Errors)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine();
                }
                Console.ResetColor();
                Console.WriteLine(CLOptions.Usage());
            }
            else
            {
                try
                {
                    RunAsync(opts).Wait();
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();
                    Console.WriteLine();
                }

            }
            return -1;
        }

        private static async Task RunAsync(CLOptions opts)
        {
           
            IProgress<ProgressData> prog = new Progress<ProgressData>((p) =>
            {
                lock (_locker)
                {
                    Console.Write(p.Status);
                    if (p.Percent > 0)
                        Console.Write(": {0}%", p.Percent);
                    Console.WriteLine();
                }
            });


            switch (opts.Action)
            {
                case CLOptions.Actions.PrintUsage:
                    Console.WriteLine(CLOptions.Usage());
                    break;

                case CLOptions.Actions.InstallMe:
                    SelfInstaller.InstallMe();
                    break;

                case CLOptions.Actions.Build:
                    await Packager.BuildPackageAsync(opts, prog).ConfigureAwait(false);
                    break;

                case CLOptions.Actions.Install:
                    var installed = await Installer.InstallAsync(opts.PackageFile, prog, true).ConfigureAwait(false);
                    if (!installed.Success)
                        throw installed.Error;
                    var installedData = await XmlData.ReadAsync(Path2.LocalPackage(installed.Id)).ConfigureAwait(false);
                    Process.Start(Path2.InstalledExe(installedData));
                    break;

                case CLOptions.Actions.Update:
                    if (opts.ProcessId > 0)
                    {
                        try
                        {
                            var proc = Process.GetProcessById(opts.ProcessId);
                            proc.WaitForExit();
                        }
                        catch { }
                    }
                    var updated = await Installer.InstallAsync(Path2.ServerPackage(XmlData.Read(Path2.LocalPackage(opts.AppId))), prog, false).ConfigureAwait(false);
                    if (!updated.Success)
                        throw updated.Error;                    
                    var updatedData = await XmlData.ReadAsync(Path2.LocalPackage(updated.Id)).ConfigureAwait(false);
                    Process.Start(Path2.InstalledExe(updatedData));
                    break;

                case CLOptions.Actions.Uninstall:
                    await Uninstaller.UninstallAsync(opts.AppId, prog).ConfigureAwait(false);
                    break;
            }
        }
    }
}
