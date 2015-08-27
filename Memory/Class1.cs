using System;
using System.IO;
using System.IO.Pipes;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

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

        Process procs = null;

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
                if (Module.ModuleName.Contains("dpvs"))
                    dpvsModule = Module.BaseAddress;
                if (Module.ModuleName.Contains("DSETUP"))
                    dsetupModule = Module.BaseAddress;
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

        public string LoadCode(string name, string file)
        {
            StringBuilder returnCode = new StringBuilder(1024);
            uint read_ini_result = GetPrivateProfileString("codes", name, "", returnCode, (uint)file.Length, file);
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

        private UIntPtr LoadUIntPtrCode(string name, string path)
        {
            string theCode = LoadCode(name, path);
            UIntPtr uintValue;

            string newOffset = theCode.Substring(theCode.IndexOf('+') + 1);

            if (theCode.Contains("base"))
                uintValue = (UIntPtr)((int)mainModule.BaseAddress + Convert.ToInt32(newOffset, 16));
            else
                uintValue = (UIntPtr)Convert.ToInt32(newOffset, 16);

            return (UIntPtr)uintValue;
        }

        private ProcessModule mainModule;
        private IntPtr dpvsModule;
        private IntPtr dsetupModule;

        /*public string CutString(string mystring)
        {
            char[] chArray = mystring.ToCharArray();
            string str = "";
            for (int i = 0; i < mystring.Length; i++)
            {
                if ((chArray[i] == ' ') && (chArray[i + 1] == ' '))
                {
                    return str;
                }
                if (chArray[i] == '\0')
                {
                    return str;
                }
                str = str + chArray[i].ToString();
            }
            return mystring.TrimEnd(new char[] { '0' });
        }*/

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
        public float readFloat(string code, string file)
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
                return (float)Math.Round(address, 2);
            }
            else
                return 0;
        }

        public string readString(string code, string file)
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

        public int readInt(string code, string file)
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

        public uint readUInt(string code, string file)
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

        public int read2ByteMove(string code, string file, int moveQty)
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

        public int readIntMove(string code, string file, int moveQty)
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

            UIntPtr newCode = UIntPtr.Add(theCode,moveQty);

            if (ReadProcessMemory(pHandle, newCode, memory, (UIntPtr)8, IntPtr.Zero))
                return BitConverter.ToUInt64(memory, 0);
            else
                return 0;
        }

        public int read2Byte(string code, string file)
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

        public int readByte(string code, string file)
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

        public int readPByte(UIntPtr address, string code, string file)
        {
            byte[] memory = new byte[4];
            if (ReadProcessMemory(pHandle, address + LoadIntCode(code, file), memory, (UIntPtr)1, IntPtr.Zero))
                return BitConverter.ToInt32(memory, 0);
            else
                return 0;
        }

        public float readPFloat(UIntPtr address, string code, string file)
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

        public int readPInt(UIntPtr address, string code, string file)
        {
            byte[] memory = new byte[4];
            if (ReadProcessMemory(pHandle, address + LoadIntCode(code, file), memory, (UIntPtr)4, IntPtr.Zero))
                return BitConverter.ToInt32(memory, 0);
            else
                return 0;
        }

        public string readPString(UIntPtr address, string code, string file)
        {
            byte[] memoryNormal = new byte[32];
            if (ReadProcessMemory(pHandle, address + LoadIntCode(code, file), memoryNormal, (UIntPtr)32, IntPtr.Zero))
                return CutString(System.Text.Encoding.ASCII.GetString(memoryNormal));
            else
                return "";
        }
        #endregion

        #region writeMemory
        public bool writeMemory(string code, string file, string type, string write)
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

        public void writeUIntPtr(string code, string file, byte[] write)
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
            bool main = false;
            bool dpvs = false;
            bool dsetup = false;
            string newOffsets = theCode;
            if (theCode.Contains("base") || theCode.Contains("dpvs") || theCode.Contains("dsetup"))
                newOffsets = theCode.Substring(theCode.IndexOf('+') + 1);
            if (theCode.Contains("base") )
                main = true;
            else if (theCode.Contains("dpvs"))
                dpvs = true;
            else if (theCode.Contains("dsetup"))
                dsetup = true;

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

                if (main == true)
                    ReadProcessMemory(pHandle, (UIntPtr)((int)mainModule.BaseAddress + offsets[0]), memoryAddress, (UIntPtr)size, IntPtr.Zero);
                else if (dpvs == true)
                    ReadProcessMemory(pHandle, (UIntPtr)((int)dpvsModule + offsets[0]), memoryAddress, (UIntPtr)size, IntPtr.Zero);
                else if (dsetup == true)
                    ReadProcessMemory(pHandle, (UIntPtr)((int)dsetupModule + offsets[0]), memoryAddress, (UIntPtr)size, IntPtr.Zero);
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
                if (main == true)
                    ReadProcessMemory(pHandle, (UIntPtr)((int)mainModule.BaseAddress + trueCode), memoryAddress, (UIntPtr)size, IntPtr.Zero);
                else if (dpvs == true)
                    ReadProcessMemory(pHandle, (UIntPtr)((int)dpvsModule + trueCode), memoryAddress, (UIntPtr)size, IntPtr.Zero);
                else if (dsetup == true)
                    ReadProcessMemory(pHandle, (UIntPtr)((int)dsetupModule + trueCode), memoryAddress, (UIntPtr)size, IntPtr.Zero);
                else
                    ReadProcessMemory(pHandle, (UIntPtr)(trueCode), memoryAddress, (UIntPtr)size, IntPtr.Zero);

                uint num1 = BitConverter.ToUInt32(memoryAddress, 0);

                //UIntPtr base1 = (UIntPtr)0;

                //for (int i = 1; i < 0; i++)
                //{
                UIntPtr base1 = new UIntPtr(num1);
                    //ReadProcessMemory(pHandle, base1, memoryAddress, (UIntPtr)size, IntPtr.Zero);
                    num1 = BitConverter.ToUInt32(memoryAddress, 0);
                //}
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
    }
}
