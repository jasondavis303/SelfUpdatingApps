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

        private async void frmPackager_Load(object sender, EventArgs e)
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

                await Packager.BuildPackageAsync(_opts, prog);
            }
            catch (AggregateException ex)
            {
                Program.ShowErrors(ex.InnerExceptions, false);
            }
            catch (Exception ex)
            {
                Program.ShowErrors(new Exception[] { ex }, false);
            }

            Close();
        }
    }
}
