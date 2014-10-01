// Program is protected under GPL Licensing and Copyrighted to alias DJ Shepherd

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Web;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Threading;
using DevComponents.DotNetBar;
using X360;

namespace Le_Fluffie
{
    public partial class Updater : Office2007Form
    {
        string UpdateLoc = "";
        public Updater()
        {
            InitializeComponent();
            textBoxX1.Text = "Checking X360...";
            UpdateReturn xReturn = XAbout.CheckForUpdate();
            if (!xReturn.ServerReached)
                textBoxX2.Text = "X360: Error";
            else if (xReturn.NeedsUpdate)
            {
                textBoxX2.Text = "X360: Not Up-To-Date";
                textBoxX4.Text += "\r\nX360: " + xReturn.UpdateNotes + "\r\n";
                buttonX1.Enabled =
                    buttonX2.Enabled = true;
            }
            else textBoxX2.Text = "X360: Current";
            textBoxX1.Text = "Checking Le Fluffie...";
            try
            {
                StreamReader x = X360.Other.VariousFunctions.GetWebPageResponse("http://skunkiebutt.com/ProductCheck.php?product=Le Fluffie&command=read");
                string version = x.ReadLine();
                if ((version != Application.ProductVersion))
                {
                    textBoxX3.Text = "Le Fluffie: Not Up-To-Date";
                    UpdateLoc = x.ReadLine();
                    textBoxX4.Text += "\r\nLe Fluffie: " + x.ReadLine();
                    buttonX1.Enabled =
                        buttonX2.Enabled = true;
                }
                else textBoxX3.Text = "Le Fluffie: Current";
                x.Dispose();
            }
            catch { textBoxX2.Text = "Le Fluffie: Error"; }
            textBoxX1.Text = "Status: Idle...";
        }

        public bool UpdateNeeded { get { return buttonX1.Enabled; } }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            Process.Start("http://skunkiebutt.com/programs.html");
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            try
            {
                X360.IO.DJsIO x = new X360.IO.DJsIO(Application.StartupPath + "/LFLiveUpdater.exe", X360.IO.DJFileMode.Create, true);
                x.Position = 0;
                x.Write(global::Le_Fluffie.Properties.Resources.LFLiveUpdater);
                x.Flush();
                x.Dispose();
                ProcessStartInfo y = new ProcessStartInfo(Application.StartupPath + "/LFLiveUpdater.exe");
                y.UseShellExecute = false;
                Process updater = Process.Start(y);
                Visible = false;
                updater.WaitForExit();
                Visible = true;
            }
            catch { }
            MessageBox.Show("Error in updating, please manually download");
        }
    }
}
