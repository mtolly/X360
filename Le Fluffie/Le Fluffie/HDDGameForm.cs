// Program is protected under GPL Licensing and Copyrighted to alias DJ Shepherd

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevComponents;
using DevComponents.DotNetBar.Controls;
using DevComponents.DotNetBar;
using X360.SVOD;
using X360.STFS;
using X360.IO;
using System.IO;
using System.Threading;

namespace Le_Fluffie
{
    public partial class HDDGameForm : Office2007Form
    {
        SVODPackage xGame;
        MainForm xparent;
        string xfile;

        public HDDGameForm(SVODPackage x, string file, MainForm parent)
        {
            xGame = x;
            InitializeComponent();
            xparent = parent;
            xfile = file;
            List<PackageType> xTypes = new List<PackageType>();
            xTypes.Add(PackageType.OriginalXboxGame);
            xTypes.Add(PackageType.HDDInstalledGame);
            xTypes.Add(PackageType.GamesOnDemand);
            xTypes.Add(PackageType.SocialTitle);
            comboBoxEx1.DataSource = xTypes.ToArray();
            comboBoxEx1.SelectedItem = x.Header.ThisType;
            textBoxX2.Text = "Deviation: 0x" + x.Deviation.ToString("X");
        }

        void b1(object xIO)
        {
            xGame.ExtractData((DJsIO)xIO);
            Thread.CurrentThread.Abort();
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            if (buttonX1.Text == "Load Data")
            {
                string y = X360.Other.VariousFunctions.GetUserFolderLocale("Open the folder location", xGame.FileParentPath);
                if (y == null)
                    return;
                if (!xGame.LoadData(y))
                {
                    MessageBox.Show("Error when loading data");
                    return;
                }
                buttonX1.Text = "Extract Data";
                checkBoxX1.Enabled = true;
                return;
            }
            DJsIO xIO = new DJsIO(DJFileMode.Create, "Save to where?", "", true);
            if (!xIO.Accessed)
                return;
            buttonX1.Enabled = false;
            buttonX2.Enabled = false;
            textBoxX1.Text = "Status: Extracting...";
            Thread x = new Thread(new ParameterizedThreadStart(b1));
            x.Start(xIO);
            while (x.IsAlive)
                Application.DoEvents();
            textBoxX1.Text = "Status: Idle...";
            buttonX1.Enabled = true;
            buttonX2.Enabled = true;
        }

        void b2()
        {
            X360.STFS.RSAParams xparams;
            if (radioButton1.Checked)
                xparams = xparent.PublicKV;
            else if (radioButton2.Checked)
                xparams = new X360.STFS.RSAParams(StrongSigned.PIRS);
            else xparams = new X360.STFS.RSAParams(StrongSigned.LIVE);
            if (checkBoxX1.Checked)
                xGame.FixPackage(xparams);
            else xGame.WriteHeader(xparams);
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked && MessageBox.Show("Are you sure you want to do this? Signing it wif\na different KV other than your own will cause the game not\nto work on a stock unJTAG'ed system",
                "WARNING", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            buttonX1.Enabled = false;
            buttonX2.Enabled = false;
            textBoxX1.Text = "Status: Fixing...";
            Thread x = new Thread(new ThreadStart(b2));
            x.Start();
            while (x.IsAlive)
                Application.DoEvents();
            textBoxX1.Text = "Status: Idle...";
            buttonX1.Enabled = true;
            buttonX2.Enabled = true;
        }

        private void HDDGameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            xGame.Close();
            xparent.Files.Remove(xfile);
        }

        private void comboBoxEx1_SelectedIndexChanged(object sender, EventArgs e)
        {
            xGame.Header.ThisType = (PackageType)comboBoxEx1.SelectedItem;
        }
    }
}
