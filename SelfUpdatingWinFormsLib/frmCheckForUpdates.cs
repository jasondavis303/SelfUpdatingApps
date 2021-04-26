using System;
using System.Windows.Forms;

namespace SelfUpdatingApp
{
    partial class frmCheckForUpdates : Form
    {
        readonly string _appId;

        public frmCheckForUpdates(string appId)
        {
            InitializeComponent();
            pbImage.Image = Properties.Resources.install;
            _appId = appId;
        }

        public bool UpdateAvailable { get; set; }

        private async void frmCheckForUpdates_Load(object sender, EventArgs e)
        {
            try
            {
                if (await Installer.IsUpdateAvailableAsync(_appId))
                    UpdateAvailable = true;
            }
            catch (AggregateException aex)
            {
                foreach (var ex in aex.InnerExceptions)
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Close();
        }
    }
}
