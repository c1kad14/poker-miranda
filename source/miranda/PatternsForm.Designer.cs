namespace miranda
{
    partial class PatternsForm
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
            this.btnSelectPicture = new System.Windows.Forms.Button();
            this.pbSelectedPicture = new System.Windows.Forms.PictureBox();
            this.btnProcess = new System.Windows.Forms.Button();
            this.pbProcessedPicture = new System.Windows.Forms.PictureBox();
            this.picZoom = new System.Windows.Forms.PictureBox();
            this.tbZoomScale = new System.Windows.Forms.TrackBar();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblLastFileName = new System.Windows.Forms.Label();
            this.btnSaveFile = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.pbSelectedPicture)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbProcessedPicture)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picZoom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbZoomScale)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSelectPicture
            // 
            this.btnSelectPicture.Location = new System.Drawing.Point(168, 12);
            this.btnSelectPicture.Name = "btnSelectPicture";
            this.btnSelectPicture.Size = new System.Drawing.Size(116, 23);
            this.btnSelectPicture.TabIndex = 0;
            this.btnSelectPicture.Text = "Select picture";
            this.btnSelectPicture.UseVisualStyleBackColor = true;
            this.btnSelectPicture.Click += new System.EventHandler(this.btnSelectPicture_Click);
            // 
            // pbSelectedPicture
            // 
            this.pbSelectedPicture.Location = new System.Drawing.Point(290, 12);
            this.pbSelectedPicture.Name = "pbSelectedPicture";
            this.pbSelectedPicture.Size = new System.Drawing.Size(100, 50);
            this.pbSelectedPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pbSelectedPicture.TabIndex = 1;
            this.pbSelectedPicture.TabStop = false;
            this.pbSelectedPicture.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pbSelectedPicture_MouseMove);
            // 
            // btnProcess
            // 
            this.btnProcess.Location = new System.Drawing.Point(168, 77);
            this.btnProcess.Name = "btnProcess";
            this.btnProcess.Size = new System.Drawing.Size(116, 23);
            this.btnProcess.TabIndex = 2;
            this.btnProcess.Text = "Process";
            this.btnProcess.UseVisualStyleBackColor = true;
            this.btnProcess.Click += new System.EventHandler(this.button1_Click);
            // 
            // pbProcessedPicture
            // 
            this.pbProcessedPicture.Location = new System.Drawing.Point(290, 77);
            this.pbProcessedPicture.Name = "pbProcessedPicture";
            this.pbProcessedPicture.Size = new System.Drawing.Size(100, 50);
            this.pbProcessedPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pbProcessedPicture.TabIndex = 3;
            this.pbProcessedPicture.TabStop = false;
            this.pbProcessedPicture.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pbProcessedPicture_MouseMove);
            // 
            // picZoom
            // 
            this.picZoom.Location = new System.Drawing.Point(12, 12);
            this.picZoom.Name = "picZoom";
            this.picZoom.Size = new System.Drawing.Size(150, 133);
            this.picZoom.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picZoom.TabIndex = 4;
            this.picZoom.TabStop = false;
            // 
            // tbZoomScale
            // 
            this.tbZoomScale.LargeChange = 1;
            this.tbZoomScale.Location = new System.Drawing.Point(32, 151);
            this.tbZoomScale.Maximum = 6;
            this.tbZoomScale.Minimum = 2;
            this.tbZoomScale.Name = "tbZoomScale";
            this.tbZoomScale.Size = new System.Drawing.Size(104, 45);
            this.tbZoomScale.TabIndex = 5;
            this.tbZoomScale.Value = 2;
            this.tbZoomScale.ValueChanged += new System.EventHandler(this.tbZoomScale_ValueChanged);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(312, 231);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 6;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // lblLastFileName
            // 
            this.lblLastFileName.AutoSize = true;
            this.lblLastFileName.Location = new System.Drawing.Point(13, 12);
            this.lblLastFileName.Name = "lblLastFileName";
            this.lblLastFileName.Size = new System.Drawing.Size(0, 13);
            this.lblLastFileName.TabIndex = 7;
            // 
            // btnSaveFile
            // 
            this.btnSaveFile.Location = new System.Drawing.Point(168, 151);
            this.btnSaveFile.Name = "btnSaveFile";
            this.btnSaveFile.Size = new System.Drawing.Size(116, 23);
            this.btnSaveFile.TabIndex = 8;
            this.btnSaveFile.Text = "Save";
            this.btnSaveFile.UseVisualStyleBackColor = true;
            this.btnSaveFile.Click += new System.EventHandler(this.btnSaveFile_Click);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "*.bmp|*.bmp";
            // 
            // PatternsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(399, 266);
            this.Controls.Add(this.btnSaveFile);
            this.Controls.Add(this.lblLastFileName);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.tbZoomScale);
            this.Controls.Add(this.picZoom);
            this.Controls.Add(this.pbProcessedPicture);
            this.Controls.Add(this.btnProcess);
            this.Controls.Add(this.pbSelectedPicture);
            this.Controls.Add(this.btnSelectPicture);
            this.Name = "PatternsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "PatternsForm";
            ((System.ComponentModel.ISupportInitialize)(this.pbSelectedPicture)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbProcessedPicture)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picZoom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbZoomScale)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSelectPicture;
        private System.Windows.Forms.PictureBox pbSelectedPicture;
        private System.Windows.Forms.Button btnProcess;
        private System.Windows.Forms.PictureBox pbProcessedPicture;
        private System.Windows.Forms.PictureBox picZoom;
        private System.Windows.Forms.TrackBar tbZoomScale;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblLastFileName;
        private System.Windows.Forms.Button btnSaveFile;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
    }
}