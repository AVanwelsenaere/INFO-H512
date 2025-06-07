using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAnalytics
{
    class ListBox_Results_Helper
    {
        public string DisplayText { get; set; }
        public string BookName { get; set; }
        public int PageId { get; set; }
        public string TextType { get; set; }
        public string FullText { get; set; }
        public float Score { get; set; }
        public int Page_Number { get; set; }

        public ListBox_Results_Helper(string displayText, string bookName, int pageId, string textType, string fullText, float score, int page_number)
        {
            DisplayText = displayText;
            BookName = bookName;
            PageId = pageId;
            TextType = textType;
            FullText = fullText;
            Score = score;
            Page_Number = page_number;
        }
        public override string ToString()
        {
            return DisplayText;
        }
    }
}
