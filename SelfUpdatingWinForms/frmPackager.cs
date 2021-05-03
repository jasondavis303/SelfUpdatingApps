using System;
using System.Threading;
using System.Windows.Forms;

namespace SelfUpdatingApp
{
    partial class frmPackager : Form
    {
        readonly CLOptions.BuildOptions _opts;

        public frmPackager(CLOptions.BuildOptions opts)
        {
            InitializeComponent();
            pbImage.Image = Properties.Resources.install;
            _opts = opts;
        }

        public int ErrorCode { get; set; } = -1;

        private async void frmPackager_Load(object sender, EventArgs e)
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

                await Packager.BuildPackageAsync(_opts, prog);
                await mre.WaitOneAsync();
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
