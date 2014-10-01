// Program is protected under GPL Licensing and Copyrighted to alias DJ Shepherd

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using X360.Other;

namespace Le_Fluffie
{
    public partial class GoToDialog : Office2007Form
    {
        public GoToDialog()
        {
            InitializeComponent();
        }

        public string ChosenPath { get { return textBoxX1.Text; } }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            if (textBoxX1.Text != "")
            {
                textBoxX1.Text = textBoxX1.Text.Replace("\\", "/");
                if (textBoxX1.Text[0] == '/')
                    textBoxX1.Text = textBoxX1.Text.Substring(0, textBoxX1.Text.Length - 1);
                if (textBoxX1.Text[textBoxX1.Text.Length - 1] == '/')
                    textBoxX1.Text = textBoxX1.Text.Substring(1, textBoxX1.Text.Length - 1);
                string[] Folders = textBoxX1.Text.Split(new char[] { '/' });
                foreach (string x in Folders)
                {
                    try { x.IsValidXboxName(); }
                    catch { MessageBox.Show("Please make sure the path is valid"); return; }
                }
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
