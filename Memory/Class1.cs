using System;
using System.IO;
using System.IO.Pipes;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Globalization;

namespace Memory
{
    public class Mem
    {
        #region DllImports
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(
            UInt32 dwDesiredAccess,
            Int32 bInheritHandle,
            Int32 dwProcessId
            );

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr w, IntPtr l);

        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            string lpBuffer,
            UIntPtr nSize,
            out IntPtr lpNumberOfBytesWritten
        );

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern uint GetPrivateProfileString(
           string lpAppName,
           string lpKeyName,
           string lpDefault,
           StringBuilder lpReturnedString,
           uint nSize,
           string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool VirtualFreeEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            UIntPtr dwSize,
            uint dwFreeType
            );

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            uint dwSize,
            uint flAllocationType,
            uint flProtect
        );

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern UIntPtr GetProcAddress(
            IntPtr hModule,
            string procName
        );

        [DllImport("kernel32.dll", EntryPoint = "CloseHandle")]
        private static extern bool _CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        public static extern Int32 CloseHandle(
        IntPtr hObject
        );

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(
            string lpModuleName
        );

        [DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
        internal static extern Int32 WaitForSingleObject(
            IntPtr handle,
            Int32 milliseconds
        );

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32")]
        public static extern IntPtr CreateRemoteThread(
          IntPtr hProcess,
          IntPtr lpThreadAttributes,
          uint dwStackSize,
          UIntPtr lpStartAddress, // raw Pointer into remote process  
          IntPtr lpParameter,
          uint dwCreationFlags,
          out IntPtr lpThreadId
        );

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        // privileges
        const int PROCESS_CREATE_THREAD = 0x0002;
        const int PROCESS_QUERY_INFORMATION = 0x0400;
        const int PROCESS_VM_OPERATION = 0x0008;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_READ = 0x0010;

        // used for memory allocation
        const uint MEM_COMMIT = 0x00001000;
        const uint MEM_RESERVE = 0x00002000;
        const uint PAGE_READWRITE = 4;
        #endregion

        public static IntPtr pHandle;
        public Process procs = null;

        public bool OpenGameProcess(int procID)
        {
            if (procID != 0) //getProcIDFromName returns 0 if there was a problem
                procs = Process.GetProcessById(procID);
            else
                return false;

            if (procs.Responding == false)
                return false;

            pHandle = OpenProcess(0x1F0FFF, 1, procID);
            mainModule = procs.MainModule;
            foreach (ProcessModule Module in procs.Modules)
            {
                if (Module.ModuleName != "" && Module.ModuleName != null && !modules.ContainsKey(Module.ModuleName))
                    modules.Add(Module.ModuleName, Module.BaseAddress);
            }
            return true;
        }

        public void setFocus()
        {
            //int style = GetWindowLong(procs.MainWindowHandle, -16);
            //if ((style & 0x20000000) == 0x20000000) //minimized
            //    SendMessage(procs.Handle, 0x0112, (IntPtr)0xF120, IntPtr.Zero);
            SetForegroundWindow(procs.MainWindowHandle);
        }

        public int getProcIDFromName(string name) //new 1.0.2 function
        {
            Process[] processlist = Process.GetProcesses();

            foreach (Process theprocess in processlist)
            {
                if (theprocess.ProcessName == name) //find (name).exe in the process list (use task manager to find the name)
                    return theprocess.Id;
            }

            return 0; //if we fail to find it
        }

        public string LoadCode(string name, string file/*, bool isString*/) //version 1.0.4 added isString
        {
            StringBuilder returnCode = new StringBuilder(1024);
            uint read_ini_result;
            if (file != "")
                read_ini_result = GetPrivateProfileString("codes", name, "", returnCode, (uint)file.Length, file);
            else
                returnCode.Append(name);
            return returnCode.ToString();
        }

        private Int32 LoadIntCode(string name, string path)
        {
            int intValue = Convert.ToInt32(LoadCode(name, path), 16);
            if (intValue >= 0)
                return intValue;
            else
                return 0;
        }

        public Dictionary<string, IntPtr> modules = new Dictionary<string, IntPtr>();

        public void ThreadStartClient(object obj)
        {
            ManualResetEvent SyncClientServer = (ManualResetEvent)obj;
            using (NamedPipeClientStream pipeStream = new NamedPipeClientStream("EQTPipe"))
            {
                if (!pipeStream.IsConnected)
                    pipeStream.Connect();

                //MessageBox.Show("[Client] Pipe connection established");
                using (StreamWriter sw = new StreamWriter(pipeStream))
                {
                    if (sw.AutoFlush == false)
                        sw.AutoFlush = true;
                    sw.WriteLine("warp");
                }
            }
        }

        private ProcessModule mainModule;

        private UIntPtr LoadUIntPtrCode(string name, string path = "")
        {
            string theCode = LoadCode(name, path);
            UIntPtr uintValue;

            string newOffset = theCode.Substring(theCode.IndexOf('+') + 1);

            if (String.IsNullOrEmpty(newOffset))
                return (UIntPtr)0;

            int intToUint = 0;

            if (Convert.ToInt32(newOffset, 16) > 0)
                intToUint = Convert.ToInt32(newOffset, 16);

            if (theCode.Contains("base"))
                uintValue = (UIntPtr)((int)mainModule.BaseAddress + intToUint);
            else
                uintValue = (UIntPtr)intToUint;

            return (UIntPtr)uintValue;
        }

        public string CutString(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if (c >= ' ' && c <= '~')
                    sb.Append(c);
                else
                    break;
            }
            return sb.ToString();
        }

        public string sanitizeString(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if (c >= ' ' && c <= '~')
                    sb.Append(c);
            }
            return sb.ToString();
        }

        public IntPtr AoBScan(int min, int length, string code, string file = "")
        {
            string[] stringByteArray = LoadCode(code, file).Split(' ');
            byte[] myPattern = new byte[stringByteArray.Length];
            string mask = "";
            int i = 0;
            foreach (string ba in stringByteArray)
            {
                if (ba == "??")
                {
                    myPattern[i] = 0xFF;
                    mask += "?";
                }
                else
                {
                    myPattern[i] = Byte.Parse(ba, NumberStyles.HexNumber);
                    mask += "x";
                }
                i++;
            }
            SigScan _sigScan = new SigScan(procs, new IntPtr(min), length);
            IntPtr pAddr = _sigScan.FindPattern(myPattern, mask, 0);
            return pAddr;
        }

        #region readMemory
        public float readFloat(string code, string file = "")
        {
            byte[] memory = new byte[4];

            UIntPtr theCode;
            if (!LoadCode(code, file).Contains(","))
                theCode = LoadUIntPtrCode(code, file);
            else
                theCode = getCode(code, file);

            if (ReadProcessMemory(pHandle, theCode, memory, (UIntPtr)4, IntPtr.Zero))
            {
                float address = BitConverter.ToSingle(memory, 0);
                float returnValue = (float)Math.Round(address, 2);
                if (returnValue < -99999 || returnValue > 99999)
                    return 0;
                else
                    return returnValue;
            }
            else
                return 0;
        }

        public string readString(string code, string file = "")
        {
            byte[] memoryNormal = new byte[32];
            UIntPtr theCode;
            theCode = getCode(code, file);
            if (!LoadCode(code, file).Contains(","))
                theCode = LoadUIntPtrCode(code, file);
            else
                theCode = getCode(code, file);

            if (ReadProcessMemory(pHandle, theCode, memoryNormal, (UIntPtr)32, IntPtr.Zero))
                return System.Text.Encoding.UTF8.GetString(memoryNormal);
            else
                return "";
        }

        public int readUIntPtr(UIntPtr code)
        {
            byte[] memory = new byte[4];
            if (ReadProcessMemory(pHandle, code, memory, (UIntPtr)4, IntPtr.Zero))
                return BitConverter.ToInt32(memory, 0);
            else
                return 0;
        }

        public int readInt(string code, string file = "")
        {
            byte[] memory = new byte[4];
            UIntPtr theCode;

            if (!LoadCode(code, file).Contains(","))
                theCode = LoadUIntPtrCode(code, file);
            else
                theCode = getCode(code, file);

            if (ReadProcessMemory(pHandle, theCode, memory, (UIntPtr)4, IntPtr.Zero))
                return BitConverter.ToInt32(memory, 0);
            else
                return 0;
        }

        public uint readUInt(string code, string file = "")
        {
            byte[] memory = new byte[4];
            UIntPtr theCode;
            if (!LoadCode(code, file).Contains(","))
                theCode = LoadUIntPtrCode(code, file);
            else
                theCode = getCode(code, file);

            if (ReadProcessMemory(pHandle, theCode, memory, (UIntPtr)4, IntPtr.Zero))
                return BitConverter.ToUInt32(memory, 0);
            else
                return 0;
        }

        public int read2ByteMove(string code, int moveQty, string file = "")
        {
            byte[] memory = new byte[4];
            UIntPtr theCode;
            if (!LoadCode(code, file).Contains(","))
                theCode = LoadUIntPtrCode(code, file);
            else
                theCode = getCode(code, file);

            UIntPtr newCode = UIntPtr.Add(theCode, moveQty);

            if (ReadProcessMemory(pHandle, newCode, memory, (UIntPtr)2, IntPtr.Zero))
                return BitConverter.ToInt32(memory, 0);
            else
                return 0;
        }

        public int readIntMove(string code, int moveQty, string file = "")
        {
            byte[] memory = new byte[4];
            UIntPtr theCode;
            if (!LoadCode(code, file).Contains(","))
                theCode = LoadUIntPtrCode(code, file);
            else
                theCode = getCode(code, file);

            UIntPtr newCode = UIntPtr.Add(theCode, moveQty);

            if (ReadProcessMemory(pHandle, newCode, memory, (UIntPtr)4, IntPtr.Zero))
                return BitConverter.ToInt32(memory, 0);
            else
                return 0;
        }

        public ulong readUIntMove(string code, string file, int moveQty)
        {
            byte[] memory = new byte[8];
            UIntPtr theCode;
            if (!LoadCode(code, file).Contains(","))
                theCode = LoadUIntPtrCode(code, file);
            else
                theCode = getCode(code, file, 8);

            UIntPtr newCode = UIntPtr.Add(theCode, moveQty);

            if (ReadProcessMemory(pHandle, newCode, memory, (UIntPtr)8, IntPtr.Zero))
                return BitConverter.ToUInt64(memory, 0);
            else
                return 0;
        }

        public int read2Byte(string code, string file = "")
        {
            byte[] memoryTiny = new byte[4];

            UIntPtr theCode;
            if (!LoadCode(code, file).Contains(","))
                theCode = LoadUIntPtrCode(code, file);
            else
                theCode = getCode(code, file);

            if (ReadProcessMemory(pHandle, theCode, memoryTiny, (UIntPtr)2, IntPtr.Zero))
                return BitConverter.ToInt32(memoryTiny, 0);
            else
                return 0;
        }

        public int readByte(string code, string file = "")
        {
            byte[] memoryTiny = new byte[4];

            UIntPtr theCode;
            if (!LoadCode(code, file).Contains(","))
                theCode = LoadUIntPtrCode(code, file);
            else
                theCode = getCode(code, file);

            if (ReadProcessMemory(pHandle, theCode, memoryTiny, (UIntPtr)1, IntPtr.Zero))
                return BitConverter.ToInt32(memoryTiny, 0);
            else
                return 0;
        }

        public int readPByte(UIntPtr address, string code, string file = "")
        {
            byte[] memory = new byte[4];
            if (ReadProcessMemory(pHandle, address + LoadIntCode(code, file), memory, (UIntPtr)1, IntPtr.Zero))
                return BitConverter.ToInt32(memory, 0);
            else
                return 0;
        }

        public float readPFloat(UIntPtr address, string code, string file = "")
        {
            byte[] memory = new byte[4];
            if (ReadProcessMemory(pHandle, address + LoadIntCode(code, file), memory, (UIntPtr)4, IntPtr.Zero))
            {
                float spawn = BitConverter.ToSingle(memory, 0);
                return (float)Math.Round(spawn, 2);
            }
            else
                return 0;
        }

        public int readPInt(UIntPtr address, string code, string file = "")
        {
            byte[] memory = new byte[4];
            if (ReadProcessMemory(pHandle, address + LoadIntCode(code, file), memory, (UIntPtr)4, IntPtr.Zero))
                return BitConverter.ToInt32(memory, 0);
            else
                return 0;
        }

        public string readPString(UIntPtr address, string code, string file = "")
        {
            byte[] memoryNormal = new byte[32];
            if (ReadProcessMemory(pHandle, address + LoadIntCode(code, file), memoryNormal, (UIntPtr)32, IntPtr.Zero))
                return CutString(System.Text.Encoding.ASCII.GetString(memoryNormal));
            else
                return "";
        }
        #endregion

        #region writeMemory
        public bool writeMemory(string code, string type, string write, string file = "")
        {
            byte[] memory = new byte[4];
            int size = 4;

            UIntPtr theCode;
            if (!LoadCode(code, file).Contains(","))
                theCode = LoadUIntPtrCode(code, file);
            else
                theCode = getCode(code, file);

            if (type == "float")
            {
                memory = BitConverter.GetBytes(Convert.ToSingle(write));
                size = 4;
            }
            else if (type == "int")
            {
                memory = BitConverter.GetBytes(Convert.ToInt32(write));
                size = 4;
            }
            else if (type == "byte")
            {
                memory = new byte[1];
                memory = BitConverter.GetBytes(Convert.ToInt32(write));
                size = 1;
            }
            else if (type == "string")
            {
                memory = new byte[write.Length];
                memory = System.Text.Encoding.UTF8.GetBytes(write);
                size = write.Length;
            }

            if (WriteProcessMemory(pHandle, theCode, memory, (UIntPtr)size, IntPtr.Zero))
                return true;
            else
                return false;
        }

        public bool writeMove(string code, string type, string write, int moveQty, string file = "") //version v1.0.3
        {
            byte[] memory = new byte[4];
            int size = 4;

            UIntPtr theCode;
            if (!LoadCode(code, file).Contains(","))
                theCode = LoadUIntPtrCode(code, file);
            else
                theCode = getCode(code, file);

            if (type == "float")
            {
                memory = BitConverter.GetBytes(Convert.ToSingle(write));
                size = 4;
            }
            else if (type == "int")
            {
                memory = BitConverter.GetBytes(Convert.ToInt32(write));
                size = 4;
            }
            else if (type == "byte")
            {
                memory = new byte[1];
                memory = BitConverter.GetBytes(Convert.ToInt32(write));
                size = 1;
            }
            else if (type == "string")
            {
                memory = new byte[write.Length];
                memory = System.Text.Encoding.UTF8.GetBytes(write);
                size = write.Length;
            }

            UIntPtr newCode = UIntPtr.Add(theCode, moveQty);

            if (WriteProcessMemory(pHandle, newCode, memory, (UIntPtr)size, IntPtr.Zero))
                return true;
            else
                return false;
        }

        public void writeUIntPtr(string code, byte[] write, string file = "")
        {
            WriteProcessMemory(pHandle, LoadUIntPtrCode(code, file), write, (UIntPtr)write.Length, IntPtr.Zero);
        }

        public void writeByte(UIntPtr code, byte[] write, int size)
        {
            WriteProcessMemory(pHandle, code, write, (UIntPtr)size, IntPtr.Zero);
        }
        #endregion

        private UIntPtr getCode(string name, string path, int size = 4)
        {
            string theCode = LoadCode(name, path);
            if (theCode == "")
                return UIntPtr.Zero;
            string newOffsets = theCode;
            if (theCode.Contains("+"))
                newOffsets = theCode.Substring(theCode.IndexOf('+') + 1);

            byte[] memoryAddress = new byte[size];

            if (newOffsets.Contains(','))
            {
                List<int> offsetsList = new List<int>();

                string[] newerOffsets = newOffsets.Split(',');
                foreach (string oldOffsets in newerOffsets)
                {
                    offsetsList.Add(Convert.ToInt32(oldOffsets, 16));
                }
                int[] offsets = offsetsList.ToArray();

                if (theCode.Contains("base") || theCode.Contains("main"))
                    ReadProcessMemory(pHandle, (UIntPtr)((int)mainModule.BaseAddress + offsets[0]), memoryAddress, (UIntPtr)size, IntPtr.Zero);
                else if (!theCode.Contains("base") && !theCode.Contains("main") && theCode.Contains("+"))
                {
                    string[] moduleName = theCode.Split('+');
                    IntPtr altModule = modules[moduleName[0]];
                    ReadProcessMemory(pHandle, (UIntPtr)((int)altModule + offsets[0]), memoryAddress, (UIntPtr)size, IntPtr.Zero);
                }
                else
                    ReadProcessMemory(pHandle, (UIntPtr)(offsets[0]), memoryAddress, (UIntPtr)size, IntPtr.Zero);

                uint num1 = BitConverter.ToUInt32(memoryAddress, 0);

                UIntPtr base1 = (UIntPtr)0;

                for (int i = 1; i < offsets.Length; i++)
                {
                    base1 = new UIntPtr(num1 + Convert.ToUInt32(offsets[i]));
                    ReadProcessMemory(pHandle, base1, memoryAddress, (UIntPtr)size, IntPtr.Zero);
                    num1 = BitConverter.ToUInt32(memoryAddress, 0);
                }
                return base1;
            }
            else
            {
                int trueCode = Convert.ToInt32(newOffsets, 16);

                if (theCode.Contains("base") || theCode.Contains("main"))
                    ReadProcessMemory(pHandle, (UIntPtr)((int)mainModule.BaseAddress + trueCode), memoryAddress, (UIntPtr)size, IntPtr.Zero);
                else if (!theCode.Contains("base") && !theCode.Contains("main") && theCode.Contains("+"))
                {
                    string[] moduleName = theCode.Split('+');
                    IntPtr altModule = modules[moduleName[0]];
                    ReadProcessMemory(pHandle, (UIntPtr)((int)altModule + trueCode), memoryAddress, (UIntPtr)size, IntPtr.Zero);
                }
                else
                    ReadProcessMemory(pHandle, (UIntPtr)(trueCode), memoryAddress, (UIntPtr)size, IntPtr.Zero);

                uint num1 = BitConverter.ToUInt32(memoryAddress, 0);
                
                UIntPtr base1 = new UIntPtr(num1);
                num1 = BitConverter.ToUInt32(memoryAddress, 0);
                return base1;
            }
        }

        public void closeProcess()
        {
            CloseHandle(pHandle);
        }

        public void InjectDLL(String strDLLName)
        {
            IntPtr bytesout;

            foreach (ProcessModule pm in procs.Modules)
            {
                if (pm.ModuleName.StartsWith("inject", StringComparison.InvariantCultureIgnoreCase))
                    return;
            }

            if (procs.Responding == false)
                return;

            Int32 LenWrite = strDLLName.Length + 1;
            IntPtr AllocMem = (IntPtr)VirtualAllocEx(pHandle, (IntPtr)null, (uint)LenWrite, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

            WriteProcessMemory(pHandle, AllocMem, strDLLName, (UIntPtr)LenWrite, out bytesout);
            UIntPtr Injector = (UIntPtr)GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");

            if (Injector == null)
                return;

            IntPtr hThread = (IntPtr)CreateRemoteThread(pHandle, (IntPtr)null, 0, Injector, AllocMem, 0, out bytesout);
            if (hThread == null)
                return;

            int Result = WaitForSingleObject(hThread, 10 * 1000);
            if (Result == 0x00000080L || Result == 0x00000102L)
            {
                if (hThread != null)
                    CloseHandle(hThread);
                return;
            }
            VirtualFreeEx(pHandle, AllocMem, (UIntPtr)0, 0x8000);

            if (hThread != null)
                CloseHandle(hThread);

            return;
        }

        /** 
        * sigScan C# Implementation - Written by atom0s [aka Wiccaan] 
        * Class Version: 2.0.0 
        * 
        * [ CHANGE LOG ] ------------------------------------------------------------------------- 
        * 
        *      2.0.0 
        *          - Updated to no longer require unsafe or fixed code. 
        *          - Removed unneeded methods and code. 
        *          
        *      1.0.0 
        *          - First version written and release. 
        *          
        * [ CREDITS ] ---------------------------------------------------------------------------- 
        * 
        *      sigScan is based on the FindPattern code written by 
        *      dom1n1k and Patrick at GameDeception.net 
        *      
        *      Full credit to them for the purpose of this code. I, atom0s, simply 
        *      take credit for converting it to C#. 
        * 
        */
        public class SigScan
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool ReadProcessMemory(
                IntPtr hProcess,
                IntPtr lpBaseAddress,
                [Out] byte[] lpBuffer,
                int dwSize,
                out int lpNumberOfBytesRead
                );

            private byte[] m_vDumpedRegion;
            private Process m_vProcess;
            private IntPtr m_vAddress;
            private Int32 m_vSize;

            public SigScan()
            {
                this.m_vProcess = null;
                this.m_vAddress = IntPtr.Zero;
                this.m_vSize = 0;
                this.m_vDumpedRegion = null;
            }
            public SigScan(Process proc, IntPtr addr, int size)
            {
                this.m_vProcess = proc;
                this.m_vAddress = addr;
                this.m_vSize = size;
            }

            #region "sigScan Class Private Methods" 
            /// <summary> 
            /// DumpMemory 
            ///  
            ///     Internal memory dump function that uses the set class 
            ///     properties to dump a memory region. 
            /// </summary> 
            /// <returns>Boolean based on RPM results and valid properties.</returns> 
            private bool DumpMemory()
            {
                try
                {
                    // Checks to ensure we have valid data. 
                    if (this.m_vProcess == null)
                        return false;
                    if (this.m_vProcess.HasExited)
                        return false;
                    if (this.m_vAddress == IntPtr.Zero)
                        return false;
                    if (this.m_vSize == 0)
                        return false;

                    // Create the region space to dump into. 
                    this.m_vDumpedRegion = new byte[this.m_vSize];

                    int nBytesRead;

                    // Dump the memory. 
                    var ret = ReadProcessMemory(
                        this.m_vProcess.Handle, this.m_vAddress, this.m_vDumpedRegion, this.m_vSize, out nBytesRead
                        );

                    // Validation checks. 
                    return ret && nBytesRead == this.m_vSize;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            /// <summary> 
            /// MaskCheck 
            ///  
            ///     Compares the current pattern byte to the current memory dump 
            ///     byte to check for a match. Uses wildcards to skip bytes that 
            ///     are deemed unneeded in the compares. 
            /// </summary> 
            /// <param name="nOffset">Offset in the dump to start at.</param> 
            /// <param name="btPattern">Pattern to scan for.</param> 
            /// <param name="strMask">Mask to compare against.</param> 
            /// <returns>Boolean depending on if the pattern was found.</returns> 
            private bool MaskCheck(int nOffset, IEnumerable<byte> btPattern, string strMask)
            {
                // Loop the pattern and compare to the mask and dump. 
                return !btPattern.Where((t, x) => strMask[x] != '?' && ((strMask[x] == 'x') && (t != this.m_vDumpedRegion[nOffset + x]))).Any();

                // The loop was successful so we found the pattern. 
            }

            #endregion

            #region "sigScan Class Public Methods" 
            /// <summary> 
            /// FindPattern 
            ///  
            ///     Attempts to locate the given pattern inside the dumped memory region 
            ///     compared against the given mask. If the pattern is found, the offset 
            ///     is added to the located address and returned to the user. 
            /// </summary> 
            /// <param name="btPattern">Byte pattern to look for in the dumped region.</param> 
            /// <param name="strMask">The mask string to compare against.</param> 
            /// <param name="nOffset">The offset added to the result address.</param> 
            /// <returns>IntPtr - zero if not found, address if found.</returns> 
            public IntPtr FindPattern(byte[] btPattern, string strMask, int nOffset)
            {
                try
                {
                    // Dump the memory region if we have not dumped it yet. 
                    if (this.m_vDumpedRegion == null || this.m_vDumpedRegion.Length == 0)
                    {
                        if (!this.DumpMemory())
                            return IntPtr.Zero;
                    }

                    // Ensure the mask and pattern lengths match. 
                    if (strMask.Length != btPattern.Length)
                        return IntPtr.Zero;

                    // Loop the region and look for the pattern. 
                    for (int x = 0; x < this.m_vDumpedRegion.Length; x++)
                    {
                        if (this.MaskCheck(x, btPattern, strMask))
                        {
                            // The pattern was found, return it. 
                            return new IntPtr((int)this.m_vAddress + (x + nOffset));
                        }
                    }

                    // Pattern was not found. 
                    return IntPtr.Zero;
                }
                catch (Exception)
                {
                    return IntPtr.Zero;
                }
            }

            /// <summary> 
            /// ResetRegion 
            ///  
            ///     Resets the memory dump array to nothing to allow 
            ///     the class to redump the memory. 
            /// </summary> 
            public void ResetRegion()
            {
                this.m_vDumpedRegion = null;
            }
            #endregion

            #region "sigScan Class Properties" 
            public Process Process
            {
                get { return this.m_vProcess; }
                set { this.m_vProcess = value; }
            }
            public IntPtr Address
            {
                get { return this.m_vAddress; }
                set { this.m_vAddress = value; }
            }
            public Int32 Size
            {
                get { return this.m_vSize; }
                set { this.m_vSize = value; }
            }
            #endregion

        }
    }
}
