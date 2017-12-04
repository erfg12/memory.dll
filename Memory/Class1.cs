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
using System.Threading.Tasks;

namespace Memory
{
    /// <summary>
    /// Memory.dll class. Full documentation at https://github.com/erfg12/memory.dll/wiki
    /// </summary>
    public class Mem
    {
        #region DllImports
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(
            UInt32 dwDesiredAccess,
            bool bInheritHandle,
            Int32 dwProcessId
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION32 lpBuffer, int dwLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION64 lpBuffer, int dwLength);

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

        [DllImport("dbghelp.dll")]
        static extern bool MiniDumpWriteDump(
            IntPtr hProcess,
            Int32 ProcessId,
            IntPtr hFile,
            MINIDUMP_TYPE DumpType,
            IntPtr ExceptionParam,
            IntPtr UserStreamParam,
            IntPtr CallackParam);

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

        [DllImport("kernel32.dll")]
        static extern int GetProcessId(IntPtr handle);

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

        [DllImport("kernel32")]
        public static extern bool IsWow64Process(IntPtr hProcess, out bool lpSystemInfo);

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

        /// <summary>
        /// The process handle that was opened. (Use OpenProcess function to populate this variable)
        /// </summary>
        public IntPtr pHandle;

        public Process procs = null;
        public int procID = 0;
        public byte[] dumpBytes;

        internal enum MINIDUMP_TYPE
        {
            MiniDumpNormal = 0x00000000,
            MiniDumpWithDataSegs = 0x00000001,
            MiniDumpWithFullMemory = 0x00000002,
            MiniDumpWithHandleData = 0x00000004,
            MiniDumpFilterMemory = 0x00000008,
            MiniDumpScanMemory = 0x00000010,
            MiniDumpWithUnloadedModules = 0x00000020,
            MiniDumpWithIndirectlyReferencedMemory = 0x00000040,
            MiniDumpFilterModulePaths = 0x00000080,
            MiniDumpWithProcessThreadData = 0x00000100,
            MiniDumpWithPrivateReadWriteMemory = 0x00000200,
            MiniDumpWithoutOptionalData = 0x00000400,
            MiniDumpWithFullMemoryInfo = 0x00000800,
            MiniDumpWithThreadInfo = 0x00001000,
            MiniDumpWithCodeSegs = 0x00002000
        }

        /// <summary>
        /// Open the PC game process with all security and access rights.
        /// </summary>
        /// <param name="id">You can use the getProcIDFromName function to get this.</param>
        /// <returns></returns>
        public bool OpenProcess(int id)
        {
            if (isAdmin() == false)
            {
                Debug.Write("WARNING: You are NOT running this program as admin!! Visit https://github.com/erfg12/memory.dll/wiki/Administrative-Privileges");
                MessageBox.Show("WARNING: You are NOT running this program as admin!!");
            } else
                Debug.Write("Program is operating at Administrative level. Now opening process id #" + id + "." + Environment.NewLine);

            try
            {
                Process.EnterDebugMode();
                if (id != 0)
                { //getProcIDFromName returns 0 if there was a problem
                    procs = Process.GetProcessById(id);
                    procID = id;
                }
                else
                    return false;

                if (procs.Responding == false)
                    return false;

                pHandle = OpenProcess(0x1F0FFF, true, id);

                if (pHandle == IntPtr.Zero)
                {
                    var eCode = Marshal.GetLastWin32Error();
                }

                Debug.Write("Now storing module addresses for process id #" + id + "." + Environment.NewLine);

                mainModule = procs.MainModule;
                getModules();
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Check if program is running with administrative privileges. Read about it here: https://github.com/erfg12/memory.dll/wiki/Administrative-Privileges
        /// </summary>
        /// <returns></returns>
        public bool isAdmin()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        /// <summary>
        /// Check if opened process is 64bit. Used primarily for getCode().
        /// </summary>
        /// <returns></returns>
        public bool is64bit()
        {
            bool is32bit = false;
            IsWow64Process(pHandle, out is32bit);

            if (is32bit)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Builds the process modules dictionary (names with addresses).
        /// </summary>
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

        /// <summary>
        /// Get the process ID number by process name.
        /// </summary>
        /// <param name="name">Example: "eqgame". Use task manager to find the name. Do not include .exe</param>
        /// <returns></returns>
        public int getProcIDFromName(string name) //new 1.0.2 function
        {
            Process[] processlist = Process.GetProcesses();

            if (name.Contains(".exe"))
                name = name.Replace(".exe", "");

            foreach (Process theprocess in processlist)
            {
                if (theprocess.ProcessName.Equals(name)) //find (name).exe in the process list (use task manager to find the name)
                    return theprocess.Id;
            }

            return 0; //if we fail to find it
        }

        /// <summary>
        /// Get code from ini file.
        /// </summary>
        /// <param name="name">label for address or code</param>
        /// <param name="file">path and name of ini file</param>
        /// <returns></returns>
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

        private int LoadIntCode(string name, string path)
        {
            int intValue = Convert.ToInt32(LoadCode(name, path), 16);
            if (intValue >= 0)
                return intValue;
            else
                return 0;
        }

        /// <summary>
        /// Dictionary with our opened process module names with addresses.
        /// </summary>
        public Dictionary<string, IntPtr> modules = new Dictionary<string, IntPtr>();

        /// <summary>
        /// Make a named pipe (if not already made) and call to a remote function.
        /// </summary>
        /// <param name="func">remote function to call</param>
        /// <param name="name">name of the thread</param>
        public void ThreadStartClient(string func, string name)
        {
            //ManualResetEvent SyncClientServer = (ManualResetEvent)obj;
            using (NamedPipeClientStream pipeStream = new NamedPipeClientStream(name))
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

        /*private UIntPtr LoadUIntPtrCode(string name, string path = "")
        {
            string theCode = LoadCode(name, path);

            if (!theCode.Contains("+")) return (UIntPtr)(uint)Convert.ToInt32(theCode); //just return the code

            UIntPtr uintValue;

            string newOffset = theCode.Substring(theCode.IndexOf('+') + 1);

            if (String.IsNullOrEmpty(newOffset))
                return (UIntPtr)0;

            int intToUint = 0;

            if (Convert.ToInt32(newOffset, 16) > 0)
                intToUint = Convert.ToInt32(newOffset, 16);

            if (theCode.Contains("base") || theCode.Contains("main"))
                uintValue = (UIntPtr)((Int64)procs.MainModule.BaseAddress + intToUint);
            else if (!theCode.Contains("base") && !theCode.Contains("main") && theCode.Contains("+"))
            {
                string[] moduleName = theCode.Split('+');

                if (modules.Count == 0 || !modules.ContainsKey(moduleName[0]))
                    getModules();

                if (modules.ContainsKey(moduleName[0]))
                {
                    IntPtr altModule = modules[moduleName[0]];
                    uintValue = (UIntPtr)((Int64)altModule + intToUint);
                }
                else
                {
                    Debug.WriteLine("ERROR! Module " + moduleName[0] + " not found! Visit https://github.com/erfg12/memory.dll/wiki/List-Modules");
                    MessageBox.Show("ERROR! Module " + moduleName[0] + " not found!");
                    return (UIntPtr)0;
                }
            }
            else
                uintValue = (UIntPtr)intToUint;

            return (UIntPtr)uintValue;
        }*/

        /// <summary>
        /// Cut a string that goes on for too long or one that is possibly merged with another string.
        /// </summary>
        /// <param name="str">The string you want to cut.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Clean up a string that has bad characters in it.
        /// </summary>
        /// <param name="str">The string you want to sanitize.</param>
        /// <returns></returns>
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
        /// <summary>
        /// Read a float value from an address.
        /// </summary>
        /// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        /// <param name="file">path and name of ini file. (OPTIONAL)</param>
        /// <returns></returns>
        public float readFloat(string code, string file = "")
        {
            byte[] memory = new byte[4];

            UIntPtr theCode;
            //if (!LoadCode(code, file).Contains(","))
            //    theCode = LoadUIntPtrCode(code, file);
            //else
                theCode = getCode(code, file);
            //Debug.Write(theCode + Environment.NewLine);
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

        /// <summary>
        /// Read a string value from an address.
        /// </summary>
        /// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        /// <param name="file">path and name of ini file. (OPTIONAL)</param>
        /// <returns></returns>
        public string readString(string code, string file = "")
        {
            //Debug.Write("readString:" + code + ", FILE:" + file + Environment.NewLine);
            byte[] memoryNormal = new byte[32];
            UIntPtr theCode;
            //theCode = getCode(code, file);
           // if (!LoadCode(code, file).Contains(","))
            //    theCode = LoadUIntPtrCode(code, file);
            //else
                theCode = getCode(code, file);
            //Debug.WriteLine(theCode);
            if (ReadProcessMemory(pHandle, theCode, memoryNormal, (UIntPtr)32, IntPtr.Zero))
                return Encoding.UTF8.GetString(memoryNormal);
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

        /// <summary>
        /// Read an integer from an address.
        /// </summary>
        /// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        /// <param name="file">path and name of ini file. (OPTIONAL)</param>
        /// <returns></returns>
        public int readInt(string code, string file = "")
        {
            byte[] memory = new byte[4];
            UIntPtr theCode;
            //Debug.Write("readString:" + code + ", FILE:" + file + Environment.NewLine);
            //if (!LoadCode(code, file).Contains(","))
            //    theCode = LoadUIntPtrCode(code, file);
            //else
            theCode = getCode(code, file);
            //Debug.WriteLine(theCode);
            if (ReadProcessMemory(pHandle, theCode, memory, (UIntPtr)4, IntPtr.Zero))
                return BitConverter.ToInt32(memory, 0);
            else
                return 0;
        }

        /// <summary>
        /// Read a long value from an address.
        /// </summary>
        /// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        /// <param name="file">path and name of ini file. (OPTIONAL)</param>
        /// <returns></returns>
        public long readLong(string code, string file = "")
        {
            byte[] memory = new byte[16];
            UIntPtr theCode;

            //if (!LoadCode(code, file).Contains(","))
            //    theCode = LoadUIntPtrCode(code, file);
            //else
                theCode = getCode(code, file);

            if (ReadProcessMemory(pHandle, theCode, memory, (UIntPtr)16, IntPtr.Zero))
                return BitConverter.ToInt64(memory, 0);
            else
                return 0;
        }

        /// <summary>
        /// Read a UInt value from address.
        /// </summary>
        /// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        /// <param name="file">path and name of ini file. (OPTIONAL)</param>
        /// <returns></returns>
        public UInt64 readUInt(string code, string file = "")
        {
            byte[] memory = new byte[4];
            UIntPtr theCode;
            //if (!LoadCode(code, file).Contains(","))
            //    theCode = LoadUIntPtrCode(code, file);
            //else
                theCode = getCode(code, file);

            if (ReadProcessMemory(pHandle, theCode, memory, (UIntPtr)4, IntPtr.Zero))
                return BitConverter.ToUInt64(memory, 0);
            else
                return 0;
        }

        /// <summary>
        /// Reads a 2 byte value from an address and moves the address.
        /// </summary>
        /// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        /// <param name="moveQty">Quantity to move.</param>
        /// <param name="file">path and name of ini file (OPTIONAL)</param>
        /// <returns></returns>
        public int read2ByteMove(string code, int moveQty, string file = "")
        {
            byte[] memory = new byte[4];
            UIntPtr theCode;
            //if (!LoadCode(code, file).Contains(","))
            //    theCode = LoadUIntPtrCode(code, file);
            //else
                theCode = getCode(code, file);

            UIntPtr newCode = UIntPtr.Add(theCode, moveQty);

            if (ReadProcessMemory(pHandle, newCode, memory, (UIntPtr)2, IntPtr.Zero))
                return BitConverter.ToInt32(memory, 0);
            else
                return 0;
        }

        /// <summary>
        /// Reads an integer value from address and moves the address.
        /// </summary>
        /// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        /// <param name="moveQty">Quantity to move.</param>
        /// <param name="file">path and name of ini file (OPTIONAL)</param>
        /// <returns></returns>
        public int readIntMove(string code, int moveQty, string file = "")
        {
            byte[] memory = new byte[4];
            UIntPtr theCode;
            //if (!LoadCode(code, file).Contains(","))
            //    theCode = LoadUIntPtrCode(code, file);
            //else
                theCode = getCode(code, file);

            UIntPtr newCode = UIntPtr.Add(theCode, moveQty);

            if (ReadProcessMemory(pHandle, newCode, memory, (UIntPtr)4, IntPtr.Zero))
                return BitConverter.ToInt32(memory, 0);
            else
                return 0;
        }

        /// <summary>
        /// Get UInt and move to another address by moveQty. Use in a for loop.
        /// </summary>
        /// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        /// <param name="moveQty">Quantity to move.</param>
        /// <param name="file">path and name of ini file (OPTIONAL)</param>
        /// <returns></returns>
        public ulong readUIntMove(string code, int moveQty, string file = "")
        {
            byte[] memory = new byte[8];
            UIntPtr theCode;
            //if (!LoadCode(code, file).Contains(","))
            //    theCode = LoadUIntPtrCode(code, file);
            //else
                theCode = getCode(code, file, 8);

            UIntPtr newCode = UIntPtr.Add(theCode, moveQty);

            if (ReadProcessMemory(pHandle, newCode, memory, (UIntPtr)8, IntPtr.Zero))
                return BitConverter.ToUInt64(memory, 0);
            else
                return 0;
        }

        /// <summary>
        /// Read a 2 byte value from an address. Returns an integer.
        /// </summary>
        /// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        /// <param name="file">path and file name to ini file. (OPTIONAL)</param>
        /// <returns></returns>
        public int read2Byte(string code, string file = "")
        {
            byte[] memoryTiny = new byte[4];

            UIntPtr theCode;
            //if (!LoadCode(code, file).Contains(","))
            //    theCode = LoadUIntPtrCode(code, file);
            //else
                theCode = getCode(code, file);

            if (ReadProcessMemory(pHandle, theCode, memoryTiny, (UIntPtr)2, IntPtr.Zero))
                return BitConverter.ToInt32(memoryTiny, 0);
            else
                return 0;
        }

        /// <summary>
        /// Read 1 byte from address.
        /// </summary>
        /// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        /// <param name="file">path and file name of ini file. (OPTIONAL)</param>
        /// <returns></returns>
        public int readByte(string code, string file = "")
        {
            byte[] memoryTiny = new byte[4];

            UIntPtr theCode;
            //if (!LoadCode(code, file).Contains(","))
            //    theCode = LoadUIntPtrCode(code, file);
            //else
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
        ///<summary>
        ///Write to memory address. See https://github.com/erfg12/memory.dll/wiki/writeMemory() for more information.
        ///</summary>
        ///<param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        ///<param name="type">byte, bytes, float, int, string or long.</param>
        ///<param name="write">value to write to address.</param>
        ///<param name="file">path and name of .ini file (OPTIONAL)</param>
        public bool writeMemory(string code, string type, string write, string file = "")
        {
            byte[] memory = new byte[4];
            int size = 4;

            UIntPtr theCode;
            //if (!LoadCode(code, file).Contains(","))
            //    theCode = LoadUIntPtrCode(code, file);
            //else
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
                memory[0] = Convert.ToByte(write, 16);
                size = 1;
            }
            else if (type == "bytes")
            {
                if (write.Contains(",") || write.Contains(" "))
                {
                    string[] stringBytes;
                    if (write.Contains(","))
                        stringBytes = write.Split(',');
                    else
                        stringBytes = write.Split(' ');
                    //Debug.WriteLine("write:" + write + " stringBytes:" + stringBytes);

                    int c = stringBytes.Count();
                    memory = new byte[c];
                    for (int i = 0; i < c; i++)
                    {
                        memory[i] = Convert.ToByte(stringBytes[i], 16);
                    }
                    size = stringBytes.Count();
                }
                else //somehow we wrote 1 byte instead of a byte array
                {
                    memory = new byte[1];
                    memory[0] = Convert.ToByte(write, 16);
                    size = 1;
                }
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
            //Debug.Write("DEBUG: Writing bytes [TYPE:" + type + " ADDR:" + theCode + "] " + String.Join(",", memory) + Environment.NewLine);
            if (WriteProcessMemory(pHandle, theCode, memory, (UIntPtr)size, IntPtr.Zero))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Write to address and move by moveQty. Good for byte arrays. See https://github.com/erfg12/memory.dll/wiki/Writing-a-Byte-Array for more information.
        /// </summary>
        ///<param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        ///<param name="type">byte, bytes, float, int, string or long.</param>
        /// <param name="write">byte to write</param>
        /// <param name="moveQty">quantity to move</param>
        /// <param name="file">path and name of .ini file (OPTIONAL)</param>
        /// <returns></returns>
        public bool writeMove(string code, string type, string write, int moveQty, string file = "") //version v1.0.3
        {
            byte[] memory = new byte[4];
            int size = 4;

            UIntPtr theCode;
            //if (!LoadCode(code, file).Contains(","))
            //    theCode = LoadUIntPtrCode(code, file);
            //else
                theCode = getCode(code, file);

            if (type == "float")
            {
                memory = new byte[write.Length];
                memory = BitConverter.GetBytes(Convert.ToSingle(write));
                size = write.Length;
            }
            else if (type == "int")
            {
                memory = BitConverter.GetBytes(Convert.ToInt32(write));
                size = 4;
            }
            else if (type == "byte")
            {
                memory = new byte[1];
                memory[0] = Convert.ToByte(write, 16);
                size = 1;
            }
            else if (type == "string")
            {
                memory = new byte[write.Length];
                memory = System.Text.Encoding.UTF8.GetBytes(write);
                size = write.Length;
            }

            UIntPtr newCode = UIntPtr.Add(theCode, moveQty);

            Debug.Write("DEBUG: Writing bytes [TYPE:" + type + " ADDR:[O]" + theCode + " [N]" + newCode + " MQTY:" + moveQty + "] " + String.Join(",", memory) + Environment.NewLine);
            Thread.Sleep(1000);
            if (WriteProcessMemory(pHandle, newCode, memory, (UIntPtr)size, IntPtr.Zero))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Write byte array to addresses.
        /// </summary>
        /// <param name="code">address to write to</param>
        /// <param name="write">byte array to write</param>
        /// <param name="file">path and name of ini file. (OPTIONAL)</param>
        public void writeBytes(string code, byte[] write, string file = "")
        {
            UIntPtr theCode;
            //if (!LoadCode(code, file).Contains(","))
            //    theCode = LoadUIntPtrCode(code, file);
            //else
                theCode = getCode(code, file);
            WriteProcessMemory(pHandle, theCode, write, (UIntPtr)write.Length, IntPtr.Zero);
        }
        #endregion

        /// <summary>
        /// Convert code from string to real address. If path is not blank, will pull from ini file.
        /// </summary>
        /// <param name="name">label in ini file</param>
        /// <param name="path">path to ini file</param>
        /// <param name="size">size of address (default is 8)</param>
        /// <returns></returns>
        private UIntPtr getCode(string name, string path, int size = 8)
        {
            if (is64bit())
            {
                //Debug.WriteLine("Changing to 64bit code...");
                if (size == 8) size = 16; //change to 64bit
                return get64bitCode(name, path, size); //jump over to 64bit code grab
            }

            string theCode = LoadCode(name, path);

            if (theCode == "")
            {
                //Debug.WriteLine("ERROR: LoadCode returned blank. NAME:" + name + " PATH:" + path);
                return UIntPtr.Zero;
            } else
            {
                //Debug.WriteLine("Found code=" + theCode + " NAME:" + name + " PATH:" + path);
            }

            if (!theCode.Contains("+") && !theCode.Contains(",")) return new UIntPtr(Convert.ToUInt32(theCode, 16));

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
                    string test = oldOffsets;
                    if (oldOffsets.Contains("0x")) test = oldOffsets.Replace("0x","");
                    offsetsList.Add(Int32.Parse(test, NumberStyles.HexNumber));
                }
                int[] offsets = offsetsList.ToArray();

                if (theCode.Contains("base") || theCode.Contains("main"))
                    ReadProcessMemory(pHandle, (UIntPtr)((int)mainModule.BaseAddress + offsets[0]), memoryAddress, (UIntPtr)size, IntPtr.Zero);
                else if (!theCode.Contains("base") && !theCode.Contains("main") && theCode.Contains("+"))
                {
                    string[] moduleName = theCode.Split('+');
                    IntPtr altModule = IntPtr.Zero;
                    if (!moduleName[0].Contains(".dll") && !moduleName[0].Contains(".exe"))
                    {
                        string theAddr = moduleName[0];
                        if (theAddr.Contains("0x")) theAddr = theAddr.Replace("0x", "");
                        altModule = (IntPtr)Int32.Parse(theAddr, NumberStyles.HexNumber);
                    }
                    else
                        altModule = modules[moduleName[0]];
                    ReadProcessMemory(pHandle, (UIntPtr)((int)altModule + offsets[0]), memoryAddress, (UIntPtr)size, IntPtr.Zero);
                }
                else
                    ReadProcessMemory(pHandle, (UIntPtr)(offsets[0]), memoryAddress, (UIntPtr)size, IntPtr.Zero);

                uint num1 = BitConverter.ToUInt32(memoryAddress, 0); //ToUInt64 causes arithmetic overflow.

                UIntPtr base1 = (UIntPtr)0;

                for (int i = 1; i < offsets.Length; i++)
                {
                    base1 = new UIntPtr(num1 + Convert.ToUInt32(offsets[i]));
                    ReadProcessMemory(pHandle, base1, memoryAddress, (UIntPtr)size, IntPtr.Zero);
                    num1 = BitConverter.ToUInt32(memoryAddress, 0); //ToUInt64 causes arithmetic overflow.
                }
                return base1;
            }
            else
            {
                int trueCode = Convert.ToInt32(newOffsets, 16);
                IntPtr altModule = IntPtr.Zero;
                //Debug.WriteLine("newOffsets=" + newOffsets);
                if (theCode.Contains("base") || theCode.Contains("main"))
                    altModule = mainModule.BaseAddress;
                else if (!theCode.Contains("base") && !theCode.Contains("main") && theCode.Contains("+"))
                {
                    string[] moduleName = theCode.Split('+');
                    if (!moduleName[0].Contains(".dll") && !moduleName[0].Contains(".exe"))
                    {
                        string theAddr = moduleName[0];
                        if (theAddr.Contains("0x")) theAddr = theAddr.Replace("0x", "");
                        altModule = (IntPtr)Int32.Parse(theAddr, NumberStyles.HexNumber);
                    }
                    else
                        altModule = modules[moduleName[0]];
                }
                else
                    altModule = modules[theCode.Split('+')[0]];
                return (UIntPtr)((int)altModule + trueCode);
            }
        }

        /// <summary>
        /// Convert code from string to real address. If path is not blank, will pull from ini file.
        /// </summary>
        /// <param name="name">label in ini file</param>
        /// <param name="path">path to ini file</param>
        /// <param name="size">size of address (default is 16)</param>
        /// <returns></returns>
        private UIntPtr get64bitCode(string name, string path, int size = 16)
        {
            string theCode = LoadCode(name, path);
            if (theCode == "")
                return UIntPtr.Zero;
            string newOffsets = theCode;
            if (theCode.Contains("+"))
                newOffsets = theCode.Substring(theCode.IndexOf('+') + 1);

            byte[] memoryAddress = new byte[size];

            if (!theCode.Contains("+") && !theCode.Contains(",")) return new UIntPtr(Convert.ToUInt64(theCode, 16));

            if (newOffsets.Contains(','))
            {
                List<Int64> offsetsList = new List<Int64>();

                string[] newerOffsets = newOffsets.Split(',');
                foreach (string oldOffsets in newerOffsets)
                {
                    string test = oldOffsets;
                    if (oldOffsets.Contains("0x")) test = oldOffsets.Replace("0x", "");
                    offsetsList.Add(Int64.Parse(test, System.Globalization.NumberStyles.HexNumber));
                }
                Int64[] offsets = offsetsList.ToArray();

                if (theCode.Contains("base") || theCode.Contains("main"))
                    ReadProcessMemory(pHandle, (UIntPtr)((Int64)mainModule.BaseAddress + offsets[0]), memoryAddress, (UIntPtr)size, IntPtr.Zero);
                else if (!theCode.Contains("base") && !theCode.Contains("main") && theCode.Contains("+"))
                {
                    string[] moduleName = theCode.Split('+');
                    IntPtr altModule = IntPtr.Zero;
                    if (!moduleName[0].Contains(".dll") && !moduleName[0].Contains(".exe"))
                        altModule = (IntPtr)Int64.Parse(moduleName[0], System.Globalization.NumberStyles.HexNumber);
                    else
                        altModule = modules[moduleName[0]];
                    ReadProcessMemory(pHandle, (UIntPtr)((Int64)altModule + offsets[0]), memoryAddress, (UIntPtr)size, IntPtr.Zero);
                }
                else
                    ReadProcessMemory(pHandle, (UIntPtr)(offsets[0]), memoryAddress, (UIntPtr)size, IntPtr.Zero);

                UInt64 num1 = BitConverter.ToUInt64(memoryAddress, 0);

                UIntPtr base1 = (UIntPtr)0;

                for (int i = 1; i < offsets.Length; i++)
                {
                    base1 = new UIntPtr(num1 + Convert.ToUInt64(offsets[i]));
                    ReadProcessMemory(pHandle, base1, memoryAddress, (UIntPtr)size, IntPtr.Zero);
                    num1 = BitConverter.ToUInt64(memoryAddress, 0);
                }
                return base1;
            }
            else
            {
                Int64 trueCode = Convert.ToInt64(newOffsets, 16);
                IntPtr altModule = IntPtr.Zero;
                //Debug.WriteLine("newOffsets=" + newOffsets);
                if (theCode.Contains("base") || theCode.Contains("main"))
                    altModule = mainModule.BaseAddress;
                else if (!theCode.Contains("base") && !theCode.Contains("main") && theCode.Contains("+"))
                {
                    string[] moduleName = theCode.Split('+');
                    if (!moduleName[0].Contains(".dll") && !moduleName[0].Contains(".exe"))
                    {
                        string theAddr = moduleName[0];
                        if (theAddr.Contains("0x")) theAddr = theAddr.Replace("0x", "");
                        altModule = (IntPtr)Int64.Parse(theAddr, NumberStyles.HexNumber);
                    }
                    else
                        altModule = modules[moduleName[0]];
                }
                else
                    altModule = modules[theCode.Split('+')[0]];
                return (UIntPtr)((Int64)altModule + trueCode);
            }
            /*else
            {
                Int64 trueCode = Convert.ToInt64(newOffsets, 16);

                if (theCode.Contains("base") || theCode.Contains("main"))
                    ReadProcessMemory(pHandle, (UIntPtr)((Int64)mainModule.BaseAddress + trueCode), memoryAddress, (UIntPtr)size, IntPtr.Zero);
                else if (!theCode.Contains("base") && !theCode.Contains("main") && theCode.Contains("+"))
                {
                    string[] moduleName = theCode.Split('+');
                    IntPtr altModule = modules[moduleName[0]];
                    ReadProcessMemory(pHandle, (UIntPtr)((Int64)altModule + trueCode), memoryAddress, (UIntPtr)size, IntPtr.Zero);
                }
                else
                    ReadProcessMemory(pHandle, (UIntPtr)(trueCode), memoryAddress, (UIntPtr)size, IntPtr.Zero);

                UInt64 num1 = BitConverter.ToUInt64(memoryAddress, 0);

                UIntPtr base1 = new UIntPtr(num1);
                num1 = BitConverter.ToUInt64(memoryAddress, 0);
                return base1;
            }*/
        }

        /// <summary>
        /// Close the process when finished.
        /// </summary>
        public void closeProcess()
        {
            CloseHandle(pHandle);
        }

        /// <summary>
        /// Inject a DLL file.
        /// </summary>
        /// <param name="strDLLName">path and name of DLL file.</param>
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

            int LenWrite = strDLLName.Length + 1;
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

        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        public static void SuspendProcess(int pid)
        {
            var process = Process.GetProcessById(pid);

            if (process.ProcessName == string.Empty)
                return;

            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);
                if (pOpenThread == IntPtr.Zero)
                    continue;

                SuspendThread(pOpenThread);
                CloseHandle(pOpenThread);
            }
        }

        public static void ResumeProcess(int pid)
        {
            var process = Process.GetProcessById(pid);
            if (process.ProcessName == string.Empty)
                return;

            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);
                if (pOpenThread == IntPtr.Zero)
                    continue;

                var suspendCount = 0;
                do
                {
                    suspendCount = ResumeThread(pOpenThread);
                } while (suspendCount > 0);
                CloseHandle(pOpenThread);
            }
        }

        async Task PutTaskDelay(int delay)
        {
            await Task.Delay(delay);
        }

        void AppendAllBytes(string path, byte[] bytes)
        {
            using (var stream = new FileStream(path, FileMode.Append))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        public byte[] fileToBytes(string path, bool dontDelete = false) {
            byte[] newArray = File.ReadAllBytes(path);
            if (dontDelete == false)
                File.Delete(path);
            return newArray;
        }

        private bool MaskCheck(Int64 nOffset, string[] btPattern, string strMask, byte[] dumpRegion)
        {
            if (cts.IsCancellationRequested) return false;

            for (int x = 0; x < btPattern.Length; x++)
            {
                if (cts.IsCancellationRequested) break;

                if ((nOffset + x) >= dumpRegion.Length || x >= btPattern.Length || x >= strMask.Length) return false;

                string theCode = btPattern[x].ToUpper();
                string theCode2 = dumpRegion[nOffset + x].ToString("x2").ToUpper();
                // If the mask char is a wildcard.
                if (strMask[x] == '?')
                {
                    if (theCode == "??")
                        continue;
                    else
                    { //50% wildcard
                        if (theCode[0].Equals('?'))
                        { //ex: ?5
                            if (!theCode2[1].Equals(theCode[1]))
                                return false;
                        }
                        else if (theCode[1].Equals('?'))
                        { //ex: 5?
                            if (!theCode2[0].Equals(theCode[0]))
                                return false;
                        }
                    }
                }

                if (strMask[x] == 'x')
                    if (!theCode.Equals(theCode2)) return false;
            }
            return true;
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
            {
                hex.AppendFormat("{0:x2} ", b);
            }
            return hex.ToString();
        }

        /// <summary>
        /// Convert a byte array to hex values in a string.
        /// </summary>
        /// <param name="ba">your byte array to convert</param>
        /// <returns></returns>
        public static string ByteArrayToHexString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            int i = 1;
            foreach (byte b in ba)
            {
                if (i == 16)
                {
                    hex.AppendFormat("{0:x2}{1}", b, Environment.NewLine);
                    i = 0;
                }
                else
                    hex.AppendFormat("{0:x2} ", b);
                i++;
            }
            return hex.ToString().ToUpper();
        }

        public async Task<Int64> PageFindPattern(byte[] haystack, string[] needle, string strMask, Int64 start = 0) //for pages
        {
            //try
            //{
                if (cts.IsCancellationRequested) return 0;
                //if ( haystack.All(singleByte => singleByte == 0xFF) || haystack.All(singleByte => singleByte == 0x00))
                //    return 0;
                //Debug.Write("DEBUG: PageFindPattern starting at 0x" + start.ToString("x8") + " with length 0x" + haystack.Length.ToString("x8") + Environment.NewLine);

                //Debug.Write(ByteArrayToString(haystack));
                for (int x = 0; x < haystack.Length; x++)
                {
                    if (cts.IsCancellationRequested) break;
                    //Debug.Write("PageFindPattern now at 0x" + (start + x).ToString("x8") + Environment.NewLine);
                    if (MaskCheck(x, needle, strMask, haystack))
                    {
                        Debug.Write("[DEBUG] FOUND ADDRESS:0x" + (x + start).ToString("x8") + " start:0x" + start.ToString("x8") + " x:" + x.ToString("x8") + " base:0x" + procs.MainModule.BaseAddress.ToString("x8") + Environment.NewLine);
                        return (x + start);
                    }
                }
            /*} catch
            {
                Debug.Write("PageFindPattern failed");
            }*/
            return 0;// IntPtr.Zero;
        }

        public Int64 FindPattern(byte[] haystack, string[] needle, string strMask, Int64 start = 0, Int64 length = 0)
        {
            //Debug.Write("[DEBUG] FindPattern starting... (" + DateTime.Now.ToString("h:mm:ss tt") + ")" + Environment.NewLine);

            //Debug.Write("[DEBUG] starting AoB scan at 0x" + start.ToString("x8") + " and going 0x" + length.ToString("x8") + " length. (" + start.ToString("x8") + "-" + (start + length).ToString("x8") + ")" + Environment.NewLine);
            //Debug.Write("[DEBUG] AoB mask is " + strMask + Environment.NewLine);
            //Debug.Write("[DEBUG] AoB pattern is " + ByteArrayToString(needle) + Environment.NewLine);
            //Debug.Write("[DEBUG] haystack size is 0x" + haystack.Length.ToString("x8") + Environment.NewLine);

            if (start > 0)
                start = start - (Int64)procs.MainModule.BaseAddress;
            if (length > 0)
                length = length - (Int64)procs.MainModule.BaseAddress;

            if (length == 0)
                length = (haystack.Length - start);

            //Debug.Write("[DEBUG] searching dump file start:0x" + start.ToString("x8") + " end:0x" + (start + length).ToString("x8") + ". Dump starts at 0x" + procs.MainModule.BaseAddress + Environment.NewLine);
            //Debug.Write("Searching for AoB pattern, please wait..." + Environment.NewLine);
            for (Int64 x = start; x < (start + length); x++)
            {
                if (MaskCheck(x, needle, strMask, haystack))
                {
                    //string total = (x + diff).ToString("x8");
                    //Debug.Write("[DEBUG] base address is " + procs.MainModule.BaseAddress.ToString("x8") + " and resulting offset is " + x.ToString("x8") + " min address is " + getMinAddress().ToString("x8") + Environment.NewLine);
                    //ResumeProcess(procID);
                    //Debug.Write("[DEBUG] FindPattern ended " + DateTime.Now.ToString("h:mm:ss tt"));
                    return (x + (Int64)procs.MainModule.BaseAddress);
                }
                //Debug.Write("[DEBUG] FindPattern searching " + x.ToString("x8") + Environment.NewLine);
            }
            //Debug.Write("[DEBUG] FindPattern ended " + DateTime.Now.ToString("h:mm:ss tt") + Environment.NewLine);
            return 0;
        }
        public struct SYSTEM_INFO
        {
            public ushort processorArchitecture;
            ushort reserved;
            public uint pageSize;
            public IntPtr minimumApplicationAddress;
            public IntPtr maximumApplicationAddress;
            public IntPtr activeProcessorMask;
            public uint numberOfProcessors;
            public uint processorType;
            public uint allocationGranularity;
            public ushort processorLevel;
            public ushort processorRevision;
        }

        public struct MEMORY_BASIC_INFORMATION32
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        public struct MEMORY_BASIC_INFORMATION64 {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public UInt32 __alignment1;
            public long RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
            public UInt32 __alignment2;
        }


        public ulong getMinAddress()
        {
            SYSTEM_INFO SI;
            GetSystemInfo(out SI);
            return (ulong)SI.minimumApplicationAddress;
        }

        int diff = 0;

        /// <summary>
        /// Dump memory page by page to a dump.dmp file. Can be used with Cheat Engine.
        /// </summary>
        public bool DumpMemory(string file = "dump.dmp")
        {
            Debug.Write("[DEBUG] memory dump starting... (" + DateTime.Now.ToString("h:mm:ss tt") + ")" + Environment.NewLine);
            SYSTEM_INFO sys_info = new SYSTEM_INFO();
            GetSystemInfo(out sys_info);

            IntPtr proc_min_address = sys_info.minimumApplicationAddress;
            IntPtr proc_max_address = sys_info.maximumApplicationAddress;

            // saving the values as long ints so I won't have to do a lot of casts later
            Int64 proc_min_address_l = (Int64)procs.MainModule.BaseAddress;
            Int64 proc_max_address_l = (Int64)procs.VirtualMemorySize64 + proc_min_address_l;
            
            //int arrLength = 0;
            if (File.Exists(file))
                File.Delete(file);

            if (is64bit())
            {
                MEMORY_BASIC_INFORMATION64 mem_basic_info64 = new MEMORY_BASIC_INFORMATION64();
                while (proc_min_address_l < proc_max_address_l)
                {
                    VirtualQueryEx(pHandle, proc_min_address, out mem_basic_info64, Marshal.SizeOf(mem_basic_info64));
                    byte[] buffer = new byte[(Int64)mem_basic_info64.RegionSize];
                    UIntPtr test = (UIntPtr)((Int64)mem_basic_info64.RegionSize);
                    UIntPtr test2 = (UIntPtr)((Int64)mem_basic_info64.BaseAddress);

                    ReadProcessMemory(pHandle, test2, buffer, test, IntPtr.Zero);

                    AppendAllBytes(file, buffer); //due to memory limits, we have to dump it then store it in an array.
                                                         //arrLength += buffer.Length;

                    proc_min_address_l += (Int64)mem_basic_info64.RegionSize;
                    proc_min_address = new IntPtr(proc_min_address_l);
                }
            }
            else
            {
                MEMORY_BASIC_INFORMATION32 mem_basic_info32 = new MEMORY_BASIC_INFORMATION32();
                while (proc_min_address_l < proc_max_address_l)
                {
                    VirtualQueryEx(pHandle, proc_min_address, out mem_basic_info32, Marshal.SizeOf(mem_basic_info32));
                    byte[] buffer = new byte[(Int64)mem_basic_info32.RegionSize];
                    UIntPtr test = (UIntPtr)((Int64)mem_basic_info32.RegionSize);
                    UIntPtr test2 = (UIntPtr)((Int64)mem_basic_info32.BaseAddress);

                    ReadProcessMemory(pHandle, test2, buffer, test, IntPtr.Zero);

                    AppendAllBytes(file, buffer); //due to memory limits, we have to dump it then store it in an array.
                                                         //arrLength += buffer.Length;

                    proc_min_address_l += (Int64)mem_basic_info32.RegionSize;
                    proc_min_address = new IntPtr(proc_min_address_l);
                }
            }
            Debug.Write("[DEBUG] memory dump completed. Saving dump file to " + file + ". (" + DateTime.Now.ToString("h:mm:ss tt") + ")" + Environment.NewLine);
            return true;
        }

        CancellationTokenSource cts = new CancellationTokenSource();

        /// <summary>
        /// Array of Byte scan. Returns first address found.
        /// </summary>
        /// <param name="start">Your starting address.</param>
        /// <param name="length">Length to scan.</param>
        /// <param name="search">array of bytes to search for, OR your ini code label.</param>
        /// <param name="file">ini file (OPTIONAL)</param>
        /// <returns></returns>
        public async Task<Int64> AoBScan(string start, Int64 length, string search, string file = "")
        {
            Int64 ar = 0;
            var list = new List<string>();

            UIntPtr gCode;
            //if (!LoadCode(start, file).Contains(","))
            //    gCode = LoadUIntPtrCode(start, file);
            //else
                gCode = getCode(start, file);

            Int64 theCode = (Int64)gCode;

            string memCode = LoadCode(search, file);

            string[] stringByteArray = memCode.Split(' ');
            string mask = "";
            int i = 0;
            foreach (string ba in stringByteArray)
            {
                if (ba == "??")
                    mask += "?";
                else if (Char.IsLetterOrDigit(ba[0]) && ba[1] == '?')
                    mask += "?";
                else if (Char.IsLetterOrDigit(ba[1]) && ba[0] == '?')
                    mask += "?";
                else
                    mask += "x";
                i++;
            }

            SYSTEM_INFO sys_info = new SYSTEM_INFO();
            GetSystemInfo(out sys_info);

            IntPtr proc_min_address = sys_info.minimumApplicationAddress;
            IntPtr proc_max_address = sys_info.maximumApplicationAddress;

            Int64 proc_min_address_l = (Int64)procs.MainModule.BaseAddress;
            Int64 proc_max_address_l = (Int64)procs.VirtualMemorySize64 + proc_min_address_l;

            /*if (start < proc_min_address_l) { //if our start is too low, we should adjust it to main module base address
                start = proc_min_address_l;
                Debug.Write("AoB scan cannot start at less than main module base address, changing to " + proc_min_address_l + "." + Environment.NewLine);
            }*/
            
            Debug.Write("[DEBUG] memory scan starting... (base:0x" + proc_min_address_l.ToString("x16") + " max:" + proc_max_address_l.ToString("x16") + " time:" + DateTime.Now.ToString("h:mm:ss tt") + ")" + Environment.NewLine);

            if (is64bit())
            {
                MEMORY_BASIC_INFORMATION64 mem_basic_info64 = new MEMORY_BASIC_INFORMATION64();
                while (proc_min_address_l < proc_max_address_l)
                {
                    VirtualQueryEx(pHandle, proc_min_address, out mem_basic_info64, Marshal.SizeOf(mem_basic_info64));
                    if (mem_basic_info64.Protect >= 1 && mem_basic_info64.State == MEM_COMMIT) //this makes it fast :)
                    {
                        Int64 regionsize = mem_basic_info64.RegionSize;
                        Int64 BaseAddress = (Int64)mem_basic_info64.BaseAddress;
                        //Debug.Write("we got one here! start:" + theCode.ToString("x16") + " min:" + proc_min_address_l.ToString("x16") + " size:" + mem_basic_info64.RegionSize.ToString("x16") + Environment.NewLine);
                        if (theCode < proc_min_address_l && proc_min_address_l < (theCode + length))
                        {
                            //Debug.Write("[" + ar + "] Adding 0x" + proc_min_address.ToString("x16") + " to list arr. Length:0x" + mem_basic_info64.RegionSize.ToString("x16") + Environment.NewLine);
                            //Task.Run(() => test(proc_min_address_l, search, file));
                            list.Add((Int64)proc_min_address + "|" + regionsize + "|" + BaseAddress);
                            //compareScan((Int64)proc_min_address, memCode, stringByteArray, mask, regionsize, BaseAddress);
                            ar++;
                        }
                    }
                    /*else
                    {
                        Debug.Write("[" + ar + "] Cannot add 0x" + proc_min_address.ToString("x16") + " to list arr. Size:0x" + mem_basic_info64.RegionSize.ToString("x16") + " Protection:" + mem_basic_info64.Protect + " State:" + mem_basic_info64.State + " (64bit)" + Environment.NewLine);
                    }*/
                    //Debug.Write("region size: " + mem_basic_info.RegionSize + Environment.NewLine);
                    proc_min_address_l += mem_basic_info64.RegionSize;
                    proc_min_address = new IntPtr(proc_min_address_l);
                }
            }
            else
            {
                MEMORY_BASIC_INFORMATION32 mem_basic_info32 = new MEMORY_BASIC_INFORMATION32();
                while (proc_min_address_l < proc_max_address_l)
                {
                    VirtualQueryEx(pHandle, proc_min_address, out mem_basic_info32, Marshal.SizeOf(mem_basic_info32));
                    if (mem_basic_info32.Protect == PAGE_READWRITE && mem_basic_info32.State == MEM_COMMIT) //this makes it fast :)
                    {
                        Int64 regionsize = (Int64)mem_basic_info32.RegionSize;
                        Int64 BaseAddress = (Int64)mem_basic_info32.BaseAddress;
                        if (theCode < proc_min_address_l && proc_min_address_l < (theCode + length))
                        {
                            //Debug.Write("[" + ar + "] Adding 0x" + proc_min_address.ToString("x8") + " to list arr. Length:0x" + mem_basic_info32.RegionSize.ToString("x8") + Environment.NewLine);
                            //Task.Run(() => test(proc_min_address_l, search, file));
                            list.Add((Int64)proc_min_address + "|" + regionsize + "|" + BaseAddress);
                            //compareScan((Int64)proc_min_address, memCode, stringByteArray, mask, regionsize, BaseAddress);
                            ar++;
                        }
                    }
                    /*else
                    {
                        Debug.Write("[" + ar + "] Cannot add 0x" + proc_min_address.ToString("x8") + " to list arr. Is it protected? Length:0x" + mem_basic_info32.RegionSize.ToString("x8") + " Protection Code:" + mem_basic_info32.Protect + " (32bit)" + Environment.NewLine);
                    }*/
                    //Debug.Write("region size: " + mem_basic_info.RegionSize + Environment.NewLine);
                    proc_min_address_l += (Int64)mem_basic_info32.RegionSize;
                    proc_min_address = new IntPtr(proc_min_address_l);
                }
            }

            //Debug.Write("[DEBUG] VirtualQueryEx finished at 0x" + proc_min_address_l.ToString("x8") + ". Last region size is 0x" + (proc_min_address_l - proc_max_address_l).ToString("x8") + " (" + DateTime.Now.ToString("h:mm:ss tt") + ")" + Environment.NewLine);
            ParallelOptions po = new ParallelOptions();
            po.CancellationToken = cts.Token;
            Int64 pageCount = list.Count;
            Int64[] results = new Int64[pageCount];
            //try
            //{
                memCode = memCode.Replace('?', '.').Replace(' ', '-').ToUpper(); //for compareScan regex
                ParallelLoopResult result = Parallel.For(0, list.Count, po, async (int index, ParallelLoopState parallelLoopState) =>
                {
                    results[index] = await compareScan(Convert.ToInt64(list[index].Split('|')[0]), memCode, stringByteArray, mask, Convert.ToInt64(list[index].Split('|')[1]), Convert.ToInt64(list[index].Split('|')[2]));
                    //po.CancellationToken.ThrowIfCancellationRequested();
                    if (results[index] > 0)
                    {
                        cts.CancelAfter(TimeSpan.FromSeconds(2));
                        //Debug.Write("STOPPING PARALLEL LOOP STATE!" + Environment.NewLine);
                        parallelLoopState.Stop();
                    }
                });
                
                while (true)
                {
                    if (result.IsCompleted || cts.IsCancellationRequested)
                    {
                        foreach (int r in results)
                        {
                            if (r > 0)
                            {
                                Debug.Write("[DEBUG] memory scan completed. (" + DateTime.Now.ToString("h:mm:ss tt") + ")" + Environment.NewLine);
                                return r;
                            }
                        }
                        return 0; //if we fail
                    }
                }
            /*}
            catch (OperationCanceledException e)
            {
                foreach (int r in results)
                {
                    if (r > 0)
                    {
                        Debug.Write("[DEBUG] memory scan completed. (" + DateTime.Now.ToString("h:mm:ss tt") + ")" + Environment.NewLine);
                        return r;
                    }
                }
                return 0; //if we fail
            }
            finally
            {
                cts.Dispose();
            }*/
        }

        public async Task<Int64> compareScan(Int64 start, string memCode, string[] stringByteArray, string mask, Int64 regionsize, Int64 BaseAddress)
        {
            //try
            //{
                if (cts.IsCancellationRequested) return 0;
                byte[] buffer = new byte[regionsize];
                UIntPtr test2 = (UIntPtr)BaseAddress;
                UIntPtr test = (UIntPtr)regionsize;
                ReadProcessMemory(pHandle, test2, buffer, test, IntPtr.Zero);

                //Debug.Write("PageFindPattern starting... 0x" + start.ToString("x8") + " buffer length=" + buffer.Length + Environment.NewLine);
                string hexString = BitConverter.ToString(buffer);
                
                if (Regex.IsMatch(hexString, memCode))
                {
                    //Debug.Write("I found something in 0x" + start.ToString("x8") + Environment.NewLine);
                    return await PageFindPattern(buffer, stringByteArray, mask, start);
                }
            /*}
            catch (Exception ex)
            {
                //StackTrace st = new StackTrace(ex, true);
                //StackFrame frame = st.GetFrame(0);
                //Debug.Write( "ERROR ON LINE " + frame.GetFileLineNumber() + Environment.NewLine );
            }*/
            return 0;
        }
        
        /*public void DumpMemory(int type = 0)
        {
            int procID = GetProcessId(pHandle);
            var str = "";
            string rawFile = Process.GetProcessById(procID).ProcessName + ".DMP";
            string txtFile = Process.GetProcessById(procID).ProcessName + ".TXT";
            //Debug.Write("[DEBUG] Now dumping process ID:" + procID.ToString() + " (" + Process.GetProcessById(procID).ProcessName + ")" + Environment.NewLine);

            FileStream fsToDump = null;
            if (File.Exists(rawFile))
                fsToDump = File.Open(rawFile, FileMode.Append);
            else
                fsToDump = File.Create(rawFile);

            //Debug.Write("[DEBUG] writing raw values to dump file.." + Environment.NewLine);
            Process thisProcess = Process.GetCurrentProcess();
            MiniDumpWriteDump(pHandle, procID, fsToDump.SafeFileHandle.DangerousGetHandle(),
                 MINIDUMP_TYPE.MiniDumpWithDataSegs | MINIDUMP_TYPE.MiniDumpWithFullMemory | MINIDUMP_TYPE.MiniDumpWithHandleData | MINIDUMP_TYPE.MiniDumpFilterMemory
            | MINIDUMP_TYPE.MiniDumpScanMemory | MINIDUMP_TYPE.MiniDumpWithUnloadedModules | MINIDUMP_TYPE.MiniDumpWithIndirectlyReferencedMemory | MINIDUMP_TYPE.MiniDumpFilterModulePaths
            | MINIDUMP_TYPE.MiniDumpWithProcessThreadData | MINIDUMP_TYPE.MiniDumpWithPrivateReadWriteMemory | MINIDUMP_TYPE.MiniDumpWithFullMemoryInfo
            | MINIDUMP_TYPE.MiniDumpWithThreadInfo | MINIDUMP_TYPE.MiniDumpWithCodeSegs,
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            fsToDump.Close();

            if (type == 0)
            {
                dumpBytes = File.ReadAllBytes(rawFile);
                diff = Convert.ToInt32(procs.WorkingSet) - dumpBytes.Length;
                //MessageBox.Show(diff.ToString() + " 0x" + diff.ToString("x8"));
            }

            if (type == 1)
            {
                //Debug.Write("[DEBUG] converting raw values to hex string values, this can take a while..." + Environment.NewLine);
                byte[] bytes = File.ReadAllBytes(rawFile);
                str = ByteArrayToHexString(bytes);
            }

            if (type < 2)
                File.Delete(rawFile);

            if (type == 1)
            {
                //Debug.Write("[DEBUG] deleting raw dump file and writing hex string values to text file..." + Environment.NewLine);
                File.WriteAllText(txtFile, str);
            }
        }*/
    }
}
