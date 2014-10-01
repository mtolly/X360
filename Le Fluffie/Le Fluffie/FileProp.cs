// Program is protected under GPL Licensing and Copyrighted to alias DJ Shepherd

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using X360.FATX;
using DevComponents.DotNetBar;

namespace Le_Fluffie
{
    public partial class FileProp : Office2007Form
    {
        FATXFileEntry xfile;

        public FileProp(FATXFileEntry xin, string stfsname)
        {
            InitializeComponent();
            xfile = xin;
            textBoxX1.Text = xin.Name;
            textBoxX2.Text = stfsname;
            textBoxX3.Text = xin.Size.ToString() + " bytes";
        }
    }
}
