using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAnalytics
{
    class BookHelper
    {
        public int iBookId { get; set; }
        public string sDisplayName { get; set; } 

        public override string ToString()
        {
            return sDisplayName; 
        }
    }
}
