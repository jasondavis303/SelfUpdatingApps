using System;
using System.Windows.Forms;

namespace SelfUpdatingApp
{
    partial class frmPackager : Form
    {
        readonly CLOptions.BuildOptions _opts;
        readonly object _locker = new object();

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
                IProgress<ProgressData> prog = new WaitProgress<ProgressData>((p) =>
                {
                    lock (_locker)
                    {
                        lblStatus.Text = p.Status;
                        pbProgress.Value = p.Percent;
                    }
                });

                await Packager.BuildPackageAsync(_opts, prog);
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
