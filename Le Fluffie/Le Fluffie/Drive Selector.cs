// Program is protected under GPL Licensing and Copyrighted to alias DJ Shepherd

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using X360.FATX;
using X360.IO;
using DevComponents.DotNetBar;

namespace Le_Fluffie
{
    public partial class Drive_Selector : Office2007Form
    {
        public Drive_Selector(MainForm xparent)
        {
            InitializeComponent();
            xDrives = new List<FATXDrive>();
            DeviceReturn[] xdrives = FATXManagement.GetFATXDrives(10);
            foreach (DeviceReturn x in xdrives)
            {
                xDrives.Add(new FATXDrive(x));
                listBox1.Items.Add(x.Name + ":" + xDrives[xDrives.Count - 1].Type.ToString() + ":" + xDrives[xDrives.Count - 1].DriveSizeFriendly);
            }
            par = xparent;
        }

        MainForm par;
        List<FATXDrive> xDrives;
        FATXDrive xChosenDrive;
        public FATXDrive ChosenDrive { get { return xChosenDrive; }}
        public string xfile;

        private void button1_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                string parse = ((string)listBox1.SelectedItem).Split(new char[] { ':' })[0];
                if (!par.Files.Contains(parse))
                {
                    button1.Enabled = true;
                    xChosenDrive = xDrives[listBox1.SelectedIndex];
                    return;
                }
            }
            button1.Enabled = false;
            xChosenDrive = null;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void openImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string xfilez = X360.Other.VariousFunctions.GetUserFileLocale("Open a file", "Binary Image|*.bin|All Files|*.*", true);
            if (xfilez == null)
                return;
            if (par.Files.Contains(xfilez))
            {
                MessageBox.Show("Error: Already opened");
                return;
            }
            FATXDrive xBU = xChosenDrive;
            try { xChosenDrive = new FATXDrive(xfilez); }
            catch (Exception x)
            {
                xChosenDrive = xBU;
                MessageBox.Show(x.Message);
                return;
            }
            xfile = xfilez;
            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
