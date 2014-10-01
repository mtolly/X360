using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using DevComponents.AdvTree;
using X360.FATX;
using X360.Other;
using X360.IO;
using X360;

namespace Le_Fluffie
{
    public partial class FATXViewer : Office2007Form
    {
        FATXDrive xDrive;
        MainForm xparent;
        string xthisfile;
        Node lastselected = new Node();

        public FATXViewer(FATXDrive drivein, string filein, MainForm par)
        {
            InitializeComponent();
            node1.Text = drivein.DriveName;
            if (drivein.IsDriveIO)
                createDiskImageToolStripMenuItem.Enabled = restoreImageToolStripMenuItem.Enabled = false;
            xDrive = drivein;
            xthisfile = filein;
            xparent = par;
            set();
        }

        void set()
        {
            node1.Nodes.Clear();
            foreach (FATXPartition part in xDrive.Partitions)
            {
                Node node = new Node();
                node.Tag = part;
                node.Text = part.PartitionName;
                foreach (FATXFolderEntry x in part.Folders)
                {
                    Node node2 = new Node();
                    node2.Text = x.Name;
                    node2.Tag = x;
                    node2.NodeClick += new EventHandler(node_NodeClick);
                    node.Nodes.Add(node2);
                }
                foreach (FATXPartition x in part.SubPartitions)
                {
                    Node node2 = new Node();
                    node2.Text = x.PartitionName;
                    node2.Tag = x;
                    node2.NodeClick += new EventHandler(node2_NodeClick);
                    node.Nodes.Add(node2);
                }
                listView1.Items.Clear();
                foreach (FATXFileEntry x in part.Files)
                    listView1.Items.Add(GetItem(x));
                node1.Nodes.Add(node);
            }
        }

        void node2_NodeClick(object sender, EventArgs e)
        {
            if (lastselected == advTree1.SelectedNode)
                return;
            Node sndr = lastselected = advTree1.SelectedNode;
            sndr.Nodes.Clear();
            listView1.Items.Clear();
            FATXPartition part = (FATXPartition)sndr.Tag;
            listView1.Enabled = advTree1.Enabled = menuStrip1.Enabled = false;
            foreach (FATXFolderEntry x in part.Folders)
            {
                Node node2 = new Node();
                node2.Text = x.Name;
                node2.Tag = x;
                node2.NodeClick += new EventHandler(node_NodeClick);
                sndr.Nodes.Add(node2);
            }
            foreach (FATXPartition x in part.SubPartitions)
            {
                Node node2 = new Node();
                node2.Text = x.PartitionName;
                node2.Tag = x;
                node2.NodeClick += new EventHandler(node2_NodeClick);
                sndr.Nodes.Add(node2);
            }
            listView1.Items.Clear();
            foreach (FATXFileEntry x in part.Files)
                listView1.Items.Add(GetItem(x));
            advTree1.SelectedNode.Expand();
            listView1.Enabled = advTree1.Enabled = menuStrip1.Enabled = true;
        }

        ListViewItem GetItem(FATXFileEntry tag)
        {
            ListViewItem x = new ListViewItem(tag.Name);
            x.Tag = tag;
            x.SubItems.Add("");
            x.SubItems.Add(tag.Size.ToString());
            return x;
        }

        void node_NodeClick(object sender, EventArgs e)
        {
            if (lastselected == advTree1.SelectedNode)
                return;
            listView1.Items.Clear();
            lastselected = advTree1.SelectedNode;
            foreach (Node y in advTree1.SelectedNode.Parent.Nodes)
                y.Nodes.Clear();
            FATXFolderEntry sndr = (FATXFolderEntry)((Node)sender).Tag;
            FATXReadContents x = sndr.Read();
            if (x == null)
                return;
            listView1.Enabled = advTree1.Enabled = menuStrip1.Enabled = false;
            List<ListViewItem> xlist = new List<ListViewItem>();
            foreach (FATXPartition y in x.SubPartitions)
            {
                Node node2 = new Node();
                node2.Text = y.PartitionName;
                node2.Tag = y;
                node2.NodeClick += new EventHandler(node2_NodeClick);
                advTree1.SelectedNode.Nodes.Add(node2);
            }
            foreach (FATXFolderEntry y in x.Folders)
            {
                Node node2 = new Node();
                node2.Text = y.Name;
                node2.Tag = y;
                node2.NodeClick += new EventHandler(node_NodeClick);
                node2.ContextMenu = contextMenuStrip2;
                advTree1.SelectedNode.Nodes.Add(node2);
            }
            foreach (FATXFileEntry y in x.Files)
                xlist.Add(GetItem(y));
            listView1.Items.Clear();
            listView1.Items.AddRange(xlist.ToArray());
            advTree1.SelectedNode.Expand();
            listView1.Enabled = advTree1.Enabled = menuStrip1.Enabled = true;
        }

        private void createDiskImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string file = VariousFunctions.GetUserFileLocale("Save to where?", "Binary Image|*.bin", false);
            if (file == null)
                return;
            advTree1.Enabled = listView1.Enabled = menuStrip1.Enabled = false;
            xDrive.ExtractImage(file);
            advTree1.Enabled = listView1.Enabled = menuStrip1.Enabled = true;
        }

        private void restoreImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string file = VariousFunctions.GetUserFileLocale("Save to where?", "Binary Image|*.bin", false);
            if (file == null)
                return;
            advTree1.Enabled = listView1.Enabled = menuStrip1.Enabled = false;
            xDrive.RestoreImage(file);
            set();
            advTree1.Enabled = listView1.Enabled = menuStrip1.Enabled = true;
        }

        private void FATXViewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            xparent.Files.Remove(xthisfile);
            xDrive.Close();
        }

        private void getSTFSNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            advTree1.Enabled = listView1.Enabled = menuStrip1.Enabled = false;
            foreach (ListViewItem x in listView1.Items)
                x.SubItems[1].Text = ((FATXFileEntry)x.Tag).GetSTFSName();
            advTree1.Enabled = listView1.Enabled = menuStrip1.Enabled = true;
        }

        private void extractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string y = VariousFunctions.GetUserFolderLocale("Extract to where?");
            if (y == null)
                return;
            advTree1.Enabled = listView1.Enabled = menuStrip1.Enabled = false;
            FATXFolderEntry ent = (FATXFolderEntry)advTree1.Tag;
            ent.Extract(y + "/" + ent.Name, true);
            advTree1.Enabled = listView1.Enabled = menuStrip1.Enabled = true;
        }

        private void addFolderToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Renamer x = new Renamer("<Folder Name>", false);
            if (x.ShowDialog() != DialogResult.OK)
                return;
            advTree1.Enabled = listView1.Enabled = menuStrip1.Enabled = false;
            ((FATXFolderEntry)advTree1.Tag).AddFolder(x.FileName);
            lastselected = new Node();
            node_NodeClick(null, null);
            advTree1.Enabled = listView1.Enabled = menuStrip1.Enabled = true;
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete these items?", "WARNING", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            advTree1.Enabled = listView1.Enabled = menuStrip1.Enabled = false;
            foreach (ListViewItem x in listView1.SelectedItems)
                ((FATXFileEntry)x.Tag).Delete();
            advTree1.Enabled = listView1.Enabled = menuStrip1.Enabled = true;
        }

        private void injectRiskyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FATXFileEntry x = ((FATXFileEntry)listView1.SelectedItems[0].Tag);
            string y = VariousFunctions.GetUserFileLocale("Open a file", VariousFunctions.GetFilter(x.Name), x.Name, true);
            if (y == null)
                return;
            advTree1.Enabled = listView1.Enabled = menuStrip1.Enabled = false;
            x.Inject(y);
            advTree1.Enabled = listView1.Enabled = menuStrip1.Enabled = true;
        }

        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FATXFileEntry x = ((FATXFileEntry)listView1.SelectedItems[0].Tag);
            string filter = VariousFunctions.GetFilter(x.Name);
            if (filter != "")
                filter += "|All|*.*";
            string y = VariousFunctions.GetUserFileLocale("Open a file", filter, x.Name, true);
            if (y == null)
                return;
            advTree1.Enabled = listView1.Enabled = menuStrip1.Enabled = false;
            x.Replace(y);
            advTree1.Enabled = listView1.Enabled = menuStrip1.Enabled = true;
        }

        private void extractToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                FATXFileEntry x = ((FATXFileEntry)listView1.SelectedItems[0].Tag);
                string y = VariousFunctions.GetUserFileLocale("Save to where?", VariousFunctions.GetFilter(x.Name), x.Name, false);
                if (y == null)
                    return;
                advTree1.Enabled = listView1.Enabled = menuStrip1.Enabled = false;
                x.Extract(y);
            }
            else
            {
                string y = VariousFunctions.GetUserFolderLocale("Save to where?");
                if (y == null)
                    return;
                advTree1.Enabled = listView1.Enabled = menuStrip1.Enabled = false;
                foreach (ListViewItem z in listView1.SelectedItems)
                {
                    FATXFileEntry x = ((FATXFileEntry)z.Tag);
                    x.Extract(y + "/" + x.Name);
                }
            }
            advTree1.Enabled = listView1.Enabled = menuStrip1.Enabled = true;
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                extractToolStripMenuItem1.Enabled = replaceToolStripMenuItem.Enabled =
                    injectRiskyToolStripMenuItem.Enabled = deleteToolStripMenuItem1.Enabled = false;
                return;
            }
            if (listView1.SelectedItems.Count != 1)
                replaceToolStripMenuItem.Enabled = injectRiskyToolStripMenuItem.Enabled = false;
            else replaceToolStripMenuItem.Enabled = injectRiskyToolStripMenuItem.Enabled = true;
            extractToolStripMenuItem1.Enabled = deleteToolStripMenuItem1.Enabled = true;
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.All;
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            menuStrip1.Enabled = advTree1.Enabled = listView1.Enabled = false;
            string[] xfiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            List<DJsIO> xIOs = new List<DJsIO>();
            for (int i = 0; i < xfiles.Length; i++)
            {
                try
                {
                    DJsIO xio = new DJsIO(xfiles[i], DJFileMode.Open, true);
                    if (xio.Accessed)
                        xIOs.Add(xio);
                }
                catch { }
            }
            foreach (DJsIO io in xIOs)
            {
                bool contin = true;
                foreach (ListViewItem x in listView1.Items)
                {
                    if (x.Text == io.FileNameShort)
                    {
                        contin = (MessageBox.Show("Do you want to replace this file?", "WARNING", MessageBoxButtons.YesNo) ==
                            DialogResult.Yes);
                        break;
                    }
                }
                if (!contin)
                    continue;
                io.Close();
                ((FATXFolderEntry)advTree1.SelectedNode.Tag).AddFile(io.FileNameShort, io.FileNameLong, AddType.Replace) ;
                io.Dispose();
            }
            lastselected = new Node();
            menuStrip1.Enabled = advTree1.Enabled = listView1.Enabled = true;
            node_NodeClick(null, null);
        }
    }
}
