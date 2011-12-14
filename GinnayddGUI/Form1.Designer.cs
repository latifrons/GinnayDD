namespace GinnayddGUI
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.button1 = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.lblTaskMemory = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.lblTaskDB = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.lblTaskFinished = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.lblPicCount = new System.Windows.Forms.Label();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.lblWorkerCount = new System.Windows.Forms.Label();
			this.txtWorkerCount = new System.Windows.Forms.TextBox();
			this.button3 = new System.Windows.Forms.Button();
			this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
			this.txtNewTask = new System.Windows.Forms.TextBox();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.button4 = new System.Windows.Forms.Button();
			this.txtLog = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.lblProxyReady = new System.Windows.Forms.Label();
			this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
			this.button2 = new System.Windows.Forms.Button();
			this.button5 = new System.Windows.Forms.Button();
			this.tableLayoutPanel1.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.flowLayoutPanel2.SuspendLayout();
			this.flowLayoutPanel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.button1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.lblTaskMemory, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.label5, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.lblTaskDB, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this.label7, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.lblTaskFinished, 1, 4);
			this.tableLayoutPanel1.Controls.Add(this.label9, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.lblPicCount, 1, 5);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 7);
			this.tableLayoutPanel1.Controls.Add(this.txtLog, 0, 8);
			this.tableLayoutPanel1.Controls.Add(this.label2, 0, 6);
			this.tableLayoutPanel1.Controls.Add(this.lblProxyReady, 1, 6);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel3, 1, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 9;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(640, 373);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// button1
			// 
			this.button1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.button1.Location = new System.Drawing.Point(3, 6);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 0;
			this.button1.Text = "Start";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 46);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(47, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Workers";
			// 
			// label3
			// 
			this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(3, 70);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(118, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "Task Queue In Memory";
			// 
			// lblTaskMemory
			// 
			this.lblTaskMemory.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblTaskMemory.AutoSize = true;
			this.lblTaskMemory.Location = new System.Drawing.Point(136, 70);
			this.lblTaskMemory.Name = "lblTaskMemory";
			this.lblTaskMemory.Size = new System.Drawing.Size(78, 13);
			this.lblTaskMemory.TabIndex = 5;
			this.lblTaskMemory.Text = "lblTaskMemory";
			// 
			// label5
			// 
			this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(3, 83);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(127, 13);
			this.label5.TabIndex = 6;
			this.label5.Text = "Task Queue In Database";
			// 
			// lblTaskDB
			// 
			this.lblTaskDB.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblTaskDB.AutoSize = true;
			this.lblTaskDB.Location = new System.Drawing.Point(136, 83);
			this.lblTaskDB.Name = "lblTaskDB";
			this.lblTaskDB.Size = new System.Drawing.Size(56, 13);
			this.lblTaskDB.TabIndex = 7;
			this.lblTaskDB.Text = "lblTaskDB";
			// 
			// label7
			// 
			this.label7.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(3, 96);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(73, 13);
			this.label7.TabIndex = 8;
			this.label7.Text = "Task Finished";
			// 
			// lblTaskFinished
			// 
			this.lblTaskFinished.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblTaskFinished.AutoSize = true;
			this.lblTaskFinished.Location = new System.Drawing.Point(136, 96);
			this.lblTaskFinished.Name = "lblTaskFinished";
			this.lblTaskFinished.Size = new System.Drawing.Size(80, 13);
			this.lblTaskFinished.TabIndex = 9;
			this.lblTaskFinished.Text = "lblTaskFinished";
			// 
			// label9
			// 
			this.label9.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(3, 109);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(85, 13);
			this.label9.TabIndex = 10;
			this.label9.Text = "Pic Downloaded";
			// 
			// lblPicCount
			// 
			this.lblPicCount.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblPicCount.AutoSize = true;
			this.lblPicCount.Location = new System.Drawing.Point(136, 109);
			this.lblPicCount.Name = "lblPicCount";
			this.lblPicCount.Size = new System.Drawing.Size(60, 13);
			this.lblPicCount.TabIndex = 11;
			this.lblPicCount.Text = "lblPicCount";
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.AutoSize = true;
			this.flowLayoutPanel1.Controls.Add(this.lblWorkerCount);
			this.flowLayoutPanel1.Controls.Add(this.txtWorkerCount);
			this.flowLayoutPanel1.Controls.Add(this.button3);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(136, 38);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(501, 29);
			this.flowLayoutPanel1.TabIndex = 12;
			// 
			// lblWorkerCount
			// 
			this.lblWorkerCount.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblWorkerCount.AutoSize = true;
			this.lblWorkerCount.Location = new System.Drawing.Point(3, 8);
			this.lblWorkerCount.Name = "lblWorkerCount";
			this.lblWorkerCount.Size = new System.Drawing.Size(80, 13);
			this.lblWorkerCount.TabIndex = 4;
			this.lblWorkerCount.Text = "lblWorkerCount";
			// 
			// txtWorkerCount
			// 
			this.txtWorkerCount.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.txtWorkerCount.Location = new System.Drawing.Point(89, 4);
			this.txtWorkerCount.Name = "txtWorkerCount";
			this.txtWorkerCount.Size = new System.Drawing.Size(57, 20);
			this.txtWorkerCount.TabIndex = 5;
			this.txtWorkerCount.Text = "1";
			// 
			// button3
			// 
			this.button3.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.button3.Location = new System.Drawing.Point(152, 3);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(111, 23);
			this.button3.TabIndex = 6;
			this.button3.Text = "Set Worker Count";
			this.button3.UseVisualStyleBackColor = true;
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// flowLayoutPanel2
			// 
			this.flowLayoutPanel2.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel2, 2);
			this.flowLayoutPanel2.Controls.Add(this.txtNewTask);
			this.flowLayoutPanel2.Controls.Add(this.comboBox1);
			this.flowLayoutPanel2.Controls.Add(this.button4);
			this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 138);
			this.flowLayoutPanel2.Name = "flowLayoutPanel2";
			this.flowLayoutPanel2.Size = new System.Drawing.Size(634, 56);
			this.flowLayoutPanel2.TabIndex = 13;
			// 
			// txtNewTask
			// 
			this.txtNewTask.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.flowLayoutPanel2.SetFlowBreak(this.txtNewTask, true);
			this.txtNewTask.Location = new System.Drawing.Point(3, 3);
			this.txtNewTask.Name = "txtNewTask";
			this.txtNewTask.Size = new System.Drawing.Size(461, 20);
			this.txtNewTask.TabIndex = 0;
			// 
			// comboBox1
			// 
			this.comboBox1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox1.FormattingEnabled = true;
			this.comboBox1.Location = new System.Drawing.Point(3, 31);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(121, 21);
			this.comboBox1.TabIndex = 1;
			// 
			// button4
			// 
			this.button4.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.button4.Location = new System.Drawing.Point(130, 30);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(75, 23);
			this.button4.TabIndex = 2;
			this.button4.Text = "Add Task";
			this.button4.UseVisualStyleBackColor = true;
			this.button4.Click += new System.EventHandler(this.button4_Click);
			// 
			// txtLog
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.txtLog, 2);
			this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtLog.Location = new System.Drawing.Point(3, 200);
			this.txtLog.Multiline = true;
			this.txtLog.Name = "txtLog";
			this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtLog.Size = new System.Drawing.Size(634, 190);
			this.txtLog.TabIndex = 14;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(3, 122);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(67, 13);
			this.label2.TabIndex = 15;
			this.label2.Text = "Proxy Ready";
			// 
			// lblProxyReady
			// 
			this.lblProxyReady.AutoSize = true;
			this.lblProxyReady.Location = new System.Drawing.Point(136, 122);
			this.lblProxyReady.Name = "lblProxyReady";
			this.lblProxyReady.Size = new System.Drawing.Size(74, 13);
			this.lblProxyReady.TabIndex = 16;
			this.lblProxyReady.Text = "lblProxyReady";
			// 
			// flowLayoutPanel3
			// 
			this.flowLayoutPanel3.AutoSize = true;
			this.flowLayoutPanel3.Controls.Add(this.button2);
			this.flowLayoutPanel3.Controls.Add(this.button5);
			this.flowLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel3.Location = new System.Drawing.Point(136, 3);
			this.flowLayoutPanel3.Name = "flowLayoutPanel3";
			this.flowLayoutPanel3.Size = new System.Drawing.Size(501, 29);
			this.flowLayoutPanel3.TabIndex = 17;
			// 
			// button2
			// 
			this.button2.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.button2.Location = new System.Drawing.Point(3, 3);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 23);
			this.button2.TabIndex = 2;
			this.button2.Text = "Stop";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// button5
			// 
			this.button5.Location = new System.Drawing.Point(84, 3);
			this.button5.Name = "button5";
			this.button5.Size = new System.Drawing.Size(75, 23);
			this.button5.TabIndex = 3;
			this.button5.Text = "GC";
			this.button5.UseVisualStyleBackColor = true;
			this.button5.Click += new System.EventHandler(this.button5_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(640, 373);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.flowLayoutPanel2.ResumeLayout(false);
			this.flowLayoutPanel2.PerformLayout();
			this.flowLayoutPanel3.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label lblTaskMemory;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label lblTaskDB;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label lblTaskFinished;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label lblPicCount;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.Label lblWorkerCount;
		private System.Windows.Forms.TextBox txtWorkerCount;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
		private System.Windows.Forms.TextBox txtNewTask;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.TextBox txtLog;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label lblProxyReady;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button5;
	}
}

