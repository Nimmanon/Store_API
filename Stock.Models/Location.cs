using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock.Models
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Descprition { get; set; }
        public bool IsActive { get; set; } = true;
        public int? InputBy { get; set; } = 1;
        public DateTime InputDate { get; set; } = DateTime.Now;
        public int? ModifyBy { get; set; } = null!;
        public DateTime? ModifyDate { get; set; } = null!;
    }
}
