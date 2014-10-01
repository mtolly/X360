using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using X360.Profile;
using DevComponents.DotNetBar;

namespace Le_Fluffie
{
    public partial class Game_Orderer : Office2007Form
    {
        GameGPD xRef;

        public Game_Orderer(ref GameGPD game)
        {
            InitializeComponent();
            xRef = game;
            for (int i = 0; i < game.Achievements.Length; i++)
            {
                if (game.Achievements[i].Unlocked)
                {
                    ListViewItem x = new ListViewItem(game.Achievements[i].Title);
                    x.Tag = i;
                    listView1.Items.Add(x);
                }
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;
            textBoxX1.Text = xRef.Achievements[(int)listView1.SelectedItems[0].Tag].Description1;
            try { dateTimePicker2.Value = dateTimePicker1.Value = xRef.Achievements[(int)listView1.SelectedItems[0].Tag].UnlockTime; }
            catch { dateTimePicker2.Value = dateTimePicker1.Value = DateTime.Now; }
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;
            int indx = listView1.SelectedItems[0].Index;
            if (indx == 0)
                return;
            ListViewItem x = listView1.SelectedItems[0];
            listView1.Items.RemoveAt(indx--);
            listView1.Items.Insert(indx, x);
            listView1.Items[indx].Selected = true;
            listView1.Select();
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;
            int indx = listView1.SelectedItems[0].Index;
            if (indx == (listView1.Items.Count - 1))
                return;
            ListViewItem x = listView1.SelectedItems[0];
            listView1.Items.RemoveAt(indx++);
            listView1.Items.Insert(indx, x);
            listView1.Items[indx].Selected = true;
            listView1.Select();
        }

        private void dateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;
            xRef.Achievements[(int)listView1.SelectedItems[0].Tag].UnlockTime = new DateTime(dateTimePicker1.Value.Year,
                dateTimePicker1.Value.Month, dateTimePicker1.Value.Day, dateTimePicker2.Value.Hour,
                dateTimePicker2.Value.Minute, dateTimePicker2.Value.Second);
        }

        private void buttonX3_Click(object sender, EventArgs e)
        {
            Enabled = false;
            for (int i = listView1.Items.Count - 1; i >= 0; i--)
                xRef.Achievements[(int)listView1.Items[i].Tag].Update();
            Close();
            MessageBox.Show("Done!");
        }
    }
}
