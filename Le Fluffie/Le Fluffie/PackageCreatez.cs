// Program is protected under GPL Licensing and Copyrighted to alias DJ Shepherd

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using X360.STFS;
using X360.Other;
using X360.IO;
using X360.SVOD;
using X360.GDFX;
using DevComponents;
using DevComponents.DotNetBar.Controls;
using DevComponents.DotNetBar;
using DevComponents.AdvTree;
using System.IO;

namespace Le_Fluffie
{
    public partial class PackageCreatez : Office2007Form
    {
        bool ptcheck(PackageType xin)
        {
            if (xin == PackageType.HDDInstalledGame ||
                xin == PackageType.OriginalXboxGame ||
                xin == PackageType.GamesOnDemand ||
                xin == PackageType.SocialTitle)
                return true;
            else return false;
        }

        public PackageCreatez(MainForm xParent, PackType xThisType)
        {
            this.MdiParent = xParent;
            xparent = xParent;
            InitializeComponent();
            List<PackageType> y = new List<PackageType>();
            if (xThisType == PackType.STFS)
            {
                xsession = new CreateSTFS();
                PackageType[] x = (PackageType[])Enum.GetValues(typeof(PackageType));
                y.AddRange(x);
                y.RemoveAll(new Predicate<PackageType>(ptcheck));
                node1.DataKey = (ushort)0xFFFF;
                node1.NodeClick += new EventHandler(xReturn_NodeClick);
                comboBoxEx1.SelectedIndex = 0;
                comboBoxEx4.DataSource = Enum.GetValues(typeof(SphereColor));
                comboBoxEx4.SelectedIndex = 0;
                comboBoxEx5.DataSource = Enum.GetValues(typeof(DashStyle));
                comboBoxEx5.SelectedIndex = 0;
                numericUpDown4.Value = xsession.ThemeSettings.AvatarLightingAmbient;
                numericUpDown7.Value = xsession.ThemeSettings.AvatarLightingDirectional3;
                numericUpDown8.Value = xsession.ThemeSettings.AvatarLightingDirectional0;
                numericUpDown9.Value = xsession.ThemeSettings.AvatarLightingDirectional1;
                numericUpDown10.Value = xsession.ThemeSettings.AvatarLightingDirectional2;
                advTree1.SelectedIndex = 0;
                tabItem7.Visible = false;
            }
            else
            {
                y.Add(PackageType.OriginalXboxGame);
                y.Add(PackageType.HDDInstalledGame);
                y.Add(PackageType.GamesOnDemand);
                y.Add(PackageType.SocialTitle);
                tabItem6.Visible = false;
                xhead = new HeaderData();
                tabItem5.Visible = false;
                tabItem4.Visible = false;
                comboBoxEx1.Visible = false;
            }
            xtype = xThisType;
            comboBoxEx2.DataSource = y.ToArray();
            comboBoxEx2.SelectedIndex = 0;
            comboBoxEx3.DataSource = Enum.GetValues(typeof(Languages));
            comboBoxEx3.SelectedIndex = 0;
            SetText();
        }

        MainForm xparent;
        CreateSTFS xsession = null;
        public GDFImage ximg = null;
        PackType xtype = PackType.STFS;
        HeaderData xhead = null;

        HeaderData HeaderInfo
        {
            get
            {
                if (xhead != null)
                    return xhead;
                return xsession.HeaderData;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RSAParams xParams;
            string xOut = null;
            if (xtype == PackType.STFS)
            {
                xOut = VariousFunctions.GetUserFileLocale("Save to where?", "", false);
                if (xOut == null)
                    return;
            }
            else
            {
                if (ximg == null)
                {
                    MessageBox.Show("Please select an image");
                    return;
                }
                string y = VariousFunctions.GetUserFolderLocale("Select a save location");
                if (y == null)
                    return;
                xOut = y;
            }
            if (radioButton1.Checked)
                xParams = xparent.PublicKV;
            else if (radioButton2.Checked)
                xParams = new RSAParams(StrongSigned.LIVE);
            else xParams = new RSAParams(StrongSigned.PIRS);
            if (xtype == PackType.STFS)
            {
                LogRecord rec = new LogRecord();
                STFSPackage xy = new STFSPackage(xsession, xParams, xOut, rec);
                PackageExplorer z = new PackageExplorer(xparent);
                z.set(ref xy);
                z.listBox4.Items.AddRange(rec.Log);
                this.Close();
                z.Show();
            }
            else
            {
                CreateSVOD z = new CreateSVOD(ximg, xOut, xhead);
                if (z.Create(xParams, (PackageType)comboBoxEx2.SelectedItem))
                    MessageBox.Show("Completed successfully");
                else MessageBox.Show("Unsuccessful build");
            }
            this.Dispose();
        }

        private void comboBoxEx1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxEx1.SelectedIndex == 0)
                xsession.STFSType = STFSType.Type0;
            else xsession.STFSType = STFSType.Type1;
        }
        
        void SetText()
        {
            HeaderInfo.SetLanguage((Languages)comboBoxEx3.Items[comboBoxEx3.SelectedIndex]);
            if (radioButton4.Checked)
                textBoxX4.Text = HeaderInfo.Title_Display;
            else textBoxX4.Text = HeaderInfo.Description;
        }

        private void comboBoxEx3_SelectedIndexChanged(object sender, EventArgs e) { SetText(); }

        private void radioButton4_CheckedChanged(object sender, EventArgs e) { SetText(); }

        private void radioButton5_CheckedChanged(object sender, EventArgs e) { SetText(); }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
                HeaderInfo.Description = textBoxX4.Text;
            else HeaderInfo.Title_Display = textBoxX4.Text;
        }

        private void textBoxX5_TextChanged(object sender, EventArgs e) { HeaderInfo.Publisher = textBoxX5.Text; }

        private void textBoxX6_TextChanged(object sender, EventArgs e) { HeaderInfo.Title_Package = textBoxX6.Text; }

        private void comboBoxEx2_SelectedIndexChanged(object sender, EventArgs e)
        {
            PackageType xVal = (PackageType)comboBoxEx2.SelectedItem;
            HeaderInfo.ThisType = xVal;
            if (xVal == PackageType.TV || xVal == PackageType.Video ||
                xVal == PackageType.ViralVideo)
                tabControlPanel4.Enabled = true;
            else if (xVal == PackageType.ThematicSkin)
                tabControlPanel5.Enabled = true;
            else
            {
                tabControlPanel4.Enabled = tabControlPanel5.Enabled = false;
                numericUpDown2.Value = numericUpDown3.Value = 0;
                textBoxX7.Text = textBoxX8.Text = "00000000000000000000";
            }
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e) { HeaderInfo.SeasonNumber = (ushort)numericUpDown2.Value; }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e) { HeaderInfo.EpidsodeNumber = (ushort)numericUpDown3.Value; }

        byte[] ToHex(string xIn)
        {
            for (int i = 0; i < (xIn.Length % 2); i++)
                xIn = "0" + xIn;
            List<byte> xReturn = new List<byte>();
            for (int i = 0; i < (xIn.Length / 2); i++)
                xReturn.Add(Convert.ToByte(xIn.Substring(i * 2, 2), 16));
            return xReturn.ToArray();
        }

        private void textBoxX8_TextChanged(object sender, EventArgs e)
        {
            try { HeaderInfo.SeriesID = ToHex(textBoxX8.Text); }
            catch { textBoxX8.Text = HeaderInfo.SeriesID.HexString(); }
        }

        private void addFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Title = "Open files to add";
            if (ofd.ShowDialog() != DialogResult.OK)
                return;
            string xPath = "";
            foreach (string locale in ofd.FileNames)
            {
                if (advTree1.SelectedNode != advTree1.Nodes[0])
                    xPath = ((CFolderEntry)advTree1.SelectedNode.Tag).Path + "/" + Path.GetFileName(locale);
                else xPath = Path.GetFileName(locale);
                if (!xsession.AddFile(locale, xPath))
                    continue;
                CFileEntry ent = xsession.GetFile(xPath);
                ListViewItem xitem = new ListViewItem(ent.Name);
                xitem.Tag = ent;
                listView1.Items.Add(xitem);
            }
        }

        void GetSelFiles(CFolderEntry Folder)
        {
            advTree1.Enabled = listView1.Enabled = false;
            List<ListViewItem> xItems = new List<ListViewItem>();
            CFileEntry[] x = Folder.GetFiles();
            foreach (CFileEntry y in x)
            {
                ListViewItem z = new ListViewItem(y.Name);
                z.Tag = y;
                xItems.Add(z);
            }
            listView1.Items.Clear();
            listView1.Items.AddRange(xItems.ToArray());
            advTree1.Enabled = listView1.Enabled = true;
        }

        Node GetFoldNode(CFolderEntry x)
        {
            Node xReturn = new Node();
            xReturn.Text = x.Name;
            xReturn.Tag = x;
            xReturn.ContextMenu = contextMenuStrip3;
            xReturn.NodeClick += new EventHandler(xReturn_NodeClick);
            return xReturn;
        }

        void xReturn_NodeClick(object sender, EventArgs e)
        {
            Node x = (Node)sender;
            if (advTree1.Nodes[0] != x)
                GetSelFiles((CFolderEntry)x.Tag);
            else GetSelFiles(xsession.RootPath);
        }

        private void addFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Renamer y = new Renamer("", true);
            if (y.ShowDialog() != DialogResult.OK)
                return;
            string xPath = "";
            if (advTree1.SelectedNode != advTree1.Nodes[0])
                xPath = ((CFolderEntry)advTree1.SelectedNode.Tag).Path + "/" + y.FileName;
            else xPath = y.FileName;
            if (!xsession.AddFolder(xPath))
                return;
            advTree1.SelectedNode.Nodes.Add(GetFoldNode(xsession.GetFolder(xPath)));
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            HeaderInfo.TitleID = (uint)numericUpDown1.Value;
        }

        private void textBoxX7_TextChanged(object sender, EventArgs e)
        {
            try { HeaderInfo.SeasonID = ToHex(textBoxX7.Text); }
            catch { textBoxX7.Text = HeaderInfo.SeasonID.HexString(); }
        }

        void SetTransfer()
        {
            if (checkBoxX1.Checked)
                HeaderInfo.IDTransfer = checkBoxX2.Checked ? TransferLock.AllowTransfer : TransferLock.ProfileAllowOnly;
            else if (checkBoxX2.Checked)
                HeaderInfo.IDTransfer = TransferLock.DeviceAllowOnly;
            else HeaderInfo.IDTransfer = TransferLock.NoTransfer;
        }

        private void checkBoxX1_CheckedChanged(object sender, EventArgs e) { SetTransfer(); }

        private void checkBoxX2_CheckedChanged(object sender, EventArgs e) { SetTransfer(); }

        private void textBoxX1_TextChanged(object sender, EventArgs e)
        {
            try { HeaderInfo.DeviceID = ToHex(textBoxX1.Text); }
            catch { textBoxX1.Text = HeaderInfo.DeviceID.HexString(); }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (advTree1.SelectedNode != null)
                addFileToolStripMenuItem.Enabled = true;
            else addFileToolStripMenuItem.Enabled = false;
            if (listView1.SelectedIndices.Count > 0)
            {
                renameToolStripMenuItem.Enabled =
                    deleteToolStripMenuItem.Enabled = true;
            }
            else
            {
                renameToolStripMenuItem.Enabled =
                    deleteToolStripMenuItem.Enabled = false;
            }
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            HeaderInfo.SaveConsoleID = (long)numericUpDown5.Value;
        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            HeaderInfo.ProfileID = (long)numericUpDown6.Value;
        }

        private void numericUpDown8_ValueChanged(object sender, EventArgs e)
        {
            xsession.ThemeSettings.AvatarLightingDirectional0 = numericUpDown8.Value;
        }

        private void numericUpDown9_ValueChanged(object sender, EventArgs e)
        {
            xsession.ThemeSettings.AvatarLightingDirectional1 = numericUpDown9.Value;
        }

        private void numericUpDown10_ValueChanged(object sender, EventArgs e)
        {
            xsession.ThemeSettings.AvatarLightingDirectional2 = numericUpDown10.Value;
        }

        private void numericUpDown7_ValueChanged(object sender, EventArgs e)
        {
            xsession.ThemeSettings.AvatarLightingDirectional3 = (uint)numericUpDown7.Value;
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            xsession.ThemeSettings.AvatarLightingAmbient = (uint)numericUpDown4.Value;
        }

        private void comboBoxEx5_SelectedIndexChanged(object sender, EventArgs e)
        {
            DashStyle xval = (DashStyle)comboBoxEx5.SelectedItem;
            xsession.ThemeSettings.StyleType = xval;
        }

        private void comboBoxEx4_SelectedIndexChanged(object sender, EventArgs e)
        {
            SphereColor xval = (SphereColor)comboBoxEx4.SelectedItem;
            xsession.ThemeSettings.Sphere = xval;
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CFileEntry val = (CFileEntry)listView1.SelectedItems[0].Tag;
            Renamer x = new Renamer(val.Name, true);
            if (x.ShowDialog() != DialogResult.OK)
                return;
            listView1.SelectedItems[0].Text = val.Name = x.FileName;
        }

        private void renameToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CFolderEntry x = (CFolderEntry)advTree1.SelectedNode.Tag;
            Renamer y = new Renamer(x.Name, true);
            if (y.ShowDialog() != DialogResult.OK)
                return;
            node1.Text = x.Name = y.FileName;
        }

        private void deleteFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete?", "WARNING", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            CFolderEntry x = (CFolderEntry)advTree1.SelectedNode.Tag;
            xsession.DeleteFolder(x.Path);
            advTree1.SelectedNode.Remove();
            advTree1.SelectedIndex = 0;
        }

        private void addImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DJsIO xIO = new DJsIO(DJFileMode.Open, "Open a PNG", "PNG File|*.png", true);
            if (!xIO.Accessed)
                return;
            if (xIO.Length > 0x4000)
            {
                MessageBox.Show("Error: Image is too big");
                return;
            }
            byte[] x = xIO.ReadStream();
            try { Image y = x.BytesToImage(); pictureBox1.Image = y; }
            catch (Exception z) { MessageBox.Show(z.Message); return; }
            HeaderInfo.ContentImageBinary = x;
        }

        private void addImageToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DJsIO xIO = new DJsIO(DJFileMode.Open, "Open a PNG", "PNG File|*.png", true);
            if (!xIO.Accessed)
                return;
            if (xIO.Length > 0x4000)
            {
                MessageBox.Show("Error: Image is too big");
                return;
            }
            byte[] x = xIO.ReadStream();
            try { Image y = x.BytesToImage(); pictureBox2.Image = y; }
            catch (Exception z) { MessageBox.Show(z.Message); return; }
            HeaderInfo.PackageImageBinary = x;
        }

        private void PackageCreatez_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ximg != null)
                ximg.Close();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CFileEntry x = (CFileEntry)listView1.SelectedItems[0].Tag;
            xsession.DeleteFile(x.Path);
            listView1.Items.Remove(listView1.SelectedItems[0]);
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            string result = VariousFunctions.GetUserFileLocale("Open a GDF Image", "", true);
            if (result == null)
                return;
            if (ximg != null)
                ximg.Close();
            try
            {
                ximg = new GDFImage(result, (uint)numericUpDown11.Value);
                if (ximg.Valid)
                    textBoxX2.Text = ximg.FileNameLong;
            }
            catch (Exception x)
            {
                ximg = null;
                textBoxX2.Text = "";
                MessageBox.Show(x.Message);
            }
        }
    }

    public static class BytesExt
    {
        public static string HexString(this byte[] x)
        {
            string xReturn = "";
            foreach (byte y in x)
                xReturn += y.ToString("X2");
            return xReturn;
        }
    }
}
