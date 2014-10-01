using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using System.Threading;

namespace Le_Fluffie
{
    public partial class SS : Office2007Form
     {
        public SS()
        {
            InitializeComponent();
            Opacity = 0;
            labelX1.Text = "Le Fluffie Build: " + Application.ProductVersion;
            labelX2.Text = "X360 Build: " + X360.XAbout.Build;
        }

        private void SS_Shown(object sender, EventArgs e)
        {
            for (double i = 0; i < 61; i++)
            {
                Opacity = (i / 60);
                Select();
                Focus();
                Application.DoEvents();
                Thread.Sleep(50);
            }
            Thread.Sleep(2000);
            Close();
        }
     }
}
