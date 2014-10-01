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

namespace Le_Fluffie
{
    public partial class StatsForm : Office2007Form
    {
        public StatsForm(uint ID)
        {
            InitializeComponent();
            numericUpDown1.Value = ID;
        }

        public uint ChosenID { get { return (uint)numericUpDown1.Value; } }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}