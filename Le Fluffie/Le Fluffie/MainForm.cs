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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using DevComponents;
using DevComponents.DotNetBar.Controls;
using DevComponents.DotNetBar;
using X360;
using X360.IO;
using X360.STFS;
using X360.FATX;
using X360.Other;
using X360.Media;
using X360.Profile;
using X360.SVOD;
using X360.GDFX;

namespace Le_Fluffie
{
    public partial class MainForm : Office2007Form
    {
        public List<string> Files = new List<string>();
        public RSAParams PublicKV;
        Updater updr = null;
        bool adjusturl = true;

        void startss(object devmode)
        {
            SS x = null;
            if (!((bool)devmode))
            {
                x = new SS();
                x.Show();
            }
            if (x != null)
                x.labelX4.Text = "Loading Plug - In's...";
            try
            {
                ToolStripMenuItem current = new ToolStripMenuItem("Current Plugins");
                string[] flz = Directory.GetFiles(Application.StartupPath + "/plugins");
                foreach (string s in flz)
                {
                    try
                    {
                        if (Path.GetExtension(s) != ".dll" || Path.GetFileName(s).ToLower() == "x360.dll")
                            continue;
                        LFPlugIn v = new LFPlugIn(s);
                        if (v.valid)
                        {
                            current.DropDownItems.Add(v.Name, null, new EventHandler(plugclick));
                            current.DropDownItems[current.DropDownItems.Count - 1].Tag = v;
                        }
                    }
                    catch { }
                }
                pluginsToolStripMenuItem.DropDownItems.Add(current);
                pluginsToolStripMenuItem.DropDownItems.Add("Download Plugins", null, new EventHandler(download_click));

            }
            catch { }
            if (x != null)
            {
                while (x.Visible)
                    Application.DoEvents();
            }
            Thread.CurrentThread.Abort();
        }

        void download_click(object sender, EventArgs e)
        {
            PIDownloader pid = new PIDownloader();
            pid.ShowDialog();
        }

        void readrss()
        {
            listView1.Enabled = false;
            listView1.Items.Clear();
            try
            {
                System.Xml.XmlDocument x = new System.Xml.XmlDocument();
                x.Load("http://skunkiebutt.com/?feed=rss");
                System.Xml.XmlNodeList y = x.GetElementsByTagName("title");
                System.Xml.XmlNodeList z = x.GetElementsByTagName("link");
                for (int i = 1; i <= 20; i++)
                {
                    ListViewItem it = new ListViewItem(y[i].InnerText);
                    it.Tag = z[i].InnerText;
                    listView1.Items.Add(it);
                }
                listView1.Enabled = true;
            }
            catch { listView1.Items.Add(new ListViewItem("Connection Error")); }
            try
            {
                StreamReader x = new StreamReader(WebRequest.Create("http://skunkiebutt.com/News.txt").GetResponse().GetResponseStream());
                richTextBox1.Text += "\r\n" + x.ReadLine();
                x.Close();
            }
            catch { }
            Thread.CurrentThread.Abort();
        }

        void upd(object devmode)
        {
            if (!((bool)devmode))
            {
                updr = new Updater();
                if (updr.UpdateNeeded)
                    updr.ShowDialog();
            }
            Thread.CurrentThread.Abort();
        }

        public MainForm()
        {
            CheckForIllegalCrossThreadCalls = false;
            string proc = Process.GetCurrentProcess().ProcessName;
            int id = Process.GetCurrentProcess().Id;
            Process[] lst = Process.GetProcesses();
            foreach (Process p in lst)
            {
                if (p.ProcessName != proc || p.Id == id)
                    continue;
                MessageBox.Show("This program is already open");
                Process.GetCurrentProcess().Kill();
                return;
            }
            InitializeComponent();
            VariousFunctions.DeleteFile(Application.StartupPath + "/LFLiveUpdater.exe");
            bool devmode = AssemblyFunctions.GrabParentProcessName() == "devenv";
            Thread y = new Thread(new ParameterizedThreadStart(upd));
            y.Start(devmode);
            Thread x = new Thread(new ParameterizedThreadStart(startss));
            x.Start(devmode);
            VariousFunctions.DeleteTempFiles();
            Thread z = new Thread(new ThreadStart(readrss));
            z.Start();
            PublicKV = new RSAParams(Application.StartupPath + "/KV.bin");
            if (!PublicKV.Valid)
            {
                MessageBox.Show("Cannot load KV");
                Process.GetCurrentProcess().Kill();
                return;
            }
            XAbout.WriteLegalLocally();
            while (x.IsAlive)
                Application.DoEvents();
            Show();
            while (y.IsAlive)
                Application.DoEvents();
            Enabled = true;
            Select();
            Focus();
            if (!devmode)
            {
                updr.Dispose();
                updr = null;
            }
            checkForUpdatesToolStripMenuItem.Enabled = true;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            this.Refresh();
        }

        void ReadFile(string file)
        {
            try
            {
                switch (VariousFunctions.ReadFileType(file))
                {
                    case XboxFileType.STFS:
                        {
                            LogRecord x = new LogRecord();
                            STFSPackage xPackage = new STFSPackage(file, x);
                            if (!xPackage.ParseSuccess)
                                return;
                            PackageExplorer xExplorer = new PackageExplorer(this);
                            xExplorer.listBox4.Items.AddRange(x.Log);
                            x.WhenLogged += new LogRecord.OnLog(xExplorer.xAddLog);
                            xExplorer.set(ref xPackage);
                            xExplorer.Show();
                        }
                        break;

                    case XboxFileType.SVOD:
                        {
                            SVODPackage hd = new SVODPackage(file, null);
                            if (!hd.IsValid)
                                return;
                            HDDGameForm frm = new HDDGameForm(hd, file, this);
                            frm.MdiParent = this;
                            frm.Show();
                        }
                        break;

                    case XboxFileType.Music:
                        {
                            MusicFile xfile = new MusicFile(file);
                            MusicView xview = new MusicView(this, file, xfile);
                            xview.MdiParent = this;
                            xview.Show();
                        }
                        break;

                    case XboxFileType.GPD:
                        {
                            GameGPD y = new GameGPD(file, 0xFFFFFFFF);
                            GPDViewer z = new GPDViewer(y, file, this);
                            z.MdiParent = this;
                            z.Show();
                        }
                        break;

                    case XboxFileType.FATX:
                        {
                            FATXDrive xdrive = new FATXDrive(file);
                            Files.Add(file);
                            FATXBrowser y = new FATXBrowser(xdrive, file, this);
                            y.MdiParent = this;
                            y.Show();
                        }
                        break;

                    case XboxFileType.GDF:
                        {
                            StatsForm x = new StatsForm(0);
                            x.Text = "Select Deviation";
                            if (x.ShowDialog() != DialogResult.OK)
                                return;
                            GDFImage ximg = new GDFImage(file, x.ChosenID);
                            if (!ximg.Valid)
                                throw new Exception("Invalid package");
                            GDFViewer xViewer = new GDFViewer(ximg, this);
                            xViewer.MdiParent = this;
                            xViewer.Show();
                        }
                        break;

                    default: MessageBox.Show("Error: Unknown file"); return;
                }
                Files.Add(file);
            }
            catch (Exception x) { MessageBox.Show(x.Message); }
        }
       
        private void openAFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string file = VariousFunctions.GetUserFileLocale("Open an Xbox File", "", true);
            if (file == null || Files.Contains(file))
                return;
            ReadFile(file);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About xAbout = new About();
            xAbout.ShowDialog();
            xAbout.Dispose();
        }

        private void packageCreationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PackageTypeSel xSel = new PackageTypeSel();
            if (xSel.ShowDialog() != DialogResult.OK)
                return;
            PackageCreatez xCreate = new PackageCreatez(this, xSel.SelectedType);
            xCreate.Show();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            VariousFunctions.DeleteTempFiles();
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            updr = new Updater();
            updr.ShowDialog();
            updr.Dispose();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            BringToFront();
        }

        private void fATXExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Drive_Selector x = new Drive_Selector(this);
            if (x.ShowDialog() != DialogResult.OK)
                return;
            Form y = null;
            if (x.ChosenDrive.IsDriveIO)
            {
                if (x.radioButton1.Checked)
                    y = (Form)new FATXViewer(x.ChosenDrive, x.ChosenDrive.DriveName, this);
                else y = (Form)new FATXBrowser(x.ChosenDrive, x.ChosenDrive.DriveName, this);
                Files.Add(x.ChosenDrive.DriveName);
            }
            else
            {
                if (Files.Contains(x.xfile))
                    return;
                if (x.radioButton1.Checked)
                    y = (Form)new FATXViewer(x.ChosenDrive, x.xfile, this);
                else y = (Form)new FATXBrowser(x.ChosenDrive, x.xfile, this);
            }
            y.MdiParent = this;
            y.Show();
        }

        private void donateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(XAbout.Donate);
        }

        private void multiSTFSFixerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            multiSTFSFixerToolStripMenuItem.Enabled = false;
            MultiSTFS x = new MultiSTFS(this);
            x.MdiParent = this;
            x.Show();
        }

        private void achievementsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string xtext = "How to Edit Achievements and Add Games\r\n\r\n" +
                "After you open your profile, locate the Profile>Profile Tab." +
                "  You will see a list of games, which is all your games played.\r\n\r\n" +
                "To unlock any of those achievements, simply click on that game and go " +
                " to the Profile>Achievements tab where you will be able to mod the achievements." +
                "  Just select an achievement, mod it, hit Save Achievement, do all that you want, " +
                " and then hit Save to Profile, Save Dash GPD, vwallah.\r\n\r\nAfter unlocking all that you want, " +
                "go to the Security tab and click Fix, you are all good to go.\r\n\r\n" +
                "To add games, simple open the multiadder, locate all that you want, and hit OK.";
            Helper xhlp = new Helper(xtext, (ToolStripMenuItem)sender);
            xhlp.MdiParent = this;
            xhlp.Show();
        }

        private void packageCreationToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string xtext = "Package Creation\r\n\r\nEverything is pretty much self " +
                "explanitory, the one thing I do need to explain is the Deviation for SVOD " +
                "(Game) packages.  If you create a clean ISO from scratch, the Deviation will be 0, " +
                "but if it was extracted from a previous SVOD package, you need to know the Deviation from " +
                "that package.";
            Helper xhlp = new Helper(xtext, (ToolStripMenuItem)sender);
            xhlp.MdiParent = this;
            xhlp.Show();
        }

        private void gamertagEditingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string xtext = "How to Edit Your Gamertag\r\n\r\n" +
                "After you open your profile, locate the Profile>Profile Tab." +
                "  You will see a textbox wif your gamertag in that.  Simply edit the gamertag " +
                "and hit Save Account.  Be sure to go to Security and hit Fix <3";
            Helper xhlp = new Helper(xtext, (ToolStripMenuItem)sender);
            xhlp.MdiParent = this;
            xhlp.Show();
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string x in files)
                ReadFile(x);
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
        }

        void plugclick(object sender, EventArgs e)
        {
            string locale = VariousFunctions.GetUserFileLocale("Open a File", "", true);
            if (locale == null)
                return;
            // Integrate log choice
            STFSPackage x = new STFSPackage(locale, null);
            if (!x.ParseSuccess)
                return;
            try { ((LFPlugIn)((ToolStripItem)sender).Tag).xConst.Invoke(new object[] { x, (Form)this }); }
            catch (Exception z) { x.CloseIO(); MessageBox.Show(z.Message); }
        }

        private void expandablePanel1_ExpandedChanged(object sender, ExpandedChangeEventArgs e)
        {
            Refresh();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count == 0)
                return;
            Process.Start((string)listView1.SelectedItems[0].Tag);
        }

        private void refreshChatPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            adjusturl = true;
            webBrowser1.Url = new Uri("http://skunkiebutt.com/chat");
            webBrowser1.Update();
        }

        private void webBrowser1_NewWindow(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (webBrowser1.Url.Host == "skunkiebutt.com")
                return;
            if (!adjusturl)
                return;
            try
            {
                while (webBrowser1.Document == null)
                    Application.DoEvents();
                HtmlElement xele = webBrowser1.Document.GetElementById("linkSkip");
                if (xele != null)
                {
                    adjusturl = false;
                    webBrowser1.Navigate(xele.GetAttribute("href"));
                }
            }
            catch { }
        }
    }
}
