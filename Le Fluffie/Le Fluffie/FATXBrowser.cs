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
using System.Threading;
using X360;
using X360.FATX;
using X360.IO;
using X360.Other;
using DevComponents.DotNetBar;

namespace Le_Fluffie
{
    public partial class FATXBrowser : Office2007Form
    {
        FATXDrive xDrive;
        MainForm xparent;
        string xthisfile;
        MouseButtons x1 = MouseButtons.None;
        MouseButtons x2 = MouseButtons.None;
        void setbutton(MouseButtons x)
        {
            x1 = x2;
            x2 = x;
        }
        bool isitem = false;
        FATXFolderEntry LastFolder;
        string temp = null;
        FileSystemWatcher xWatcher = new FileSystemWatcher("\\");
       
        public FATXBrowser(FATXDrive xDriveIn, string file, MainForm parent)
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            xDrive = xDriveIn;
            xthisfile = file;
            xparent = parent;
            setdrive();
        }
        
        void xWatch_Created(object sender, FileSystemEventArgs e)
        {
            string droppath = Directory.GetParent(e.FullPath).FullName;
            temp = null;
            menuStrip1.Enabled = menuStrip2.Enabled = listView1.Enabled = false;
            status = "Extracting items";
            foreach (ListViewItem y in listView1.SelectedItems)
            {
                switch (y.ImageIndex)
                {
                    case 1:
                        {
                            FATXFolderEntry ent = (FATXFolderEntry)y.Tag;
                            ent.Extract(droppath, true);
                            break;
                        }

                    case 2:
                        {
                            FATXFileEntry ent = ((FATXFileEntry)y.Tag);
                            string xfile = droppath + "/" + ent.Name;
                            ent.Extract(xfile);
                            break;
                        }

                    default: break;
                }
            }
            VariousFunctions.DeleteFile(e.FullPath);
            status = "Idle";
            isitem = false;
            menuStrip1.Enabled = menuStrip2.Enabled = listView1.Enabled = true;
        }

        string status { set { statusToolStripMenuItem.Text = "Status: " + value + " ..."; } }

        void setdrive()
        {
            textBoxX1.Text = "";
            listView1.Items.Clear();
            List<ListViewItem> xlist = new List<ListViewItem>();
            foreach (FATXPartition x in xDrive.Partitions)
                xlist.Add(GetItem(x.PartitionName, 0, x));
            listView1.Items.AddRange(xlist.ToArray());
        }

        void refresh()
        {
            setview(textBoxX1.Text);
        }

        ListViewItem GetItem(string Name, int piccode, object tag)
        {
            ListViewItem x = new ListViewItem(Name);
            x.Font = new Font(x.Font, FontStyle.Bold);
            x.ForeColor = Color.LightBlue;
            x.ImageIndex = piccode;
            x.Tag = tag;
            if (piccode == 2)
                x.Name = x.ToolTipText = ((FATXFileEntry)tag).GetSTFSName();
            return x;
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (x1 != MouseButtons.Left || x2 != MouseButtons.Left)
                return;
            if (listView1.SelectedItems.Count != 1)
                return;
            listView1.Enabled = false;
            status = "Reading Directory";
            ListViewItem selitem = listView1.SelectedItems[0];
            switch (selitem.ImageIndex)
            {
                case 0:
                    {
                        textBoxX1.Text += ((FATXPartition)selitem.Tag).PartitionName + "/";
                        FATXPartition x = ((FATXPartition)selitem.Tag);
                        List<ListViewItem> xlist = new List<ListViewItem>();
                        foreach (FATXFolderEntry y in x.Folders)
                            xlist.Add(GetItem(y.Name, 1, y));
                        foreach (FATXFileEntry y in x.Files)
                            xlist.Add(GetItem(y.Name, 2, y));
                        for (int i = 0; i < x.SubPartitions.Length; i++)
                            xlist.Add(GetItem(x.SubPartitions[i].PartitionName, 0, x.SubPartitions[i]));
                        listView1.Items.Clear();
                        listView1.Items.AddRange(xlist.ToArray());
                    }
                    break;

                case 1:
                    {
                        LastFolder = ((FATXFolderEntry)selitem.Tag);
                        FATXReadContents x = LastFolder.Read();
                        if (x == null)
                            break;
                        textBoxX1.Text += LastFolder.Name + "/";
                        List<ListViewItem> xlist = new List<ListViewItem>();
                        foreach (FATXFolderEntry y in x.Folders)
                            xlist.Add(GetItem(y.Name, 1, y));
                        foreach (FATXFileEntry y in x.Files)
                            xlist.Add(GetItem(y.Name, 2, y));
                        listView1.Items.Clear();
                        listView1.Items.AddRange(xlist.ToArray());
                    }
                    break;

                default: break;
            }
            status = "Idle";
            listView1.Enabled = true;
        }

        private void goToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GoToDialog x = new GoToDialog();
            if (x.ShowDialog() != DialogResult.OK)
                return;
            listView1.Enabled = false;
            status = "Reading Directory";
            FATXFolderEntry xfold;
            FATXReadContents z = xDrive.ReadToFolder(x.ChosenPath, out xfold);
            if (z == null)
            {
                MessageBox.Show("Error: Bad path?");
                status = "Idle";
                listView1.Enabled = true;
                return;
            }
            LastFolder = xfold;
            List<ListViewItem> xlist = new List<ListViewItem>();
            foreach (FATXFolderEntry u in z.Folders)
                xlist.Add(GetItem(u.Name, 1, u));
            foreach (FATXFileEntry u in z.Files)
                xlist.Add(GetItem(u.Name, 2, u));
            foreach (FATXPartition u in z.SubPartitions)
                xlist.Add(GetItem(u.PartitionName, 0, u));
            listView1.Items.Clear();
            listView1.Items.AddRange(xlist.ToArray());
            textBoxX1.Text = x.ChosenPath + "/";
            status = "Idle";
            listView1.Enabled = true;
        }

        private void upDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textBoxX1.Text == "")
                return;
            listView1.Enabled = false;
            if (textBoxX1.Text.CountOfBreak() == 1)
            {
                setdrive();
                listView1.Enabled = true;
                return;
            }
            status = "Reading Directory";
            string manipulate = textBoxX1.Text.Substring(0, textBoxX1.Text.Length - 1);
            int lastidx = manipulate.LastIndexOf('/');
            if (lastidx != -1)
                manipulate = manipulate.Substring(0, lastidx);
            else manipulate = "";
            FATXFolderEntry xfold = null;
            FATXReadContents x = xDrive.ReadToFolder(manipulate, out xfold);
            if (x == null)
            {
                MessageBox.Show("Error: Bad path?");
                status = "Idle";
                listView1.Enabled = true;
                return;
            }
            LastFolder = xfold;
            listView1.Items.Clear();
            foreach (FATXFolderEntry y in x.Folders)
                listView1.Items.Add(GetItem(y.Name, 1, y));
            foreach (FATXFileEntry y in x.Files)
                listView1.Items.Add(GetItem(y.Name, 2, y));
            foreach (FATXPartition y in x.SubPartitions)
                listView1.Items.Add(GetItem(y.PartitionName, 0, y));
            textBoxX1.Text = manipulate + "/";
            status = "Idle";
            listView1.Enabled = true;
        }

        void setview(string locale)
        {
            listView1.Enabled = false;
            status = "Reading Directory";
            FATXFolderEntry xfold = null;
            FATXReadContents x = xDrive.ReadToFolder(locale, out xfold);
            if (x == null)
            {
                MessageBox.Show("Error: Bad path?");
                status = "Idle";
                listView1.Enabled = true;
                return;
            }
            LastFolder = xfold;
            List<ListViewItem> xlist = new List<ListViewItem>();
            foreach (FATXFolderEntry y in x.Folders)
                xlist.Add(GetItem(y.Name, 1, y));
            foreach (FATXFileEntry y in x.Files)
                xlist.Add(GetItem(y.Name, 2, y));
            listView1.Items.Clear();
            listView1.Items.AddRange(xlist.ToArray());
            if (locale[locale.Length - 1] == '/')
                locale = locale.Substring(0, locale.Length - 1);
            textBoxX1.Text = locale + "/";
            listView1.Enabled = true;
            status = "Idle";
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e) { setdrive(); }

        private void contentsToolStripMenuItem_Click(object sender, EventArgs e) { setview("Content/Content/0000000000000000"); }

        private void profilesToolStripMenuItem_Click(object sender, EventArgs e) { setview("Content/Content"); }

        private void gamerPicturesToolStripMenuItem_Click(object sender, EventArgs e) { setview("Content/Content/0000000000000000/FFFE07D1/00020000"); }

        private void themesToolStripMenuItem_Click(object sender, EventArgs e) { setview("Content/Content/0000000000000000/FFFE07D1/00030000"); }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (isitem || textBoxX1.Text.Length < 4)
                return;
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.All;
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            if (isitem)
                return;
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
                    if (x.ImageIndex == 1 && x.Text == io.FileNameShort)
                    {
                        contin = (MessageBox.Show("Do you want to replace this file?", "WARNING", MessageBoxButtons.YesNo) ==
                            DialogResult.Yes);
                        break;
                    }
                }
                if (!contin)
                    continue;
                io.Close();
                if (LastFolder.AddFile(io.FileNameShort, io.FileNameLong, AddType.Replace))
                    refresh();
                io.Dispose();
            }
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            setbutton(e.Button);
            if (e.Button == MouseButtons.Right)
            {
                listView1.ContextMenuStrip = null;
                switch (listView1.SelectedIndices.Count)
                {
                    case 1:
                        {
                            switch (listView1.SelectedItems[0].ImageIndex)
                            {
                                case 0: break;

                                case 1: { listView1.ContextMenuStrip = contextMenuStrip2; }
                                    break;

                                case 2: { listView1.ContextMenuStrip = contextMenuStrip1; }
                                    break;

                                default: break;
                            }
                        }
                        break;

                    default: listView1.ContextMenuStrip = contextMenuStrip3; break;
                }
            }
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 1)
                return;
            FileProp x = new FileProp((FATXFileEntry)listView1.SelectedItems[0].Tag, listView1.SelectedItems[0].ToolTipText);
            x.ShowDialog();
        }

        private void listView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;
            if (e.Button != MouseButtons.Left)
                return;
            isitem = true;
            xparent.Enabled = false;
            string folder = VariousFunctions.GetTempFileLocale();
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            string[] file;
            byte[] rand = new byte[10];
            new Random().NextBytes(rand);
            temp = rand.HexString();
            file = new string[1] { folder + "/" + temp };
            var data = new DataObject(DataFormats.FileDrop, file);
            data.SetData(DataFormats.StringFormat, file[0]);
            File.Create(file[0]).Dispose();
            File.SetAttributes(folder + "/" + temp, FileAttributes.Hidden | FileAttributes.Temporary);
            xWatcher.Filter = temp;
            xWatcher.NotifyFilter = NotifyFilters.FileName;
            xWatcher.Created += new FileSystemEventHandler(xWatch_Created);
            xWatcher.EnableRaisingEvents = xWatcher.IncludeSubdirectories = true;
            DoDragDrop(data, DragDropEffects.Move);
            xparent.Enabled = true;
            Application.DoEvents();
            VariousFunctions.DeleteFile(folder + "/" + temp);
        }

        private void FATXBrowser_FormClosing(object sender, FormClosingEventArgs e)
        {
            xparent.Files.Remove(xthisfile);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete these?", "WARNING", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            listView1.Enabled = false;
            status = "Deleting files";
            foreach (ListViewItem x in listView1.SelectedItems)
            {
                switch (x.ImageIndex)
                {
                    case 2:
                        {
                            FATXFileEntry y = (FATXFileEntry)x.Tag;
                            y.Delete();
                        }
                        break;
                    default: continue;
                }
            }
            status = "Idle";
            refresh();
            listView1.Enabled = true;
        }

        private void replaceBinaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 1)
                return;
            if (MessageBox.Show("Are you sure you want to delete this?", "WARNING", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            FATXFileEntry x = ((FATXFileEntry)listView1.SelectedItems[0].Tag);
            string locale = VariousFunctions.GetUserFileLocale("Save to where?", "", false);
            if (locale == null)
                return;
            listView1.Enabled = false;
            status = "Replacing File";
            if (x.Replace(locale))
                refresh();
            status = "Idle";
            listView1.Enabled = true;
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 1)
                return;
            Renamer x = new Renamer(listView1.SelectedItems[0].Text, false);
            if (x.ShowDialog() != DialogResult.OK)
                return;
            listView1.Enabled = false;
            status = "Renaming";
            FATXFileEntry y = ((FATXFileEntry)listView1.SelectedItems[0].Tag);
            y.Name = x.FileName;
            if (y.WriteEntry())
                refresh();
            listView1.Enabled = true;
            status = "Idle";
        }

        private void extractToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string y = VariousFunctions.GetUserFolderLocale("Select a save location");
            if (y == null)
                return;
            menuStrip1.Enabled = menuStrip2.Enabled = listView1.Enabled = false;
            ((FATXFolderEntry)listView1.SelectedItems[0].Tag).Extract(y, true);
            menuStrip1.Enabled = menuStrip2.Enabled = listView1.Enabled = true;
        }

        private void extractToToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 1)
                extractToolStripMenuItem.Enabled = false;
            else extractToolStripMenuItem.Enabled = true;
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (listView1.SelectedItems.Count != 1)
                contextMenuStrip1.Enabled = false;
            else contextMenuStrip1.Enabled = true;
        }

        private void injectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 1)
                return;
            FATXFileEntry x = ((FATXFileEntry)listView1.SelectedItems[0].Tag);
            string locale = VariousFunctions.GetUserFileLocale("Open File", "", true);
            if (locale == null)
                return;
            listView1.Enabled = false;
            status = "Injecting File";
            if (x.Inject(locale))
                refresh();
            status = "Idle";
            listView1.Enabled = true;
        }

        private void extractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 1)
                return;
            FATXFileEntry x = ((FATXFileEntry)listView1.SelectedItems[0].Tag);
            string locale = VariousFunctions.GetUserFileLocale("Save to where?", "", false);
            if (locale == null)
                return;
            listView1.Enabled = false;
            status = "Injecting File";
            if (x.Extract(locale))
                refresh();
            status = "Idle";
            listView1.Enabled = true;
        }
    }

    static class rawr
    {
        public static int CountOfBreak(this string x)
        {
            int count = 0;
            foreach (char y in x)
            {
                if (y == '/')
                    count++;
            }
            return count;
        }
    }
}
