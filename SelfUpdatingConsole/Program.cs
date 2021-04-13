using CommandLine;
using System;
using System.Diagnostics;
using System.Text;

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
                Console.WriteLine();
                Console.WriteLine("Self Updating Console App");
                Console.WriteLine();


                string lastStatus = null;
                int lastPercent = 0;
                int cursorLeft = 0;
                int cursorTop = Console.CursorTop;
                bool wroteColon = false;
                int lastLen = 0;

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


                try
                {
                    Console.SetCursorPosition(cursorLeft, cursorTop);

                    //If that didn't throw, then we can do better console output

                    prog = new Progress<ProgressData>((p) =>
                    {
                        lock (_locker)
                        {
                            if(p.Status != lastStatus)
                            {
                                if(lastLen > 0)
                                {
                                    Console.SetCursorPosition(0, cursorTop);
                                    Console.Write(new string(' ', lastLen));
                                }
                                Console.SetCursorPosition(0, cursorTop);
                                Console.Write(p.Status);
                                lastStatus = p.Status;
                                cursorLeft = Console.CursorLeft;
                                wroteColon = false;
                                lastPercent = 0;
                                lastLen = Console.CursorLeft;
                            }
                            
                            if(p.Percent > lastPercent)
                            {
                                if(!wroteColon)
                                {
                                    Console.Write(": ");
                                    wroteColon = true;
                                    cursorLeft = Console.CursorLeft;
                                    lastLen = Console.CursorLeft;
                                }

                                Console.SetCursorPosition(cursorLeft, cursorTop);
                                Console.Write($"{p.Percent}%");
                                lastLen = Console.CursorLeft;
                                lastPercent = p.Percent;
                            }

                        }
                    });
                }
                catch { }

                

                var parsed = Parser.Default.ParseArguments<CLOptions.BuildOptions, CLOptions.InstallMeOptions, CLOptions.InstallOptions, CLOptions.UpdateOptions, CLOptions.UninstallOptions>(args);
                

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
                    string exePath = Path2.InstalledExe(XmlData.Read(Path2.LocalManifest(updated.Id)));
                    if (string.IsNullOrWhiteSpace(opts.RelaunchArgs))
                    {
                        Process.Start(exePath);
                    }
                    else
                    {
                        string relaunchArgs = Encoding.UTF8.GetString(Convert.FromBase64String(opts.RelaunchArgs));
                        Process.Start(exePath, relaunchArgs);
                    }
                });


                parsed.WithParsed<CLOptions.UninstallOptions>(opts => Uninstaller.UninstallAsync(opts.AppId, prog).Wait());


                parsed.WithParsed<CLOptions.InstallMeOptions>(opts =>
                {
                    SelfInstaller.InstallMe(opts);
                    Console.WriteLine("SAUC Installed");
                    Console.ReadLine();
                });


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
