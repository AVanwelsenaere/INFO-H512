using System;
using System.Drawing;
using System.Windows.Forms;

namespace DocAnalytics
{
    public partial class Form_RichTextDisplay : Form
    {
        public Form_RichTextDisplay(string title, string fullText, string query)
        {
            InitializeAllComponent();
            this.Text = title;

            richTextBox_Display.Text = fullText;
            HighlightQueryWords(query);
        }

        private void HighlightQueryWords(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return;

            string[] queryWords = query.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string word in queryWords)
            {
                int startIndex = 0;
                while ((startIndex = richTextBox_Display.Text.IndexOf(word, startIndex, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    richTextBox_Display.Select(startIndex, word.Length);
                    richTextBox_Display.SelectionColor = Color.Red;
                    startIndex += word.Length;
                }
            }
        }

        private void InitializeAllComponent()
        {
            this.richTextBox_Display = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // richTextBox_Display
            // 
            this.richTextBox_Display.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox_Display.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.richTextBox_Display.Location = new System.Drawing.Point(0, 0);
            this.richTextBox_Display.Name = "richTextBox_Display";
            this.richTextBox_Display.Size = new System.Drawing.Size(800, 600);
            this.richTextBox_Display.TabIndex = 0;
            this.richTextBox_Display.Text = "";
            // 
            // Form_RichTextDisplay
            // 
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.richTextBox_Display);
            this.Name = "Form_RichTextDisplay";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Texte Complet";
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.RichTextBox richTextBox_Display;
    }
}
