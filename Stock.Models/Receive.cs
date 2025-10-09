using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Stock.Models
{
    public class Receive
    {
        public int Id { get; set; }
        public int EmployeeCode { get; set; }
        public string Product { get; set; }
        public virtual Location Location { get; set; } = null!;          
        public decimal Qty { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public int? InputBy { get; set; } = 1;
        public DateTime InputDate { get; set; } = DateTime.Now;
        public int? ModifyBy { get; set; } = null!;
        public DateTime? ModifyDate { get; set; } = null!;
    }
}
