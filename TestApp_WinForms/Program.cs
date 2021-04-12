using SelfUpdatingApp;
using System;
using System.Windows.Forms;

namespace TestApp_WinForms
{
    static class Program
    {
        public const string APP_ID = "7DF9AC05-8469-4330-825F-E25A670E61F3";

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool launchInstaller = false;
            try
            {
                launchInstaller = Installer.IsUpdateAvailableAsync(APP_ID).Result;
            }
            catch  (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if(launchInstaller)
            {
                Installer.Launch(APP_ID);
                return;
            }

            Application.Run(new Form1());
        }
    }
}
