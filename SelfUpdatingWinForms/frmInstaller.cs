using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
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
                using var mre = new ManualResetEvent(false);
                IProgress<ProgressData> prog = new Progress<ProgressData>(p =>
                {
                    lblStatus.Text = p.Status;
                    pbProgress.Value = p.Percent;
                    if (p.Done)
                        mre.Set();
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
                await mre.WaitOneAsync();
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
                Program.ShowErrors(ex.InnerExceptions);
                if (ex.HResult != 0)
                    ErrorCode = ex.HResult;
            }
            catch (Exception ex)
            {
                Program.ShowErrors(new Exception[] { ex });
                if (ex.HResult != 0)
                    ErrorCode = ex.HResult;
            }

            Close();
        }
    }
}
