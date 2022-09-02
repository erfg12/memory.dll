using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Memory
{
    public class Imps
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(
            uint dwDesiredAccess,
            bool bInheritHandle,
            int dwProcessId
            );

#if WINXP
#else
        [DllImport("kernel32.dll", EntryPoint = "VirtualQueryEx")]
        public static extern UIntPtr Native_VirtualQueryEx(IntPtr hProcess, UIntPtr lpAddress,
            out MEMORY_BASIC_INFORMATION32 lpBuffer, UIntPtr dwLength);

        [DllImport("kernel32.dll", EntryPoint = "VirtualQueryEx")]
        public static extern UIntPtr Native_VirtualQueryEx(IntPtr hProcess, UIntPtr lpAddress,
            out MEMORY_BASIC_INFORMATION64 lpBuffer, UIntPtr dwLength);


        [DllImport("kernel32.dll")]
        public static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);
#endif

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        public static extern int ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(
            IntPtr hProcess,
            UIntPtr lpBaseAddress,
            string lpBuffer,
            UIntPtr nSize,
            out IntPtr lpNumberOfBytesWritten
        );

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern uint GetPrivateProfileString(
           string lpAppName,
           string lpKeyName,
           string lpDefault,
           StringBuilder lpReturnedString,
           uint nSize,
           string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool VirtualFreeEx(
            IntPtr hProcess,
            UIntPtr lpAddress,
            UIntPtr dwSize,
            uint dwFreeType
            );

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] IntPtr lpBuffer, UIntPtr nSize, out ulong lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern UIntPtr VirtualAllocEx(
            IntPtr hProcess,
            UIntPtr lpAddress,
            uint dwSize,
            uint flAllocationType,
            uint flProtect
        );

        [DllImport("kernel32.dll")]
        public static extern bool VirtualProtectEx(IntPtr hProcess, UIntPtr lpAddress,
            IntPtr dwSize, MemoryProtection flNewProtect, out MemoryProtection lpflOldProtect);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern UIntPtr GetProcAddress(
            IntPtr hModule,
            string procName
        );

        [DllImport("kernel32.dll")]
        public static extern int CloseHandle(
        IntPtr hObject
        );

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(
            string lpModuleName
        );

        [DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
        internal static extern int WaitForSingleObject(
            IntPtr handle,
            int milliseconds
        );

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesWritten);

        // Added to avoid casting to UIntPtr
        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32")]
        public static extern IntPtr CreateRemoteThread(
          IntPtr hProcess,
          IntPtr lpThreadAttributes,
          uint dwStackSize,
          UIntPtr lpStartAddress, // raw Pointer into remote process  
          UIntPtr lpParameter,
          uint dwCreationFlags,
          out IntPtr lpThreadId
        );

        [DllImport("kernel32")]
        public static extern bool IsWow64Process(IntPtr hProcess, out bool lpSystemInfo);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        //ReSharper disable once InconsistentNaming
        public static extern IntPtr CreateToolhelp32Snapshot([In] UInt32 dwFlags, [In] UInt32 th32ProcessID);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool Process32First([In] IntPtr hSnapshot, ref PROCESSENTRY32 lppe);
        [DllImport("kernel32.dll")]
        public static extern bool Module32First(IntPtr hSnapshot, ref MODULEENTRY32 lpme);
        [DllImport("kernel32.dll")]
        public static extern bool Module32Next(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool Process32Next([In] IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        /*
         typedef NTSTATUS (WINAPI *LPFUN_NtCreateThreadEx)
            (
              OUT PHANDLE hThread,
              IN ACCESS_MASK DesiredAccess,
              IN LPVOID ObjectAttributes,
              IN HANDLE ProcessHandle,
              IN LPTHREAD_START_ROUTINE lpStartAddress,
              IN LPVOID lpParameter,
              IN BOOL CreateSuspended,
              IN ULONG StackZeroBits,
              IN ULONG SizeOfStackCommit,
              IN ULONG SizeOfStackReserve,
              OUT LPVOID lpBytesBuffer
            );
         */
        [DllImport("ntdll.dll", SetLastError = true)]
        internal static extern NTSTATUS NtCreateThreadEx(out IntPtr hProcess, AccessMask desiredAccess, IntPtr objectAttributes, UIntPtr processHandle, IntPtr startAddress, IntPtr parameter, ThreadCreationFlags inCreateSuspended, Int32 stackZeroBits, Int32 sizeOfStack, Int32 maximumStackSize, IntPtr attributeList);

        // privileges
        public const int ProcessCreateThread = 0x0002;
        public const int ProcessQueryInformation = 0x0400;
        public const int ProcessVmOperation = 0x0008;
        public const int ProcessVmWrite = 0x0020;
        public const int ProcessVmRead = 0x0010;

        // used for memory allocation
        public const uint MemFree = 0x10000;
        public const uint MemCommit = 0x00001000;
        public const uint MemReserve = 0x00002000;

        public const uint Readonly = 0x02;
        public const uint Readwrite = 0x04;
        public const uint Writecopy = 0x08;
        public const uint ExecuteReadwrite = 0x40;
        public const uint ExecuteWritecopy = 0x80;
        public const uint Execute = 0x10;
        public const uint ExecuteRead = 0x20;

        public const uint Guard = 0x100;
        public const uint Noaccess = 0x01;

        public const uint MemPrivate = 0x20000;
        public const uint MemImage = 0x1000000;
        public const uint MemMapped = 0x40000;

        internal enum NTSTATUS
        {
            Success = 0x00
        }

        internal enum AccessMask
        {
            SpecificRightsAll = 0xFFFF,
            StandardRightsAll = 0x1F0000
        }
        internal enum ThreadCreationFlags
        {
            Immediately = 0x0,
            CreateSuspended = 0x01,
            HideFromDebugger = 0x04,
            StackSizeParamIsAReservation = 0x10000
        }

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

        public struct SYSTEM_INFO
        {
            public ushort ProcessorArchitecture;
            ushort reserved;
            public uint PageSize;
            public UIntPtr MinimumApplicationAddress;
            public UIntPtr MaximumApplicationAddress;
            public IntPtr ActiveProcessorMask;
            public uint NumberOfProcessors;
            public uint ProcessorType;
            public uint AllocationGranularity;
            public ushort ProcessorLevel;
            public ushort ProcessorRevision;
        }

        public struct MEMORY_BASIC_INFORMATION32
        {
            public UIntPtr BaseAddress;
            public UIntPtr AllocationBase;
            public uint AllocationProtect;
            public uint RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        public struct MEMORY_BASIC_INFORMATION64
        {
            public UIntPtr BaseAddress;
            public UIntPtr AllocationBase;
            public uint AllocationProtect;
            public uint Alignment1;
            public ulong RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
            public uint Alignment2;
        }

        public struct MEMORY_BASIC_INFORMATION
        {
            public UIntPtr BaseAddress;
            public UIntPtr AllocationBase;
            public uint AllocationProtect;
            public long RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        private enum SnapshotFlags : uint
        {
            HeapList = 0x00000001,
            Process = 0x00000002,
            Thread = 0x00000004,
            Module = 0x00000008,
            Module32 = 0x00000010,
            Inherit = 0x80000000,
            All = 0x0000001F,
            NoHeaps = 0x40000000
        }

        [Flags]
        public enum ThreadAccess : int
        {
            Terminate = (0x0001),
            SuspendResume = (0x0002),
            GetContext = (0x0008),
            SetContext = (0x0010),
            SetInformation = (0x0020),
            QueryInformation = (0x0040),
            SetThreadToken = (0x0080),
            Impersonate = (0x0100),
            DirectImpersonation = (0x0200)
        }

        [Flags]
        public enum MemoryProtection : uint
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierFlag = 0x100,
            NoCacheModifierFlag = 0x200,
            WriteCombineModifierFlag = 0x400
        }

        //inner struct used only internally
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct PROCESSENTRY32
        {
            const int MAX_PATH = 260;
            private uint dwSize;
            private uint cntUsage;
            private uint th32ProcessID;
            private IntPtr th32DefaultHeapID;
            private uint th32ModuleID;
            private uint cntThreads;
            private uint th32ParentProcessID;
            private int pcPriClassBase;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            private string szExeFile;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public struct MODULEENTRY32
        {
            private uint dwSize;
            private uint th32ModuleID;
            private uint th32ProcessID;
            private uint GlblcntUsage;
            private uint ProccntUsage;
            private IntPtr modBaseAddr;
            private uint modBaseSize;
            private IntPtr hModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            private string szModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            private string szExePath;
        }

        [DllImport("ntdll.dll", SetLastError = true)]
        internal static extern int NtQueryInformationThread(
        IntPtr threadHandle,
        ThreadInfoClass threadInformationClass,
        IntPtr threadInformation,
        int threadInformationLength,
        IntPtr returnLengthPtr);
        public enum ThreadInfoClass
        {
            ThreadQuerySetWin32StartAddress = 9
        }


    }
}