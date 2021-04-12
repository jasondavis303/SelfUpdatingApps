using CommandLine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SelfUpdatingApp
{
    static class Program
    {
        static readonly object _locker = new object();


        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            int ret = 0;
            bool consoleOnly = false;

            try
            {
                var parsed = Parser.Default.ParseArguments<CLOptions.BuildOptions, CLOptions.InstallMeOptions, CLOptions.UpdateOptions, CLOptions.UninstallOptions>(args);

                parsed.WithParsed<CLOptions.BuildOptions>(opts =>
                {
                    consoleOnly = opts.ConsoleOnly;
                    if (consoleOnly)
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

                        Packager.BuildPackageAsync(opts, prog).Wait();
                    }
                    else
                    {
                        EnableWin();
                        Application.Run(new frmPackager(opts));
                    }
                });


                parsed.WithParsed<CLOptions.InstallOptions>(opts =>
                {
                    EnableWin();
                    Application.Run(new frmInstaller(opts.Package, 0, true));
                });


                parsed.WithParsed<CLOptions.UpdateOptions>(opts =>
                {
                    EnableWin();
                    string packageFile = Path2.DepoManifest(XmlData.Read(Path2.LocalManifest(opts.AppId)));
                    Application.Run(new frmInstaller(packageFile, opts.ProcessId, false));
                });


                parsed.WithParsed<CLOptions.UninstallOptions>(opts =>
                {
                    EnableWin();
                    Application.Run(new frmUninstaller(opts));
                });
           


                parsed.WithParsed<CLOptions.InstallMeOptions>(opts => SelfInstaller.InstallMe());


                parsed.WithNotParsed<object>(opts =>
                {
                    //Arguments error
                    ret = -1;
                });
            }
            catch (AggregateException ex)
            {
                ShowErrors(ex.InnerExceptions, consoleOnly);
                ret = ex.HResult == 0 ? -1 : ex.HResult;
            }
            catch (Exception ex)
            {
                ShowErrors(new Exception[] { ex }, consoleOnly);
                ret = ex.HResult == 0 ? -1 : ex.HResult;
            }

            return ret;
        }

        static void EnableWin()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
        }


        public static void ShowErrors(IEnumerable<Exception> exes, bool consoleOnly)
        {
            if (consoleOnly)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
            }


            foreach (var ex in exes)
                if (consoleOnly)
                    Console.WriteLine(ex.Message);
                else
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (consoleOnly)
            {
                Console.ResetColor();
                Console.WriteLine();
            }
        }

        //static void Run(CLOptionsOld opts)
        //{
        //    switch (opts.Action)
        //    {
        //        case CLOptionsOld.Actions.PrintUsage:
        //            MessageBox.Show(CLOptionsOld.Usage(), "Usage", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //            break;

        //        case CLOptionsOld.Actions.InstallMe:
        //            SelfInstaller.InstallMe();
        //            break;

        //        case CLOptionsOld.Actions.Build:
        //            Application.Run(new frmPackager(opts));
        //            break;

        //        case CLOptionsOld.Actions.Install:
        //        case CLOptionsOld.Actions.Update:
        //            Application.Run(new frmInstaller(opts));
        //            break;

        //        case CLOptionsOld.Actions.Uninstall:
        //            Application.Run(new frmUninstaller(opts));
        //            break;
        //    }
        //}

    }
}
