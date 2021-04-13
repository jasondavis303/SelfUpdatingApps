using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SelfUpdatingApp
{
    partial class frmInstaller : Form
    {
        readonly string _packageFile;
        readonly int _processId;
        readonly bool _createShortcuts;
        readonly string _relaunchArgs;

        readonly object _locker = new object();

        public frmInstaller(string packageFile, int processId, bool createShortcuts, string relaunchArgs)
        {
            InitializeComponent();
            pbImage.Image = Properties.Resources.install;
            _packageFile = packageFile;
            _processId = processId;
            _createShortcuts = createShortcuts;
            _relaunchArgs = relaunchArgs;
        }

        public int ErrorCode { get; set; } = -1;

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

                if(_processId > 0)
                {
                    await Task.Run(() =>
                    {
                        prog.Report(new ProgressData("Waiting for previous app to close"));
                        try { Process.GetProcessById(_processId).WaitForExit(); }
                        catch { }
                    });
                }

                var ret = await Installer.InstallAsync(_packageFile, prog, _createShortcuts);
                if (!ret.Success)
                    throw ret.Error;

                string exePath = Path2.InstalledExe(XmlData.Read(Path2.LocalManifest(ret.Id)));
                if (string.IsNullOrWhiteSpace(_relaunchArgs))
                {
                    Process.Start(exePath);
                }
                else
                {
                    string args = Encoding.UTF8.GetString(Convert.FromBase64String(_relaunchArgs));
                    Process.Start(exePath, args);                    
                }

                ErrorCode = 0;
            }
            catch (AggregateException ex)
            {
                Program.ShowErrors(ex.InnerExceptions, false);
                if (ex.HResult != 0)
                    ErrorCode = ex.HResult;
            }
            catch (Exception ex)
            {
                Program.ShowErrors(new Exception[] { ex }, false);
                if (ex.HResult != 0)
                    ErrorCode = ex.HResult;
            }

            Close();
        }
    }
}
