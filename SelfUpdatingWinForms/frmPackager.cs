using System;
using System.Windows.Forms;

namespace SelfUpdatingApp
{
    partial class frmPackager : Form
    {
        readonly CLOptions _opts;
        readonly object _locker = new object();

        public frmPackager(CLOptions opts)
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Close();
            Application.Exit();
        }
    }
}
