// Program is protected under GPL Licensing and Copyrighted to alias DJ Shepherd

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Controls;
using DevComponents.AdvTree;
using X360;
using X360.IO;
using X360.Other;
using X360.STFS;
using X360.Profile;
using System.IO;

namespace Le_Fluffie
{
    public partial class PackageExplorer : Office2007Form
    {
        public void Log(string xProcess)
        {
            listBox1.Items.Add(DateTime.Now.ToString() + " - " + xProcess);
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
            Application.DoEvents();
        }

        public List<byte> PassIndex(PassCode[] xIn)
        {
            List<byte> xReturn = new List<byte>();
            for (int i = 0; i < xIn.Length; i++)
            {
                switch (xIn[i])
                {
                    case PassCode.UpDPad:
                        xReturn.Add(1);
                        break;

                    case PassCode.DownDPad:
                        xReturn.Add(2);
                        break;

                    case PassCode.LeftDPad:
                        xReturn.Add(3);
                        break;

                    case PassCode.RightDPad:
                        xReturn.Add(4);
                        break;

                    case PassCode.X:
                        xReturn.Add(5);
                        break;

                    case PassCode.Y:
                        xReturn.Add(6);
                        break;

                    case PassCode.LTrigger:
                        xReturn.Add(7);
                        break;

                    case PassCode.RTrigger:
                        xReturn.Add(8);
                        break;

                    case PassCode.LBumper:
                        xReturn.Add(9);
                        break;

                    case PassCode.RBumper:
                        xReturn.Add(10);
                        break;

                    default:
                        xReturn.Add(0);
                        break;
                }
            }
            return xReturn;
        }

        MainForm xMForm;
        ProfilePackage xprof;
        STFSPackage xpack;
        STFSPackage xPackage
        {
            get
            {
                if (xprof != null)
                    return xprof;
                return xpack;
            }
        }
        GameGPD xLoadedGPD;
        FileEntry xLoadedEntry;
        bool has1 = false, has2 = false;
        RSAParams KV { get { return xMForm.PublicKV; } }

        int SortPointer(FolderEntry x, FolderEntry y)
        {
            return x.FolderPointer.CompareTo(y.FolderPointer);
        }

        void SetNodes()
        {
            node1.Nodes.Clear();
            listView1.Items.Clear();
            node1.DataKey = xPackage.RootDirectory;
            FolderEntry[] x = xPackage.RootDirectory.GetSubFolders();
            foreach (FolderEntry y in x)
                node1.Nodes.Add(GetNode(y));
            xReturn_NodeClick(node1, null);
        }

        Node GetNode(FolderEntry x)
        {
            Node xReturn = new Node();
            xReturn.Text = x.Name;
            xReturn.DataKey = x;
            xReturn.ContextMenu = contextMenuStrip2;
            xReturn.NodeClick += new EventHandler(xReturn_NodeClick);
            return xReturn;
        }

        void xReturn_NodeClick(object sender, EventArgs e)
        {
            Node xsender = (Node)sender;
            FolderEntry x = (FolderEntry)xsender.DataKey;
            FileEntry[] xFiles = x.GetSubFiles();
            FolderEntry[] xFolders = x.GetSubFolders();
            listView1.Items.Clear();
            foreach (FileEntry y in xFiles)
            {
                ListViewItem z = new ListViewItem(y.Name);
                z.SubItems.Add(y.Size.ToString());
                z.Tag = y;
                listView1.Items.Add(z);
            }
            xsender.Nodes.Clear();
            foreach (FolderEntry y in xFolders)
                xsender.Nodes.Add(GetNode(y));
        }

        public void set(ref STFSPackage x)
        {
            xpack = x;
            Log("Package parsed");
            node1.NodeClick += new EventHandler(xReturn_NodeClick);
            SetNodes();
            node1.SetChecked(true, eTreeAction.Expand);
            Log("Contents loaded");
            if (xPackage.FileNameShort.Length < 30)
                this.Text = "Package - " + xPackage.FileNameShort;
            else this.Text = "Package - " + xPackage.FileNameShort.Substring(0, 30) + "...";
            textBoxX1.Text = xPackage.FileNameLong;
            textBoxX2.Text = xPackage.Header.Title_Display;
            textBoxX3.Text = xPackage.Header.Description;
            try { pictureBox1.Image = xPackage.Header.ContentImage; }
            catch { pictureBox1.Enabled = false; }
            try { pictureBox2.Image = xPackage.Header.PackageImage; }
            catch { pictureBox2.Enabled = false; }
            comboBoxEx1.SelectedIndex = 0;
            numericUpDown5.Value = xPackage.Header.SaveConsoleID;
            numericUpDown6.Value = xPackage.Header.ProfileID;
            textBoxX17.Text = xPackage.Header.DeviceID.HexString();
            numericUpDown9_ValueChanged(null, null);
            if (xPackage.Header.IDTransfer == TransferLock.NoTransfer)
                checkBoxX1.Checked = false;
            else
            {
                checkBoxX2.Checked = ((byte)xPackage.Header.IDTransfer & 1) == 1;
                checkBoxX3.Checked = (((byte)xPackage.Header.IDTransfer >> 1) & 1) == 1;
            }
            labelX8.Text = "Title ID: " + xpack.Header.TitleID.ToString("X");
            if (xPackage.Header.ThisType != PackageType.Profile)
            {
                tabItem4.Visible = false;
                return;
            }
            xprof = new ProfilePackage(ref x);
            xpack = null;
            xprof.RemovePhDAT();
            if (xprof.HasValidAccount)
            {
                if (xprof.UserFile.IsLiveEnabled)
                {
                    textBoxX9.Text = VariousFunctions.EndianConvert(BitConverter.GetBytes(xprof.UserFile.XUID)).HexString();
                    textBoxX9.Enabled =
                    buttonX4.Enabled =
                    checkBoxX5.Enabled = true;
                    PassCode[] xPass = xprof.UserFile.GetPassCode();
                    if (xPass.Length > 0)
                    {
                        checkBoxX5.Checked = true;
                        comboBoxEx3.SelectedItem = xPass[0];
                        comboBoxEx4.SelectedItem = xPass[1];
                        comboBoxEx5.SelectedItem = xPass[2];
                        comboBoxEx6.SelectedItem = xPass[3];
                    }
                }
                textBoxX8.Text = xprof.UserFile.GetGamertag();
                buttonX5.Enabled = true;
                Log("Account loaded");
            }
            if (xprof.HasDashGPD)
            {
                UserInfo xInfo = xprof.GetUserStrings();
                if (xInfo.Bio != null)
                    textBoxX10.Text = xInfo.Bio;
                if (xInfo.Motto != null)
                    textBoxX11.Text = xInfo.Motto;
                if (xInfo.Name != null)
                    textBoxX12.Text = xInfo.Name;
                if (xInfo.Location != null)
                    textBoxX13.Text = xInfo.Location;
                Setting temp = xprof.UserGPD.GetSetting(GPDIDs.GCardZone);
                if (temp != null)
                {
                    comboBoxEx7.Enabled = true;
                    GamerZone[] zones = (GamerZone[])Enum.GetValues(typeof(GamerZone));
                    comboBoxEx7.DataSource = zones;
                    if (Enum.IsDefined(typeof(GamerZone), temp.Data))
                        comboBoxEx7.SelectedItem = (GamerZone)((uint)temp.Data);
                    else comboBoxEx7.SelectedItem = GamerZone.Unknown;
                    has1 = buttonX16.Enabled = true;
                }
                temp = xprof.UserGPD.GetSetting(GPDIDs.GCardRep, SettingType.Float);
                if (temp != null)
                {
                    numericUpDown1.Enabled = true;
                    numericUpDown1.Value = (decimal)((float)temp.Data);
                    has2 = buttonX16.Enabled = true;
                }
                SetList();
                buttonX15.Enabled =
                listBox3.Enabled = true;
                // groupPanel2.Enabled = true;
                SetNmric(GPDIDs.GCardCredit, textBoxX15);
                Log("Profile loaded");
            }
            if (!xprof.IsValidProfile)
            {
                if (xprof.HasValidAccount && !xprof.HasDashGPD)
                    MessageBox.Show("This file does not have valid Dash information, therefore achievement modding is disabled");
                else if (!xprof.HasValidAccount && xprof.HasDashGPD)
                    MessageBox.Show("This file does not have a valid user account, therefore, account block editing is disabled");
                else
                {
                    tabItem4.Visible = false;
                    MessageBox.Show("This file has no valid user information, profile - specific editing is disabled");
                }
            }
        }

        public PackageExplorer(MainForm xParent)
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            this.MdiParent = xParent;
            xMForm = xParent;
        }

        void SetList()
        {
            listBox3.Items.Clear();
            List<string> xItems = new List<string>();
            for (int i = 0; i < xprof.UserGPD.TitlesPlayed.Length; i++)
                xItems.Add(xprof.UserGPD.TitlesPlayed[i].Title + " (" + xprof.UserGPD.TitlesPlayed[i].TitleID.ToString("X") + ")");
            listBox3.Items.AddRange(xItems.ToArray());
        }

        void SetNmric(GPDIDs xID, TextBoxX xNum)
        {
            Setting x = xprof.UserGPD.GetSetting(xID, SettingType.UInt32);
            if (x != null)
                xNum.Text = ((uint)x.Data).ToString();
        }

        void b1()
        {
            try
            {
                listView2.Items.Clear();
                Log("Verifying Hash Structure... (Give it a few minutes)");
                Verified[] xVeri = xPackage.VerifyHashTables();
                Log("Hash Structure checked");
                List<ListViewItem> xList = new List<ListViewItem>();
                for (int i = 0; i < xVeri.Length; i++)
                {
                    ListViewItem xItem = new ListViewItem(xVeri[i].ThisType.ToString());
                    xItem.SubItems.Add(xVeri[i].InputLocale.ToString("X"));
                    xItem.SubItems.Add(xVeri[i].VerifyLocale.ToString("X"));
                    xItem.SubItems.Add(xVeri[i].IsValid.ToString());
                    xList.Add(xItem);
                }
                Log("Verifying Header...");
                xVeri = xPackage.VerifyHeader();
                Log("Header checked");
                foreach (Verified x in xVeri)
                {
                    ListViewItem i = new ListViewItem(x.ThisType.ToString());
                    i.SubItems.Add(x.InputLocale.ToString("X"));
                    i.SubItems.Add(x.VerifyLocale.ToString("X"));
                    i.SubItems.Add(x.IsValid.ToString());
                    xList.Add(i);
                }
                listView2.Items.AddRange(xList.ToArray());
            }
            catch { }
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            tabControl1.Enabled = false;
            Thread x = new Thread(new ThreadStart(b1));
            x.Start();
            while (x.IsAlive)
                Application.DoEvents();
            tabControl1.Enabled = true;
        }

        private void extractFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count == 0)
                return;
            FileEntry xent = (FileEntry)listView1.SelectedItems[0].Tag;
            string File = xent.Name;
            string xOut = VariousFunctions.GetUserFileLocale("Open a save location", "", false);
            if (xOut == null)
                return;
            this.Enabled = false;
            Log("Extracting " + File + "...");
            if (!xent.Extract(xOut))
            {
                Log("Extraction error");
                this.Enabled = true;
                return;
            }
            Log(File + " extracted");
            this.Enabled = true;
        }

        private void fixHashesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count == 0)
                return;
            FileEntry xent = (FileEntry)listView1.SelectedItems[0].Tag;
            DJsIO xio = new DJsIO(DJFileMode.Open, "Open a File", "", true);
            if (!xio.Accessed)
                return;
            Log("Fixing file...");
            if (!xent.FixHashes(null))
            {
                Log("Error in fixing");
                return;
            }
            Log("Finished");
        }

        void b2()
        {
            listView2.Items.Clear();
            Log("Writing to package");
            RSAParams x = null;
            if (radioButton1.Checked)
                x = KV;
            else if (radioButton2.Checked)
                x = new RSAParams(StrongSigned.LIVE);
            else if (radioButton3.Checked)
                x = new RSAParams(StrongSigned.PIRS);
            if (checkBoxX4.Checked)
                xPackage.FlushPackage(x);
            else xPackage.UpdateHeader(x);
            Log("Package fixed");
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            tabControl1.Enabled = false;
            Thread x = new Thread(new ThreadStart(b2));
            x.Start();
            while (x.IsAlive)
                Application.DoEvents();
            tabControl1.Enabled = true;
        }

        private void PackageExplorer_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (xprof != null)
                xprof.DeleteProfileFiles();
            xPackage.CloseIO();
            xMForm.Files.Remove(xPackage.FileNameLong);
            try { this.Dispose(); }
            catch { }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (listView1.SelectedIndices.Count == 0)
            {
                fixHashesToolStripMenuItem.Enabled = false;
                extractFileToolStripMenuItem.Enabled = false;
                injectFileToolStripMenuItem.Enabled = false;
                replaceFileToolStripMenuItem.Enabled = false;
                return;
            }
            fixHashesToolStripMenuItem.Enabled = true;
            extractFileToolStripMenuItem.Enabled = true;
            injectFileToolStripMenuItem.Enabled = true;
            replaceFileToolStripMenuItem.Enabled = true;
            int index = listView1.SelectedIndices[0];
            string File = listView1.Items[index].SubItems[0].Text;
            string sub = "";
            if (File.Length > 4)
                sub = File.Substring(File.Length - 4, 4);
            if (sub == ".gpd")
                loadInViewerToolStripMenuItem.Enabled = true;
            else loadInViewerToolStripMenuItem.Enabled = false;
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex < 0 || xLoadedGPD == null)
                return;
            pictureBox3.Image = xLoadedGPD.GetAchievementImage(listBox2.SelectedIndex);
            textBoxX5.Text = xLoadedGPD.Achievements[listBox2.SelectedIndex].Description1;
            textBoxX6.Text = xLoadedGPD.Achievements[listBox2.SelectedIndex].Description2;
            textBoxX5.MaxLength = textBoxX5.Text.Length;
            textBoxX6.MaxLength = textBoxX6.Text.Length;
            textBoxX7.Text = xLoadedGPD.Achievements[listBox2.SelectedIndex].Title;
            textBoxX7.MaxLength = textBoxX7.Text.Length;
            switch (xLoadedGPD.Achievements[listBox2.SelectedIndex].LockType)
            {
                case FlagType.Locked:
                    comboBoxEx1.SelectedIndex = 0;
                    break;
                        
                case FlagType.UnlockedOffline:
                    comboBoxEx1.SelectedIndex = 1;
                    break;

                case FlagType.UnlockedOnline:
                    comboBoxEx1.SelectedIndex = 2;
                    break;
            }
            try { dateTimePicker2.Value = dateTimePicker1.Value = xLoadedGPD.Achievements[listBox2.SelectedIndex].UnlockTime; }
            catch { dateTimePicker2.Value = dateTimePicker1.Value = DateTime.Now; }
        }

        private void checkBoxX5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxX5.Checked)
                groupPanel1.Enabled = true;
            else groupPanel1.Enabled = false;
        }

        private void buttonX4_Click(object sender, EventArgs e)
        {
            uint num = 0;
            while (File.Exists("C:/PBU" + num.ToString("X")))
            {
                num++;
                if (num > 0xFFFFFFFF)
                    break;
            }
            if (num > 0xFFFFFFFF)
            {
                MessageBox.Show("Cannot make backup");
                return;
            }
            if (xprof.MakeHDDAccountCopy("C:/PBU" + num.ToString("X")))
                MessageBox.Show("Success, saved to C:/PBU" + num.ToString("X"));
            else MessageBox.Show("Error");
        }

        private void buttonX5_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            if (xprof.HasValidAccount)
            {
                Log("Saving account data...");
                xprof.UserFile.SaveGamertag(textBoxX8.Text);
                xprof.SaveAccount();
                Log("Account info saved");
            }
            this.Enabled = true;
        }

        private void injectFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count == 0)
                return;
            FileEntry xent = (FileEntry)listView1.SelectedItems[0].Tag;
            DJsIO xio = new DJsIO(DJFileMode.Open, "Open a File", "", true);
            if (!xio.Accessed)
                return;
            Log("Injecting file...");
            if (!xent.Inject(xio))
            {
                Log("Error in injecting");
                return;
            }
            Log("Finished");
        }

        public void xAddLog(string x)
        {
            try
            {
                listBox4.Items.Add(x);
                listBox4.SelectedIndex = (listBox4.Items.Count - 1);
            }
            catch { }
        }

        private void buttonX8_Click(object sender, EventArgs e)
        {
            tabControl1.Enabled = false;
            xPackage.ExtractPayload(true, "Extract package", true);
            tabControl1.Enabled = true;
        }

        private void buttonX9_Click(object sender, EventArgs e)
        {
            tabControl1.Enabled = false;
            bool success = false;
            if (radioButton1.Checked)
                success = xPackage.RebuildPackage(new RSAParams(Application.StartupPath + "/KV.bin"));
            else if (radioButton2.Checked)
                success = xPackage.RebuildPackage(new RSAParams(StrongSigned.LIVE));
            else if (radioButton3.Checked)
                success = xPackage.RebuildPackage(new RSAParams(StrongSigned.PIRS));
            if (success)
                Log("Done");
            else Log("Error");
            tabControl1.Enabled = true;
        }

        private void PackageExplorer_DragDrop(object sender, DragEventArgs e)
        {
            MessageBox.Show("Rawr");
        }

        private void tabControlPanel1_DragDrop(object sender, DragEventArgs e)
        {
            MessageBox.Show("rawr");
        }

        private void replaceFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count == 0)
                return;
            FileEntry xent = (FileEntry)listView1.SelectedItems[0].Tag;
            string file = VariousFunctions.GetUserFileLocale("Open a File", "", true);
            if (file == null)
                return;
            Log("Replacing file...");
            if (!xent.Replace(file))
            {
                Log("Error in Replacing");
                return;
            }
            Log("Finished");
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.All;
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            List<string> xData = new List<string>((string[])e.Data.GetData(DataFormats.FileDrop));
            List<DJsIO> xIO = new List<DJsIO>();
            for (int i = 0; i < xData.Count; i++)
            {
                DJsIO blah = new DJsIO(xData[i], DJFileMode.Open, true);
                if (blah.Accessed)
                    xIO.Add(blah);
            }
            if (xIO.Count == 0)
                return;
            if (MessageBox.Show("Are you sure you want to add " + ((xIO.Count == 1) ? "this accessed file?" : ("these " + xIO.Count.ToString() + " accessed files?")), "Rawr?", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            foreach (DJsIO x in xIO)
                xPackage.MakeFile(x.FileNameShort, x, ((FolderEntry)advTree1.SelectedNode.DataKey).EntryID, AddType.NoOverWrite);
            xReturn_NodeClick(advTree1.SelectedNode, null);
        }

        private void addFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DJsIO x = new DJsIO(DJFileMode.Open, "Open", "", true);
            if (!x.Accessed)
                return;
            Renamer rename = new Renamer(x.FileNameShort, true);
            if (rename.ShowDialog() != DialogResult.OK)
            {
                x.Close();
                return;
            }
            xPackage.MakeFile(rename.FileName, x, ((FolderEntry)advTree1.SelectedNode.DataKey).EntryID, AddType.NoOverWrite);
            Log("Done adding");
        }

        private void buttonX11_Click(object sender, EventArgs e)
        {
            xLoadedEntry.Replace(xLoadedGPD.GetStream());
            xprof.UserGPD.UpdateTitle(xLoadedGPD);
            xprof.SaveDash();
            SetNmric(GPDIDs.GCardCredit, textBoxX15);
        }

        private void loadInViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count == 0)
                return;
            FileEntry xent = (FileEntry)listView1.SelectedItems[0].Tag;
            this.Enabled = false;
            Log("Extracting " + xent.Name + "...");
            string xOut = VariousFunctions.GetTempFileLocale();
            if (!xent.Extract(xOut))
            {
                Log("Extraction error");
                this.Enabled = true;
                return;
            }
            GameGPD xload = new GameGPD(xOut, ProfileTools.GPDNameToID(xent.Name));
            if (!xload.IsValid)
            {
                Log("Error when parsing GPD");
                xload.Close();
                try { VariousFunctions.DeleteFile(xOut); }
                catch { }
                this.Enabled = true;
                return;
            }
            GPDViewer x = new GPDViewer(xload);
            x.MdiParent = xMForm;
            x.Show();
            Log("GPD Loaded");
            this.Enabled = true;
        }

        private void buttonX7_Click(object sender, EventArgs e)
        {
            xPackage.Header.MakeAnonymous();
            checkBoxX1.Checked = checkBoxX2.Checked = checkBoxX3.Checked = true;
            numericUpDown5.Value = xPackage.Header.SaveConsoleID;
            numericUpDown6.Value = xPackage.Header.ProfileID;
            textBoxX17.Text = xPackage.Header.DeviceID.HexString();
            numericUpDown9_ValueChanged(null, null);
        }

        private void textBoxX2_TextChanged(object sender, EventArgs e)
        {
            xPackage.Header.Title_Display = textBoxX2.Text;
        }

        private void textBoxX3_TextChanged(object sender, EventArgs e)
        {
            xPackage.Header.Description = textBoxX3.Text;
        }

        private void numericUpDown9_ValueChanged(object sender, EventArgs e)
        {
            int idx = (int)numericUpDown9.Value;
            numericUpDown8.Value = xPackage.Header.Liscenses[idx].ID;
            numericUpDown7.Value = xPackage.Header.Liscenses[idx].Var1;
            numericUpDown4.Value = xPackage.Header.Liscenses[idx].Flags;
        }

        private void buttonX3_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex == -1)
                return;
            switch (comboBoxEx1.SelectedIndex)
            {
                case 0: xLoadedGPD.Achievements[listBox2.SelectedIndex].LockType = FlagType.Locked;
                    break;

                case 1: xLoadedGPD.Achievements[listBox2.SelectedIndex].LockType = FlagType.UnlockedOffline;
                    break;

                case 2: 
                    {
                        xLoadedGPD.Achievements[listBox2.SelectedIndex].LockType = FlagType.UnlockedOnline;
                        xLoadedGPD.Achievements[listBox2.SelectedIndex].UnlockTime =
                            new DateTime(dateTimePicker1.Value.Year, dateTimePicker1.Value.Month, dateTimePicker1.Value.Day,
                            dateTimePicker2.Value.Hour, dateTimePicker2.Value.Minute, dateTimePicker2.Value.Second); 
                    }
                    break;
            }
            xLoadedGPD.Achievements[listBox2.SelectedIndex].Update();
        }

        private void buttonX10_Click(object sender, EventArgs e)
        {
            int type = comboBoxEx1.SelectedIndex;
            tabControl1.Enabled = false;
            for (int i = 0; i < listBox2.Items.Count; i++)
            {
                listBox2.SelectedIndex = i;
                comboBoxEx1.SelectedIndex = type;
                buttonX3_Click(null, null);
            }
            tabControl1.Enabled = true;
        }

        void killachievetab()
        {
            tabControlPanel10.Enabled = true;
            textBoxX4.Text = textBoxX5.Text =
                textBoxX6.Text = textBoxX7.Text = "";
            listBox2.Items.Clear();
            xLoadedEntry = null;
            xLoadedGPD = null;
            buttonX6.Enabled = false;
            pictureBox4.Image = pictureBox3.Image = null;
        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox3.SelectedIndex < 0)
                return;
            tabControl1.Enabled = false;
            buttonX12.Enabled = true;
            TitlePlayedEntry x = xprof.UserGPD.TitlesPlayed[listBox3.SelectedIndex];
            try { dateTimePicker3.Value = dateTimePicker4.Value = x.LastLoadedDT; }
            catch { dateTimePicker3.Value = dateTimePicker4.Value = DateTime.Now; }
            textBoxX14.Text = "Unlocked " + x.EarnedCount.ToString() + " of " + x.PossibleCount.ToString();
            textBoxX14.Text += Environment.NewLine + "Total of " + x.EarnedWorth.ToString() + " of " + x.PossibleWorth.ToString();
            string path = x.TitleID.ToString("X").ToUpper() + ".gpd";
            FileEntry xent = xPackage.GetFile(path);
            if (xent == null)
            {
                Log("Error: could not find GPD");
                killachievetab();
                tabControl1.Enabled = true;
                return;
            }
            string xOut = VariousFunctions.GetTempFileLocale();
            if (!xent.Extract(xOut))
            {
                Log("Extraction error");
                killachievetab();
                tabControl1.Enabled = true;
                return;
            }
            GameGPD xload = new GameGPD(xOut, x.TitleID);
            if (!xload.IsValid)
            {
                Log("Error when parsing GPD");
                xload.Close();
                try { VariousFunctions.DeleteFile(xOut); }
                catch { }
                xload = null;
                killachievetab();
                tabControl1.Enabled = true;
                return;
            }
            listBox2.Items.Clear();
            for (int i = 0; i < xload.Achievements.Length; i++)
                listBox2.Items.Add(xload.Achievements[i].Title);
            Image xTitleIMGS = xload.GetImageByID(0x8000);
            if (xTitleIMGS != null)
                pictureBox4.Image = xTitleIMGS;
            else pictureBox4.Image = PublicResources.NoImage;
            string xTitleStrings = xload.GetStringByID(0x8000);
            if (xTitleStrings != null)
                textBoxX4.Text = xTitleStrings;
            else textBoxX4.Text = "Unknown";
            xLoadedGPD = xload;
            xLoadedEntry = xent;
            if (listBox2.Items.Count > 0)
                listBox2.SelectedIndex = 0;
            listBox2_SelectedIndexChanged(null, null);
            buttonX11.Enabled =
            buttonX3.Enabled =
            buttonX10.Enabled =
            tabControl1.Enabled =
            buttonX6.Enabled =
            tabControlPanel10.Enabled = true;
            Log("GPD Loaded");
        }

        private void buttonX12_Click(object sender, EventArgs e)
        {
            if (listBox3.SelectedIndex < 0)
                return;
            TitlePlayedEntry x = xprof.UserGPD.TitlesPlayed[listBox3.SelectedIndex];
            x.LastLoadedDT = new DateTime(
                dateTimePicker4.Value.Year, dateTimePicker4.Value.Month, dateTimePicker4.Value.Day,
                dateTimePicker3.Value.Hour, dateTimePicker3.Value.Minute, dateTimePicker3.Value.Second);
            if (x.Update())
                buttonX14_Click_1(null, null);
        }

        private void buttonX13_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            for (int i = 0; i < listBox3.Items.Count; i++)
            {
                listBox3.SelectedIndex = i;
                if (xprof.UserGPD.TitlesPlayed[i].PossibleCount != xprof.UserGPD.TitlesPlayed[i].EarnedCount &&
                    listBox2.Items.Count > 0)
                {
                    comboBoxEx1.SelectedIndex = 2;
                    buttonX10_Click(null, null);
                    buttonX11_Click(null, null);
                }
            }
            this.Enabled = true;
            MessageBox.Show("Done");
        }

        private void buttonX15_Click(object sender, EventArgs e)
        {
            if (!xprof.HasDashGPD)
                return;
            MultiAdder x = new MultiAdder();
            if (x.ShowDialog() != DialogResult.OK)
                return;
            tabControl1.Enabled = false;
            foreach (ListViewItem y in x.listView1.Items)
            {
                try
                {
                    GameGPD z = null;
                    try { z = new GameGPD(y.SubItems[2].Text, Convert.ToUInt32(y.SubItems[1].Text, 16)); }
                    catch { continue; }
                    if (xprof.AddGame(z))
                        listBox3.Items.Add(y.SubItems[0].Text + " (" + y.SubItems[1].Text + ")");
                    else SetList();
                    z.Close();
                }
                catch { }
                Application.DoEvents();
            }
            SetNmric(GPDIDs.GCardCredit, textBoxX15);
            tabControl1.Enabled = true;
        }

        private void buttonX16_Click(object sender, EventArgs e)
        {
            tabControl1.Enabled = false;
            Log("Updating items");
            Setting temp = null;
            if (has2)
            {
                temp = xprof.UserGPD.GetSetting(GPDIDs.GCardRep);
                if (temp != null)
                {
                    float tmp = (float)numericUpDown1.Value;
                    temp.Data = tmp;
                    temp.Update(SyncType.Server);
                }
            }
            if (has1)
            {
                temp = xprof.UserGPD.GetSetting(GPDIDs.GCardZone);
                if (temp != null)
                {
                    temp.Data = (uint)((GamerZone)comboBoxEx7.SelectedItem);
                    temp.Update(SyncType.Server);
                }
            }
            Log("Items updated");
            buttonX14_Click_1(null, null);
            tabControl1.Enabled = true;
        }

        private void contextMenuStrip3_Opening(object sender, CancelEventArgs e)
        {
            if (listBox3.SelectedIndex < 0)
                loadInViewerToolStripMenuItem1.Enabled = false;
            else loadInViewerToolStripMenuItem1.Enabled = true;
        }

        private void loadInViewerToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            FileEntry xent = xprof.GetFile(xprof.UserGPD.TitlesPlayed[listBox3.SelectedIndex].ID.ToString("X") + ".gpd");
            if (xent == null)
            {
                Log("Could not find specified file");
                this.Enabled = true;
                return;
            }
            Log("Extracting " + xent.Name + "...");
            string xOut = VariousFunctions.GetTempFileLocale();
            if (!xent.Extract(xOut))
            {
                Log("Extraction error");
                VariousFunctions.DeleteFile(xOut);
                this.Enabled = true;
                return;
            }
            GameGPD xload = new GameGPD(xOut, ProfileTools.GPDNameToID(xent.Name));
            if (!xload.IsValid)
            {
                Log("Error when parsing GPD");
                xload.Close();
                try { VariousFunctions.DeleteFile(xOut); }
                catch { }
                this.Enabled = true;
                return;
            }
            GPDViewer x = new GPDViewer(xload);
            x.MdiParent = xMForm;
            x.Show();
            Log("GPD Loaded");
            this.Enabled = true;
        }

        private void buttonX14_Click_1(object sender, EventArgs e)
        {
            xprof.SaveDash();
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            xPackage.Header.SaveConsoleID = (long)numericUpDown5.Value;
        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            xPackage.Header.ProfileID = (long)numericUpDown6.Value;
        }

        private void textBoxX17_TextChanged(object sender, EventArgs e)
        {
            // Device ID
            try { xPackage.Header.DeviceID = textBoxX17.Text.HexToBytes(); }
            catch { textBoxX17.Text = xPackage.Header.DeviceID.HexString(); }
        }

        private void buttonX6_Click(object sender, EventArgs e)
        {
            Game_Orderer x = new Game_Orderer(ref xLoadedGPD);
            x.ShowDialog();
        }
    }
}