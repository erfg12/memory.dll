using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Memory.Imps;

namespace Memory
{
    public partial class Mem
    {
        ConcurrentDictionary<string, CancellationTokenSource> FreezeTokenSrcs = new ConcurrentDictionary<string, CancellationTokenSource>();

        /// <summary>
        /// Freeze a value to an address.
        /// </summary>
        /// <param name="address">Your address</param>
        /// <param name="type">byte, 2bytes, bytes, float, int, string, double or long.</param>
        /// <param name="value">Value to freeze</param>
        /// <param name="file">ini file to read address from (OPTIONAL)</param>
        public bool FreezeValue(string address, string type, string value, string file = "")
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            if (FreezeTokenSrcs.ContainsKey(address))
            {
                Debug.WriteLine("Changing Freezing Address " + address + " Value " + value);
                try
                {
                    FreezeTokenSrcs[address].Cancel();
                    FreezeTokenSrcs.TryRemove(address, out _);
                }
                catch
                {
                    Debug.WriteLine("ERROR: Avoided a crash. Address " + address + " was not frozen.");
                    return false;
                }
            }
            else {
                Debug.WriteLine("Adding Freezing Address " + address + " Value " + value);
            }

            FreezeTokenSrcs.TryAdd(address, cts);

            Task.Factory.StartNew(() =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    WriteMemory(address, type, value, file);
                    Thread.Sleep(25);
                }
            },
            cts.Token);

            return true;
        }

        /// <summary>
        /// Unfreeze a frozen value at an address
        /// </summary>
        /// <param name="address">address where frozen value is stored</param>
        public void UnfreezeValue(string address)
        {
            Debug.WriteLine("Un-Freezing Address " + address);
            try
            {
                lock (FreezeTokenSrcs)
                {
                    FreezeTokenSrcs[address].Cancel();
                    FreezeTokenSrcs.TryRemove(address, out _);
                }
            }
            catch
            {
                Debug.WriteLine("ERROR: Address " + address + " was not frozen.");
            }
        }

        ///<summary>
        ///Write to memory address. See https://github.com/erfg12/memory.dll/wiki/writeMemory() for more information.
        ///</summary>
        ///<param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        ///<param name="type">byte, 2bytes, bytes, float, int, string, double or long.</param>
        ///<param name="write">value to write to address.</param>
        ///<param name="file">path and name of .ini file (OPTIONAL)</param>
        ///<param name="stringEncoding">System.Text.Encoding.UTF8 (DEFAULT). Other options: ascii, unicode, utf32, utf7</param>
        ///<param name="RemoveWriteProtection">If building a trainer on an emulator (Ex: RPCS3) you'll want to set this to false</param>
        public bool WriteMemory(string code, string type, string write, string file = "", System.Text.Encoding stringEncoding = null, bool RemoveWriteProtection = true)
        {
            byte[] memory = new byte[4];
            int size = 4;

            UIntPtr theCode;
            theCode = GetCode(code, file);

            if (theCode == null || theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return false;

            if (type.ToLower() == "float")
            {
                write = Convert.ToString(float.Parse(write, CultureInfo.InvariantCulture));
                memory = BitConverter.GetBytes(Convert.ToSingle(write));
                size = 4;
            }
            else if (type.ToLower() == "int")
            {
                memory = BitConverter.GetBytes(Convert.ToInt32(write));
                size = 4;
            }
            else if (type.ToLower() == "byte")
            {
                memory = new byte[1];
                memory[0] = Convert.ToByte(write, 16);
                size = 1;
            }
            else if (type.ToLower() == "2bytes")
            {
                memory = new byte[2];
                memory[0] = (byte)(Convert.ToInt32(write) % 256);
                memory[1] = (byte)(Convert.ToInt32(write) / 256);
                size = 2;
            }
            else if (type.ToLower() == "bytes")
            {
                if (write.Contains(",") || write.Contains(" ")) //check if it's a proper array
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
                else //wasnt array, only 1 byte
                {
                    memory = new byte[1];
                    memory[0] = Convert.ToByte(write, 16);
                    size = 1;
                }
            }
            else if (type.ToLower() == "double")
            {
                memory = BitConverter.GetBytes(Convert.ToDouble(write));
                size = 8;
            }
            else if (type.ToLower() == "long")
            {
                memory = BitConverter.GetBytes(Convert.ToInt64(write));
                size = 8;
            }
            else if (type.ToLower() == "string")
            {
                if (stringEncoding == null)
                    memory = System.Text.Encoding.UTF8.GetBytes(write);
                else
                    memory = stringEncoding.GetBytes(write);
                size = memory.Length;
            }

            //Debug.Write("DEBUG: Writing bytes [TYPE:" + type + " ADDR:" + theCode + "] " + String.Join(",", memory) + Environment.NewLine);
            MemoryProtection OldMemProt = 0x00;
            bool WriteProcMem = false;
            //if (RemoveWriteProtection)
            //    ChangeProtection(code, MemoryProtection.ExecuteReadWrite, out OldMemProt, file); // change protection
            WriteProcMem = WriteProcessMemory(mProc.Handle, theCode, memory, (UIntPtr)size, IntPtr.Zero);
            //if (RemoveWriteProtection)
            //    ChangeProtection(code, OldMemProt, out _, file); // restore
            return WriteProcMem;
        }

        /// <summary>
        /// Write to address and move by moveQty. Good for byte arrays. See https://github.com/erfg12/memory.dll/wiki/Writing-a-Byte-Array for more information.
        /// </summary>
        ///<param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        ///<param name="type">byte, bytes, float, int, string or long.</param>
        /// <param name="write">byte to write</param>
        /// <param name="MoveQty">quantity to move</param>
        /// <param name="file">path and name of .ini file (OPTIONAL)</param>
        /// <param name="SlowDown">milliseconds to sleep between each byte</param>
        /// <returns></returns>
        public bool WriteMove(string code, string type, string write, int MoveQty, string file = "", int SlowDown = 0)
        {
            byte[] memory = new byte[4];
            int size = 4;

            UIntPtr theCode;
            theCode = GetCode(code, file);

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
            else if (type == "double")
            {
                memory = BitConverter.GetBytes(Convert.ToDouble(write));
                size = 8;
            }
            else if (type == "long")
            {
                memory = BitConverter.GetBytes(Convert.ToInt64(write));
                size = 8;
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

            UIntPtr newCode = UIntPtr.Add(theCode, MoveQty);

            //Debug.Write("DEBUG: Writing bytes [TYPE:" + type + " ADDR:[O]" + theCode + " [N]" + newCode + " MQTY:" + MoveQty + "] " + String.Join(",", memory) + Environment.NewLine);
            Thread.Sleep(SlowDown);
            return WriteProcessMemory(mProc.Handle, newCode, memory, (UIntPtr)size, IntPtr.Zero);
        }

        /// <summary>
        /// Write byte array to addresses.
        /// </summary>
        /// <param name="code">address to write to</param>
        /// <param name="write">byte array to write</param>
        /// <param name="file">path and name of ini file. (OPTIONAL)</param>
        public void WriteBytes(string code, byte[] write, string file = "")
        {
            UIntPtr theCode;
            theCode = GetCode(code, file);
            WriteProcessMemory(mProc.Handle, theCode, write, (UIntPtr)write.Length, IntPtr.Zero);
        }

        /// <summary>
        /// Takes an array of 8 booleans and writes to a single byte
        /// </summary>
        /// <param name="code">address to write to</param>
        /// <param name="bits">Array of 8 booleans to write</param>
        /// <param name="file">path and name of ini file. (OPTIONAL)</param>
        public void WriteBits(string code, bool[] bits, string file = "")
        {
            if (bits.Length != 8)
                throw new ArgumentException("Not enough bits for a whole byte", nameof(bits));

            byte[] buf = new byte[1];

            UIntPtr theCode = GetCode(code, file);

            for (var i = 0; i < 8; i++)
            {
                if (bits[i])
                    buf[0] |= (byte)(1 << i);
            }

            WriteProcessMemory(mProc.Handle, theCode, buf, (UIntPtr)1, IntPtr.Zero);
        }

        /// <summary>
        /// Write byte array to address
        /// </summary>
        /// <param name="address">Address to write to</param>
        /// <param name="write">Byte array to write to</param>
        public void WriteBytes(UIntPtr address, byte[] write)
        {
            WriteProcessMemory(mProc.Handle, address, write, (UIntPtr)write.Length, out IntPtr bytesRead);
        }
    }
}
