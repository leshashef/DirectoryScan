using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryScan.Model
{
    internal class SortingModel
    {
        public string Column { get; set; } = null!;
        public bool IsAscending { get; set; }
    }
}
