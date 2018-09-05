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
    public partial class mainForm : Form
    {
        private const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)]string lParam);

        List<Product> products;

        int sel = 0;
        string Password = "";

        public mainForm()
        {
            InitializeComponent();
        }

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < productsBox.Items.Count; i++)
            {
                if (((DataRowView)productsBox.Items[i])["name"].ToString().IndexOf(searchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    productsBox.SetSelected(i, true);
                }
            }
        }

        private void mainForm_Load(object sender, EventArgs e)
        {
            if (!File.Exists("Keystore.db"))
            {
                using (var passwordform = new passwordForm())
                {
                    var result = passwordform.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        Password = passwordform.Password;
                    }
                }
            }

            Database.Create(Password);

            bool exit = false;

            if (Password == string.Empty)
            {
                if (!Database.Test(Password))
                {
                    bool valid = false;
                    while (!valid)
                    {
                        using (var passwordform = new passwordForm())
                        {
                            var result = passwordform.ShowDialog();
                            if (result == DialogResult.OK)
                            {
                                Password = passwordform.Password;
                            }
                            else
                            {
                                exit = true;
                                break;
                            }
                        }

                        valid = Database.Test(Password);
                    }
                }
            }

            if (exit)
                Environment.Exit(1);

            SendMessage(searchBox.Handle, EM_SETCUEBANNER, 0, "Search product");
            SendMessage(productBox.Handle, EM_SETCUEBANNER, 0, "Add product");

            products = new List<Product>(Database.GetProducts(Password));
            productsBox.DataSource = products;
            productsBox.DisplayMember = "Name";
            productsBox.ValueMember = "Id";
        }

        private void searchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void productsBox_DoubleClick(object sender, EventArgs e)
        {
            if (productsBox.SelectedIndex >= 0)
            {
                keyForm keyform = new keyForm(Password, ((Product)productsBox.Items[productsBox.SelectedIndex]).Id, ((Product)productsBox.Items[productsBox.SelectedIndex]).Name);
                keyform.ShowDialog();
            }
        }

        private void helpButton_Click(object sender, EventArgs e)
        {
            aboutForm aboutform = new aboutForm();
            aboutform.ShowDialog();
        }

        private void productBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (productBox.Text != string.Empty)
                {
                    sel = productsBox.SelectedIndex;
                    Database.AddProduct(productBox.Text, Password);
                    products = new List<Product>(Database.GetProducts(Password));
                    productsBox.DataSource = null;
                    productsBox.DataSource = products;
                    productsBox.DisplayMember = "Name";
                    productsBox.ValueMember = "Id";
                    productsBox.SelectedIndex = sel;
                }

                e.Handled = true;
                e.SuppressKeyPress = true;
                productBox.Clear();
            }
        }

        private void productsBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DialogResult dr = MessageBox.Show("Are you sure you wish to delete this product?\r\nAll the associated keys will also be deleted.\r\nThis is irreversible!", "Keystore", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (dr == DialogResult.OK)
                {
                    Database.DeleteProduct(((Product)productsBox.Items[productsBox.SelectedIndex]).Id, Password);
                    products = new List<Product>(Database.GetProducts(Password));
                    productsBox.DataSource = null;
                    productsBox.DataSource = products;
                    productsBox.DisplayMember = "Name";
                    productsBox.ValueMember = "Id";
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
                    sel = productsBox.SelectedIndex;
                    var lines = File.ReadLines(importDialog.FileName);
                    foreach (var line in lines)
                    {
                        Database.AddProduct(line, Password);
                        products = new List<Product>(Database.GetProducts(Password));
                        productsBox.DataSource = null;
                        productsBox.DataSource = products;
                        productsBox.DisplayMember = "Name";
                        productsBox.ValueMember = "Id";
                    }
                    productsBox.SelectedIndex = sel;
                }

                MessageBox.Show("Import complete.");
                importDialog.FileName = "";
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            if (exportDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = File.AppendText(exportDialog.FileName))
                {
                    foreach (Product line in productsBox.Items)
                        sw.WriteLine(line.Name);
                }
                MessageBox.Show("Export complete.");
                exportDialog.FileName = "";
            }
        }

        private void setPasswordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string password = Microsoft.VisualBasic.Interaction.InputBox("This will either password protect an unprotected database, change the current password or unprotect the database if you leave the field empty.", "Set password", Password, -1, -1);
            Database.SetPassword(password, Password);
            Password = password;
        }

        private void enabledTimer_Tick(object sender, EventArgs e)
        {
            if (productsBox.Items.Count > 0)
                exportToolStripMenuItem.Enabled = true;
            else
                exportToolStripMenuItem.Enabled = false;

            if (productsBox.SelectedIndex >= 0)
            {
                modifyToolStripMenuItem.Enabled = true;
                randomKeyToolStripMenuItem.Enabled = true;
                randomKeyDeleteToolStripMenuItem.Enabled = true;
            }
            else
            {
                modifyToolStripMenuItem.Enabled = false;
                randomKeyToolStripMenuItem.Enabled = false;
                randomKeyDeleteToolStripMenuItem.Enabled = false;
            }

            if (Password == string.Empty)
                setPasswordToolStripMenuItem.Text = "&Set password";
            else
                setPasswordToolStripMenuItem.Text = "&Change password";
        }

        private void modifyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string newname = Microsoft.VisualBasic.Interaction.InputBox("Modify", "Modify", ((Product)productsBox.Items[productsBox.SelectedIndex]).Name, -1, -1);
            if (newname != string.Empty)
            {
                sel = productsBox.SelectedIndex;
                Database.UpdateProduct(((Product)productsBox.Items[productsBox.SelectedIndex]).Id, newname, Password);
                products = new List<Product>(Database.GetProducts(Password));
                productsBox.DataSource = null;
                productsBox.DataSource = products;
                productsBox.DisplayMember = "Name";
                productsBox.ValueMember = "Id";
                productsBox.SelectedIndex = sel;
            }
        }

        private void randomKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string randomkey = Database.RandomKey(false, ((Product)productsBox.Items[productsBox.SelectedIndex]).Id, Password);
            if (randomkey != string.Empty)
            {
                Clipboard.SetText(randomkey);
                MessageBox.Show("Random key copied to clipboard.");
            }
        }

        private void randomKeyDeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Are you sure you wish to copy & delete a random key?\r\nThis is irreversible!", "Keystore", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (dr == DialogResult.OK)
            {
                string randomkey = Database.RandomKey(true, ((Product)productsBox.Items[productsBox.SelectedIndex]).Id, Password);
                if (randomkey != string.Empty)
                {
                    Clipboard.SetText(randomkey);
                    MessageBox.Show("Random key copied and deleted!");
                }
            }
        }
    }
}
