using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock.Models.ViewModels
{
    public class StockRequest
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public virtual MasterListVM? Location { get; set; } = null!;        
        public string Product { get; set; }
        public int? InputBy { get; set; } = 1;
    }
}
