using System;
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
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                CLOptions opts = CLOptions.Parse(args);
                if (opts.Errors.Count > 0)
                {
                    foreach (var ex in opts.Errors)
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MessageBox.Show(CLOptions.Usage(), "Usage", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    Run(opts);
                    return 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return -1;
        }

        static void Run(CLOptions opts)
        {
            switch (opts.Action)
            {
                case CLOptions.Actions.PrintUsage:
                    MessageBox.Show(CLOptions.Usage(), "Usage", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;

                case CLOptions.Actions.InstallMe:
                    SelfInstaller.InstallMe();
                    break;

                case CLOptions.Actions.Build:
                    Application.Run(new frmPackager(opts));
                    break;

                case CLOptions.Actions.Install:
                case CLOptions.Actions.Update:
                    Application.Run(new frmInstaller(opts));
                    break;

                case CLOptions.Actions.Uninstall:
                    Application.Run(new frmUninstaller(opts));
                    break;
            }
        }

    }
}
