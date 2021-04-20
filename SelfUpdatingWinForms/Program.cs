using CommandLine;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SelfUpdatingApp
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            int ret = -1;

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                var parsed = Parser.Default.ParseArguments<CLOptions.BuildOptions, CLOptions.InstallMeOptions, CLOptions.InstallOptions, CLOptions.UpdateOptions, CLOptions.UninstallOptions>(args);

                parsed.WithParsed<CLOptions.BuildOptions>(opts =>
                {
                    using var frm = new frmPackager(opts);
                    Application.Run(frm);
                    ret = frm.ErrorCode;
                });


                parsed.WithParsed<CLOptions.InstallOptions>(opts =>
                {
                    using var frm = new frmInstaller(opts.Package, 0, true, null);
                    Application.Run(frm);
                    ret = frm.ErrorCode;
                });


                parsed.WithParsed<CLOptions.UpdateOptions>(opts =>
                {
                    string packageFile = Path2.DepoManifest(XmlData.Read(Path2.LocalManifest(opts.AppId)));
                    using var frm = new frmInstaller(packageFile, opts.ProcessId, false, opts.RelaunchArgs);
                    Application.Run(frm);
                    ret = frm.ErrorCode;
                });


                parsed.WithParsed<CLOptions.UninstallOptions>(opts =>
                {
                    using var frm = new frmUninstaller(opts);
                    Application.Run(frm);
                    ret = frm.ErrorCode;
                });



                parsed.WithParsed<CLOptions.InstallMeOptions>(opts =>
                {
                    if(Manager.InstallMe(opts))
                        if(!opts.NoGui)
                            MessageBox.Show("SUAG Installed", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ret = 0;
                });
            }
            catch (AggregateException ex)
            {
                ShowErrors(ex.InnerExceptions);
                if (ex.HResult != 0)
                    ret = ex.HResult;
            }
            catch (Exception ex)
            {
                ShowErrors(new Exception[] { ex });
                if (ex.HResult != 0)
                    ret = ex.HResult;
            }

            return ret;
        }

        
        public static void ShowErrors(IEnumerable<Exception> exes)
        {
            foreach (var ex in exes)
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
