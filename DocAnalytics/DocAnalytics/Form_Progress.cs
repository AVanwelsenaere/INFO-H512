using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DocAnalytics
{
    public partial class Form_Progress : Form
    {
        public Form_Progress()
        {
            InitializeComponent();
        }

        public void SetProgressBar(int iProcent)
        {
            ProgressBar_Progress.Value = iProcent;
            ProgressBar_Progress.Refresh();
        }

        public void SetMessage(string sMsg)
        {
            Label_Progress_Status.Text = sMsg;
            Label_Progress_Status.Refresh();
        }
    }
}
