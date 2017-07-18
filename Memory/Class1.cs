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
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Security.Principal;

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
            if (isAdmin() == false)
            {
                Debug.Write("WARNING: You are NOT running this program as admin!! Visit https://github.com/erfg12/memory.dll/wiki/Administrative-Privileges");
                MessageBox.Show("WARNING: You are NOT running this program as admin!!" + Environment.NewLine + "Visit https://github.com/erfg12/memory.dll/wiki/Administrative-Privileges");
            }

            try
            {
                Process.EnterDebugMode();
                if (procID != 0) //getProcIDFromName returns 0 if there was a problem
                    procs = Process.GetProcessById(procID);
                else
                    return false;

                if (procs.Responding == false)
                    return false;

                pHandle = OpenProcess(0x1F0FFF, 1, procID);

                if (pHandle == IntPtr.Zero)
                {
                    var eCode = Marshal.GetLastWin32Error();
                }

                mainModule = procs.MainModule;
                getModules();
                return true;
            } catch { return false; }
        }

        public bool isAdmin()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public void getModules()
        {
            if (procs == null)
                return;

            modules.Clear();
            foreach (ProcessModule Module in procs.Modules)
            {
                if (Module.ModuleName != "" && Module.ModuleName != null && !modules.ContainsKey(Module.ModuleName))
                    modules.Add(Module.ModuleName, Module.BaseAddress);
            }
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

        public void ThreadStartClient(string func)
        {
            //ManualResetEvent SyncClientServer = (ManualResetEvent)obj;
            using (NamedPipeClientStream pipeStream = new NamedPipeClientStream("EQTPipe"))
            {
                if (!pipeStream.IsConnected)
                    pipeStream.Connect();

                //MessageBox.Show("[Client] Pipe connection established");
                using (StreamWriter sw = new StreamWriter(pipeStream))
                {
                    if (sw.AutoFlush == false)
                        sw.AutoFlush = true;
                    sw.WriteLine(func);
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

            if (theCode.Contains("base") || theCode.Contains("main"))
                uintValue = (UIntPtr)((int)mainModule.BaseAddress + intToUint);
            else if (!theCode.Contains("base") && !theCode.Contains("main") && theCode.Contains("+"))
            {
                string[] moduleName = theCode.Split('+');

                if (modules.Count == 0 || !modules.ContainsKey(moduleName[0]))
                    getModules();

                if (modules.ContainsKey(moduleName[0]))
                {
                    IntPtr altModule = modules[moduleName[0]];
                    uintValue = (UIntPtr)((int)altModule + intToUint);
                }
                else
                {
                    Debug.WriteLine("ERROR! Module " + moduleName[0] + " not found! Visit https://github.com/erfg12/memory.dll/wiki/List-Modules");
                    MessageBox.Show("ERROR! Module " + moduleName[0] + " not found!" + Environment.NewLine + "Visit https://github.com/erfg12/memory.dll/wiki/List-Modules");
                    return (UIntPtr)0;
                }
            }
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

        public long readLong(string code, string file = "")
        {
            byte[] memory = new byte[16];
            UIntPtr theCode;

            if (!LoadCode(code, file).Contains(","))
                theCode = LoadUIntPtrCode(code, file);
            else
                theCode = getCode(code, file);

            if (ReadProcessMemory(pHandle, theCode, memory, (UIntPtr)16, IntPtr.Zero))
                return BitConverter.ToInt64(memory, 0);
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
            else if (type == "bytes")
            {
                string[] script_instruction_value_split = write.Split(' ');
                int num_bytes = script_instruction_value_split.Length;
                byte[] write_bytes = new byte[num_bytes];
                long script_instruction_value_int = 0;
                byte[] script_instruction_value_bytes;
                for (int i = 0; i < num_bytes; i++)
                {
                    script_instruction_value_int = Int32.Parse(script_instruction_value_split[i], NumberStyles.AllowHexSpecifier);
                    script_instruction_value_bytes = BitConverter.GetBytes(script_instruction_value_int);
                    write_bytes[i] = script_instruction_value_bytes[0];
                }
                writeByte(theCode, write_bytes, num_bytes);
                return true;
            }
            else if (type == "long")
            {
                memory = BitConverter.GetBytes(Convert.ToInt64(write));
                size = 16;
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

        public IntPtr AoBScan(uint min, int length, string code, string file = "")
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
                else if (Char.IsLetterOrDigit(ba[0]) && ba[1] == '?') //partial match
                {
                    myPattern[i] = Encoding.ASCII.GetBytes("0x" + ba[0] + "F")[0];
                    mask += "?"; //show it's still a wildcard of some kind
                }
                else if (Char.IsLetterOrDigit(ba[1]) && ba[0] == '?') //partial match
                {
                    myPattern[i] = Encoding.ASCII.GetBytes("0xF" + ba[1])[0];
                    mask += "?"; //show it's still a wildcard of some kind
                }
                else
                {
                    myPattern[i] = Byte.Parse(ba, NumberStyles.HexNumber);
                    mask += "x";
                }
                i++;
            }

            DumpMemory((UIntPtr)min,length);
            IntPtr pAddr = FindPattern(myPattern, mask, 0);
            return pAddr;
        }

        byte[] dumpRegion = null;
        UIntPtr dumpAddress = (UIntPtr)0x00000000;

        private bool DumpMemory(UIntPtr addr, Int32 size)
        {
            dumpAddress = addr;
            try
            {
                dumpRegion = new byte[size];
                var ret = ReadProcessMemory(pHandle, dumpAddress, dumpRegion, (UIntPtr)size, IntPtr.Zero);
                return ret;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool MaskCheck(int nOffset, byte[] btPattern, string strMask)
        {
            // Loop the pattern and compare to the mask and dump.
            for (int x = 0; x < btPattern.Length; x++)
            {
                // If the mask char is a wildcard.
                if (strMask[x] == '?')
                {
                    if (btPattern[x] == 0xFF) //100% wildcard
                        continue;
                    else
                    { //50% wildcard
                        if (dumpRegion[nOffset + x].ToString("X").Length == 2) //byte must be 2 characters long
                        {
                            if (btPattern[x].ToString("X")[0] == '?') { //ex: ?5
                                if (dumpRegion[nOffset + x].ToString("X")[1] != btPattern[x].ToString("X")[1])
                                    return false;
                            }
                            else if (btPattern[x].ToString("X")[1] == '?') //ex: 5?
                            {
                                if (dumpRegion[nOffset + x].ToString("X")[0] != btPattern[x].ToString("X")[0])
                                    return false;
                            }
                        }
                    }
                }

                // If the mask char is not a wildcard, ensure a match is made in the pattern.
                if ((strMask[x] == 'x') && (btPattern[x] != dumpRegion[nOffset + x]))
                    return false;
            }

            // The loop was successful so we found 1 pattern match.
            return true;
        }

        public IntPtr FindPattern(byte[] btPattern, string strMask, int nOffset)
        {
            try
            {
                if (strMask.Length != btPattern.Length)
                    return IntPtr.Zero;
                
                for (int x = 0; x < dumpRegion.Length; x++)
                {
                    if (MaskCheck(x, btPattern, strMask))
                    {
                        return new IntPtr((int)dumpAddress + (x + nOffset));
                    }
                }
                return IntPtr.Zero;
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
        }
    }
}
