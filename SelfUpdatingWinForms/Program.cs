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
            int ret = -1;
            bool consoleOnly = false;

            try
            {
                Console.WriteLine();
                Console.WriteLine("Self Updating GUI App");
                Console.WriteLine();

                var parsed = Parser.Default.ParseArguments<CLOptions.BuildOptions, CLOptions.InstallMeOptions, CLOptions.InstallOptions, CLOptions.UpdateOptions, CLOptions.UninstallOptions>(args);

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
                        ret = 0;
                    }
                    else
                    {
                        EnableWin();
                        using var frm = new frmPackager(opts);
                        Application.Run(frm);
                        ret = frm.ErrorCode;
                    }
                });


                parsed.WithParsed<CLOptions.InstallOptions>(opts =>
                {
                    EnableWin();
                    using var frm = new frmInstaller(opts.Package, 0, true);
                    Application.Run(frm);
                    ret = frm.ErrorCode;
                });


                parsed.WithParsed<CLOptions.UpdateOptions>(opts =>
                {
                    EnableWin();
                    string packageFile = Path2.DepoManifest(XmlData.Read(Path2.LocalManifest(opts.AppId)));
                    using var frm = new frmInstaller(packageFile, opts.ProcessId, false);
                    Application.Run(frm);
                    ret = frm.ErrorCode;
                });


                parsed.WithParsed<CLOptions.UninstallOptions>(opts =>
                {
                    EnableWin();
                    using var frm = new frmUninstaller(opts);
                    Application.Run(frm);
                    ret = frm.ErrorCode;
                });



                parsed.WithParsed<CLOptions.InstallMeOptions>(opts =>
                {
                    SelfInstaller.InstallMe();
                    ret = 0;
                });
            }
            catch (AggregateException ex)
            {
                ShowErrors(ex.InnerExceptions, consoleOnly);
                if (ex.HResult != 0)
                    ret = ex.HResult;
            }
            catch (Exception ex)
            {
                ShowErrors(new Exception[] { ex }, consoleOnly);
                if (ex.HResult != 0)
                    ret = ex.HResult;
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
    }
}
