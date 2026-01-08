using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock.Models.ViewModels
{
    public class ActionVM
    {
        public int Id { get; set; }
        public string? Remark { get; set; }
        public int InputBy { get; set; }
    }
}
