using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock.Models.ViewModels
{
    public class StockOnHandDto
    {
        public string Product { get; set; }
        public int LocationId { get; set; }
        public string LocationName { get; set; }

        public decimal BfQty { get; set; }      // ยอดยกมา
        public decimal ReceiveQty { get; set; } // รับเข้า
        public decimal IssueQty { get; set; }   // จ่ายออก
        public decimal OnHand { get; set; }     // คงเหลือ
    }
}
