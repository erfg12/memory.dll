using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memory
{
    struct MemoryRegionResult
    {
        public IntPtr CurrentBaseAddress { get; set; }
        public long RegionSize { get; set; }
        public IntPtr RegionBase { get; set; }

    }
}
