using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Memory.Imps;

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
            StringBuilder sb = new();
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
            if (theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return null;

            return !ReadProcessMemory(MProc.Handle, theCode, memory, (UIntPtr)length, IntPtr.Zero) ? null : memory;
        }

        /// <summary>
        /// Read a float value from an address.
        /// </summary>
        /// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        /// <param name="file">path and name of ini file. (OPTIONAL)</param>
        /// <param name="round">Round the value to 2 decimal places</param>
        /// <returns></returns>
        public float ReadFloat(string code, string file = "", bool round = false)
        {
            byte[] memory = new byte[4];

            UIntPtr theCode = GetCode(code, file);
            if (theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return 0;

            try
            {
                if (!ReadProcessMemory(MProc.Handle, theCode, memory, (UIntPtr)4, IntPtr.Zero))
                    return 0;
                
                float address = BitConverter.ToSingle(memory, 0);
                float returnValue = address;
                if (round)
                    returnValue = (float)Math.Round(address, 2);
                return returnValue;

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
        public string ReadString(string code, string file = "", int length = 32, bool zeroTerminated = true, Encoding stringEncoding = null)
        {
            if (stringEncoding == null)
                stringEncoding = Encoding.UTF8;

            byte[] memoryNormal = new byte[length];
            UIntPtr theCode = GetCode(code, file);
            if (theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return "";

            if (ReadProcessMemory(MProc.Handle, theCode, memoryNormal, (UIntPtr)length, IntPtr.Zero))
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
        public double ReadDouble(string code, string file = "", bool round = false)
        {
            byte[] memory = new byte[8];

            UIntPtr theCode = GetCode(code, file);
            if (theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return 0;

            try
            {
                if (!ReadProcessMemory(MProc.Handle, theCode, memory, (UIntPtr)8, IntPtr.Zero))
                    return 0;
                
                double address = BitConverter.ToDouble(memory, 0);
                double returnValue = address;
                if (round)
                    returnValue = Math.Round(address, 2);
                return returnValue;

            }
            catch
            {
                return 0;
            }
        }

        public int ReadUIntPtr(UIntPtr code)
        {
            byte[] memory = new byte[4];
            if (ReadProcessMemory(MProc.Handle, code, memory, (UIntPtr)4, IntPtr.Zero))
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
            if (theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return 0;

            return ReadProcessMemory(MProc.Handle, theCode, memory, (UIntPtr)4, IntPtr.Zero)
                ? BitConverter.ToInt32(memory, 0)
                : 0;
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

            if (theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return 0;

            if (ReadProcessMemory(MProc.Handle, theCode, memory, (UIntPtr)8, IntPtr.Zero))
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
        public uint ReadUInt(string code, string file = "")
        {
            byte[] memory = new byte[4];
            UIntPtr theCode = GetCode(code, file);
            if (theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return 0;

            return ReadProcessMemory(MProc.Handle, theCode, memory, (UIntPtr)4, IntPtr.Zero)
                ? BitConverter.ToUInt32(memory, 0)
                : 0;
        }
        
        /// <summary>
        /// Read a UShort value from address.
        /// </summary>
        /// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        /// <param name="file">path and name of ini file. (OPTIONAL)</param>
        /// <returns></returns>
        public ushort ReadUShort(string code, string file = "")
        {
            byte[] memory = new byte[2];
            UIntPtr theCode = GetCode(code, file);
            if (theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return 0;

            return ReadProcessMemory(MProc.Handle, theCode, memory, (UIntPtr)2, IntPtr.Zero)
                ? BitConverter.ToUInt16(memory, 0)
                : (ushort)0;
        }

        /// <summary>
        /// Read a ULong value from address.
        /// </summary>
        /// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        /// <param name="file">path and name of ini file. (OPTIONAL)</param>
        /// <returns></returns>
        public ulong ReadULong(string code, string file = "")
        {
            byte[] memory = new byte[8];
            UIntPtr theCode = GetCode(code, file);
            if (theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return 0;

            return ReadProcessMemory(MProc.Handle, theCode, memory, (UIntPtr)8, IntPtr.Zero)
                ? BitConverter.ToUInt64(memory, 0)
                : 0;
        }

        /// <summary>
        /// Reads a 2 byte value from an address and moves the address.
        /// </summary>
        /// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        /// <param name="moveQty">Quantity to move.</param>
        /// <param name="file">path and name of ini file (OPTIONAL)</param>
        /// <returns></returns>
        public int ReadShortMove(string code, int moveQty, string file = "")
        {
            byte[] memory = new byte[4];
            UIntPtr theCode = GetCode(code, file);
            if (theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return 0;

            UIntPtr newCode = UIntPtr.Add(theCode, moveQty);

            if (ReadProcessMemory(MProc.Handle, newCode, memory, (UIntPtr)2, IntPtr.Zero))
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
            if (theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return 0;

            UIntPtr newCode = UIntPtr.Add(theCode, moveQty);

            return ReadProcessMemory(MProc.Handle, newCode, memory, (UIntPtr)4, IntPtr.Zero)
                ? BitConverter.ToInt32(memory, 0)
                : 0;
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
            UIntPtr theCode = GetCode(code, file);
            if (theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return 0;

            UIntPtr newCode = UIntPtr.Add(theCode, moveQty);

            return ReadProcessMemory(MProc.Handle, newCode, memory, (UIntPtr)8, IntPtr.Zero)
                ? BitConverter.ToUInt64(memory, 0)
                : 0;
        }

        /// <summary>
        /// Read a 2 byte value from an address. Returns an integer.
        /// </summary>
        /// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        /// <param name="file">path and file name to ini file. (OPTIONAL)</param>
        /// <returns></returns>
        public int ReadShort(string code, string file = "")
        {
            byte[] memoryTiny = new byte[4];

            UIntPtr theCode = GetCode(code, file);
            if (theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return 0;

            return ReadProcessMemory(MProc.Handle, theCode, memoryTiny, (UIntPtr)2, IntPtr.Zero)
                ? BitConverter.ToInt32(memoryTiny, 0)
                : 0;
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
            if (theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return 0;

            return ReadProcessMemory(MProc.Handle, theCode, memoryTiny, (UIntPtr)1, IntPtr.Zero) ? memoryTiny[0] : 0;
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

            if (theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return ret;

            if (!ReadProcessMemory(MProc.Handle, theCode, buf, (UIntPtr)1, IntPtr.Zero))
                return ret;


            if (!BitConverter.IsLittleEndian)
                throw new Exception("Should be little endian");

            for (int i = 0; i < 8; i++)
                ret[i] = Convert.ToBoolean(buf[0] & (1 << i));

            return ret;

        }

        public int ReadByte(UIntPtr address, string code, string file = "")
        {
            byte[] memory = new byte[4];
            return ReadProcessMemory(MProc.Handle, address + LoadIntCode(code, file), memory, (UIntPtr)1, IntPtr.Zero)
                ? BitConverter.ToInt32(memory, 0)
                : 0;
        }

        public float ReadFloat(UIntPtr address, string code, string file = "", bool round = false)
        {
            byte[] memory = new byte[4];
            if (!ReadProcessMemory(MProc.Handle, address + LoadIntCode(code, file), memory, (UIntPtr)4,
                    IntPtr.Zero)) return 0;
            float spawn = BitConverter.ToSingle(memory, 0);
            return round ? (float)Math.Round(spawn, 2) : spawn;

        }

        public int ReadInt(UIntPtr address, string code, string file = "")
        {
            byte[] memory = new byte[4];
            return ReadProcessMemory(MProc.Handle, address + LoadIntCode(code, file), memory, (UIntPtr)4, IntPtr.Zero)
                ? BitConverter.ToInt32(memory, 0)
                : 0;
        }

        public string ReadString(UIntPtr address, string code, string file = "")
        {
            byte[] memoryNormal = new byte[32];
            return ReadProcessMemory(MProc.Handle, address + LoadIntCode(code, file), memoryNormal, (UIntPtr)32,
                IntPtr.Zero)
                ? CutString(Encoding.ASCII.GetString(memoryNormal))
                : "";
        }
        
        public long ReadLong(UIntPtr address, string code, string file = "")
        {
            byte[] memory = new byte[8];
            return ReadProcessMemory(MProc.Handle, address + LoadIntCode(code, file), memory, (UIntPtr)8, IntPtr.Zero)
                ? BitConverter.ToInt64(memory, 0)
                : 0;
        }
        
        public short ReadShort(UIntPtr address, string code, string file = "")
        {
            byte[] memoryTiny = new byte[4];
            return ReadProcessMemory(MProc.Handle, address + LoadIntCode(code, file), memoryTiny, (UIntPtr)2,
                IntPtr.Zero)
                ? BitConverter.ToInt16(memoryTiny, 0)
                : (short)0;
        }

        public double ReadDouble(UIntPtr address, string code, string file = "", bool round = false)
        {
            byte[] memory = new byte[8];
            if (!ReadProcessMemory(MProc.Handle, address + LoadIntCode(code, file), memory, (UIntPtr)8,
                    IntPtr.Zero)) return 0;
            double spawn = BitConverter.ToDouble(memory, 0);
            return round ? Math.Round(spawn, 2) : spawn;
        }
        
        public uint ReadUInt(UIntPtr address, string code, string file = "")
        {
            byte[] memory = new byte[4];
            return ReadProcessMemory(MProc.Handle, address + LoadIntCode(code, file), memory, (UIntPtr)8, IntPtr.Zero)
                ? BitConverter.ToUInt32(memory, 0)
                : 0;
        }
        
        public ulong ReadULong(UIntPtr address, string code, string file = "")
        {
            byte[] memory = new byte[8];
            return ReadProcessMemory(MProc.Handle, address + LoadIntCode(code, file), memory, (UIntPtr)8, IntPtr.Zero)
                ? BitConverter.ToUInt64(memory, 0)
                : 0;
        }
        
        public ushort ReadUShort(UIntPtr address, string code, string file = "")
        {
            byte[] memoryTiny = new byte[4];
            return ReadProcessMemory(MProc.Handle, address + LoadIntCode(code, file), memoryTiny, (UIntPtr)2,
                IntPtr.Zero)
                ? BitConverter.ToUInt16(memoryTiny, 0)
                : (ushort)0;
        }

        public T ReadMemory<T>(string address, string file = "")
        {
            object readOutput = Type.GetTypeCode(typeof(T)) switch
            {
                TypeCode.Byte => ReadByte(address, file),
                TypeCode.Int16 => ReadShort(address, file),
                TypeCode.Int32 => ReadInt(address, file),
                TypeCode.Int64 => ReadLong(address, file),
                TypeCode.UInt16 => ReadUShort(address, file),
                TypeCode.UInt32 => ReadUInt(address, file),
                TypeCode.UInt64 => ReadULong(address, file),
                TypeCode.Single => ReadFloat(address, file),
                TypeCode.Double => ReadDouble(address, file),
                TypeCode.String => ReadString(address, file),
                _ => null
            };

            if (readOutput != null)
                return (T)Convert.ChangeType(readOutput, typeof(T));
            return default;
        }
        public T ReadMemory<T>(UIntPtr address, string code, string file = "")
        {
            object readOutput = Type.GetTypeCode(typeof(T)) switch
            {
                TypeCode.Byte => ReadByte(address, code, file),
                TypeCode.Int16 => ReadShort(address, code, file),
                TypeCode.Int32 => ReadInt(address, code, file),
                TypeCode.Int64 => ReadLong(address, code, file),
                TypeCode.UInt16 => ReadUShort(address, code, file),
                TypeCode.UInt32 => ReadUInt(address, code, file),
                TypeCode.UInt64 => ReadULong(address, code, file),
                TypeCode.Single => ReadFloat(address, code, file),
                TypeCode.Double => ReadDouble(address, code, file),
                TypeCode.String => ReadString(address, code, file),
                _ => null
            };

            if (readOutput != null)
                return (T)Convert.ChangeType(readOutput, typeof(T));
            return default;
        }

        ConcurrentDictionary<string, CancellationTokenSource> _readTokenSrcs = new();
        /// <summary>
        /// Reads a memory address, keeps value in UI object. Ex: BindToUI("0x12345678,0x02,0x05", v => ObjName.Invoke((MethodInvoker)delegate { if (String.Compare(v, ObjName.Text) != 0) { ObjName.Text = v; } }));
        /// </summary>
        /// <param name="address">Your code or INI file variable name</param>
        /// <param name="uiObject">Returning variable to bind to UI object. See example in summary.</param>
        /// <param name="file">OPTIONAL: INI file path and file name with extension</param>
        public void BindToUi(string address, Action<string> uiObject, string file = "")
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            if (_readTokenSrcs.ContainsKey(address))
            {
                try
                {
                    _readTokenSrcs[address].Cancel();
                    _readTokenSrcs.TryRemove(address, out _);
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

            _readTokenSrcs.TryAdd(address, cts);

            Task.Factory.StartNew(() =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    uiObject(ReadMemory<string>(address, file));
                    Thread.Sleep(100);
                }
            },
            cts.Token);
        }
    }
}
