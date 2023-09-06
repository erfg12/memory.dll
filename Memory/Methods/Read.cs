using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Memory.Imps;
using System.Reflection;

namespace Memory
{
    public partial class Mem
    {
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
        /// Reads up to `length ` bytes from an address.
        /// </summary>
        /// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        /// <param name="length">The maximum bytes to read.</param>
        /// <param name="file">path and name of ini file.</param>
        /// <returns>The bytes read or null</returns>
        public byte[] ReadBytes(string code, long length, string file = "")
        {
            byte[] memory = new byte[length];
            UIntPtr theCode = GetCode(code, file);
            if (theCode == null || theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return null;

            if (!ReadProcessMemory(mProc.Handle, theCode, memory, (UIntPtr)length, IntPtr.Zero))
                return null;

            return memory;
        }

        /// <summary>
        /// Read a float value from an address.
        /// </summary>
        /// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        /// <param name="file">path and name of ini file. (OPTIONAL)</param>
        /// <param name="round">Round the value to 2 decimal places</param>
        /// <returns></returns>
        public float ReadFloat(string code, string file = "", bool round = true)
        {
            byte[] memory = new byte[4];

            UIntPtr theCode = GetCode(code, file);
            if (theCode == null || theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return 0;

            try
            {
                if (ReadProcessMemory(mProc.Handle, theCode, memory, (UIntPtr)4, IntPtr.Zero))
                {
                    float address = BitConverter.ToSingle(memory, 0);
                    float returnValue = (float)address;
                    if (round)
                        returnValue = (float)Math.Round(address, 2);
                    return returnValue;
                }
                else
                    return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Read a string value from an address.
        /// </summary>
        /// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        /// <param name="file">path and name of ini file. (OPTIONAL)</param>
        /// <param name="length">length of bytes to read (OPTIONAL)</param>
        /// <param name="zeroTerminated">terminate string at null char</param>
        /// <param name="stringEncoding">System.Text.Encoding.UTF8 (DEFAULT). Other options: ascii, unicode, utf32, utf7</param>
        /// <returns></returns>
        public string ReadString(string code, string file = "", int length = 32, bool zeroTerminated = true, System.Text.Encoding stringEncoding = null)
        {
            if (stringEncoding == null)
                stringEncoding = System.Text.Encoding.UTF8;

            byte[] memoryNormal = new byte[length];
            UIntPtr theCode = GetCode(code, file);
            if (theCode == null || theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return "";

            if (ReadProcessMemory(mProc.Handle, theCode, memoryNormal, (UIntPtr)length, IntPtr.Zero))
                return (zeroTerminated) ? stringEncoding.GetString(memoryNormal).Split('\0')[0] : stringEncoding.GetString(memoryNormal);
            else
                return "";
        }

        /// <summary>
        /// Read a double value
        /// </summary>
        /// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        /// <param name="file">path and name of ini file. (OPTIONAL)</param>
        /// <param name="round">Round the value to 2 decimal places</param>
        /// <returns></returns>
        public double ReadDouble(string code, string file = "", bool round = true)
        {
            byte[] memory = new byte[8];

            UIntPtr theCode = GetCode(code, file);
            if (theCode == null || theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return 0;

            try
            {
                if (ReadProcessMemory(mProc.Handle, theCode, memory, (UIntPtr)8, IntPtr.Zero))
                {
                    double address = BitConverter.ToDouble(memory, 0);
                    double returnValue = (double)address;
                    if (round)
                        returnValue = (double)Math.Round(address, 2);
                    return returnValue;
                }
                else
                    return 0;
            }
            catch
            {
                return 0;
            }
        }

        public int ReadUIntPtr(UIntPtr code)
        {
            byte[] memory = new byte[4];
            if (ReadProcessMemory(mProc.Handle, code, memory, (UIntPtr)4, IntPtr.Zero))
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
        public int ReadInt(string code, string file = "")
        {
            byte[] memory = new byte[4];
            UIntPtr theCode = GetCode(code, file);
            if (theCode == null || theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return 0;

            if (ReadProcessMemory(mProc.Handle, theCode, memory, (UIntPtr)4, IntPtr.Zero))
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
        public long ReadLong(string code, string file = "")
        {
            byte[] memory = new byte[16];
            UIntPtr theCode = GetCode(code, file);

            if (theCode == null || theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return 0;

            if (ReadProcessMemory(mProc.Handle, theCode, memory, (UIntPtr)8, IntPtr.Zero))
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
        public UInt32 ReadUInt(string code, string file = "")
        {
            byte[] memory = new byte[4];
            UIntPtr theCode = GetCode(code, file);
            if (theCode == null || theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return 0;

            if (ReadProcessMemory(mProc.Handle, theCode, memory, (UIntPtr)4, IntPtr.Zero))
                return BitConverter.ToUInt32(memory, 0);
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
        public int Read2ByteMove(string code, int moveQty, string file = "")
        {
            byte[] memory = new byte[4];
            UIntPtr theCode = GetCode(code, file);
            if (theCode == null || theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return 0;

            UIntPtr newCode = UIntPtr.Add(theCode, moveQty);

            if (ReadProcessMemory(mProc.Handle, newCode, memory, (UIntPtr)2, IntPtr.Zero))
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
        public int ReadIntMove(string code, int moveQty, string file = "")
        {
            byte[] memory = new byte[4];
            UIntPtr theCode = GetCode(code, file);
            if (theCode == null || theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return 0;

            UIntPtr newCode = UIntPtr.Add(theCode, moveQty);

            if (ReadProcessMemory(mProc.Handle, newCode, memory, (UIntPtr)4, IntPtr.Zero))
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
        public ulong ReadUIntMove(string code, int moveQty, string file = "")
        {
            byte[] memory = new byte[8];
            UIntPtr theCode = GetCode(code, file, 8);
            if (theCode == null || theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return 0;

            UIntPtr newCode = UIntPtr.Add(theCode, moveQty);

            if (ReadProcessMemory(mProc.Handle, newCode, memory, (UIntPtr)8, IntPtr.Zero))
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
        public int Read2Byte(string code, string file = "")
        {
            byte[] memoryTiny = new byte[4];

            UIntPtr theCode = GetCode(code, file);
            if (theCode == null || theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return 0;

            if (ReadProcessMemory(mProc.Handle, theCode, memoryTiny, (UIntPtr)2, IntPtr.Zero))
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
        public int ReadByte(string code, string file = "")
        {
            byte[] memoryTiny = new byte[1];

            UIntPtr theCode = GetCode(code, file);
            if (theCode == null || theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return 0;

            if (ReadProcessMemory(mProc.Handle, theCode, memoryTiny, (UIntPtr)1, IntPtr.Zero))
                return memoryTiny[0];

            return 0;
        }

        /// <summary>
        /// Reads a byte from memory and splits it into bits
        /// </summary>
        /// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        /// <param name="file">path and file name of ini file. (OPTIONAL)</param>
        /// <returns>Array of 8 booleans representing each bit of the byte read</returns>
        public bool[] ReadBits(string code, string file = "")
        {
            byte[] buf = new byte[1];

            UIntPtr theCode = GetCode(code, file);

            bool[] ret = new bool[8];

            if (theCode == null || theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return ret;

            if (!ReadProcessMemory(mProc.Handle, theCode, buf, (UIntPtr)1, IntPtr.Zero))
                return ret;


            if (!BitConverter.IsLittleEndian)
                throw new Exception("Should be little endian");

            for (var i = 0; i < 8; i++)
                ret[i] = Convert.ToBoolean(buf[0] & (1 << i));

            return ret;

        }

        public int ReadPByte(UIntPtr address, string code, string file = "")
        {
            byte[] memory = new byte[4];
            if (ReadProcessMemory(mProc.Handle, address + LoadIntCode(code, file), memory, (UIntPtr)1, IntPtr.Zero))
                return BitConverter.ToInt32(memory, 0);
            else
                return 0;
        }

        public float ReadPFloat(UIntPtr address, string code, string file = "")
        {
            byte[] memory = new byte[4];
            if (ReadProcessMemory(mProc.Handle, address + LoadIntCode(code, file), memory, (UIntPtr)4, IntPtr.Zero))
            {
                float spawn = BitConverter.ToSingle(memory, 0);
                return (float)Math.Round(spawn, 2);
            }
            else
                return 0;
        }

        public int ReadPInt(UIntPtr address, string code, string file = "")
        {
            byte[] memory = new byte[4];
            if (ReadProcessMemory(mProc.Handle, address + LoadIntCode(code, file), memory, (UIntPtr)4, IntPtr.Zero))
                return BitConverter.ToInt32(memory, 0);
            else
                return 0;
        }

        public string ReadPString(UIntPtr address, string code, string file = "")
        {
            byte[] memoryNormal = new byte[32];
            if (ReadProcessMemory(mProc.Handle, address + LoadIntCode(code, file), memoryNormal, (UIntPtr)32, IntPtr.Zero))
                return CutString(System.Text.Encoding.ASCII.GetString(memoryNormal));
            else
                return "";
        }

        public T ReadMemory<T>(string address, string file = "")
        {
            object ReadOutput = null;

            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.String:
                    ReadOutput = ReadString(address, file);
                    break;
                case TypeCode.Int32:
                    ReadOutput = ReadInt(address, file);
                    break;
                case TypeCode.Int64:
                    ReadOutput = ReadLong(address, file);
                    break;
                case TypeCode.Byte:
                    ReadOutput = ReadByte(address, file);
                    break;
                case TypeCode.Double:
                    ReadOutput = ReadDouble(address, file);
                    break;
                case TypeCode.Decimal:
                    ReadOutput = ReadFloat(address, file);
                    break;
                case TypeCode.UInt32:
                    ReadOutput = ReadUInt(address, file);
                    break;
                default:
                    break;
            }

            if (ReadOutput != null)
                return (T)Convert.ChangeType(ReadOutput, typeof(T));
            else
                return default(T);
        }

        ConcurrentDictionary<string, CancellationTokenSource> ReadTokenSrcs = new ConcurrentDictionary<string, CancellationTokenSource>();
        /// <summary>
        /// Reads a memory address, keeps value in UI object. Ex: BindToUI("0x12345678,0x02,0x05", v => ObjName.Invoke((MethodInvoker)delegate { if (String.Compare(v, ObjName.Text) != 0) { ObjName.Text = v; } }));
        /// </summary>
        /// <param name="address">Your code or INI file variable name</param>
        /// <param name="UIObject">Returning variable to bind to UI object. See example in summary.</param>
        /// <param name="file">OPTIONAL: INI file path and file name with extension</param>
        public void BindToUI<T>(string address, Action<string> UIObject, string file = "")
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            if (ReadTokenSrcs.ContainsKey(address))
            {
                try
                {
                    ReadTokenSrcs[address].Cancel();
                    ReadTokenSrcs.TryRemove(address, out _);
                }
                catch
                {
                    Debug.WriteLine("ERROR: Avoided a crash. Address " + address + " was not bound.");
                }
            }
            else
            {
                Debug.WriteLine("Adding Bound Address " + address);
            }

            ReadTokenSrcs.TryAdd(address, cts);

            Task.Factory.StartNew(() =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    UIObject(ReadMemory<T>(address, file).ToString());
                    Thread.Sleep(100);
                }
            },
            cts.Token);
        }
    }
}
