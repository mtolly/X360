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
using X360.Profile;
using X360.IO;
using X360;

namespace Le_Fluffie
{
    public partial class GPDViewer : Office2007Form
    {
        GameGPD xgame;
        MainForm xparent;
        string xfile;

        void set(GameGPD xin)
        {
            xgame = xin;
            for (int i = 0; i < xin.Achievements.Length; i++)
                listBox2.Items.Add(xin.Achievements[i].Title);
            if (listBox2.Items.Count > 0)
                listBox2.SelectedIndex = 0;
            Image xTitleIMGS = xin.GetImageByID(0x8000);
            if (xTitleIMGS != null)
                pictureBox4.Image = xTitleIMGS;
            else pictureBox4.Image = PublicResources.NoImage;
            string xTitleStrings = xin.GetStringByID(0x8000);
            if (xTitleStrings != null)
                textBoxX4.Text = xTitleStrings;
            else textBoxX4.Text = "Unknown";
            for (int i = 0; i < xin.SyncRecords.Length; i++)
                listBox1.Items.Add(i);
            for (int i = 0; i < xin.IndexRecords.Length; i++)
                listBox3.Items.Add(i);
            foreach (Setting x in xin.UserSettings)
            {
                ListViewItem y = new ListViewItem(x.ID.ToString("X"));
                y.SubItems.Add(((GPDIDs)x.ID).ToString());
                listView2.Items.Add(y);
            }
            foreach (ImageEntry x in xin.Images)
                listBox4.Items.Add(x.ID);
            if (listBox4.Items.Count > 0)
                listBox4.SelectedIndex = 0;
            if (listView2.Items.Count > 0)
                listView2.Items[0].Selected = true;
            //Setting x1 = xin.GetSettingByType(GPDIDs.GCardPictureKey);
            //Setting x2 = xin.GetSettingByType(GPDIDs.GCardPersonalPicture);
            //x1.Data = x2.Data;
            //x1.Update(true);
        }

        public GPDViewer(GameGPD xin)
        {
            InitializeComponent();
            set(xin);
        }

        public GPDViewer(GameGPD xin, string file, MainForm parent)
        {
            InitializeComponent();
            xparent = parent;
            xfile = file;
            set(xin);
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = listBox2.SelectedIndex;
            if (idx < 0)
                return;
            textBoxX7.Text = xgame.Achievements[idx].Title;
            textBoxX5.Text = xgame.Achievements[idx].Description1;
            textBoxX6.Text = xgame.Achievements[idx].Description2;
            pictureBox3.Image = xgame.GetAchievementImage(idx);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = listBox1.SelectedIndex;
            if (idx < 0)
                return;
            textBoxX1.Text = xgame.SyncRecords[idx].ServerSync.ToString();
            textBoxX2.Text = xgame.SyncRecords[idx].Last.ToString("X2");
            textBoxX3.Text = xgame.SyncRecords[idx].Next.ToString("X2");
        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = listBox3.SelectedIndex;
            if (idx < 0)
                return;
            listView1.Items.Clear();
            foreach (SyncPair x in xgame.IndexRecords[idx].SyncPairs)
            {
                ListViewItem y = new ListViewItem(x.ID.ToString("X"));
                y.SubItems.Add(x.Sync.ToString("X"));
                listView1.Items.Add(y);
            }
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedIndices.Count == 0)
                return;
            int idx = listView2.SelectedIndices[0];
            if (xgame.UserSettings[idx].ContentType == SettingType.Null)
                richTextBox1.Text = "null";
            else if (xgame.UserSettings[idx].ContentType == SettingType.Binary ||
                xgame.UserSettings[idx].ContentType == SettingType.Context)
            {
                string hex = ((byte[])xgame.UserSettings[idx].Data).HexString();
                string spaces = hex.Substring(0, 2);
                for (int i = 1; i < (hex.Length / 2); i++)
                    spaces += " " + hex.Substring(i * 2, 2);
                richTextBox1.Text = spaces;
            }
            else richTextBox1.Text = xgame.UserSettings[idx].Data.ToString();
        }

        private void GPDViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (xparent != null)
                xparent.Files.Remove(xfile);
        }

        private void listBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox4.SelectedIndex != -1)
                pictureBox1.Image = xgame.Images[listBox4.SelectedIndex].ImageOutput;
        }
    }
}
