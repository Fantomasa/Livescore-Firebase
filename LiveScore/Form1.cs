using System;
using System.Windows.Forms;

namespace LiveScore
{
    public partial class Form1 : Form
    {
        public static Form1 instance;

        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Program.ClearData().Wait();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Program.ClearResult().Wait();
        }
    }
}
