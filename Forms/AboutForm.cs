using System;
using System.Windows.Forms;

namespace ScintillaNET_Kitchen.Forms
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void AboutForm_KeyUp(object sender, KeyEventArgs e)
        {
            this.Close();
        }
    }
}
