using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsgoDeathAnalyser
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process.Start("https://vk.com/exideprod");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Process.Start("https://youtu.be/8tVz5C8idT4");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Process.Start("https://docs.google.com/spreadsheets/d/1VI_lBiO8_8LNanWjb3uKF9wa-jb7mlba9LPbnj9C7SM/edit?usp=sharing");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Process.Start("https://vk.com/exxide");
        }
    }
}
