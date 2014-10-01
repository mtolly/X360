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
    public partial class PackageTypeSel : Office2007Form
    {
        public PackageTypeSel()
        {
            InitializeComponent();
            PackType[] xenums = (PackType[])Enum.GetValues(typeof(PackType));
            comboBoxEx1.DataSource = xenums;
            comboBoxEx1.SelectedIndex = 0;
        }

        internal PackType SelectedType = PackType.STFS;

        private void comboBoxEx1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedType = (PackType)comboBoxEx1.SelectedItem;
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }
    }

    public enum PackType { STFS, SVOD }
}
