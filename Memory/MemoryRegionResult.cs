using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memory
{
    struct MemoryRegionResult
    {
        public UIntPtr CurrentBaseAddress { get; set; }
        public long RegionSize { get; set; }
        public UIntPtr RegionBase { get; set; }

    }
}
