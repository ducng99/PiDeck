using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using static StreamDeck.Utils;

namespace StreamDeck
{
    public class StreamDeckPC : Form
    {
        private TextBox nameTextBox;
        private Label nameLabel;
        private Label menuNameLabel;
        private FlowLayoutPanel decksPanel;
        private Panel menuNamePanel;
        private Panel titlePanel;
        private Label titleLabel;
        private Button saveButton;
        private RadioButton[] decksButton;
        private TextBox actionTextBox;
        private Label actionLabel;
        private Panel bottomPanel;
        private Label gridSizeLabel;
        private TextBox gridXTextBox;
        private Label xLabel;
        private TextBox gridYTextBox;
        private Panel menuPanel;
        private Label ipLabel;
        private TextBox ipTextBox;
        private Label bgLabel;
        private Button imageButton;
        private Panel actionPanel;
        private Panel namePanel;
        private OpenFileDialog openImageDialog;
        private Panel imagePanel;
        private TextBox imageTextBox;
        private PictureBox imageBox;
        private Label imageNoteLabel;
        private CheckBox useIconCheckBox;
        private OpenFileDialog openProgramDialog;
        private Label actionLabel2;
        private Button fileChooseButton;
        private Button viewButton;
        private Button backButton;
        private System.ComponentModel.IContainer components = null;

        private int screenWidth;
        private int screenHeight;
        private int buttonWidth;
        private int buttonHeight;
        public const string appname = "PiDeck";

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        private int gridX = 6;
        private int gridY = 4;
        private Decks[] decks;
        private static Page parentPage = null;
        private Page page;
        private static WebServer server = new WebServer(WebServer.getData);
        private NotifyIcon notifyIcon;
        private Panel barPanel;
        private Label barTitleLabel;
        private Button barCloseButton;
        private Panel extraActionPanel;
        private TextBox extraActionTextBox;
        private Label extraActionLabel;
        private ContextMenuStrip contextMenuStrip;
        private ToolStripMenuItem exitItem;
        private ListBox actionsListBox;
        private Button barMinimizeButton;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [STAThread]
        public static void Main(string[] args)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "netsh.exe",
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            foreach (string host in WebServer.hosts)
            {
                psi.Arguments = "http add urlacl url=" + host + " sddl=D:(A;;GX;;;S-1-1-0)";
                Process.Start(psi);
            }
            
            server.Run();

            OBSHandler.OBSSettings obsSettings = FileHandler.LoadObsSettings();
            OBSHandler obs = new OBSHandler(obsSettings);

            try
            {
                Task.Run(() => OBSHandler.InitConnect());
            }
            catch (TaskCanceledException)
            {
                //Yes cancel it bitch
            }

            FileHandler.readData(out parentPage);

            StreamDeckPC mainForm = new StreamDeckPC(ref parentPage);
            if (args.Length > 0)
            {
                foreach (string arg in args)
                    if (arg.Equals("/min"))
                    {
                        mainForm.WindowState = FormWindowState.Minimized;
                    }
            }

            Application.Run(mainForm);
        }

        public StreamDeckPC(ref Page page)
        {
            InitializeComponent();

            barTitleLabel.Image = ImageHandler.ResizeImage(Icon.ToBitmap(), 16, 16);
            ipTextBox.Text = WebServer.getIP();
            screenHeight = decksPanel.Size.Height - 5;
            screenWidth = decksPanel.Size.Width - 5;
            notifyIcon.Icon = this.Icon;

            //Initialize all action types
            ActionTypes[] types = (ActionTypes[])Enum.GetValues(typeof(ActionTypes));
            foreach (ActionTypes t in types)
            {
                actionsListBox.Items.Add(t);
            }

            //Adding up IDs
            Page.CountAllPages();

            InitializePage(ref page);
            InitializeDecks(ref this.decks);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        public void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StreamDeckPC));
            this.menuPanel = new System.Windows.Forms.Panel();
            this.extraActionPanel = new System.Windows.Forms.Panel();
            this.extraActionTextBox = new System.Windows.Forms.TextBox();
            this.fileChooseButton = new System.Windows.Forms.Button();
            this.viewButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
            this.extraActionLabel = new System.Windows.Forms.Label();
            this.actionLabel2 = new System.Windows.Forms.Label();
            this.actionLabel = new System.Windows.Forms.Label();
            this.actionPanel = new System.Windows.Forms.Panel();
            this.actionTextBox = new System.Windows.Forms.TextBox();
            this.useIconCheckBox = new System.Windows.Forms.CheckBox();
            this.imageBox = new System.Windows.Forms.PictureBox();
            this.imagePanel = new System.Windows.Forms.Panel();
            this.imageTextBox = new System.Windows.Forms.TextBox();
            this.imageButton = new System.Windows.Forms.Button();
            this.imageNoteLabel = new System.Windows.Forms.Label();
            this.bgLabel = new System.Windows.Forms.Label();
            this.nameLabel = new System.Windows.Forms.Label();
            this.namePanel = new System.Windows.Forms.Panel();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.menuNamePanel = new System.Windows.Forms.Panel();
            this.menuNameLabel = new System.Windows.Forms.Label();
            this.decksPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.titlePanel = new System.Windows.Forms.Panel();
            this.titleLabel = new System.Windows.Forms.Label();
            this.backButton = new System.Windows.Forms.Button();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.ipLabel = new System.Windows.Forms.Label();
            this.xLabel = new System.Windows.Forms.Label();
            this.gridSizeLabel = new System.Windows.Forms.Label();
            this.gridYTextBox = new System.Windows.Forms.TextBox();
            this.gridXTextBox = new System.Windows.Forms.TextBox();
            this.ipTextBox = new System.Windows.Forms.TextBox();
            this.openProgramDialog = new System.Windows.Forms.OpenFileDialog();
            this.openImageDialog = new System.Windows.Forms.OpenFileDialog();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exitItem = new System.Windows.Forms.ToolStripMenuItem();
            this.barPanel = new System.Windows.Forms.Panel();
            this.barMinimizeButton = new System.Windows.Forms.Button();
            this.barCloseButton = new System.Windows.Forms.Button();
            this.barTitleLabel = new System.Windows.Forms.Label();
            this.actionsListBox = new System.Windows.Forms.ListBox();
            this.menuPanel.SuspendLayout();
            this.extraActionPanel.SuspendLayout();
            this.actionPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox)).BeginInit();
            this.imagePanel.SuspendLayout();
            this.namePanel.SuspendLayout();
            this.menuNamePanel.SuspendLayout();
            this.titlePanel.SuspendLayout();
            this.bottomPanel.SuspendLayout();
            this.contextMenuStrip.SuspendLayout();
            this.barPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuPanel
            // 
            this.menuPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.menuPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.menuPanel.Controls.Add(this.actionPanel);
            this.menuPanel.Controls.Add(this.actionLabel2);
            this.menuPanel.Controls.Add(this.viewButton);
            this.menuPanel.Controls.Add(this.fileChooseButton);
            this.menuPanel.Controls.Add(this.extraActionPanel);
            this.menuPanel.Controls.Add(this.saveButton);
            this.menuPanel.Controls.Add(this.extraActionLabel);
            this.menuPanel.Location = new System.Drawing.Point(0, 392);
            this.menuPanel.Margin = new System.Windows.Forms.Padding(0);
            this.menuPanel.Name = "menuPanel";
            this.menuPanel.Size = new System.Drawing.Size(415, 235);
            this.menuPanel.TabIndex = 0;
            // 
            // extraActionPanel
            // 
            this.extraActionPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.extraActionPanel.Controls.Add(this.extraActionTextBox);
            this.extraActionPanel.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.extraActionPanel.Location = new System.Drawing.Point(147, 158);
            this.extraActionPanel.Name = "extraActionPanel";
            this.extraActionPanel.Size = new System.Drawing.Size(246, 29);
            this.extraActionPanel.TabIndex = 8;
            this.extraActionPanel.Visible = false;
            // 
            // extraActionTextBox
            // 
            this.extraActionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.extraActionTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.extraActionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.extraActionTextBox.Font = new System.Drawing.Font("Ubuntu", 10F);
            this.extraActionTextBox.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.extraActionTextBox.Location = new System.Drawing.Point(5, 6);
            this.extraActionTextBox.Margin = new System.Windows.Forms.Padding(5);
            this.extraActionTextBox.Name = "extraActionTextBox";
            this.extraActionTextBox.Size = new System.Drawing.Size(236, 16);
            this.extraActionTextBox.TabIndex = 1;
            // 
            // fileChooseButton
            // 
            this.fileChooseButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.fileChooseButton.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.fileChooseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.fileChooseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.fileChooseButton.Location = new System.Drawing.Point(363, 123);
            this.fileChooseButton.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.fileChooseButton.Name = "fileChooseButton";
            this.fileChooseButton.Size = new System.Drawing.Size(30, 29);
            this.fileChooseButton.TabIndex = 5;
            this.fileChooseButton.Text = "...";
            this.fileChooseButton.UseVisualStyleBackColor = false;
            this.fileChooseButton.Visible = false;
            this.fileChooseButton.Click += new System.EventHandler(this.fileChooseButton_Click);
            // 
            // viewButton
            // 
            this.viewButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.viewButton.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.viewButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.viewButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.viewButton.Font = new System.Drawing.Font("Ubuntu", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.viewButton.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.viewButton.Location = new System.Drawing.Point(84, 193);
            this.viewButton.Name = "viewButton";
            this.viewButton.Size = new System.Drawing.Size(161, 34);
            this.viewButton.TabIndex = 3;
            this.viewButton.Text = "View";
            this.viewButton.UseVisualStyleBackColor = false;
            this.viewButton.Visible = false;
            this.viewButton.Click += new System.EventHandler(this.viewButton_Click);
            // 
            // saveButton
            // 
            this.saveButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.saveButton.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.saveButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.saveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.saveButton.Font = new System.Drawing.Font("Ubuntu", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.saveButton.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.saveButton.Location = new System.Drawing.Point(251, 193);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(161, 34);
            this.saveButton.TabIndex = 3;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = false;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // extraActionLabel
            // 
            this.extraActionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.extraActionLabel.Font = new System.Drawing.Font("Ubuntu", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.extraActionLabel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.extraActionLabel.Location = new System.Drawing.Point(14, 161);
            this.extraActionLabel.Margin = new System.Windows.Forms.Padding(5);
            this.extraActionLabel.Name = "extraActionLabel";
            this.extraActionLabel.Size = new System.Drawing.Size(119, 20);
            this.extraActionLabel.TabIndex = 0;
            this.extraActionLabel.Text = "Args:";
            this.extraActionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.extraActionLabel.Visible = false;
            // 
            // actionLabel2
            // 
            this.actionLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.actionLabel2.Font = new System.Drawing.Font("Ubuntu", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.actionLabel2.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.actionLabel2.Location = new System.Drawing.Point(14, 125);
            this.actionLabel2.Margin = new System.Windows.Forms.Padding(5);
            this.actionLabel2.Name = "actionLabel2";
            this.actionLabel2.Size = new System.Drawing.Size(119, 20);
            this.actionLabel2.TabIndex = 0;
            this.actionLabel2.Text = "Path:";
            this.actionLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.actionLabel2.Visible = false;
            // 
            // actionLabel
            // 
            this.actionLabel.AutoSize = true;
            this.actionLabel.Font = new System.Drawing.Font("Ubuntu", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.actionLabel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.actionLabel.Location = new System.Drawing.Point(420, 92);
            this.actionLabel.Margin = new System.Windows.Forms.Padding(5);
            this.actionLabel.Name = "actionLabel";
            this.actionLabel.Size = new System.Drawing.Size(118, 25);
            this.actionLabel.TabIndex = 0;
            this.actionLabel.Text = "Actions List";
            // 
            // actionPanel
            // 
            this.actionPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.actionPanel.Controls.Add(this.actionTextBox);
            this.actionPanel.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.actionPanel.Location = new System.Drawing.Point(147, 123);
            this.actionPanel.Name = "actionPanel";
            this.actionPanel.Size = new System.Drawing.Size(213, 29);
            this.actionPanel.TabIndex = 7;
            this.actionPanel.Visible = false;
            // 
            // actionTextBox
            // 
            this.actionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.actionTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.actionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.actionTextBox.Font = new System.Drawing.Font("Ubuntu", 10F);
            this.actionTextBox.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.actionTextBox.Location = new System.Drawing.Point(5, 6);
            this.actionTextBox.Margin = new System.Windows.Forms.Padding(5);
            this.actionTextBox.Name = "actionTextBox";
            this.actionTextBox.Size = new System.Drawing.Size(203, 16);
            this.actionTextBox.TabIndex = 1;
            this.actionTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.startRecordKeys);
            // 
            // useIconCheckBox
            // 
            this.useIconCheckBox.AutoSize = true;
            this.useIconCheckBox.FlatAppearance.BorderSize = 0;
            this.useIconCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.useIconCheckBox.Font = new System.Drawing.Font("Ubuntu", 9.749999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.useIconCheckBox.Location = new System.Drawing.Point(91, 488);
            this.useIconCheckBox.Name = "useIconCheckBox";
            this.useIconCheckBox.Size = new System.Drawing.Size(195, 21);
            this.useIconCheckBox.TabIndex = 9;
            this.useIconCheckBox.Text = "Use program\'s icon as image";
            this.useIconCheckBox.UseVisualStyleBackColor = true;
            this.useIconCheckBox.Visible = false;
            this.useIconCheckBox.CheckedChanged += new System.EventHandler(this.useIconCheckBox_CheckedChanged);
            this.useIconCheckBox.VisibleChanged += new System.EventHandler(this.useIconCheckBox_VisibleChanged);
            // 
            // imageBox
            // 
            this.imageBox.Location = new System.Drawing.Point(14, 400);
            this.imageBox.Margin = new System.Windows.Forms.Padding(5);
            this.imageBox.Name = "imageBox";
            this.imageBox.Size = new System.Drawing.Size(64, 64);
            this.imageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.imageBox.TabIndex = 8;
            this.imageBox.TabStop = false;
            // 
            // imagePanel
            // 
            this.imagePanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.imagePanel.Controls.Add(this.imageTextBox);
            this.imagePanel.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.imagePanel.Location = new System.Drawing.Point(147, 435);
            this.imagePanel.Name = "imagePanel";
            this.imagePanel.Padding = new System.Windows.Forms.Padding(5);
            this.imagePanel.Size = new System.Drawing.Size(213, 29);
            this.imagePanel.TabIndex = 6;
            // 
            // imageTextBox
            // 
            this.imageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.imageTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.imageTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.imageTextBox.Font = new System.Drawing.Font("Ubuntu", 10F);
            this.imageTextBox.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.imageTextBox.Location = new System.Drawing.Point(5, 7);
            this.imageTextBox.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.imageTextBox.Name = "imageTextBox";
            this.imageTextBox.Size = new System.Drawing.Size(203, 16);
            this.imageTextBox.TabIndex = 1;
            // 
            // imageButton
            // 
            this.imageButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.imageButton.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.imageButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.imageButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.imageButton.Location = new System.Drawing.Point(363, 435);
            this.imageButton.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.imageButton.Name = "imageButton";
            this.imageButton.Size = new System.Drawing.Size(30, 29);
            this.imageButton.TabIndex = 5;
            this.imageButton.Text = "...";
            this.imageButton.UseVisualStyleBackColor = false;
            this.imageButton.Click += new System.EventHandler(this.imageChooseButton_Click);
            // 
            // imageNoteLabel
            // 
            this.imageNoteLabel.AutoSize = true;
            this.imageNoteLabel.Font = new System.Drawing.Font("Ubuntu", 8F, System.Drawing.FontStyle.Italic);
            this.imageNoteLabel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.imageNoteLabel.Location = new System.Drawing.Point(88, 466);
            this.imageNoteLabel.Margin = new System.Windows.Forms.Padding(20, 20, 20, 3);
            this.imageNoteLabel.Name = "imageNoteLabel";
            this.imageNoteLabel.Size = new System.Drawing.Size(204, 16);
            this.imageNoteLabel.TabIndex = 0;
            this.imageNoteLabel.Text = "Image will be resized and cropped to fit";
            // 
            // bgLabel
            // 
            this.bgLabel.AutoSize = true;
            this.bgLabel.Font = new System.Drawing.Font("Ubuntu", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bgLabel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.bgLabel.Location = new System.Drawing.Point(87, 438);
            this.bgLabel.Margin = new System.Windows.Forms.Padding(20);
            this.bgLabel.Name = "bgLabel";
            this.bgLabel.Size = new System.Drawing.Size(55, 20);
            this.bgLabel.TabIndex = 0;
            this.bgLabel.Text = "Image:";
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Font = new System.Drawing.Font("Ubuntu", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nameLabel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.nameLabel.Location = new System.Drawing.Point(87, 404);
            this.nameLabel.Margin = new System.Windows.Forms.Padding(20);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(54, 20);
            this.nameLabel.TabIndex = 0;
            this.nameLabel.Text = "Name:";
            // 
            // namePanel
            // 
            this.namePanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.namePanel.Controls.Add(this.nameTextBox);
            this.namePanel.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.namePanel.Location = new System.Drawing.Point(147, 400);
            this.namePanel.Name = "namePanel";
            this.namePanel.Padding = new System.Windows.Forms.Padding(5);
            this.namePanel.Size = new System.Drawing.Size(246, 29);
            this.namePanel.TabIndex = 7;
            // 
            // nameTextBox
            // 
            this.nameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nameTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.nameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.nameTextBox.Font = new System.Drawing.Font("Ubuntu", 10F);
            this.nameTextBox.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.nameTextBox.Location = new System.Drawing.Point(5, 7);
            this.nameTextBox.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(236, 16);
            this.nameTextBox.TabIndex = 1;
            // 
            // menuNamePanel
            // 
            this.menuNamePanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.menuNamePanel.Controls.Add(this.menuNameLabel);
            this.menuNamePanel.Location = new System.Drawing.Point(415, 25);
            this.menuNamePanel.Margin = new System.Windows.Forms.Padding(0);
            this.menuNamePanel.Name = "menuNamePanel";
            this.menuNamePanel.Size = new System.Drawing.Size(322, 62);
            this.menuNamePanel.TabIndex = 2;
            // 
            // menuNameLabel
            // 
            this.menuNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.menuNameLabel.Font = new System.Drawing.Font("Ubuntu", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.menuNameLabel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.menuNameLabel.Location = new System.Drawing.Point(0, 0);
            this.menuNameLabel.Name = "menuNameLabel";
            this.menuNameLabel.Size = new System.Drawing.Size(322, 65);
            this.menuNameLabel.TabIndex = 0;
            this.menuNameLabel.Text = "Decks Control";
            this.menuNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // decksPanel
            // 
            this.decksPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.decksPanel.Location = new System.Drawing.Point(0, 87);
            this.decksPanel.Margin = new System.Windows.Forms.Padding(0);
            this.decksPanel.Name = "decksPanel";
            this.decksPanel.Padding = new System.Windows.Forms.Padding(10);
            this.decksPanel.Size = new System.Drawing.Size(415, 305);
            this.decksPanel.TabIndex = 1;
            // 
            // titlePanel
            // 
            this.titlePanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.titlePanel.Controls.Add(this.titleLabel);
            this.titlePanel.Controls.Add(this.backButton);
            this.titlePanel.Location = new System.Drawing.Point(0, 25);
            this.titlePanel.Margin = new System.Windows.Forms.Padding(0);
            this.titlePanel.Name = "titlePanel";
            this.titlePanel.Size = new System.Drawing.Size(415, 62);
            this.titlePanel.TabIndex = 2;
            // 
            // titleLabel
            // 
            this.titleLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.titleLabel.Font = new System.Drawing.Font("Ubuntu", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleLabel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.titleLabel.Location = new System.Drawing.Point(48, 0);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(367, 62);
            this.titleLabel.TabIndex = 1;
            this.titleLabel.Text = "PiDeck";
            this.titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // backButton
            // 
            this.backButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.backButton.FlatAppearance.BorderSize = 0;
            this.backButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.backButton.Font = new System.Drawing.Font("Ubuntu", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.backButton.Location = new System.Drawing.Point(0, 0);
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(48, 62);
            this.backButton.TabIndex = 0;
            this.backButton.Text = "←";
            this.backButton.UseVisualStyleBackColor = true;
            this.backButton.Click += new System.EventHandler(this.backButton_Click);
            // 
            // bottomPanel
            // 
            this.bottomPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.bottomPanel.Controls.Add(this.ipLabel);
            this.bottomPanel.Controls.Add(this.xLabel);
            this.bottomPanel.Controls.Add(this.gridSizeLabel);
            this.bottomPanel.Controls.Add(this.gridYTextBox);
            this.bottomPanel.Controls.Add(this.gridXTextBox);
            this.bottomPanel.Controls.Add(this.ipTextBox);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.Location = new System.Drawing.Point(0, 627);
            this.bottomPanel.Margin = new System.Windows.Forms.Padding(0);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(737, 20);
            this.bottomPanel.TabIndex = 3;
            // 
            // ipLabel
            // 
            this.ipLabel.AutoSize = true;
            this.ipLabel.Font = new System.Drawing.Font("Ubuntu", 10F);
            this.ipLabel.Location = new System.Drawing.Point(180, 2);
            this.ipLabel.Margin = new System.Windows.Forms.Padding(0);
            this.ipLabel.Name = "ipLabel";
            this.ipLabel.Size = new System.Drawing.Size(55, 18);
            this.ipLabel.TabIndex = 2;
            this.ipLabel.Text = "Your IP:";
            // 
            // xLabel
            // 
            this.xLabel.AutoSize = true;
            this.xLabel.Font = new System.Drawing.Font("Ubuntu", 10F);
            this.xLabel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.xLabel.Location = new System.Drawing.Point(126, 3);
            this.xLabel.Margin = new System.Windows.Forms.Padding(3);
            this.xLabel.Name = "xLabel";
            this.xLabel.Size = new System.Drawing.Size(15, 18);
            this.xLabel.TabIndex = 0;
            this.xLabel.Text = "x";
            // 
            // gridSizeLabel
            // 
            this.gridSizeLabel.AutoSize = true;
            this.gridSizeLabel.Font = new System.Drawing.Font("Ubuntu", 10F);
            this.gridSizeLabel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.gridSizeLabel.Location = new System.Drawing.Point(20, 2);
            this.gridSizeLabel.Margin = new System.Windows.Forms.Padding(3);
            this.gridSizeLabel.Name = "gridSizeLabel";
            this.gridSizeLabel.Size = new System.Drawing.Size(64, 18);
            this.gridSizeLabel.TabIndex = 0;
            this.gridSizeLabel.Text = "Grid size:";
            // 
            // gridYTextBox
            // 
            this.gridYTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.gridYTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gridYTextBox.Font = new System.Drawing.Font("Ubuntu", 10F);
            this.gridYTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
            this.gridYTextBox.Location = new System.Drawing.Point(147, 2);
            this.gridYTextBox.MaxLength = 1;
            this.gridYTextBox.Name = "gridYTextBox";
            this.gridYTextBox.Size = new System.Drawing.Size(30, 16);
            this.gridYTextBox.TabIndex = 1;
            this.gridYTextBox.Text = "4";
            this.gridYTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.gridYTextBox.TextChanged += new System.EventHandler(this.gridYTextBox_TextChanged);
            // 
            // gridXTextBox
            // 
            this.gridXTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.gridXTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gridXTextBox.Font = new System.Drawing.Font("Ubuntu", 10F);
            this.gridXTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
            this.gridXTextBox.Location = new System.Drawing.Point(90, 2);
            this.gridXTextBox.MaxLength = 1;
            this.gridXTextBox.Name = "gridXTextBox";
            this.gridXTextBox.Size = new System.Drawing.Size(30, 16);
            this.gridXTextBox.TabIndex = 1;
            this.gridXTextBox.Text = "6";
            this.gridXTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.gridXTextBox.TextChanged += new System.EventHandler(this.gridXTextBox_TextChanged);
            // 
            // ipTextBox
            // 
            this.ipTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.ipTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ipTextBox.Font = new System.Drawing.Font("Ubuntu", 10F);
            this.ipTextBox.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.ipTextBox.Location = new System.Drawing.Point(238, 2);
            this.ipTextBox.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.ipTextBox.Name = "ipTextBox";
            this.ipTextBox.ReadOnly = true;
            this.ipTextBox.Size = new System.Drawing.Size(175, 16);
            this.ipTextBox.TabIndex = 1;
            // 
            // openProgramDialog
            // 
            this.openProgramDialog.Filter = "Executable files|*.exe;*.bat;*.cmd;*.url";
            this.openProgramDialog.Title = "Choose a program or shortcut";
            // 
            // openImageDialog
            // 
            this.openImageDialog.Filter = "Images|*.bmp;*.gif;*.ico;*.jpg;*.jpeg;*.png;*.exe";
            this.openImageDialog.Title = "Choose an image for the deck:";
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextMenuStrip;
            this.notifyIcon.Text = "PiDeck";
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseDoubleClick);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(93, 26);
            // 
            // exitItem
            // 
            this.exitItem.Name = "exitItem";
            this.exitItem.Size = new System.Drawing.Size(92, 22);
            this.exitItem.Text = "Exit";
            this.exitItem.Click += new System.EventHandler(this.exitItem_Click);
            // 
            // barPanel
            // 
            this.barPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.barPanel.Controls.Add(this.barMinimizeButton);
            this.barPanel.Controls.Add(this.barCloseButton);
            this.barPanel.Controls.Add(this.barTitleLabel);
            this.barPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.barPanel.Location = new System.Drawing.Point(0, 0);
            this.barPanel.Margin = new System.Windows.Forms.Padding(0);
            this.barPanel.Name = "barPanel";
            this.barPanel.Size = new System.Drawing.Size(737, 25);
            this.barPanel.TabIndex = 4;
            this.barPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.barMouseMoved);
            // 
            // barMinimizeButton
            // 
            this.barMinimizeButton.FlatAppearance.BorderSize = 0;
            this.barMinimizeButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
            this.barMinimizeButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.barMinimizeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.barMinimizeButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F);
            this.barMinimizeButton.Location = new System.Drawing.Point(659, -10);
            this.barMinimizeButton.Margin = new System.Windows.Forms.Padding(0);
            this.barMinimizeButton.Name = "barMinimizeButton";
            this.barMinimizeButton.Size = new System.Drawing.Size(39, 35);
            this.barMinimizeButton.TabIndex = 1;
            this.barMinimizeButton.Text = "_";
            this.barMinimizeButton.UseVisualStyleBackColor = true;
            this.barMinimizeButton.Click += new System.EventHandler(this.minimizeButton_Click);
            // 
            // barCloseButton
            // 
            this.barCloseButton.FlatAppearance.BorderSize = 0;
            this.barCloseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.barCloseButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Red;
            this.barCloseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.barCloseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F);
            this.barCloseButton.Location = new System.Drawing.Point(698, -5);
            this.barCloseButton.Margin = new System.Windows.Forms.Padding(0);
            this.barCloseButton.Name = "barCloseButton";
            this.barCloseButton.Size = new System.Drawing.Size(39, 30);
            this.barCloseButton.TabIndex = 1;
            this.barCloseButton.Text = "x";
            this.barCloseButton.UseVisualStyleBackColor = true;
            this.barCloseButton.Click += new System.EventHandler(this.barCloseButton_Click);
            // 
            // barTitleLabel
            // 
            this.barTitleLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.barTitleLabel.Location = new System.Drawing.Point(3, 0);
            this.barTitleLabel.Name = "barTitleLabel";
            this.barTitleLabel.Size = new System.Drawing.Size(64, 25);
            this.barTitleLabel.TabIndex = 0;
            this.barTitleLabel.Text = "PiDeck";
            this.barTitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.barTitleLabel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.barMouseMoved);
            // 
            // actionsListBox
            // 
            this.actionsListBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.actionsListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.actionsListBox.Font = new System.Drawing.Font("Ubuntu", 10F);
            this.actionsListBox.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.actionsListBox.FormattingEnabled = true;
            this.actionsListBox.IntegralHeight = false;
            this.actionsListBox.ItemHeight = 17;
            this.actionsListBox.Location = new System.Drawing.Point(420, 127);
            this.actionsListBox.Margin = new System.Windows.Forms.Padding(5);
            this.actionsListBox.Name = "actionsListBox";
            this.actionsListBox.Size = new System.Drawing.Size(312, 495);
            this.actionsListBox.TabIndex = 11;
            this.actionsListBox.SelectedIndexChanged += new System.EventHandler(this.actionsListBox_SelectedIndexChanged);
            // 
            // StreamDeckPC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.ClientSize = new System.Drawing.Size(737, 647);
            this.Controls.Add(this.actionsListBox);
            this.Controls.Add(this.useIconCheckBox);
            this.Controls.Add(this.barPanel);
            this.Controls.Add(this.imagePanel);
            this.Controls.Add(this.imageBox);
            this.Controls.Add(this.imageButton);
            this.Controls.Add(this.bottomPanel);
            this.Controls.Add(this.actionLabel);
            this.Controls.Add(this.titlePanel);
            this.Controls.Add(this.decksPanel);
            this.Controls.Add(this.menuNamePanel);
            this.Controls.Add(this.imageNoteLabel);
            this.Controls.Add(this.nameLabel);
            this.Controls.Add(this.bgLabel);
            this.Controls.Add(this.namePanel);
            this.Controls.Add(this.menuPanel);
            this.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "StreamDeckPC";
            this.Text = "PiDeck";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.StreamDeckPC_FormClosed);
            this.Resize += new System.EventHandler(this.formResize_Changed);
            this.menuPanel.ResumeLayout(false);
            this.extraActionPanel.ResumeLayout(false);
            this.extraActionPanel.PerformLayout();
            this.actionPanel.ResumeLayout(false);
            this.actionPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox)).EndInit();
            this.imagePanel.ResumeLayout(false);
            this.imagePanel.PerformLayout();
            this.namePanel.ResumeLayout(false);
            this.namePanel.PerformLayout();
            this.menuNamePanel.ResumeLayout(false);
            this.titlePanel.ResumeLayout(false);
            this.bottomPanel.ResumeLayout(false);
            this.bottomPanel.PerformLayout();
            this.contextMenuStrip.ResumeLayout(false);
            this.barPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void InitializePage(ref Page page)
        {
            this.page = page;
            this.decks = this.page.decks;
            this.gridX = this.page.x;
            this.gridY = this.page.y;
            if (this.page.preID != -1)
            {
                this.backButton.Visible = true;
                this.Text = page.name + " - " + appname;
                this.titleLabel.Text = page.name + " - " + appname;
            }
            else
            {
                this.backButton.Visible = false;
                this.Text = appname;
                this.titleLabel.Text = appname;
            }
            gridXTextBox.Text = gridX.ToString();
            gridYTextBox.Text = gridY.ToString();
            InitializeDecks(ref this.decks);
        }

        private void InitializeDecks(ref Decks[] decks)
        {
            int checkedInd = GetChecked();

            for (int i = 0; i < decks.Length; i++)
            {
                if (decks[i].Action.StartsWith("[FOLDER]") && decks[i].innerPageID == -1)
                {
                    Page innerPage = new Page(true)
                    {
                        name = decks[i].Name,
                        preID = page.id
                    };
                    innerPage.decks = new Decks[innerPage.x * innerPage.y];
                    for (int n = 0; n < innerPage.decks.Length; n++)
                    {
                        innerPage.decks[n] = new Decks();
                    }
                    decks[i].innerPageID = innerPage.id;
                    FileHandler.writeData(ref innerPage);
                }
            }

            if (decks.Length < gridX * gridY)
            {
                Decks[] tmp = new Decks[gridX * gridY - decks.Length];
                for (int i = 0; i < tmp.Length; i++)
                {
                    tmp[i] = new Decks();
                }
                decks = decks.Concat(tmp).ToArray();
                FileHandler.writeData(ref page);
            }

            this.decksButton = new RadioButton[gridX * gridY];

            buttonWidth = (screenWidth - gridX * decksPanel.Padding.All - decksPanel.Padding.All) / gridX;
            buttonHeight = (screenHeight - gridY * decksPanel.Padding.All - decksPanel.Padding.All) / gridY;

            for (int i = 0; i < gridX * gridY; i++)
            {
                decksButton[i] = new RadioButton();
                decksButton[i].Appearance = Appearance.Button;
                decksButton[i].BackColor = Color.FromArgb(50, 50, 50);
                if (!decks[i].image.Equals(""))
                {
                    decksButton[i].Image = ImageHandler.Base64ToImage(decks[i].image);
                    decksButton[i].ImageAlign = ContentAlignment.MiddleCenter;
                }
                else
                    decksButton[i].Text = decks[i].Name;
                decksButton[i].FlatAppearance.BorderSize = 0;
                decksButton[i].FlatStyle = FlatStyle.Flat;
                decksButton[i].FlatAppearance.MouseOverBackColor = Color.FromArgb(70, 70, 70);
                decksButton[i].FlatAppearance.CheckedBackColor = Color.FromArgb(70, 70, 70);
                decksButton[i].FlatAppearance.MouseDownBackColor = Color.FromArgb(80, 80, 80);
                decksButton[i].Font = new Font("Ubuntu", 12.0f);
                decksButton[i].Name = "decks" + i;
                decksButton[i].Size = new Size(buttonWidth, buttonHeight);
                decksButton[i].TabIndex = 0;
                decksButton[i].TextAlign = ContentAlignment.MiddleCenter;
                decksButton[i].TextImageRelation = TextImageRelation.Overlay;
                decksButton[i].UseVisualStyleBackColor = false;
                if (i == checkedInd) decksButton[i].Checked = true;
                decksButton[i].CheckedChanged += new EventHandler(decksButton_Click);
            }

            this.decksPanel.Controls.Clear();
            foreach (RadioButton button in decksButton)
                this.decksPanel.Controls.Add(button);
        }
        
        private void ActionFormGen(string action)
        {
            if (action.StartsWith("[PROGRAM]"))
            {
                this.actionsListBox.SelectedIndex = (int)ActionTypes.Program;
                string[] cmd = action.RemoveString("[PROGRAM]").SplitString("[ARGS]");
                this.actionTextBox.Text = cmd[0];
                if (cmd.Length > 1)
                {
                    this.extraActionTextBox.Text = cmd[1];
                }
            }
            else if (action.StartsWith("[KEYS]"))
            {
                this.actionsListBox.SelectedIndex = (int)ActionTypes.Keys;
                this.actionTextBox.Text = action.RemoveString("[KEYS]");
            }
            else if(action.StartsWith("[FOLDER]"))
            {
                this.actionsListBox.SelectedIndex = (int)ActionTypes.Folder;
                this.actionTextBox.Text = "";
            }
            else if (action.StartsWith("[CMD]"))
            {
                this.actionsListBox.SelectedIndex = (int)ActionTypes.Cmd;
                this.actionTextBox.Text = action.RemoveString("[CMD]");
            }
            else if (action.StartsWith("[OBS]"))
            {
                if (action.Contains("[StartStopStreaming]"))
                {
                    this.actionsListBox.SelectedIndex = (int)ActionTypes.OBS_StartStopStreaming;
                }
                else if (action.Contains("[StudioModeToggle]"))
                {
                    this.actionsListBox.SelectedIndex = (int)ActionTypes.OBS_StudioModeToggle;
                }
                else if (action.Contains("[SetScene]"))
                {
                    this.actionsListBox.SelectedIndex = (int)ActionTypes.OBS_SetScence;
                    this.actionTextBox.Text = action.RemoveString("[OBS][SetScene]");
                }
            }
            else if (action.StartsWith("[EMPTY]"))
            {
                this.actionsListBox.SelectedIndex = (int)ActionTypes.Empty;
                this.actionTextBox.Text = "";
            }
        }

        private void decksButton_Click(object sender, EventArgs e)
        {
            useIconCheckBox.Checked = false;
            RadioButton button = sender as RadioButton;
            int i = int.Parse(button.Name.TrimStart("decks".ToCharArray()));
            if (i != -1)
            {
                nameTextBox.Text = decks[i].Name;
                ActionFormGen(decks[i].Action);
                if (!decks[i].image.Equals(""))
                    imageBox.Image = ImageHandler.Base64ToImage(decks[i].image);
                else
                    imageBox.Image = null;
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            int i = GetChecked();
            if (i != -1)
            {
                decks[i].Name = nameTextBox.Text;
                switch ((ActionTypes)actionsListBox.SelectedIndex)
                {
                    case ActionTypes.Program:
                        decks[i].Action = ActionFormHandler((ActionTypes)actionsListBox.SelectedIndex, actionTextBox.Text + (!extraActionTextBox.Text.Equals("") ? ("[ARGS]" + extraActionTextBox.Text) : ""));
                        break;
                    case ActionTypes.Keys:
                    case ActionTypes.Cmd:
                    case ActionTypes.OBS_SetScence:
                        decks[i].Action = ActionFormHandler((ActionTypes)actionsListBox.SelectedIndex, actionTextBox.Text);
                        break;
                    case ActionTypes.OBS_StartStopStreaming:
                    case ActionTypes.OBS_StudioModeToggle:
                    case ActionTypes.Folder:
                        decks[i].Action = ActionFormHandler((ActionTypes)actionsListBox.SelectedIndex);
                        break;
                    case ActionTypes.Empty:
                        decks[i].innerPageID = -1;
                        goto case ActionTypes.Folder;
                }

                if (useIconCheckBox.Checked)
                {
                    try
                    {
                        Icon icon = Icon.ExtractAssociatedIcon(actionTextBox.Text);
                        using (Bitmap bm = icon.ToBitmap())
                        {
                            decks[i].image = ImageHandler.BitmapToBase64(bm);
                        }
                    }
                    catch (ArgumentException)
                    {
                        MessageBox.Show("Cannot grab icon from the program", appname);
                        useIconCheckBox.Checked = false;
                    }
                }
                else if (!imageTextBox.Text.Equals(""))
                {
                    if (imageTextBox.Text.EndsWith(".exe"))
                    {
                        try
                        {
                            Icon icon = Icon.ExtractAssociatedIcon(imageTextBox.Text);
                            using (Bitmap bm = icon.ToBitmap())
                            {
                                decks[i].image = ImageHandler.BitmapToBase64(bm);
                            }
                        }
                        catch (ArgumentException)
                        {
                            MessageBox.Show("Cannot grab icon from the program", appname);
                        }
                    }
                    else if (imageTextBox.Text.EndsWith(".gif"))
                    {
                        /*using (Bitmap bm = new Bitmap(imageTextBox.Text))
                        {
                            decks[i].image = ImageHandler.BitmapToBase64(bm);
                        }*/
                        decks[i].image = ImageHandler.GifToBase64(imageTextBox.Text, buttonWidth, buttonHeight);
                    }
                    else
                    {
                        using (Bitmap bm = ImageHandler.ImageSuperHandle(imageTextBox.Text, buttonWidth, buttonHeight))
                            decks[i].image = ImageHandler.BitmapToBase64(bm);
                    }
                }

                InitializeDecks(ref this.decks);
                FileHandler.writeData(ref page);
            }
        }

        private void gridXTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                gridX = int.Parse(gridXTextBox.Text);
                this.page.x = gridX;
                FileHandler.writeData(ref page);
                InitializeDecks(ref this.decks);
            }
            catch
            {
                gridXTextBox.Text = "";
            }
        }

        private void gridYTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                gridY = int.Parse(gridYTextBox.Text);
                this.page.y = gridY;
                FileHandler.writeData(ref page);
                InitializeDecks(ref this.decks);
            }
            catch
            {
                gridYTextBox.Text = "";
            }
        }

        private void StreamDeckPC_FormClosed(object sender, FormClosedEventArgs e)
        {
            server.Stop();
            OBSHandler.obs.Disconnect();
            Application.ExitThread();
        }

        private void actionsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox box = sender as ListBox;
            switch ((ActionTypes)box.SelectedIndex)
            {
                case ActionTypes.Program:     //Program
                    actionLabel2.Visible = true;
                    actionPanel.Visible = true;
                    actionTextBox.ReadOnly = false;
                    fileChooseButton.Visible = true;
                    extraActionLabel.Visible = true;
                    extraActionPanel.Visible = true;
                    viewButton.Visible = false;
                    actionLabel2.Text = "Path:";
                    useIconCheckBox.Visible = true;
                    actionPanel.Size = new Size(213, 29);
                    break;
                case ActionTypes.Keys:     //Keys
                    actionLabel2.Visible = true;
                    actionPanel.Visible = true;
                    actionTextBox.ReadOnly = true;
                    fileChooseButton.Visible = false;
                    extraActionLabel.Visible = false;
                    extraActionPanel.Visible = false;
                    viewButton.Visible = false;
                    useIconCheckBox.Visible = false;
                    actionLabel2.Text = "Keys:";
                    actionPanel.Size = new Size(246, 29);
                    break;
                case ActionTypes.Folder:     //Folder
                    actionLabel2.Visible = false;
                    actionPanel.Visible = false;
                    fileChooseButton.Visible = false;
                    extraActionLabel.Visible = false;
                    extraActionPanel.Visible = false;
                    viewButton.Visible = true;
                    useIconCheckBox.Visible = false;
                    break;
                case ActionTypes.Cmd:     //Other
                    actionLabel2.Visible = true;
                    actionPanel.Visible = true;
                    actionTextBox.ReadOnly = false;
                    fileChooseButton.Visible = false;
                    extraActionLabel.Visible = false;
                    extraActionPanel.Visible = false;
                    viewButton.Visible = false;
                    useIconCheckBox.Visible = false;
                    actionLabel2.Text = "Cmd:";
                    actionPanel.Size = new Size(246, 29);
                    break;
                case ActionTypes.OBS_SetScence:
                    actionLabel2.Visible = true;
                    actionPanel.Visible = true;
                    actionTextBox.ReadOnly = false;
                    fileChooseButton.Visible = false;
                    extraActionLabel.Visible = false;
                    extraActionPanel.Visible = false;
                    viewButton.Visible = false;
                    useIconCheckBox.Visible = false;
                    actionLabel2.Text = "Scene name:";
                    actionPanel.Size = new Size(246, 29);
                    break;
                case ActionTypes.Empty:     //Empty
                    actionLabel2.Visible = false;
                    actionPanel.Visible = false;
                    actionTextBox.ReadOnly = false;
                    fileChooseButton.Visible = false;
                    extraActionLabel.Visible = false;
                    extraActionPanel.Visible = false;
                    viewButton.Visible = false;
                    useIconCheckBox.Visible = false;
                    break;
                default:
                    break;
            }
        }

        private void fileChooseButton_Click(object sender, EventArgs e)
        {
            DialogResult result = openProgramDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                actionTextBox.Text = openProgramDialog.FileName;
            }
        }

        private void viewButton_Click(object sender, EventArgs e)
        {
            actionsListBox.SelectedIndex = (int)ActionTypes.Empty;
            int innerPageID = -1;
            int i = GetChecked();
            if (i != -1 && decks[i].innerPageID != -1)
            {
                innerPageID = this.decks[i].innerPageID;
                this.decksButton[i].Checked = false;
                FileHandler.readData(out Page innerPage, innerPageID);

                if (!innerPage.name.Equals(decks[i].Name))
                {
                    innerPage.name = decks[i].Name;
                }

                InitializePage(ref innerPage);
            }
        }

        public int GetChecked()
        {
            if (decksButton != null)
                for (int i = 0; i < decksButton.Length; i++)
                {
                    if (decksButton[i].Checked)
                        return i;
                }

            return -1;
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            actionsListBox.SelectedIndex = (int)ActionTypes.Empty;
            int checkedInd = GetChecked();
            if (checkedInd != -1) this.decksButton[GetChecked()].Checked = false;
            FileHandler.readData(out Page parent, page.preID);
            InitializePage(ref parent);
        }

        private void startRecordKeys(object sender, KeyEventArgs e)
        {
            if (actionsListBox.SelectedIndex == (int)ActionTypes.Keys)
            {
                string keys = "";
                foreach (Key key in GetDownKeys())
                {
                    keys += (keys.Equals("") ? "" : "+") + key.ToString();
                }

                actionTextBox.Text = keys;
            }
        }

        private void imageChooseButton_Click(object sender, EventArgs e)
        {
            DialogResult result = openImageDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                imageTextBox.Text = openImageDialog.FileName;

                if (openImageDialog.FileName.EndsWith(".exe"))
                {
                    try
                    {
                        Icon icon = Icon.ExtractAssociatedIcon(openImageDialog.FileName);
                        imageBox.Image = icon.ToBitmap();
                    }
                    catch
                    {
                        MessageBox.Show("Cannot grab icon from file!", appname);
                    }
                }
                else
                {
                    using (Image bg = Image.FromFile(imageTextBox.Text))
                    {
                        Bitmap bm;
                        if (bg.Height >= bg.Width && bg.Height > imageBox.Size.Height)
                        {
                            int height = imageBox.Size.Height;
                            int width = (int)Math.Round((bg.Width / (double)bg.Height) * imageBox.Size.Height);
                            bm = ImageHandler.ResizeImage(bg, width, height);
                        }
                        else if (bg.Width > bg.Height && bg.Width > imageBox.Size.Width)
                        {
                            int width = imageBox.Size.Width;
                            int height = (int)Math.Round((bg.Height / (double)bg.Width) * imageBox.Size.Width);
                            bm = ImageHandler.ResizeImage(bg, width, height);
                        }
                        else
                            bm = new Bitmap(bg);
                        imageBox.Image = bm;
                    }
                }
            }
        }

        private void useIconCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            imagePanel.Enabled = !useIconCheckBox.Checked;
            imageButton.Enabled = !useIconCheckBox.Checked;
        }

        private void useIconCheckBox_VisibleChanged(object sender, EventArgs e)
        {
            imagePanel.Enabled = true;
            imageButton.Enabled = true;
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void formResize_Changed(object sender = null, EventArgs e = null)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        private void barCloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void barMouseMoved(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void minimizeButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void exitItem_Click(object sender, EventArgs e)
        {
            barCloseButton_Click(sender, e);
        }
    }
}
