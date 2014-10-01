// Program is protected under GPL Licensing and Copyrighted to alias DJ Shepherd

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using DevComponents;
using DevComponents.DotNetBar.Controls;
using DevComponents.DotNetBar;
using X360;

namespace Le_Fluffie
{
    public partial class About : Office2007Form
    {
        public About()
        {
            InitializeComponent();
            textBoxX1.Text = XAbout.Development;
            textBoxX2.Text = XAbout.Legal;
            textBoxX3.Text = XAbout.Programmer;
            textBoxX5.Text = XAbout.GNUProtected;
            textBoxX4.Text = "Build's: Le Fluffie: " + Application.ProductVersion +
                " --- X360: " + X360.XAbout.Build;
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            Process.Start(XAbout.Donate);
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void About_Shown(object sender, EventArgs e)
        {
            textBoxX1.SelectionLength = 0;
        }
    }
}
