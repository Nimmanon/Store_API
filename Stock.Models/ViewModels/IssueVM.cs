using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock.Models.ViewModels
{
    public class IssueVM
    {
        public int Id { get; set; }
        public int EmployeeCode { get; set; }
        public int Product { get; set; }
        public virtual MasterListVM Location { get; set; } = null!;
        //public virtual MasterListVM Group { get; set; } = null!;
        //public virtual MasterListVM Status { get; set; } = null!;
        //public virtual MasterListVM Unit { get; set; } = null!;
        public decimal Qty { get; set; } = 0;
        public int? InputBy { get; set; } = 1;
    }
}
