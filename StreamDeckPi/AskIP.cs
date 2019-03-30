using System;
using System.Windows.Forms;
using System.IO;

namespace StreamDeckPi
{
    public partial class AskIP : Form
    {
        public string ip;
        private PiDeck form;

        public AskIP(PiDeck form)
        {
            this.form = form;
            ip = "";
            InitializeComponent();
        }

        private void numbers_Click(object sender, EventArgs e)
        {
            Button numpad = sender as Button;

            textBox1.Text = textBox1.Text + numpad.Text;
        }

        private void delete_Click(object sender, EventArgs e)
        {
            textBox1.Text = textBox1.Text.Remove(textBox1.Text.Length - 2, 1);
        }

        private void enterButton_Click(object sender, EventArgs e)
        {
            ip = textBox1.Text;
            File.WriteAllText(@"PiDeck.ini", ip);
            this.Visible = false;
            this.form.fileCheck();
        }
    }
}
