using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Keystore
{
    public partial class passwordForm : Form
    {
        public string Password { get; set; }

        public passwordForm()
        {
            InitializeComponent();
        }

        private void passwordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                this.Password = passwordBox.Text;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
