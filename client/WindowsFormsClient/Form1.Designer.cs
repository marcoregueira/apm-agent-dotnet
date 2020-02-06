namespace WindowsFormsClient
{
	partial class Form1
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
            this.TxtLogMessage = new System.Windows.Forms.TextBox();
            this.BtnLog = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.BtnException = new System.Windows.Forms.Button();
            this.TxtExceptionMessage = new System.Windows.Forms.TextBox();
            this.TxtNLog = new System.Windows.Forms.TextBox();
            this.BtnNlog = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.TxtExcepcionNLOG = new System.Windows.Forms.TextBox();
            this.BtnExcepcionNLOG = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // TxtLogMessage
            // 
            this.TxtLogMessage.Location = new System.Drawing.Point(67, 28);
            this.TxtLogMessage.Name = "TxtLogMessage";
            this.TxtLogMessage.Size = new System.Drawing.Size(588, 20);
            this.TxtLogMessage.TabIndex = 0;
            this.TxtLogMessage.Text = "The glorified log message {datetime}";
            // 
            // BtnLog
            // 
            this.BtnLog.Location = new System.Drawing.Point(661, 26);
            this.BtnLog.Name = "BtnLog";
            this.BtnLog.Size = new System.Drawing.Size(127, 23);
            this.BtnLog.TabIndex = 1;
            this.BtnLog.Text = "Guardar log";
            this.BtnLog.UseVisualStyleBackColor = true;
            this.BtnLog.Click += new System.EventHandler(this.BtnLog_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(67, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Log personalizado";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(67, 182);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Excepción";
            // 
            // BtnException
            // 
            this.BtnException.Location = new System.Drawing.Point(661, 199);
            this.BtnException.Name = "BtnException";
            this.BtnException.Size = new System.Drawing.Size(127, 23);
            this.BtnException.TabIndex = 4;
            this.BtnException.Text = "Guardar excepción";
            this.BtnException.UseVisualStyleBackColor = true;
            this.BtnException.Click += new System.EventHandler(this.BtnException_Click);
            // 
            // TxtExceptionMessage
            // 
            this.TxtExceptionMessage.Location = new System.Drawing.Point(67, 201);
            this.TxtExceptionMessage.Name = "TxtExceptionMessage";
            this.TxtExceptionMessage.Size = new System.Drawing.Size(588, 20);
            this.TxtExceptionMessage.TabIndex = 3;
            this.TxtExceptionMessage.Text = "The glorified exception message {datetime}";
            // 
            // TxtNLog
            // 
            this.TxtNLog.Location = new System.Drawing.Point(67, 74);
            this.TxtNLog.Name = "TxtNLog";
            this.TxtNLog.Size = new System.Drawing.Size(588, 20);
            this.TxtNLog.TabIndex = 0;
            this.TxtNLog.Text = "The glorified log message {datetime}";
            // 
            // BtnNlog
            // 
            this.BtnNlog.Location = new System.Drawing.Point(661, 72);
            this.BtnNlog.Name = "BtnNlog";
            this.BtnNlog.Size = new System.Drawing.Size(127, 23);
            this.BtnNlog.TabIndex = 1;
            this.BtnNlog.Text = "Guardar NLOG";
            this.BtnNlog.UseVisualStyleBackColor = true;
            this.BtnNlog.Click += new System.EventHandler(this.BtnNlog_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(67, 55);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(126, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Log personalizado NLOG";
            // 
            // TxtExcepcionNLOG
            // 
            this.TxtExcepcionNLOG.Location = new System.Drawing.Point(67, 248);
            this.TxtExcepcionNLOG.Name = "TxtExcepcionNLOG";
            this.TxtExcepcionNLOG.Size = new System.Drawing.Size(588, 20);
            this.TxtExcepcionNLOG.TabIndex = 3;
            this.TxtExcepcionNLOG.Text = "The glorified exception message {datetime}";
            // 
            // BtnExcepcionNLOG
            // 
            this.BtnExcepcionNLOG.Location = new System.Drawing.Point(661, 246);
            this.BtnExcepcionNLOG.Name = "BtnExcepcionNLOG";
            this.BtnExcepcionNLOG.Size = new System.Drawing.Size(127, 23);
            this.BtnExcepcionNLOG.TabIndex = 4;
            this.BtnExcepcionNLOG.Text = "Guardar except. NLOG";
            this.BtnExcepcionNLOG.UseVisualStyleBackColor = true;
            this.BtnExcepcionNLOG.Click += new System.EventHandler(this.BtnExcepcionNLOG_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(67, 229);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(90, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Excepción NLOG";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(260, 325);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(127, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Traza";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(879, 413);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.BtnExcepcionNLOG);
            this.Controls.Add(this.BtnException);
            this.Controls.Add(this.TxtExcepcionNLOG);
            this.Controls.Add(this.TxtExceptionMessage);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BtnNlog);
            this.Controls.Add(this.BtnLog);
            this.Controls.Add(this.TxtNLog);
            this.Controls.Add(this.TxtLogMessage);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.TextBox TxtLogMessage;
		private System.Windows.Forms.Button BtnLog;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button BtnException;
		private System.Windows.Forms.TextBox TxtExceptionMessage;
		private System.Windows.Forms.TextBox TxtNLog;
		private System.Windows.Forms.Button BtnNlog;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox TxtExcepcionNLOG;
		private System.Windows.Forms.Button BtnExcepcionNLOG;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button button1;
	}
}

