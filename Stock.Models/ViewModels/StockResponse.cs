using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Stock.Models.ViewModels
{
    public class StockResponse
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime TransactionDate { get; set; }
        public virtual Location Location { get; set; } = null!;        
        public string Product { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BfQty { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal InQty { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OutQty { get; set; }
    }
}
