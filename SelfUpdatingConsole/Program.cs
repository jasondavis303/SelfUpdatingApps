using CommandLine;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace SelfUpdatingApp
{
    class Program
    {
        static int Main(string[] args)
        {
            
            int ret = 0;

            try
            {
                int cursorTop = -1;
                int cursorLeft = -1;
                int maxLen = 0;
                string lastStatus = null;
                try
                {
                    cursorTop = Console.CursorTop;
                    cursorLeft = Console.CursorLeft;
                }
                catch { }

                ManualResetEvent mre = new ManualResetEvent(false);
                IProgress<ProgressData> prog = new Progress<ProgressData>(p =>
                {
                    string status = p.Status;
                    if (p.Percent > 0)
                        status += $" {p.Percent}%";
                    if (maxLen > status.Length)
                        status += new string(' ', maxLen - status.Length);

                    if (status != lastStatus)
                    {
                        bool setPosWorked = false;
                        try
                        {
                            Console.SetCursorPosition(cursorLeft, cursorTop);
                            setPosWorked = true;
                        }
                        catch { }
                        Console.WriteLine(status);
                        lastStatus = status;
                        if (setPosWorked)
                            maxLen = Math.Max(maxLen, lastStatus.Length);
                    }

                    if (p.Done)
                        mre.Set();
                });

                var parsed = Parser.Default.ParseArguments<CLOptions.BuildOptions, CLOptions.InstallMeOptions, CLOptions.InstallOptions, CLOptions.UpdateOptions, CLOptions.UninstallOptions>(args);


                parsed.WithParsed<CLOptions.BuildOptions>(opts =>
                {
                    Packager.BuildPackageAsync(opts, prog).Wait();
                    mre.WaitOne();
                });


                parsed.WithParsed<CLOptions.InstallOptions>(opts =>
                {
                    var installed = Installer.InstallAsync(opts.Package, prog, true).Result;
                    mre.WaitOne();
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
                    mre.WaitOne();
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


                parsed.WithParsed<CLOptions.UninstallOptions>(opts =>
                {
                    Uninstaller.UninstallAsync(opts.AppId, prog).Wait();
                    mre.WaitOne();
                });


                parsed.WithParsed<CLOptions.InstallMeOptions>(opts =>
                {
                    if (Manager.InstallMe(opts))
                    {
                        Console.WriteLine("SAUC Installed");
                        if(!opts.NoGui)
                            Console.ReadLine();
                    }
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
