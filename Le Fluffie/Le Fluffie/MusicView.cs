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
using X360.Media;
using X360.IO;

namespace Le_Fluffie
{
    public partial class MusicView : Office2007Form
    {
        MainForm parent;
        MusicFile xfile;
        string file;

        public MusicView(MainForm xparent, string sfile, MusicFile mfile)
        {
            InitializeComponent();
            parent = xparent;
            xfile = mfile;
            file = sfile;
            textBoxX1.Text = mfile.Genre;
            textBoxX2.Text = mfile.Artist;
            textBoxX3.Text = mfile.Album;
            textBoxX4.Text = mfile.Song;
        }

        private void buttonX1_Click(object sender, EventArgs e) { xfile.ExtractWMA(); }

        private void MusicView_FormClosing(object sender, FormClosingEventArgs e)
        {
            parent.Files.Remove(file);
        }
    }
}
