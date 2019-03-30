using System;
using System.Drawing;
using System.Threading;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Net;

namespace StreamDeckPi
{
    public class PiDeck : Form
    {
        private System.ComponentModel.IContainer components = null;

        private string ipadd = "";
        private static string fileName = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase), @"PiDeck.ini").TrimStart("file:\\".ToCharArray());
        private static PiDeck form = new PiDeck();
        private static AskIP askForm;

        private int screenWidth;
        private int screenHeight;

        private int gridX;
        private int gridY;
        private static Page page;
        private FlowLayoutPanel decksPanel;
        private Panel panel2;
        private Label title;
        private Button backButton;
        private Decks[] curDecks;
        private Button closeButton;
        private Button button1;
        private Button[] decksButton;

        public static void Main(string[] args)
        {
            if (args.Length > 0)
                fileName = args[0];
            form.fileCheck();
            Application.Run(form);
        }

        public PiDeck()
        {
            InitializeComponent();
            screenWidth = decksPanel.Size.Width - 5;
            screenHeight = decksPanel.Size.Height - 5;
        }

        private void InitializeDecks(ref Decks[] decks)
        {
            this.decksButton = new Button[gridX * gridY];

            for (int i = 0; i < decksButton.Length; i++)
            {
                decksButton[i] = new Button();
                decksButton[i].BackColor = Color.FromArgb(50, 50, 50);
                decksButton[i].FlatAppearance.BorderSize = 0;
                decksButton[i].FlatStyle = FlatStyle.Flat;
                decksButton[i].FlatAppearance.MouseOverBackColor = Color.FromArgb(70, 70, 70);
                decksButton[i].FlatAppearance.MouseDownBackColor = Color.FromArgb(80, 80, 80);
                decksButton[i].Font = new Font("Ubuntu", 12.0f);
                decksButton[i].Name = "decks" + i;
                decksButton[i].Size = new Size((screenWidth - gridX * decksPanel.Padding.All - decksPanel.Padding.All) / gridX, (screenHeight - gridY * decksPanel.Padding.All - decksPanel.Padding.All) / gridY);
                decksButton[i].TabIndex = 0;

                if (!decks[i].image.Equals(""))
                {
                    decksButton[i].Image = Base64ToBitmap(decks[i].image);
                    decksButton[i].ImageAlign = ContentAlignment.MiddleCenter;
                }
                else
                    decksButton[i].Text = decks[i].Name;
                decksButton[i].TextAlign = ContentAlignment.MiddleCenter;
                decksButton[i].TextImageRelation = TextImageRelation.Overlay;
                decksButton[i].UseVisualStyleBackColor = false;
                decksButton[i].Click += new EventHandler(decksButton_Click);
            }

            this.decksPanel.Controls.Clear();
            foreach (Button button in decksButton)
                this.decksPanel.Controls.Add(button);
        }

        private void decksButton_Click(object sender, EventArgs e)
        {
            Button chosen = sender as Button;
            int i = int.Parse(chosen.Name.TrimStart("decks".ToCharArray()));

            string action = curDecks[i].Action;

            if (action.StartsWith("[EMPTY]"))
            {
            }
            else if (action.StartsWith("[FOLDER]"))
            {
                string json;
                try
                {
                    using (var client = new WebClient())
                    {
                        json = client.DownloadString("http://" + ipadd + ":3199/pideck/?q=getDeck" + this.curDecks[i].innerPageID);
                    }
                    page = JsonConvert.DeserializeObject<Page>(json);
                    gridX = page.x;
                    gridY = page.y;
                    curDecks = page.decks;
                    title.Text = page.name + " - PiDeck";
                    backButton.Visible = true;
                    InitializeDecks(ref curDecks);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "PiDeck");
                }
            }
            else
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadString("http://" + ipadd + ":3199/pideck/?q=" + action);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "PiDeck");
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.decksPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.backButton = new System.Windows.Forms.Button();
            this.title = new System.Windows.Forms.Label();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // decksPanel
            // 
            this.decksPanel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.decksPanel.Location = new System.Drawing.Point(0, 20);
            this.decksPanel.Margin = new System.Windows.Forms.Padding(0);
            this.decksPanel.Name = "decksPanel";
            this.decksPanel.Padding = new System.Windows.Forms.Padding(10);
            this.decksPanel.Size = new System.Drawing.Size(415, 305);
            this.decksPanel.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.button1);
            this.panel2.Controls.Add(this.closeButton);
            this.panel2.Controls.Add(this.backButton);
            this.panel2.Controls.Add(this.title);
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(415, 20);
            this.panel2.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.button1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Red;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Ubuntu", 12F);
            this.button1.ForeColor = System.Drawing.SystemColors.Control;
            this.button1.Location = new System.Drawing.Point(370, -6);
            this.button1.Margin = new System.Windows.Forms.Padding(0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(20, 27);
            this.button1.TabIndex = 1;
            this.button1.Text = "↻";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.reloadButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.closeButton.FlatAppearance.BorderSize = 0;
            this.closeButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.closeButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Red;
            this.closeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeButton.Font = new System.Drawing.Font("Ubuntu", 12F);
            this.closeButton.ForeColor = System.Drawing.SystemColors.Control;
            this.closeButton.Location = new System.Drawing.Point(390, -6);
            this.closeButton.Margin = new System.Windows.Forms.Padding(0);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(20, 27);
            this.closeButton.TabIndex = 1;
            this.closeButton.Text = "X";
            this.closeButton.UseVisualStyleBackColor = false;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // backButton
            // 
            this.backButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.backButton.FlatAppearance.BorderSize = 0;
            this.backButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.backButton.Font = new System.Drawing.Font("Ubuntu", 14F, System.Drawing.FontStyle.Bold);
            this.backButton.ForeColor = System.Drawing.SystemColors.Control;
            this.backButton.Location = new System.Drawing.Point(0, -8);
            this.backButton.Margin = new System.Windows.Forms.Padding(0);
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(37, 30);
            this.backButton.TabIndex = 1;
            this.backButton.Text = "←";
            this.backButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.backButton.UseVisualStyleBackColor = false;
            this.backButton.Visible = false;
            this.backButton.Click += new System.EventHandler(this.backButton_Click);
            // 
            // title
            // 
            this.title.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.title.Dock = System.Windows.Forms.DockStyle.Top;
            this.title.Font = new System.Drawing.Font("Ubuntu", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.title.ForeColor = System.Drawing.SystemColors.Control;
            this.title.Location = new System.Drawing.Point(0, 0);
            this.title.Margin = new System.Windows.Forms.Padding(0);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(415, 20);
            this.title.TabIndex = 0;
            this.title.Text = "PiDeck";
            this.title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // PiDeck
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkSlateGray;
            this.ClientSize = new System.Drawing.Size(415, 325);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.decksPanel);
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "PiDeck";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "PiDeck";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private void askIP()
        {
            askForm.Visible = true;
            askForm.BringToFront();
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            string json = "";
            try
            {
                using (var client = new WebClient())
                {
                    json = client.DownloadString("http://" + ipadd + ":3199/pideck/?q=getDeck" + page.preID);
                }
                page = JsonConvert.DeserializeObject<Page>(json);
                gridX = page.x;
                gridY = page.y;
                curDecks = page.decks;
                if (page.preID != -1)
                {
                    title.Text = page.name + " - PiDeck";
                    backButton.Visible = true;
                }
                else
                {
                    title.Text = "PiDeck";
                    backButton.Visible = false;
                }
                InitializeDecks(ref curDecks);
                }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "PiDeck");
            }
        }

        public void fileCheck()
        {
            bool ok = false;
            do
            {
                //form.Visible = false;
                if (File.Exists(fileName))
                {
                    string readFile = File.ReadAllText(fileName);
                    if (!readFile.Equals(""))
                    {
                        ipadd = readFile;
                        try
                        {
                            using (var client = new WebClient())
                            {
                                //MessageBox.Show("http://" + ipadd + ":3199/pideck/?q=getDeck0");
                                var responseString = client.DownloadString("http://" + ipadd + ":3199/pideck/?q=getDeck0");
                                //MessageBox.Show(responseString);
                                page = JsonConvert.DeserializeObject<Page>(responseString);
                                gridX = page.x;
                                gridY = page.y;
                                curDecks = page.decks;
                                //form.Visible = true;
                                ok = true;
                                InitializeDecks(ref curDecks);
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "PiDeck");
                            //askIP();
                        }
                    }
                    else
                        MessageBox.Show("Emp");
                        //askIP();
                }
                else
                    MessageBox.Show("404\n" + fileName);
                    //askIP();
                Thread.Sleep(1);
            } while (!ok);
        }

        public static Bitmap Base64ToBitmap(string base64)
        {
            byte[] imageBytes = Convert.FromBase64String(base64);

            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                Bitmap image = new Bitmap(Image.FromStream(ms, true));
                return image;
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void reloadButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var client = new WebClient())
                {
                    var responseString = client.DownloadString("http://" + ipadd + ":3199/pideck/?q=getDeck0");
                    page = JsonConvert.DeserializeObject<Page>(responseString);
                    gridX = page.x;
                    gridY = page.y;
                    curDecks = page.decks;
                    InitializeDecks(ref curDecks);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "PiDeck");
            }
        }
    }
}
