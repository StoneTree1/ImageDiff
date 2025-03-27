using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDiffConsole
{
    public class CompareResult
    {
        public string? ResultFile { get; set; }
        public string? Error { get; set; }
        public bool IsDifferent { get; set; }
    }
}
