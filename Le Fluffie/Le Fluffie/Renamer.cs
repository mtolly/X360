// Program is protected under GPL Licensing and Copyrighted to alias DJ Shepherd

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using X360.Other;

namespace Le_Fluffie
{
    public partial class Renamer : Form
    {
        public Renamer(string xName, bool IsSTFS)
        {
            InitializeComponent();
            textBoxX1.Text = xName;
            if (IsSTFS)
                textBoxX1.MaxLength = 0x28;
            else textBoxX1.MaxLength = 0x2A;
            DialogResult = DialogResult.Cancel;
        }
        string xname = "";
        public string FileName { get { return xname; }}

        private void buttonX1_Click(object sender, EventArgs e)
        {
            try { textBoxX1.Text.IsValidXboxName(); }
            catch { MessageBox.Show("Invalid Characters"); return; }
            xname = textBoxX1.Text;
            base.DialogResult = DialogResult.OK;
            this.Close();
        }

        new public DialogResult ShowDialog()
        {
            xname = textBoxX1.Text;
            base.ShowDialog();
            return DialogResult;
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
