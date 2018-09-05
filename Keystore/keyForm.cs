using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;
using Microsoft.VisualBasic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Keystore
{
    public partial class keyForm : Form
    {
        int sel = 0;

        string Password = "";
        string Title = "";
        int Id = 0;

        List<Key> keys;

        private const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)]string lParam);

        public keyForm(string password, int id, string title = "")
        {
            InitializeComponent();

            this.Password = password;
            this.Id = id;
            this.Title = title;
        }

        private void keyForm_Load(object sender, EventArgs e)
        {
            if (!File.Exists("Keystore.db"))
                this.Close();

            if (!Database.Test(Password))
                this.Close();

            if(Title != string.Empty)
              this.Text = Title + " Keys";

            SendMessage(keyBox.Handle, EM_SETCUEBANNER, 0, "Add key");

            keys = new List<Key>(Database.GetKeys(Id, Password));
            keysBox.DataSource = keys;
            keysBox.DisplayMember = "Code";
            keysBox.ValueMember = "Id";
        }

        private void keyBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (keyBox.Text != string.Empty)
                {
                    sel = keysBox.SelectedIndex;
                    Database.AddKey(Id, keyBox.Text, Password);
                    keys = new List<Key>(Database.GetKeys(Id, Password));
                    keysBox.DataSource = null;
                    keysBox.DataSource = keys;
                    keysBox.DisplayMember = "Code";
                    keysBox.ValueMember = "Id";
                    keysBox.SelectedIndex = sel;
                }

                e.Handled = true;
                e.SuppressKeyPress = true;
                keyBox.Clear();
            }
        }

        private void keysBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DialogResult dr = MessageBox.Show("Are you sure you wish to delete this key?\r\nThis is irreversible!", "Keystore", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (dr == DialogResult.OK)
                {
                    Database.DeleteKey(((Key)keysBox.Items[keysBox.SelectedIndex]).Id, Password);
                    keys = new List<Key>(Database.GetKeys(Id, Password));
                    keysBox.DataSource = null;
                    keysBox.DataSource = keys;
                    keysBox.DisplayMember = "Code";
                    keysBox.ValueMember = "Id";
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void importButton_Click(object sender, EventArgs e)
        {
            if (importDialog.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(importDialog.FileName))
                {
                    sel = keysBox.SelectedIndex;
                    var lines = File.ReadLines(importDialog.FileName);
                    foreach (var line in lines)
                    {
                        Database.AddKey(Id, line, Password);
                        keys = new List<Key>(Database.GetKeys(Id, Password));
                        keysBox.DataSource = null;
                        keysBox.DataSource = keys;
                        keysBox.DisplayMember = "Code";
                        keysBox.ValueMember = "Id";
                    }
                    keysBox.SelectedIndex = sel;
                }

                MessageBox.Show("Import complete.");
                importDialog.FileName = "";
            }
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            if (exportDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = File.AppendText(exportDialog.FileName))
                {
                    foreach (Key line in keysBox.Items)
                        sw.WriteLine(line.Code);
                }
                MessageBox.Show("Export complete.");
                exportDialog.FileName = "";
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void enabledTimer_Tick(object sender, EventArgs e)
        {
            if (keysBox.Items.Count > 0)
                exportToolStripMenuItem.Enabled = true;
            else
                exportToolStripMenuItem.Enabled = false;

            if (keysBox.SelectedIndex >= 0)
                modifyToolStripMenuItem.Enabled = true;
            else
                modifyToolStripMenuItem.Enabled = false;
        }

        private void modifyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (keysBox.SelectedIndex >= 0)
            {
                string newkey = Microsoft.VisualBasic.Interaction.InputBox("Modify", "Modify", ((Key)keysBox.Items[keysBox.SelectedIndex]).Code, -1, -1);
                if (newkey != string.Empty)
                {
                    sel = keysBox.SelectedIndex;
                    Database.UpdateKey(((Key)keysBox.Items[keysBox.SelectedIndex]).Id, newkey, Password);
                    keys = new List<Key>(Database.GetKeys(Id, Password));
                    keysBox.DataSource = null;
                    keysBox.DataSource = keys;
                    keysBox.DisplayMember = "Code";
                    keysBox.ValueMember = "Id";
                    keysBox.SelectedIndex = sel;
                }
            }
        }

        private void keysBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                if (e.KeyCode == Keys.C)
                {
                    Clipboard.SetText(((Key)keysBox.Items[keysBox.SelectedIndex]).Code);
                }
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(keysBox.SelectedIndex >= 0)
                Clipboard.SetText(((Key)keysBox.Items[keysBox.SelectedIndex]).Code);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Are you sure you wish to delete this key?\r\nThis is irreversible!", "Keystore", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (dr == DialogResult.OK)
            {
                Database.DeleteKey(((Key)keysBox.Items[keysBox.SelectedIndex]).Id, Password);
                keys = new List<Key>(Database.GetKeys(Id, Password));
                keysBox.DataSource = null;
                keysBox.DataSource = keys;
                keysBox.DisplayMember = "Code";
                keysBox.ValueMember = "Id";
            }
        }
    }
}
