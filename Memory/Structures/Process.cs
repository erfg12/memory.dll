using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Memory
{
    /// <summary>
    /// Information about the opened process.
    /// </summary>
    public class Proc
    {
        public Process Process { get; set; }
        public IntPtr Handle { get; set; }
        public bool Is64Bit { get; set; }
        //public ConcurrentDictionary<string, IntPtr> Modules { get; set; } // Use mProc.Process.Modules instead
        public ProcessModule MainModule { get; set; }
    }
}
