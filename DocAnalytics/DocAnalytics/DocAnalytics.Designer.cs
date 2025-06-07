namespace DocAnalytics
{
    partial class DocAnalytics_Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DocAnalytics_Main));
            tabControl = new TabControl();
            tabPage_Connection = new TabPage();
            tabPage1 = new TabPage();
            tabPage_Explore = new TabPage();
            tabPage_Embeddings = new TabPage();
            tabPage_Ask = new TabPage();
            tabControl.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl.Controls.Add(tabPage_Connection);
            tabControl.Controls.Add(tabPage1);
            tabControl.Controls.Add(tabPage_Explore);
            tabControl.Controls.Add(tabPage_Embeddings);
            tabControl.Controls.Add(tabPage_Ask);
            tabControl.Location = new Point(-1, 1);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(1297, 1053);
            tabControl.TabIndex = 0;
            // 
            // tabPage_Connection
            // 
            tabPage_Connection.Location = new Point(4, 24);
            tabPage_Connection.Name = "tabPage_Connection";
            tabPage_Connection.Padding = new Padding(3);
            tabPage_Connection.Size = new Size(1289, 1025);
            tabPage_Connection.TabIndex = 0;
            tabPage_Connection.Text = "Connection";
            tabPage_Connection.UseVisualStyleBackColor = true;
            // 
            // tabPage1
            // 
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Size = new Size(1289, 1025);
            tabPage1.TabIndex = 2;
            tabPage1.Text = "Import Data";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage_Explore
            // 
            tabPage_Explore.Location = new Point(4, 24);
            tabPage_Explore.Name = "tabPage_Explore";
            tabPage_Explore.Padding = new Padding(3);
            tabPage_Explore.Size = new Size(1289, 1025);
            tabPage_Explore.TabIndex = 1;
            tabPage_Explore.Text = "Explore";
            tabPage_Explore.UseVisualStyleBackColor = true;
            // 
            // tabPage_Embeddings
            // 
            tabPage_Embeddings.Location = new Point(4, 24);
            tabPage_Embeddings.Name = "tabPage_Embeddings";
            tabPage_Embeddings.Size = new Size(1289, 1025);
            tabPage_Embeddings.TabIndex = 4;
            tabPage_Embeddings.Text = "Compute Embeddings";
            tabPage_Embeddings.UseVisualStyleBackColor = true;
            // 
            // tabPage_Ask
            // 
            tabPage_Ask.Location = new Point(4, 24);
            tabPage_Ask.Name = "tabPage_Ask";
            tabPage_Ask.Size = new Size(1289, 1025);
            tabPage_Ask.TabIndex = 3;
            tabPage_Ask.Text = "Ask my Data";
            tabPage_Ask.UseVisualStyleBackColor = true;
            // 
            // DocAnalytics_Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1297, 1053);
            Controls.Add(tabControl);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "DocAnalytics_Main";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "DocAnalytics";
            tabControl.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl;
        private TabPage tabPage_Connection;
        private TabPage tabPage_Explore;
        private ComboBox comboBox_Books;
        private ComboBox comboBox_Pages;
        private TextBox textBox_PageContent;
        private Button button_Connect;
        private Button button_Disconnect;
        private Label label_Status;
        private TabPage tabPage1;
        private TabPage tabPage_Ask;
        private TabPage tabPage_Embeddings;
    }
}
