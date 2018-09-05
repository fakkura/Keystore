using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Keystore
{
    partial class aboutForm : Form
    {
        public aboutForm()
        {
            InitializeComponent();
        }

        private void labelLink_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(labelLink.Text);
        }
    }
}
