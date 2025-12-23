using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WH.Models
{
    public class Setting
    {
        public int Id { get; set; }
        public string Prefix { get; set; } = String.Empty;
        public string? Name1 { get; set; } = String.Empty;
        public string? Name2 { get; set; } = String.Empty;
        public string? Name3 { get; set; } = String.Empty;
        public string? Name4 { get; set; } = String.Empty;
        public string? Name5 { get; set; } = String.Empty;
        public string? Name6 { get; set; } = String.Empty;
        public string? Name7 { get; set; } = String.Empty;
        public string? Name8 { get; set; } = String.Empty;
        public string? Name9 { get; set; } = String.Empty;
        public string? Name10 { get; set; } = String.Empty;

        public decimal Value1 { get; set; } = 0;
        public decimal Value2 { get; set; } = 0;
        public decimal Value3 { get; set; } = 0;
        public decimal Value4 { get; set; } = 0;
        public decimal Value5 { get; set; } = 0;

        public bool Actual1 { get; set; } = false;
        public bool Actual2 { get; set; } = false;
        public bool Actual3 { get; set; } = false;
        public string? Description { get; set; } = String.Empty;
        public bool IsActive { get; set; } = true;
    }
}
