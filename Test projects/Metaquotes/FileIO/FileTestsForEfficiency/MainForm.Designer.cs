namespace TestsForEfficiency
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
      this.StatusMsg = new System.Windows.Forms.RichTextBox();
      this.btnWriteFile = new System.Windows.Forms.Button();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.btnRunAllFileTests = new System.Windows.Forms.Button();
      this.btnReadFile = new System.Windows.Forms.Button();
      this.btnWinFileIOUnitTests = new System.Windows.Forms.Button();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // StatusMsg
      // 
      this.StatusMsg.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.StatusMsg.Location = new System.Drawing.Point(9, 72);
      this.StatusMsg.Name = "StatusMsg";
      this.StatusMsg.Size = new System.Drawing.Size(947, 357);
      this.StatusMsg.TabIndex = 0;
      this.StatusMsg.Text = "";
      // 
      // btnWriteFile
      // 
      this.btnWriteFile.Location = new System.Drawing.Point(182, 14);
      this.btnWriteFile.Name = "btnWriteFile";
      this.btnWriteFile.Size = new System.Drawing.Size(75, 27);
      this.btnWriteFile.TabIndex = 6;
      this.btnWriteFile.Text = "Write File";
      this.btnWriteFile.UseVisualStyleBackColor = true;
      this.btnWriteFile.Click += new System.EventHandler(this.btnWriteFile_Click);
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.btnRunAllFileTests);
      this.groupBox2.Controls.Add(this.btnWriteFile);
      this.groupBox2.Controls.Add(this.btnReadFile);
      this.groupBox2.Location = new System.Drawing.Point(258, 12);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(270, 54);
      this.groupBox2.TabIndex = 5;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "File I/O Tests";
      // 
      // btnRunAllFileTests
      // 
      this.btnRunAllFileTests.Location = new System.Drawing.Point(12, 14);
      this.btnRunAllFileTests.Name = "btnRunAllFileTests";
      this.btnRunAllFileTests.Size = new System.Drawing.Size(75, 27);
      this.btnRunAllFileTests.TabIndex = 7;
      this.btnRunAllFileTests.Text = "Run All";
      this.btnRunAllFileTests.UseVisualStyleBackColor = true;
      this.btnRunAllFileTests.Click += new System.EventHandler(this.btnRunAllFileTests_Click);
      // 
      // btnReadFile
      // 
      this.btnReadFile.Location = new System.Drawing.Point(97, 14);
      this.btnReadFile.Name = "btnReadFile";
      this.btnReadFile.Size = new System.Drawing.Size(75, 27);
      this.btnReadFile.TabIndex = 5;
      this.btnReadFile.Text = "Read File";
      this.btnReadFile.UseVisualStyleBackColor = true;
      this.btnReadFile.Click += new System.EventHandler(this.btnReadFile_Click);
      // 
      // btnWinFileIOUnitTests
      // 
      this.btnWinFileIOUnitTests.Location = new System.Drawing.Point(588, 26);
      this.btnWinFileIOUnitTests.Name = "btnWinFileIOUnitTests";
      this.btnWinFileIOUnitTests.Size = new System.Drawing.Size(118, 27);
      this.btnWinFileIOUnitTests.TabIndex = 9;
      this.btnWinFileIOUnitTests.Text = "WinFileIO Unit Tests";
      this.btnWinFileIOUnitTests.UseVisualStyleBackColor = true;
      this.btnWinFileIOUnitTests.Click += new System.EventHandler(this.btnWinFileIOUnitTests_Click);
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(965, 441);
      this.Controls.Add(this.btnWinFileIOUnitTests);
      this.Controls.Add(this.StatusMsg);
      this.Controls.Add(this.groupBox2);
      this.Name = "MainForm";
      this.Text = "File I/O Tests For Efficiency";
      this.Closing += new System.ComponentModel.CancelEventHandler(this.MainForm_Closing);
      this.groupBox2.ResumeLayout(false);
      this.ResumeLayout(false);

		}

		#endregion

    private System.Windows.Forms.RichTextBox StatusMsg;
		private System.Windows.Forms.Button btnWriteFile;
		private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.Button btnRunAllFileTests;
    private System.Windows.Forms.Button btnReadFile;
    private System.Windows.Forms.Button btnWinFileIOUnitTests;
	}
}

