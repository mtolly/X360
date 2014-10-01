using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using DevComponents.DotNetBar;
using ICSharpCode.SharpZipLib.Zip;

namespace Le_Fluffie
{
    public partial class PIDownloader : Office2007Form
    {
        string dir = Application.StartupPath + "/plugins/";
        public PIDownloader()
        {
            InitializeComponent();
            StreamReader x = null;
            try { x = X360.Other.VariousFunctions.GetWebPageResponse("http://skunkiebutt.com/ProductCheck.php?command=disp&spcode=lfp"); }
            catch { MessageBox.Show("Could not contact server"); Dispose(); }
            while (!x.EndOfStream)
            {
                try
                {
                    ListViewItem y = new ListViewItem(x.ReadLine());
                    y.SubItems.Add(x.ReadLine());
                    y.Tag = x.ReadLine();
                    y.SubItems.Add(x.ReadLine());
                    y.SubItems.Add("");
                    if (!File.Exists(dir + y.SubItems[0].Text + ".dll"))
                        listView1.Items.Add(y);
                }
                catch { break; }
            }
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                Dispose();
                return;
            }
            uint installed = 0;
            listView1.Enabled =
            buttonX2.Enabled = false;
            WebClient wc = new WebClient();
            foreach (ListViewItem x in listView1.SelectedItems)
            {
                try
                {
                    x.SubItems[3].Text = "Downloading...";
                    Application.DoEvents();
                    string tmp = X360.Other.VariousFunctions.GetTempFileLocale();
                    wc.DownloadFile((string)x.Tag, tmp);
                    x.SubItems[3].Text = "Installing...";
                    Application.DoEvents();
                    ZipInputStream y = new ZipInputStream(File.Open(tmp, FileMode.Open, FileAccess.Read));
                    ZipEntry ent = null;
                    while ((ent = y.GetNextEntry()) != null)
                    {
                        FileStream stream = new FileStream(dir + ent.Name, FileMode.Create, FileAccess.ReadWrite);
                        int size = 2048;
                        byte[] buffer = new byte[size];
                        while (size != 0)
                        {
                            size = y.Read(buffer, 0, buffer.Length);
                            if (size != 0)
                                stream.Write(buffer, 0, size);
                        }
                        stream.Close();
                    }
                    installed++;
                    y.Close();
                    X360.Other.VariousFunctions.DeleteFile(tmp);
                    x.SubItems[3].Text = "Done";
                    Application.DoEvents();
                }
                catch { x.SubItems[3].Text = "Error"; Application.DoEvents(); }
            }
            if (installed != 0 && MessageBox.Show("This application needs to be restarded in order to enable new plugins, continue?", "NOTE", MessageBoxButtons.YesNo) == DialogResult.Yes)
                Application.Restart();
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            Dispose();
            return;
        }
    }
}
