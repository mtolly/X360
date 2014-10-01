using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using X360.STFS;
using X360.IO;
using X360.Other;

namespace Le_Fluffie
{
    public partial class MultiSTFS : Office2007Form
    {
        MainForm par;
        public MultiSTFS(MainForm parent)
        {
            InitializeComponent();
            par = parent;
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to change these packages?", "WARNING", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            menuStrip1.Enabled = groupPanel1.Enabled = listBox1.Enabled = false;
            progressBarX1.Value = 0;
            progressBarX1.Maximum = listBox1.Items.Count;
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                STFSPackage x = null;
                try { x = new STFSPackage((string)listBox1.Items[i], null); }
                catch { }
                if (x != null)
                    fix(i);
                progressBarX1.Value++;
            }
            menuStrip1.Enabled = groupPanel1.Enabled = listBox1.Enabled = true;
        }

        private void clearAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

        private void deleteFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBox1.Items.RemoveAt(listBox1.SelectedIndex);
        }

        private void addFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Title = "Open Xbox Live Packages";
            ofd.Filter = "";
            if (ofd.ShowDialog() != DialogResult.OK)
                return;
            menuStrip1.Enabled = groupPanel1.Enabled = listBox1.Enabled = false;
            progressBarX1.Value = 0;
            progressBarX1.Maximum = ofd.FileNames.Length;
            foreach (string x in ofd.FileNames)
            {
                DJsIO y = null;
                try { y = new DJsIO(x, DJFileMode.Open, true); }
                catch { progressBarX1.Value++; continue; }
                if (!y.Accessed)
                {
                    progressBarX1.Value++;
                    continue;
                }
                y.Position = 0;
                if (y.ReadUInt32() == (uint)AllMagic.CON)
                    listBox1.Items.Add(x);
                y.Dispose();
                if (checkBoxX3.Checked)
                    fix(listBox1.Items.Count - 1);
                progressBarX1.Value++;
            }
            menuStrip1.Enabled = groupPanel1.Enabled = listBox1.Enabled = true;
        }

        private void MultiSTFS_FormClosed(object sender, FormClosedEventArgs e)
        {
            par.multiSTFSFixerToolStripMenuItem.Enabled = true;
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (listBox1.SelectedIndex < 0)
                deleteFileToolStripMenuItem.Enabled = false;
            else deleteFileToolStripMenuItem.Enabled = true;
        }

        void fix(int i)
        {
            STFSPackage x = null;
            try { x = new STFSPackage((string)listBox1.Items[i], null); }
            catch { }
            if (x != null)
            {
                if (x.ParseSuccess)
                {
                    if (checkBoxX1.Checked)
                        x.Header.MakeAnonymous();
                    if (checkBoxX2.Checked)
                        x.FlushPackage(par.PublicKV);
                    else x.UpdateHeader(par.PublicKV);
                }
                x.CloseIO();
            }
        }

        private void refixToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count == 0)
                return;
            fix(listBox1.SelectedIndex);
        }

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.All;
        }

        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            menuStrip1.Enabled = groupPanel1.Enabled = listBox1.Enabled = false;
            string[] xfiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string x in xfiles)
            {
                DJsIO y = null;
                try { y = new DJsIO(x, DJFileMode.Open, true); }
                catch { continue; }
                if (!y.Accessed)
                    continue;
                y.Position = 0;
                if (y.ReadUInt32() == (uint)AllMagic.CON)
                    listBox1.Items.Add(x);
                y.Dispose();
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
                Application.DoEvents();
                if (checkBoxX3.Checked)
                    fix(listBox1.Items.Count - 1);
            }
            menuStrip1.Enabled = groupPanel1.Enabled = listBox1.Enabled = true;
        }
    }
}
