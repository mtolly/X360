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
using DevComponents.DotNetBar;
using DevComponents.AdvTree;
using X360.IO;
using X360.GDFX;
using X360.Other;

namespace Le_Fluffie
{
    public partial class GDFViewer : Office2007Form
    {
        MainForm parent;
        GDFImage ximg;
        public GDFViewer(GDFImage xIn, MainForm xparent)
        {
            InitializeComponent();
            node1.Tag = xIn.Root;
            node1.NodeClick += new EventHandler(NodeClick);
            parent = xparent;
            ximg = xIn;
            advTree1.SelectedNode = node1;
            NodeClick(node1, null);
        }

        void NodeClick(object sender, EventArgs e)
        {
            GDFFolder fold = (GDFFolder)((Node)sender).Tag;
            advTree1.SelectedNode.Nodes.Clear();
            GDFContents xread = fold.Read();
            foreach (GDFFolder x in xread.Folders)
                advTree1.SelectedNode.Nodes.Add(GetNode(x));
            listView1.Items.Clear();
            foreach (GDFFile x in xread.Files)
            {
                ListViewItem y = new ListViewItem(x.Name);
                y.SubItems.Add(x.Size.ToString());
                y.Tag = x;
                listView1.Items.Add(y);
            }
        }

        Node GetNode(GDFFolder x)
        {
            Node y = new Node();
            y.Tag = x;
            y.Text = x.Name;
            y.NodeClick +=new EventHandler(NodeClick);
            return y;
        }

        private void GDFViewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            parent.Files.Remove(ximg.FileNameLong);
            ximg.Close();
        }

        private void extractToolStripMenuItem_Click(object sender, EventArgs e)
        { 
            GDFFile x = (GDFFile)listView1.SelectedItems[0].Tag;
            string result = VariousFunctions.GetUserFileLocale("Save to Where?", "", false);
            if (result == null)
                return;
            menuStrip1.Enabled = listView1.Enabled = advTree1.Enabled = false;
            textBoxX1.Text = "Status: Extracting file...";
            x.Extract(result);
            textBoxX1.Text = "Status: Idle...";
            menuStrip1.Enabled = listView1.Enabled = advTree1.Enabled = true;
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                extractToolStripMenuItem.Enabled = injectToolStripMenuItem.Enabled = false;
            else extractToolStripMenuItem.Enabled = injectToolStripMenuItem.Enabled = true;
        }

        private void injectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GDFFile x = (GDFFile)listView1.SelectedItems[0].Tag;
            string result = VariousFunctions.GetUserFileLocale("Open a File", "", true);
            if (result == null)
                return;
            FileInfo y = new FileInfo(result);
            if (y.Length != x.Size)
            {
                MessageBox.Show("Error: Size must be the same");
                return;
            }
            menuStrip1.Enabled = listView1.Enabled = advTree1.Enabled = false;
            textBoxX1.Text = "Status: Injecting file...";
            x.Inject(result);
            textBoxX1.Text = "Status: Idle...";
            menuStrip1.Enabled = listView1.Enabled = advTree1.Enabled = true;
        }

        private void buildIntoPackageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PackageCreatez x = new PackageCreatez(parent, PackType.SVOD);
            x.numericUpDown11.Value = ximg.Deviation;
            x.buttonX2.Enabled = false;
            x.textBoxX2.Text = ximg.FileNameLong;
            x.ximg = ximg;
            this.Dispose();
            x.Show();
        }
    }
}
