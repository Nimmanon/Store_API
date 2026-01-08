using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock.Models.ViewModels
{
    public class ReceiveResponse
    {
        public string Product { get; set; }
        public int LocationId { get; set; }
        public string StatusName { get; set; } = string.Empty;       
       
    }
}
