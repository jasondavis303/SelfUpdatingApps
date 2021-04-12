using CommandLine;
using System;
using System.Diagnostics;

namespace SelfUpdatingApp
{
    class Program
    {
        static readonly object _locker = new object();

        static int Main(string[] args)
        {
            
            int ret = 0;

            try
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

                
                var parsed = Parser.Default.ParseArguments<CLOptions.BuildOptions, CLOptions.InstallMeOptions, CLOptions.UpdateOptions, CLOptions.UninstallOptions>(args);
                

                parsed.WithParsed<CLOptions.BuildOptions>(opts => Packager.BuildPackageAsync(opts, prog).Wait());


                parsed.WithParsed<CLOptions.InstallOptions>(opts =>
                {
                    var installed = Installer.InstallAsync(opts.Package, prog, true).Result;
                    if (!installed.Success)
                        throw installed.Error;
                    var installedData = XmlData.Read(Path2.LocalManifest(installed.Id));
                    Process.Start(Path2.InstalledExe(installedData));
                });


                parsed.WithParsed<CLOptions.UpdateOptions>(opts =>
                {
                    if (opts.ProcessId > 0)
                        try { Process.GetProcessById(opts.ProcessId).WaitForExit(); }
                        catch { }

                    var updated = Installer.InstallAsync(Path2.DepoManifest(XmlData.Read(Path2.LocalManifest(opts.AppId))), prog, false).Result;
                    if (!updated.Success)
                        throw updated.Error;
                    var updatedData = XmlData.Read(Path2.LocalManifest(updated.Id));
                    Process.Start(Path2.InstalledExe(updatedData));
                });


                parsed.WithParsed<CLOptions.UninstallOptions>(opts => Uninstaller.UninstallAsync(opts.AppId, prog).Wait());


                parsed.WithParsed<CLOptions.InstallMeOptions>(opts => SelfInstaller.InstallMe());


                parsed.WithNotParsed<object>(opts =>
                {
                    //Arguments error
                    ret = -1;
                });
                
            }
            catch(AggregateException ex)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                foreach(var subEx in ex.InnerExceptions)
                    Console.WriteLine(subEx.Message);
                Console.ResetColor();
                Console.WriteLine();

                ret = ex.HResult == 0 ? -1 : ex.HResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                Console.WriteLine();

                ret = ex.HResult == 0 ? -1 : ex.HResult;
            }

            return ret;
        }
    }
}
