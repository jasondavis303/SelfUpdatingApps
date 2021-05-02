using AMRE;
using System;
using System.Windows.Forms;

namespace SelfUpdatingApp
{
    partial class frmUninstaller : Form
    {
        readonly CLOptions.UninstallOptions _opts;

        public frmUninstaller(CLOptions.UninstallOptions opts)
        {
            InitializeComponent();
            pbImage.Image = Properties.Resources.uninstall;
            _opts = opts;
        }

        public int ErrorCode { get; set; } = -1;

        private async void frmUninstaller_Load(object sender, EventArgs e)
        {
            try
            {
                var mre = new AsyncManualResetEvent();
                IProgress<ProgressData> prog = new Progress<ProgressData>((p) =>
                {
                    lblStatus.Text = p.Status;
                    pbProgress.Value = p.Percent;
                    if (p.Done)
                        mre.Set();
                });

                await Uninstaller.UninstallAsync(_opts.AppId, prog);
                await mre.WaitAsync();
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
