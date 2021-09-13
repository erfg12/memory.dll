using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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

            UIntPtr theCode;
            theCode = GetCode(code, file);
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
            UIntPtr theCode;
            theCode = GetCode(code, file);

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

            UIntPtr theCode;
            theCode = GetCode(code, file);
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
            UIntPtr theCode;
            theCode = GetCode(code, file);
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
            UIntPtr theCode;

            theCode = GetCode(code, file);

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
            UIntPtr theCode;
            theCode = GetCode(code, file);

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
            UIntPtr theCode;
            theCode = GetCode(code, file);

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
            UIntPtr theCode;
            theCode = GetCode(code, file);

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
            UIntPtr theCode;
            theCode = GetCode(code, file, 8);

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

            UIntPtr theCode;
            theCode = GetCode(code, file);

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
    }
}
