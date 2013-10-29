namespace TableLogCleaner
{
    partial class MainForm
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.btSelectDir = new System.Windows.Forms.Button();
            this.tbFile = new System.Windows.Forms.TextBox();
            this.id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Coords = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btn = new System.Windows.Forms.DataGridViewButtonColumn();
            this.btnRenew = new System.Windows.Forms.Button();
            this.btnSelectTestDir = new System.Windows.Forms.Button();
            this.tbDirTestFiles = new System.Windows.Forms.TextBox();
            this.btnNextFile = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.lblCurrentFile = new System.Windows.Forms.Label();
            this.btnProcessAll = new System.Windows.Forms.Button();
            this.btnGetDefault = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(506, 76);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(608, 477);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // dataGridView1
            // 
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.id,
            this.Coords,
            this.btn});
            this.dataGridView1.Location = new System.Drawing.Point(12, 76);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(488, 477);
            this.dataGridView1.TabIndex = 1;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            this.dataGridView1.RowsRemoved += new System.Windows.Forms.DataGridViewRowsRemovedEventHandler(this.dataGridView1_RowsRemoved);
            this.dataGridView1.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.dataGridView1_UserDeletingRow);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // btSelectDir
            // 
            this.btSelectDir.Location = new System.Drawing.Point(544, 2);
            this.btSelectDir.Name = "btSelectDir";
            this.btSelectDir.Size = new System.Drawing.Size(33, 23);
            this.btSelectDir.TabIndex = 44;
            this.btSelectDir.Text = "...";
            this.btSelectDir.UseVisualStyleBackColor = true;
            this.btSelectDir.Click += new System.EventHandler(this.btSelectDir_Click);
            // 
            // tbFile
            // 
            this.tbFile.Location = new System.Drawing.Point(12, 5);
            this.tbFile.Name = "tbFile";
            this.tbFile.ReadOnly = true;
            this.tbFile.Size = new System.Drawing.Size(516, 20);
            this.tbFile.TabIndex = 43;
            // 
            // id
            // 
            this.id.HeaderText = "id";
            this.id.Name = "id";
            this.id.ReadOnly = true;
            // 
            // Coords
            // 
            this.Coords.HeaderText = "Coords";
            this.Coords.Name = "Coords";
            this.Coords.ReadOnly = true;
            // 
            // btn
            // 
            this.btn.HeaderText = "";
            this.btn.Name = "btn";
            this.btn.Text = "";
            // 
            // btnRenew
            // 
            this.btnRenew.Location = new System.Drawing.Point(731, 36);
            this.btnRenew.Name = "btnRenew";
            this.btnRenew.Size = new System.Drawing.Size(75, 23);
            this.btnRenew.TabIndex = 48;
            this.btnRenew.Text = "renew";
            this.btnRenew.UseVisualStyleBackColor = true;
            this.btnRenew.Click += new System.EventHandler(this.btnRenew_Click);
            // 
            // btnSelectTestDir
            // 
            this.btnSelectTestDir.Location = new System.Drawing.Point(611, 36);
            this.btnSelectTestDir.Name = "btnSelectTestDir";
            this.btnSelectTestDir.Size = new System.Drawing.Size(33, 23);
            this.btnSelectTestDir.TabIndex = 47;
            this.btnSelectTestDir.Text = "...";
            this.btnSelectTestDir.UseVisualStyleBackColor = true;
            this.btnSelectTestDir.Click += new System.EventHandler(this.btnSelectTestDir_Click);
            // 
            // tbDirTestFiles
            // 
            this.tbDirTestFiles.Location = new System.Drawing.Point(13, 38);
            this.tbDirTestFiles.Name = "tbDirTestFiles";
            this.tbDirTestFiles.Size = new System.Drawing.Size(515, 20);
            this.tbDirTestFiles.TabIndex = 46;
            // 
            // btnNextFile
            // 
            this.btnNextFile.Location = new System.Drawing.Point(650, 36);
            this.btnNextFile.Name = "btnNextFile";
            this.btnNextFile.Size = new System.Drawing.Size(75, 23);
            this.btnNextFile.TabIndex = 45;
            this.btnNextFile.Text = "next file";
            this.btnNextFile.UseVisualStyleBackColor = true;
            this.btnNextFile.Click += new System.EventHandler(this.btnNextFile_Click);
            // 
            // lblCurrentFile
            // 
            this.lblCurrentFile.AutoSize = true;
            this.lblCurrentFile.Location = new System.Drawing.Point(546, 41);
            this.lblCurrentFile.Name = "lblCurrentFile";
            this.lblCurrentFile.Size = new System.Drawing.Size(35, 13);
            this.lblCurrentFile.TabIndex = 49;
            this.lblCurrentFile.Text = "label5";
            // 
            // btnProcessAll
            // 
            this.btnProcessAll.Location = new System.Drawing.Point(878, 36);
            this.btnProcessAll.Name = "btnProcessAll";
            this.btnProcessAll.Size = new System.Drawing.Size(75, 23);
            this.btnProcessAll.TabIndex = 50;
            this.btnProcessAll.Text = "process all";
            this.btnProcessAll.UseVisualStyleBackColor = true;
            this.btnProcessAll.Click += new System.EventHandler(this.btnProcessAll_Click);
            // 
            // btnGetDefault
            // 
            this.btnGetDefault.Location = new System.Drawing.Point(611, 7);
            this.btnGetDefault.Name = "btnGetDefault";
            this.btnGetDefault.Size = new System.Drawing.Size(195, 23);
            this.btnGetDefault.TabIndex = 51;
            this.btnGetDefault.Text = "get default settings";
            this.btnGetDefault.UseVisualStyleBackColor = true;
            this.btnGetDefault.Click += new System.EventHandler(this.btnGetDefault_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1126, 559);
            this.Controls.Add(this.btnGetDefault);
            this.Controls.Add(this.btnProcessAll);
            this.Controls.Add(this.lblCurrentFile);
            this.Controls.Add(this.btnRenew);
            this.Controls.Add(this.btnSelectTestDir);
            this.Controls.Add(this.tbDirTestFiles);
            this.Controls.Add(this.btnNextFile);
            this.Controls.Add(this.btSelectDir);
            this.Controls.Add(this.tbFile);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.pictureBox1);
            this.Name = "MainForm";
            this.Text = "MainForm";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button btSelectDir;
        private System.Windows.Forms.TextBox tbFile;
        private System.Windows.Forms.DataGridViewTextBoxColumn id;
        private System.Windows.Forms.DataGridViewTextBoxColumn Coords;
        private System.Windows.Forms.DataGridViewButtonColumn btn;
        private System.Windows.Forms.Button btnRenew;
        private System.Windows.Forms.Button btnSelectTestDir;
        private System.Windows.Forms.TextBox tbDirTestFiles;
        private System.Windows.Forms.Button btnNextFile;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Label lblCurrentFile;
        private System.Windows.Forms.Button btnProcessAll;
        private System.Windows.Forms.Button btnGetDefault;
    }
}

