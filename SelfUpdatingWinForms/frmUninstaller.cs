﻿using System;
using System.Windows.Forms;

namespace SelfUpdatingApp
{
    partial class frmUninstaller : Form
    {
        readonly CLOptions _opts;
        readonly object _locker = new object();

        public frmUninstaller(CLOptions opts)
        {
            InitializeComponent();
            pbImage.Image = Properties.Resources.uninstall;
            _opts = opts;
        }

        private async void frmUninstaller_Load(object sender, EventArgs e)
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

                await Uninstaller.UninstallAsync(_opts.AppId, prog);
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
