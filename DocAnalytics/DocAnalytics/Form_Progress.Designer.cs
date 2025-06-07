namespace DocAnalytics
{
    partial class Form_Progress
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
            this.ProgressBar_Progress = new System.Windows.Forms.ProgressBar();
            this.Label_Progress_InProgress = new System.Windows.Forms.Label();
            this.Label_Progress_Status = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // ProgressBar_Progress
            // 
            this.ProgressBar_Progress.Location = new System.Drawing.Point(54, 111);
            this.ProgressBar_Progress.Name = "ProgressBar_Progress";
            this.ProgressBar_Progress.Size = new System.Drawing.Size(745, 23);
            this.ProgressBar_Progress.TabIndex = 0;
            // 
            // Label_Progress_InProgress
            // 
            this.Label_Progress_InProgress.AutoSize = true;
            this.Label_Progress_InProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label_Progress_InProgress.ForeColor = System.Drawing.SystemColors.MenuText;
            this.Label_Progress_InProgress.Location = new System.Drawing.Point(298, 34);
            this.Label_Progress_InProgress.Name = "Label_Progress_InProgress";
            this.Label_Progress_InProgress.Size = new System.Drawing.Size(256, 46);
            this.Label_Progress_InProgress.TabIndex = 1;
            this.Label_Progress_InProgress.Text = "In Progress...";
            // 
            // Label_Progress_Status
            // 
            this.Label_Progress_Status.AutoSize = true;
            this.Label_Progress_Status.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label_Progress_Status.Location = new System.Drawing.Point(51, 152);
            this.Label_Progress_Status.Name = "Label_Progress_Status";
            this.Label_Progress_Status.Size = new System.Drawing.Size(16, 17);
            this.Label_Progress_Status.TabIndex = 2;
            this.Label_Progress_Status.Text = "_";
            // 
            // Form_Progress
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.ClientSize = new System.Drawing.Size(847, 187);
            this.ControlBox = false;
            this.Controls.Add(this.Label_Progress_Status);
            this.Controls.Add(this.Label_Progress_InProgress);
            this.Controls.Add(this.ProgressBar_Progress);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_Progress";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Please Wait...     In progress....";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar ProgressBar_Progress;
        private System.Windows.Forms.Label Label_Progress_InProgress;
        private System.Windows.Forms.Label Label_Progress_Status;
    }
}