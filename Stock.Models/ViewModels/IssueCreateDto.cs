using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock.Models.ViewModels
{
    public class IssueCreateDto
    {
        public int EmployeeCode { get; set; }
        public string Product { get; set; } = "";
        public int LocationId { get; set; }
        public decimal Qty { get; set; }
        public int? InputBy { get; set; }
    }


}
