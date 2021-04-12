using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SelfUpdatingApp
{
    partial class frmInstaller : Form
    {
        readonly CLOptions _opts;
        readonly object _locker = new object();

        public frmInstaller(CLOptions opts)
        {
            InitializeComponent();
            pbImage.Image = Properties.Resources.install;
            _opts = opts;
        }

        private async void frmInstaller_Load(object sender, EventArgs e)
        {
            try
            {
                IProgress<ProgressData> prog = new Progress<ProgressData>((p) =>
                {
                    lock (_locker)
                    {
                        lblStatus.Text = p.Status;
                        pbProgress.Value = p.Percent;
                    }
                });

                if(_opts.ProcessId > 0)
                {
                    await Task.Run(() =>
                    {
                        prog.Report(new ProgressData("Waiting for previous app to close"));
                        try { Process.GetProcessById(_opts.ProcessId).WaitForExit(); }
                        catch { }
                    });
                }

                string uri = _opts.Action == CLOptions.Actions.Install ? _opts.PackageFile : Path2.ServerPackage(XmlData.Read(Path2.LocalPackage(_opts.AppId)));
                
                var ret = await Installer.InstallAsync(uri, prog, _opts.Action == CLOptions.Actions.Install);
                if (!ret.Success)
                    throw ret.Error;
                Process.Start(Path2.InstalledExe(XmlData.Read(Path2.LocalPackage(ret.Id))));

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Close();
            Application.Exit();
        }
    }
}
