namespace TestApplication
{
    partial class TrainerForm
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
            this.ProcTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.OpenProcessBtn = new System.Windows.Forms.Button();
            this.ProcTypeBox = new System.Windows.Forms.ComboBox();
            this.WriteTypeBox = new System.Windows.Forms.ComboBox();
            this.WriteButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.WriteTextBox = new System.Windows.Forms.TextBox();
            this.ModuleList = new System.Windows.Forms.ListView();
            this.AddrHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.NameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.UpdateModulesButton = new System.Windows.Forms.Button();
            this.ReadTypeBox = new System.Windows.Forms.ComboBox();
            this.ReadButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.ReadTextBox = new System.Windows.Forms.TextBox();
            this.WriteValueBox = new System.Windows.Forms.TextBox();
            this.ReadValueBox = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.EndAddrBox = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.StartAddrBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.AobScanList = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label6 = new System.Windows.Forms.Label();
            this.AobScanTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.ScanButton = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.ProcStatus = new System.Windows.Forms.Label();
            this.DLLOpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.SendPipeButton = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.PipeMsgTextBox = new System.Windows.Forms.TextBox();
            this.InjectButton = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.PipeNameTextBox = new System.Windows.Forms.TextBox();
            this.DLLTextBox = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.BrowseDLLButton = new System.Windows.Forms.Button();
            this.BackgroundWork = new System.ComponentModel.BackgroundWorker();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // ProcTextBox
            // 
            this.ProcTextBox.Location = new System.Drawing.Point(68, 18);
            this.ProcTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.ProcTextBox.Name = "ProcTextBox";
            this.ProcTextBox.Size = new System.Drawing.Size(120, 20);
            this.ProcTextBox.TabIndex = 0;
            this.ProcTextBox.Text = "Xae\'s Quest";
            this.ProcTextBox.TextChanged += new System.EventHandler(this.ProcTextBox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 20);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Process:";
            // 
            // OpenProcessBtn
            // 
            this.OpenProcessBtn.Location = new System.Drawing.Point(286, 15);
            this.OpenProcessBtn.Margin = new System.Windows.Forms.Padding(2);
            this.OpenProcessBtn.Name = "OpenProcessBtn";
            this.OpenProcessBtn.Size = new System.Drawing.Size(85, 26);
            this.OpenProcessBtn.TabIndex = 2;
            this.OpenProcessBtn.Text = "Open Process";
            this.OpenProcessBtn.UseVisualStyleBackColor = true;
            this.OpenProcessBtn.Click += new System.EventHandler(this.OpenProcessBtn_Click);
            // 
            // ProcTypeBox
            // 
            this.ProcTypeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ProcTypeBox.FormattingEnabled = true;
            this.ProcTypeBox.Items.AddRange(new object[] {
            "Name",
            "ID"});
            this.ProcTypeBox.Location = new System.Drawing.Point(191, 18);
            this.ProcTypeBox.Margin = new System.Windows.Forms.Padding(2);
            this.ProcTypeBox.Name = "ProcTypeBox";
            this.ProcTypeBox.Size = new System.Drawing.Size(92, 21);
            this.ProcTypeBox.TabIndex = 3;
            // 
            // WriteTypeBox
            // 
            this.WriteTypeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.WriteTypeBox.FormattingEnabled = true;
            this.WriteTypeBox.Items.AddRange(new object[] {
            "byte",
            "bytes",
            "int",
            "string",
            "float"});
            this.WriteTypeBox.Location = new System.Drawing.Point(209, 24);
            this.WriteTypeBox.Margin = new System.Windows.Forms.Padding(2);
            this.WriteTypeBox.Name = "WriteTypeBox";
            this.WriteTypeBox.Size = new System.Drawing.Size(68, 21);
            this.WriteTypeBox.TabIndex = 7;
            // 
            // WriteButton
            // 
            this.WriteButton.Location = new System.Drawing.Point(280, 24);
            this.WriteButton.Margin = new System.Windows.Forms.Padding(2);
            this.WriteButton.Name = "WriteButton";
            this.WriteButton.Size = new System.Drawing.Size(44, 20);
            this.WriteButton.TabIndex = 6;
            this.WriteButton.Text = "Write";
            this.WriteButton.UseVisualStyleBackColor = true;
            this.WriteButton.Click += new System.EventHandler(this.WriteButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(2, 26);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Address:";
            // 
            // WriteTextBox
            // 
            this.WriteTextBox.Location = new System.Drawing.Point(50, 24);
            this.WriteTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.WriteTextBox.Name = "WriteTextBox";
            this.WriteTextBox.Size = new System.Drawing.Size(156, 20);
            this.WriteTextBox.TabIndex = 4;
            this.WriteTextBox.Text = "00000000";
            // 
            // ModuleList
            // 
            this.ModuleList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ModuleList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.AddrHeader,
            this.NameHeader});
            this.ModuleList.HideSelection = false;
            this.ModuleList.Location = new System.Drawing.Point(4, 17);
            this.ModuleList.Margin = new System.Windows.Forms.Padding(2);
            this.ModuleList.Name = "ModuleList";
            this.ModuleList.Size = new System.Drawing.Size(266, 485);
            this.ModuleList.TabIndex = 8;
            this.ModuleList.UseCompatibleStateImageBehavior = false;
            this.ModuleList.View = System.Windows.Forms.View.Details;
            // 
            // AddrHeader
            // 
            this.AddrHeader.Text = "Address";
            this.AddrHeader.Width = 101;
            // 
            // NameHeader
            // 
            this.NameHeader.Text = "Name";
            this.NameHeader.Width = 157;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.UpdateModulesButton);
            this.groupBox1.Controls.Add(this.ModuleList);
            this.groupBox1.Location = new System.Drawing.Point(14, 48);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(274, 532);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Process Module List";
            // 
            // UpdateModulesButton
            // 
            this.UpdateModulesButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.UpdateModulesButton.Location = new System.Drawing.Point(2, 507);
            this.UpdateModulesButton.Name = "UpdateModulesButton";
            this.UpdateModulesButton.Size = new System.Drawing.Size(270, 23);
            this.UpdateModulesButton.TabIndex = 9;
            this.UpdateModulesButton.Text = "Get New Modules List";
            this.UpdateModulesButton.UseVisualStyleBackColor = true;
            this.UpdateModulesButton.Click += new System.EventHandler(this.UpdateModulesButton_Click);
            // 
            // ReadTypeBox
            // 
            this.ReadTypeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ReadTypeBox.FormattingEnabled = true;
            this.ReadTypeBox.Items.AddRange(new object[] {
            "string",
            "double",
            "int",
            "long",
            "float",
            "UInt",
            "byte",
            "2 byte"});
            this.ReadTypeBox.Location = new System.Drawing.Point(209, 17);
            this.ReadTypeBox.Margin = new System.Windows.Forms.Padding(2);
            this.ReadTypeBox.Name = "ReadTypeBox";
            this.ReadTypeBox.Size = new System.Drawing.Size(68, 21);
            this.ReadTypeBox.TabIndex = 13;
            // 
            // ReadButton
            // 
            this.ReadButton.Location = new System.Drawing.Point(280, 17);
            this.ReadButton.Margin = new System.Windows.Forms.Padding(2);
            this.ReadButton.Name = "ReadButton";
            this.ReadButton.Size = new System.Drawing.Size(44, 20);
            this.ReadButton.TabIndex = 12;
            this.ReadButton.Text = "Read";
            this.ReadButton.UseVisualStyleBackColor = true;
            this.ReadButton.Click += new System.EventHandler(this.ReadButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 20);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(48, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Address:";
            // 
            // ReadTextBox
            // 
            this.ReadTextBox.Location = new System.Drawing.Point(50, 18);
            this.ReadTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.ReadTextBox.Name = "ReadTextBox";
            this.ReadTextBox.Size = new System.Drawing.Size(156, 20);
            this.ReadTextBox.TabIndex = 10;
            this.ReadTextBox.Text = "00000000";
            // 
            // WriteValueBox
            // 
            this.WriteValueBox.Location = new System.Drawing.Point(50, 48);
            this.WriteValueBox.Margin = new System.Windows.Forms.Padding(2);
            this.WriteValueBox.Name = "WriteValueBox";
            this.WriteValueBox.Size = new System.Drawing.Size(275, 20);
            this.WriteValueBox.TabIndex = 14;
            // 
            // ReadValueBox
            // 
            this.ReadValueBox.Location = new System.Drawing.Point(50, 41);
            this.ReadValueBox.Margin = new System.Windows.Forms.Padding(2);
            this.ReadValueBox.Name = "ReadValueBox";
            this.ReadValueBox.Size = new System.Drawing.Size(275, 20);
            this.ReadValueBox.TabIndex = 15;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.WriteValueBox);
            this.groupBox2.Controls.Add(this.WriteTextBox);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.WriteButton);
            this.groupBox2.Controls.Add(this.WriteTypeBox);
            this.groupBox2.Location = new System.Drawing.Point(293, 48);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(328, 73);
            this.groupBox2.TabIndex = 16;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Write To Address";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 50);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(37, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Value:";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.ReadValueBox);
            this.groupBox3.Controls.Add(this.ReadTextBox);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.ReadTypeBox);
            this.groupBox3.Controls.Add(this.ReadButton);
            this.groupBox3.Location = new System.Drawing.Point(293, 125);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox3.Size = new System.Drawing.Size(328, 67);
            this.groupBox3.TabIndex = 17;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Read From Address";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 43);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(37, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "Value:";
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.EndAddrBox);
            this.groupBox4.Controls.Add(this.label10);
            this.groupBox4.Controls.Add(this.StartAddrBox);
            this.groupBox4.Controls.Add(this.label9);
            this.groupBox4.Controls.Add(this.AobScanList);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.AobScanTextBox);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Controls.Add(this.ScanButton);
            this.groupBox4.Location = new System.Drawing.Point(293, 306);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox4.Size = new System.Drawing.Size(328, 274);
            this.groupBox4.TabIndex = 17;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Array of Bytes (AoB) / Pattern Scan";
            // 
            // EndAddrBox
            // 
            this.EndAddrBox.Location = new System.Drawing.Point(209, 46);
            this.EndAddrBox.Margin = new System.Windows.Forms.Padding(2);
            this.EndAddrBox.Name = "EndAddrBox";
            this.EndAddrBox.Size = new System.Drawing.Size(116, 20);
            this.EndAddrBox.TabIndex = 19;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(181, 48);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(29, 13);
            this.label10.TabIndex = 18;
            this.label10.Text = "End:";
            // 
            // StartAddrBox
            // 
            this.StartAddrBox.Location = new System.Drawing.Point(50, 46);
            this.StartAddrBox.Margin = new System.Windows.Forms.Padding(2);
            this.StartAddrBox.Name = "StartAddrBox";
            this.StartAddrBox.Size = new System.Drawing.Size(123, 20);
            this.StartAddrBox.TabIndex = 17;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(15, 47);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(32, 13);
            this.label9.TabIndex = 16;
            this.label9.Text = "Start:";
            // 
            // AobScanList
            // 
            this.AobScanList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.AobScanList.HideSelection = false;
            this.AobScanList.Location = new System.Drawing.Point(50, 68);
            this.AobScanList.Margin = new System.Windows.Forms.Padding(2);
            this.AobScanList.Name = "AobScanList";
            this.AobScanList.Size = new System.Drawing.Size(275, 201);
            this.AobScanList.TabIndex = 9;
            this.AobScanList.UseCompatibleStateImageBehavior = false;
            this.AobScanList.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Address";
            this.columnHeader1.Width = 353;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(10, 72);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(40, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "Found:";
            // 
            // AobScanTextBox
            // 
            this.AobScanTextBox.Location = new System.Drawing.Point(50, 24);
            this.AobScanTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.AobScanTextBox.Name = "AobScanTextBox";
            this.AobScanTextBox.Size = new System.Drawing.Size(227, 20);
            this.AobScanTextBox.TabIndex = 4;
            this.AobScanTextBox.Text = "00 00 00 00";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 26);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(44, 13);
            this.label7.TabIndex = 5;
            this.label7.Text = "Pattern:";
            // 
            // ScanButton
            // 
            this.ScanButton.Location = new System.Drawing.Point(280, 24);
            this.ScanButton.Margin = new System.Windows.Forms.Padding(2);
            this.ScanButton.Name = "ScanButton";
            this.ScanButton.Size = new System.Drawing.Size(44, 20);
            this.ScanButton.TabIndex = 6;
            this.ScanButton.Text = "Scan";
            this.ScanButton.UseVisualStyleBackColor = true;
            this.ScanButton.Click += new System.EventHandler(this.ScanButton_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(385, 21);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(40, 13);
            this.label8.TabIndex = 18;
            this.label8.Text = "Status:";
            // 
            // ProcStatus
            // 
            this.ProcStatus.AutoSize = true;
            this.ProcStatus.BackColor = System.Drawing.Color.Transparent;
            this.ProcStatus.ForeColor = System.Drawing.Color.Red;
            this.ProcStatus.Location = new System.Drawing.Point(421, 21);
            this.ProcStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ProcStatus.Name = "ProcStatus";
            this.ProcStatus.Size = new System.Drawing.Size(39, 13);
            this.ProcStatus.TabIndex = 19;
            this.ProcStatus.Text = "Closed";
            // 
            // DLLOpenFileDialog
            // 
            this.DLLOpenFileDialog.FileName = "*.dll";
            this.DLLOpenFileDialog.Filter = "dll files (*.dll)|*.dll";
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox5.Controls.Add(this.SendPipeButton);
            this.groupBox5.Controls.Add(this.label13);
            this.groupBox5.Controls.Add(this.PipeMsgTextBox);
            this.groupBox5.Controls.Add(this.InjectButton);
            this.groupBox5.Controls.Add(this.label11);
            this.groupBox5.Controls.Add(this.PipeNameTextBox);
            this.groupBox5.Controls.Add(this.DLLTextBox);
            this.groupBox5.Controls.Add(this.label12);
            this.groupBox5.Controls.Add(this.BrowseDLLButton);
            this.groupBox5.Location = new System.Drawing.Point(293, 196);
            this.groupBox5.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox5.Size = new System.Drawing.Size(328, 67);
            this.groupBox5.TabIndex = 18;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Inject DLL and NamedPipe";
            // 
            // SendPipeButton
            // 
            this.SendPipeButton.Location = new System.Drawing.Point(275, 42);
            this.SendPipeButton.Margin = new System.Windows.Forms.Padding(2);
            this.SendPipeButton.Name = "SendPipeButton";
            this.SendPipeButton.Size = new System.Drawing.Size(48, 20);
            this.SendPipeButton.TabIndex = 20;
            this.SendPipeButton.Text = "Send";
            this.SendPipeButton.UseVisualStyleBackColor = true;
            this.SendPipeButton.Click += new System.EventHandler(this.SendPipeButton_Click);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(135, 45);
            this.label13.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(30, 13);
            this.label13.TabIndex = 19;
            this.label13.Text = "Msg:";
            // 
            // PipeMsgTextBox
            // 
            this.PipeMsgTextBox.Location = new System.Drawing.Point(166, 42);
            this.PipeMsgTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.PipeMsgTextBox.Name = "PipeMsgTextBox";
            this.PipeMsgTextBox.Size = new System.Drawing.Size(104, 20);
            this.PipeMsgTextBox.TabIndex = 18;
            this.PipeMsgTextBox.Text = "gameassembly";
            // 
            // InjectButton
            // 
            this.InjectButton.Location = new System.Drawing.Point(275, 18);
            this.InjectButton.Margin = new System.Windows.Forms.Padding(2);
            this.InjectButton.Name = "InjectButton";
            this.InjectButton.Size = new System.Drawing.Size(49, 20);
            this.InjectButton.TabIndex = 17;
            this.InjectButton.Text = "Inject";
            this.InjectButton.UseVisualStyleBackColor = true;
            this.InjectButton.Click += new System.EventHandler(this.InjectButton_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(15, 45);
            this.label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(31, 13);
            this.label11.TabIndex = 16;
            this.label11.Text = "Pipe:";
            // 
            // PipeNameTextBox
            // 
            this.PipeNameTextBox.Location = new System.Drawing.Point(50, 42);
            this.PipeNameTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.PipeNameTextBox.Name = "PipeNameTextBox";
            this.PipeNameTextBox.Size = new System.Drawing.Size(83, 20);
            this.PipeNameTextBox.TabIndex = 15;
            this.PipeNameTextBox.Text = "EQTPipe";
            // 
            // DLLTextBox
            // 
            this.DLLTextBox.Location = new System.Drawing.Point(50, 18);
            this.DLLTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.DLLTextBox.Name = "DLLTextBox";
            this.DLLTextBox.Size = new System.Drawing.Size(160, 20);
            this.DLLTextBox.TabIndex = 10;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(16, 21);
            this.label12.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(30, 13);
            this.label12.TabIndex = 11;
            this.label12.Text = "DLL:";
            // 
            // BrowseDLLButton
            // 
            this.BrowseDLLButton.Location = new System.Drawing.Point(214, 18);
            this.BrowseDLLButton.Margin = new System.Windows.Forms.Padding(2);
            this.BrowseDLLButton.Name = "BrowseDLLButton";
            this.BrowseDLLButton.Size = new System.Drawing.Size(57, 20);
            this.BrowseDLLButton.TabIndex = 12;
            this.BrowseDLLButton.Text = "Browse";
            this.BrowseDLLButton.UseVisualStyleBackColor = true;
            this.BrowseDLLButton.Click += new System.EventHandler(this.BrowseDLLButton_Click);
            // 
            // BackgroundWork
            // 
            this.BackgroundWork.WorkerReportsProgress = true;
            this.BackgroundWork.WorkerSupportsCancellation = true;
            this.BackgroundWork.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWork_DoWork);
            this.BackgroundWork.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.BackgroundWork_ProgressChanged);
            this.BackgroundWork.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWork_RunWorkerCompleted);
            // 
            // TrainerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(632, 596);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.ProcStatus);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.ProcTypeBox);
            this.Controls.Add(this.OpenProcessBtn);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ProcTextBox);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(586, 535);
            this.Name = "TrainerForm";
            this.Text = "Memory.dll Test Trainer";
            this.Shown += new System.EventHandler(this.TrainerForm_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox ProcTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button OpenProcessBtn;
        private System.Windows.Forms.ComboBox ProcTypeBox;
        private System.Windows.Forms.ComboBox WriteTypeBox;
        private System.Windows.Forms.Button WriteButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox WriteTextBox;
        private System.Windows.Forms.ListView ModuleList;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ColumnHeader AddrHeader;
        private System.Windows.Forms.ColumnHeader NameHeader;
        private System.Windows.Forms.ComboBox ReadTypeBox;
        private System.Windows.Forms.Button ReadButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox ReadTextBox;
        private System.Windows.Forms.TextBox WriteValueBox;
        private System.Windows.Forms.TextBox ReadValueBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.ListView AobScanList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox AobScanTextBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button ScanButton;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label ProcStatus;
        private System.Windows.Forms.TextBox EndAddrBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox StartAddrBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.OpenFileDialog DLLOpenFileDialog;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox PipeNameTextBox;
        private System.Windows.Forms.TextBox DLLTextBox;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button BrowseDLLButton;
        private System.Windows.Forms.Button SendPipeButton;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox PipeMsgTextBox;
        private System.Windows.Forms.Button InjectButton;
        private System.ComponentModel.BackgroundWorker BackgroundWork;
        private System.Windows.Forms.Button UpdateModulesButton;
    }
}

