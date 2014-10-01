// Program is protected under GPL Licensing and Copyrighted to alias DJ Shepherd

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using X360.Profile;
using X360.IO;
using System.IO;

namespace Le_Fluffie
{
    public partial class MultiAdder : Office2007Form
    {
        public MultiAdder()
        {
            InitializeComponent();
        }

        private void addFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Multiselect = true;
            if (OFD.ShowDialog() != DialogResult.OK)
                return;
            menuStrip1.Enabled = buttonX1.Enabled = buttonX2.Enabled = listView1.Enabled = false;
            List<ListViewItem> xItems = new List<ListViewItem>();
            progressBarX1.Value = 0;
            progressBarX1.Maximum = OFD.FileNames.Length;
            textBoxX1.Text = "Status: Reading files...";
            textBoxX1.Refresh();
            foreach (string x in OFD.FileNames)
            {
                GameGPD z = null;
                try
                {
                    z = new GameGPD(x, ProfileTools.GPDNameToID(Path.GetFileName(x)));
                    ListViewItem u = new ListViewItem(z.GetStringByID((long)GPDIDs.ThisTitle));
                    if (u.SubItems[0].Text == null || u.SubItems[0].Text == "")
                    {
                        z.Close();
                        continue;
                    }
                    u.SubItems.Add(z.TitleID.ToString("X2"));
                    u.SubItems.Add(x);
                    z.Close();
                    xItems.Add(u);
                }
                catch { if (z != null) z.Close(); }
                progressBarX1.Value++;
                Application.DoEvents();
            }
            listView1.Items.AddRange(xItems.ToArray());
            textBoxX1.Text = "Status: Idle...";
            menuStrip1.Enabled = buttonX1.Enabled = buttonX2.Enabled = listView1.Enabled = true;
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem x in listView1.Items)
            {
                if (x.SubItems[1].Text != "0")
                    continue;
                MessageBox.Show("At least one or more items do not have an ID, please fix. Stopped at:\n" +
                    x.SubItems[2].Text);
                return;
            }
            if (listView1.Items.Count != 0)
                DialogResult = DialogResult.OK;
            this.Close();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete?", "Warning", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            int idx = listView1.SelectedIndices[0];
            listView1.Items.RemoveAt(idx);
        }

        private void changeIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int idx = listView1.SelectedIndices[0];
            StatsForm y = new StatsForm(Convert.ToUInt32(listView1.Items[idx].SubItems[1].Text, 16));
            if (y.ShowDialog() != DialogResult.OK)
                return;
            string titid = y.ChosenID.ToString("X2");
            foreach (ListViewItem z in listView1.Items)
            {
                if (z.SubItems[1].Text != titid)
                    continue;
                MessageBox.Show("Error: Contains ID already");
                return;
            }
            listView1.Items[idx].SubItems[1].Text = titid;
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (listView1.SelectedIndices.Count == 0)
                changeIDToolStripMenuItem.Enabled = false;
            else changeIDToolStripMenuItem.Enabled = true;
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
