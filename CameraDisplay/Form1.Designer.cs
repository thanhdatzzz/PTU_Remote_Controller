
namespace CameraDisplay
{
    partial class up
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cboDevices = new System.Windows.Forms.ComboBox();
            this.Stop = new System.Windows.Forms.Button();
            this.Start = new System.Windows.Forms.Button();
            this.fileSystemWatcher1 = new System.IO.FileSystemWatcher();
            this.Refresh = new System.Windows.Forms.Button();
            this.btnCapture = new System.Windows.Forms.Button();
            this.btnRecord = new System.Windows.Forms.Button();
            this.left = new System.Windows.Forms.Button();
            this.down = new System.Windows.Forms.Button();
            this.right = new System.Windows.Forms.Button();
            this.upp = new System.Windows.Forms.Button();
            this.cboPorts = new System.Windows.Forms.ComboBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.time = new System.Windows.Forms.TextBox();
            this.btnChooseFolder = new System.Windows.Forms.Button();
            this.txtSavePath = new System.Windows.Forms.TextBox();
            this.Refresh2 = new System.Windows.Forms.Button();
            this.Home = new System.Windows.Forms.Button();
            this.lblPanPos = new System.Windows.Forms.Label();
            this.lblTiltPos = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.numPanOffset = new System.Windows.Forms.NumericUpDown();
            this.numTiltOffset = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbZoom = new System.Windows.Forms.TrackBar();
            this.txtIpAddress = new System.Windows.Forms.TextBox();
            this.rbIp = new System.Windows.Forms.RadioButton();
            this.rbUsb = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.Camera = new System.Windows.Forms.Label();
            this.cboAudioDevices = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnGoHome = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPanOffset)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTiltOffset)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbZoom)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // cboDevices
            // 
            this.cboDevices.FormattingEnabled = true;
            this.cboDevices.Location = new System.Drawing.Point(67, 13);
            this.cboDevices.Margin = new System.Windows.Forms.Padding(2);
            this.cboDevices.Name = "cboDevices";
            this.cboDevices.Size = new System.Drawing.Size(110, 21);
            this.cboDevices.TabIndex = 1;
            this.cboDevices.SelectedIndexChanged += new System.EventHandler(this.cboDevices_SelectedIndexChanged);
            // 
            // Stop
            // 
            this.Stop.AutoSize = true;
            this.Stop.BackColor = System.Drawing.Color.Red;
            this.Stop.Location = new System.Drawing.Point(244, 9);
            this.Stop.Margin = new System.Windows.Forms.Padding(2);
            this.Stop.Name = "Stop";
            this.Stop.Size = new System.Drawing.Size(56, 27);
            this.Stop.TabIndex = 4;
            this.Stop.Text = "Stop";
            this.Stop.UseVisualStyleBackColor = false;
            this.Stop.Click += new System.EventHandler(this.Stop_Click);
            // 
            // Start
            // 
            this.Start.AutoSize = true;
            this.Start.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.Start.Location = new System.Drawing.Point(184, 9);
            this.Start.Margin = new System.Windows.Forms.Padding(2);
            this.Start.Name = "Start";
            this.Start.Size = new System.Drawing.Size(56, 27);
            this.Start.TabIndex = 5;
            this.Start.Text = "Start";
            this.Start.UseVisualStyleBackColor = false;
            this.Start.Click += new System.EventHandler(this.Start_Click);
            // 
            // fileSystemWatcher1
            // 
            this.fileSystemWatcher1.EnableRaisingEvents = true;
            this.fileSystemWatcher1.SynchronizingObject = this;
            // 
            // Refresh
            // 
            this.Refresh.Location = new System.Drawing.Point(184, 40);
            this.Refresh.Margin = new System.Windows.Forms.Padding(2);
            this.Refresh.Name = "Refresh";
            this.Refresh.Size = new System.Drawing.Size(56, 19);
            this.Refresh.TabIndex = 6;
            this.Refresh.Text = "Refresh";
            this.Refresh.UseVisualStyleBackColor = true;
            this.Refresh.Click += new System.EventHandler(this.Refresh_Click);
            // 
            // btnCapture
            // 
            this.btnCapture.Location = new System.Drawing.Point(10, 196);
            this.btnCapture.Margin = new System.Windows.Forms.Padding(2);
            this.btnCapture.Name = "btnCapture";
            this.btnCapture.Size = new System.Drawing.Size(56, 20);
            this.btnCapture.TabIndex = 7;
            this.btnCapture.Text = "Capture";
            this.btnCapture.UseVisualStyleBackColor = true;
            this.btnCapture.Click += new System.EventHandler(this.btnCapture_Click);
            // 
            // btnRecord
            // 
            this.btnRecord.Location = new System.Drawing.Point(68, 196);
            this.btnRecord.Margin = new System.Windows.Forms.Padding(2);
            this.btnRecord.Name = "btnRecord";
            this.btnRecord.Size = new System.Drawing.Size(56, 20);
            this.btnRecord.TabIndex = 8;
            this.btnRecord.Text = "Record";
            this.btnRecord.UseVisualStyleBackColor = true;
            this.btnRecord.Click += new System.EventHandler(this.btnRecord_Click);
            // 
            // left
            // 
            this.left.Location = new System.Drawing.Point(61, 254);
            this.left.Margin = new System.Windows.Forms.Padding(2);
            this.left.Name = "left";
            this.left.Size = new System.Drawing.Size(56, 47);
            this.left.TabIndex = 10;
            this.left.Text = "LEFT";
            this.left.UseVisualStyleBackColor = true;
            this.left.Click += new System.EventHandler(this.left_Click);
            this.left.MouseDown += new System.Windows.Forms.MouseEventHandler(this.left_MouseDown);
            this.left.MouseUp += new System.Windows.Forms.MouseEventHandler(this.left_MouseUp);
            // 
            // down
            // 
            this.down.Location = new System.Drawing.Point(130, 326);
            this.down.Margin = new System.Windows.Forms.Padding(2);
            this.down.Name = "down";
            this.down.Size = new System.Drawing.Size(56, 47);
            this.down.TabIndex = 11;
            this.down.Text = "DOWN";
            this.down.UseVisualStyleBackColor = true;
            this.down.Click += new System.EventHandler(this.down_Click);
            this.down.MouseDown += new System.Windows.Forms.MouseEventHandler(this.down_MouseDown);
            this.down.MouseUp += new System.Windows.Forms.MouseEventHandler(this.down_MouseUp);
            // 
            // right
            // 
            this.right.Location = new System.Drawing.Point(200, 254);
            this.right.Margin = new System.Windows.Forms.Padding(2);
            this.right.Name = "right";
            this.right.Size = new System.Drawing.Size(56, 47);
            this.right.TabIndex = 12;
            this.right.Text = "RIGHT";
            this.right.UseVisualStyleBackColor = true;
            this.right.Click += new System.EventHandler(this.right_Click);
            this.right.MouseDown += new System.Windows.Forms.MouseEventHandler(this.right_MouseDown);
            this.right.MouseUp += new System.Windows.Forms.MouseEventHandler(this.right_MouseUp);
            // 
            // upp
            // 
            this.upp.Location = new System.Drawing.Point(130, 187);
            this.upp.Margin = new System.Windows.Forms.Padding(2);
            this.upp.Name = "upp";
            this.upp.Size = new System.Drawing.Size(56, 47);
            this.upp.TabIndex = 13;
            this.upp.Text = "UP";
            this.upp.UseVisualStyleBackColor = true;
            this.upp.Click += new System.EventHandler(this.upp_Click);
            this.upp.MouseDown += new System.Windows.Forms.MouseEventHandler(this.upp_MouseDown);
            this.upp.MouseUp += new System.Windows.Forms.MouseEventHandler(this.upp_MouseUp);
            // 
            // cboPorts
            // 
            this.cboPorts.FormattingEnabled = true;
            this.cboPorts.Location = new System.Drawing.Point(16, 46);
            this.cboPorts.Margin = new System.Windows.Forms.Padding(2);
            this.cboPorts.Name = "cboPorts";
            this.cboPorts.Size = new System.Drawing.Size(110, 21);
            this.cboPorts.TabIndex = 14;
            this.cboPorts.SelectedIndexChanged += new System.EventHandler(this.cboPorts_SelectedIndexChanged);
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(192, 46);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(2);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(88, 29);
            this.btnConnect.TabIndex = 17;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(7, 16);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(50, 13);
            this.lblStatus.TabIndex = 16;
            this.lblStatus.Text = "STATUS";
            this.lblStatus.Click += new System.EventHandler(this.lblStatus_Click);
            // 
            // time
            // 
            this.time.Location = new System.Drawing.Point(128, 197);
            this.time.Name = "time";
            this.time.Size = new System.Drawing.Size(170, 20);
            this.time.TabIndex = 18;
            this.time.TextChanged += new System.EventHandler(this.time_TextChanged);
            // 
            // btnChooseFolder
            // 
            this.btnChooseFolder.Location = new System.Drawing.Point(185, 170);
            this.btnChooseFolder.Margin = new System.Windows.Forms.Padding(2);
            this.btnChooseFolder.Name = "btnChooseFolder";
            this.btnChooseFolder.Size = new System.Drawing.Size(115, 20);
            this.btnChooseFolder.TabIndex = 20;
            this.btnChooseFolder.Text = "Browser";
            this.btnChooseFolder.UseVisualStyleBackColor = true;
            this.btnChooseFolder.Click += new System.EventHandler(this.btnChooseFolder_Click);
            // 
            // txtSavePath
            // 
            this.txtSavePath.Location = new System.Drawing.Point(7, 171);
            this.txtSavePath.Name = "txtSavePath";
            this.txtSavePath.Size = new System.Drawing.Size(170, 20);
            this.txtSavePath.TabIndex = 19;
            // 
            // Refresh2
            // 
            this.Refresh2.Location = new System.Drawing.Point(130, 46);
            this.Refresh2.Margin = new System.Windows.Forms.Padding(2);
            this.Refresh2.Name = "Refresh2";
            this.Refresh2.Size = new System.Drawing.Size(56, 29);
            this.Refresh2.TabIndex = 21;
            this.Refresh2.Text = "Refresh";
            this.Refresh2.UseVisualStyleBackColor = true;
            this.Refresh2.Click += new System.EventHandler(this.Refresh2_Click);
            // 
            // Home
            // 
            this.Home.Location = new System.Drawing.Point(222, 135);
            this.Home.Margin = new System.Windows.Forms.Padding(2);
            this.Home.Name = "Home";
            this.Home.Size = new System.Drawing.Size(56, 47);
            this.Home.TabIndex = 22;
            this.Home.Text = "Set Home";
            this.Home.UseVisualStyleBackColor = true;
            this.Home.Click += new System.EventHandler(this.Home_Click);
            // 
            // lblPanPos
            // 
            this.lblPanPos.AutoSize = true;
            this.lblPanPos.Location = new System.Drawing.Point(16, 90);
            this.lblPanPos.Name = "lblPanPos";
            this.lblPanPos.Size = new System.Drawing.Size(26, 13);
            this.lblPanPos.TabIndex = 23;
            this.lblPanPos.Text = "Pan";
            this.lblPanPos.Click += new System.EventHandler(this.lblPanPos_Click);
            // 
            // lblTiltPos
            // 
            this.lblTiltPos.AutoSize = true;
            this.lblTiltPos.Location = new System.Drawing.Point(16, 117);
            this.lblTiltPos.Name = "lblTiltPos";
            this.lblTiltPos.Size = new System.Drawing.Size(21, 13);
            this.lblTiltPos.TabIndex = 24;
            this.lblTiltPos.Text = "Tilt";
            this.lblTiltPos.Click += new System.EventHandler(this.lblTiltPos_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(126, 90);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 25;
            this.label1.Text = "Pan Offset:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(126, 114);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 26;
            this.label2.Text = "Tilt Offset:";
            // 
            // numPanOffset
            // 
            this.numPanOffset.Location = new System.Drawing.Point(192, 88);
            this.numPanOffset.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numPanOffset.Minimum = new decimal(new int[] {
            100000,
            0,
            0,
            -2147483648});
            this.numPanOffset.Name = "numPanOffset";
            this.numPanOffset.Size = new System.Drawing.Size(86, 20);
            this.numPanOffset.TabIndex = 27;
            this.numPanOffset.ValueChanged += new System.EventHandler(this.numPanOffset_ValueChanged);
            // 
            // numTiltOffset
            // 
            this.numTiltOffset.Location = new System.Drawing.Point(192, 112);
            this.numTiltOffset.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numTiltOffset.Minimum = new decimal(new int[] {
            100000,
            0,
            0,
            -2147483648});
            this.numTiltOffset.Name = "numTiltOffset";
            this.numTiltOffset.Size = new System.Drawing.Size(86, 20);
            this.numTiltOffset.TabIndex = 28;
            this.numTiltOffset.ValueChanged += new System.EventHandler(this.numTiltOffset_ValueChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.tbZoom);
            this.groupBox1.Controls.Add(this.txtIpAddress);
            this.groupBox1.Controls.Add(this.rbIp);
            this.groupBox1.Controls.Add(this.rbUsb);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.Camera);
            this.groupBox1.Controls.Add(this.cboAudioDevices);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.cboDevices);
            this.groupBox1.Controls.Add(this.Refresh);
            this.groupBox1.Controls.Add(this.Start);
            this.groupBox1.Controls.Add(this.Stop);
            this.groupBox1.Controls.Add(this.txtSavePath);
            this.groupBox1.Controls.Add(this.btnChooseFolder);
            this.groupBox1.Controls.Add(this.btnCapture);
            this.groupBox1.Controls.Add(this.time);
            this.groupBox1.Controls.Add(this.btnRecord);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Right;
            this.groupBox1.Location = new System.Drawing.Point(946, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(316, 610);
            this.groupBox1.TabIndex = 29;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "CAMERA AND RECORDING";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 139);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(34, 13);
            this.label4.TabIndex = 37;
            this.label4.Text = "Zoom";
            // 
            // tbZoom
            // 
            this.tbZoom.Location = new System.Drawing.Point(48, 125);
            this.tbZoom.Maximum = 100;
            this.tbZoom.Name = "tbZoom";
            this.tbZoom.Size = new System.Drawing.Size(249, 45);
            this.tbZoom.TabIndex = 36;
            this.tbZoom.Scroll += new System.EventHandler(this.tbZoom_Scroll);
            // 
            // txtIpAddress
            // 
            this.txtIpAddress.Location = new System.Drawing.Point(111, 99);
            this.txtIpAddress.Name = "txtIpAddress";
            this.txtIpAddress.Size = new System.Drawing.Size(186, 20);
            this.txtIpAddress.TabIndex = 35;
            this.txtIpAddress.TextChanged += new System.EventHandler(this.txtIpAddress_TextChanged);
            // 
            // rbIp
            // 
            this.rbIp.AutoSize = true;
            this.rbIp.Location = new System.Drawing.Point(9, 102);
            this.rbIp.Name = "rbIp";
            this.rbIp.Size = new System.Drawing.Size(96, 17);
            this.rbIp.TabIndex = 34;
            this.rbIp.TabStop = true;
            this.rbIp.Text = "Use IP Camera";
            this.rbIp.UseVisualStyleBackColor = true;
            this.rbIp.CheckedChanged += new System.EventHandler(this.rbIp_CheckedChanged);
            // 
            // rbUsb
            // 
            this.rbUsb.AutoSize = true;
            this.rbUsb.Location = new System.Drawing.Point(9, 75);
            this.rbUsb.Name = "rbUsb";
            this.rbUsb.Size = new System.Drawing.Size(91, 17);
            this.rbUsb.TabIndex = 33;
            this.rbUsb.TabStop = true;
            this.rbUsb.Text = "Use Usb/Iriun";
            this.rbUsb.UseVisualStyleBackColor = true;
            this.rbUsb.CheckedChanged += new System.EventHandler(this.rbUsb_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 46);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 13);
            this.label3.TabIndex = 32;
            this.label3.Text = "Micro";
            // 
            // Camera
            // 
            this.Camera.AutoSize = true;
            this.Camera.Location = new System.Drawing.Point(9, 20);
            this.Camera.Name = "Camera";
            this.Camera.Size = new System.Drawing.Size(43, 13);
            this.Camera.TabIndex = 31;
            this.Camera.Text = "Camera";
            // 
            // cboAudioDevices
            // 
            this.cboAudioDevices.FormattingEnabled = true;
            this.cboAudioDevices.Location = new System.Drawing.Point(66, 40);
            this.cboAudioDevices.Margin = new System.Windows.Forms.Padding(2);
            this.cboAudioDevices.Name = "cboAudioDevices";
            this.cboAudioDevices.Size = new System.Drawing.Size(110, 21);
            this.cboAudioDevices.TabIndex = 21;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnGoHome);
            this.groupBox2.Controls.Add(this.Refresh2);
            this.groupBox2.Controls.Add(this.left);
            this.groupBox2.Controls.Add(this.numTiltOffset);
            this.groupBox2.Controls.Add(this.down);
            this.groupBox2.Controls.Add(this.numPanOffset);
            this.groupBox2.Controls.Add(this.right);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.upp);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.cboPorts);
            this.groupBox2.Controls.Add(this.lblTiltPos);
            this.groupBox2.Controls.Add(this.lblStatus);
            this.groupBox2.Controls.Add(this.lblPanPos);
            this.groupBox2.Controls.Add(this.btnConnect);
            this.groupBox2.Controls.Add(this.Home);
            this.groupBox2.Location = new System.Drawing.Point(7, 225);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(304, 385);
            this.groupBox2.TabIndex = 30;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "CONTROLLER STEP MOTOR";
            // 
            // btnGoHome
            // 
            this.btnGoHome.Location = new System.Drawing.Point(130, 254);
            this.btnGoHome.Margin = new System.Windows.Forms.Padding(2);
            this.btnGoHome.Name = "btnGoHome";
            this.btnGoHome.Size = new System.Drawing.Size(56, 47);
            this.btnGoHome.TabIndex = 29;
            this.btnGoHome.Text = "GO HOME";
            this.btnGoHome.UseVisualStyleBackColor = true;
            this.btnGoHome.Click += new System.EventHandler(this.btnGoHome_Click_1);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(3, 16);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(940, 591);
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.pictureBox1);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(0, 0);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(946, 610);
            this.groupBox3.TabIndex = 31;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = " ";
            // 
            // up
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1262, 610);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "up";
            this.Text = "Z";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load_1);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.up_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.up_KeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPanOffset)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTiltOffset)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbZoom)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cboDevices;
        private System.Windows.Forms.Button Stop;
        private System.Windows.Forms.Button Start;
        private System.IO.FileSystemWatcher fileSystemWatcher1;
        private System.Windows.Forms.Button Refresh;
        private System.Windows.Forms.Button btnRecord;
        private System.Windows.Forms.Button btnCapture;
        private System.Windows.Forms.Button right;
        private System.Windows.Forms.Button down;
        private System.Windows.Forms.Button left;
        private System.Windows.Forms.Button upp;
        private System.Windows.Forms.ComboBox cboPorts;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.TextBox time;
        private System.Windows.Forms.Button btnChooseFolder;
        private System.Windows.Forms.TextBox txtSavePath;
        private System.Windows.Forms.Button Refresh2;
        private System.Windows.Forms.Button Home;
        private System.Windows.Forms.Label lblTiltPos;
        private System.Windows.Forms.Label lblPanPos;
        private System.Windows.Forms.NumericUpDown numTiltOffset;
        private System.Windows.Forms.NumericUpDown numPanOffset;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ComboBox cboAudioDevices;
        private System.Windows.Forms.Button btnGoHome;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox txtIpAddress;
        private System.Windows.Forms.RadioButton rbIp;
        private System.Windows.Forms.RadioButton rbUsb;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label Camera;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TrackBar tbZoom;
    }
}

