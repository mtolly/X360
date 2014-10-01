using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;

namespace Le_Fluffie
{
    public partial class Helper : Office2007Form
    {
        ToolStripMenuItem xref;

        public Helper(string xText, ToolStripMenuItem xRef)
        {
            InitializeComponent();
            xRef.Enabled = false;
            xref = xRef;
            textBoxX1.Text = xText;
        }

        private void Helper_FormClosed(object sender, FormClosedEventArgs e)
        {
            xref.Enabled = true;
        }

        private void Helper_Shown(object sender, EventArgs e)
        {
            textBoxX1.SelectionLength = 0;
        }
    }
}
