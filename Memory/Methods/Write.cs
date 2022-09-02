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
        ConcurrentDictionary<string, CancellationTokenSource> _freezeTokenSrcs =
            new ConcurrentDictionary<string, CancellationTokenSource>();

        /// <summary>
        /// Freeze a value to an address.
        /// </summary>
        /// <param name="address">Your address</param>
        /// <param name="value">Value to freeze</param>
        /// <param name="file">ini file to read address from (OPTIONAL)</param>
        public bool FreezeValue<T>(string address, T value, string file = "")
        {
            CancellationTokenSource cts = new();

            lock (_freezeTokenSrcs)
            {
                if (_freezeTokenSrcs.ContainsKey(address))
                {
                    Debug.WriteLine("Changing Freezing Address " + address + " Value " + value.ToString());
                    try
                    {
                        _freezeTokenSrcs[address].Cancel();
                        _freezeTokenSrcs.TryRemove(address, out _);
                    }
                    catch
                    {
                        Debug.WriteLine("ERROR: Avoided a crash. Address " + address + " was not frozen.");
                        return false;
                    }
                }
                else
                {
                    Debug.WriteLine("Adding Freezing Address " + address + " Value " + value);
                }

                _freezeTokenSrcs.TryAdd(address, cts);
            }

            Task.Factory.StartNew(() =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        WriteMemory(address, value, file);
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
                lock (_freezeTokenSrcs)
                {
                    _freezeTokenSrcs[address].Cancel();
                    _freezeTokenSrcs.TryRemove(address, out _);
                }
            }
            catch
            {
                Debug.WriteLine("ERROR: Address " + address + " was not frozen.");
            }
        }

        /// <summary>
        /// Write to memory address. See https://github.com/erfg12/memory.dll/wiki/writeMemory() for more information.
        /// </summary>
        /// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        /// <param name="write">value to write to address.</param>
        /// <param name="file">path and name of .ini file (OPTIONAL)</param>
        /// <param name="stringEncoding">System.Text.Encoding.UTF8 (DEFAULT). Other options: ascii, unicode, utf32, utf7</param>
        /// <param name="removeWriteProtection">If building a trainer on an emulator (Ex: RPCS3) you'll want to set this to false</param>
        public bool WriteMemory<T>(string code, T write, string file = "", Encoding stringEncoding = null,
            bool removeWriteProtection = true)
        {
            byte[] memory = new byte[4];
            int size = 4;

            UIntPtr theCode = GetCode(code, file);

            if (theCode == UIntPtr.Zero || theCode.ToUInt64() < 0x10000)
                return false;
    
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Single:
                    memory = BitConverter.GetBytes(Convert.ToSingle(write));
                    size = 4;
                    break;
                case TypeCode.Int32:
                    memory = BitConverter.GetBytes(Convert.ToInt32(write));
                    size = 4;
                    break;
                case TypeCode.Byte:
                    memory = new byte[1];
                    memory[0] = Convert.ToByte(write);
                    size = 1;
                    break;
                case TypeCode.Int16:
                    memory = new byte[2];
                    memory[0] = (byte)(Convert.ToInt32(write) % 256);
                    memory[1] = (byte)(Convert.ToInt32(write) / 256);
                    size = 2;
                    break;
                case TypeCode.Int64:
                    memory = BitConverter.GetBytes(Convert.ToInt64(write));
                    size = 8;
                    break;
                case TypeCode.Double:
                    memory = BitConverter.GetBytes(Convert.ToDouble(write));
                    size = 8;
                    break;
                case TypeCode.String:
                    memory = stringEncoding == null
                        ? Encoding.UTF8.GetBytes(Convert.ToString(write)!)
                        : stringEncoding.GetBytes(Convert.ToString(write)!);
                    size = memory.Length;
                    break;
                case TypeCode.Object: //assume it's a byte array because it probably is
                    byte[] bytes = (byte[])Convert.ChangeType(write, typeof(T));
                    int c = bytes.Length;
                    memory = new byte[c];

                    for (int i = 0; i < c; i++)
                    {
                        memory[i] = bytes[i];
                    }

                    size = bytes.Length;
                    break;
            }

            //Debug.Write("DEBUG: Writing bytes [TYPE:" + type + " ADDR:" + theCode + "] " + String.Join(",", memory) + Environment.NewLine);
            MemoryProtection oldMemProt = 0x00;
            if (removeWriteProtection)
                ChangeProtection(code, MemoryProtection.ExecuteReadWrite, out oldMemProt, file); // change protection
            bool writeProcMem = WriteProcessMemory(MProc.Handle, theCode, memory, (UIntPtr)size, IntPtr.Zero);
            if (removeWriteProtection)
                ChangeProtection(code, oldMemProt, out _, file); // restore

            return writeProcMem;
        }
        
        public bool WriteMemory<T>(UIntPtr address, string code, T write, string file = "", Encoding stringEncoding = null,
            bool removeWriteProtection = true)
        {
            byte[] memory = new byte[4];
            int size = 4;

            if (address + LoadIntCode(code, file) == UIntPtr.Zero || (address + LoadIntCode(code, file)).ToUInt64() < 0x10000)
                return false;
    
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Single:
                    memory = BitConverter.GetBytes(Convert.ToSingle(write));
                    size = 4;
                    break;
                case TypeCode.Int32:
                    memory = BitConverter.GetBytes(Convert.ToInt32(write));
                    size = 4;
                    break;
                case TypeCode.Byte:
                    memory = new byte[1];
                    memory[0] = Convert.ToByte(write);
                    size = 1;
                    break;
                case TypeCode.Int16:
                    memory = new byte[2];
                    memory[0] = (byte)(Convert.ToInt32(write) % 256);
                    memory[1] = (byte)(Convert.ToInt32(write) / 256);
                    size = 2;
                    break;
                case TypeCode.Int64:
                    memory = BitConverter.GetBytes(Convert.ToInt64(write));
                    size = 8;
                    break;
                case TypeCode.Double:
                    memory = BitConverter.GetBytes(Convert.ToDouble(write));
                    size = 8;
                    break;
                case TypeCode.String:
                    memory = stringEncoding == null
                        ? Encoding.UTF8.GetBytes(Convert.ToString(write)!)
                        : stringEncoding.GetBytes(Convert.ToString(write)!);
                    size = memory.Length;
                    break;
                case TypeCode.Object: //assume it's a byte array because it probably is
                    byte[] bytes = (byte[])Convert.ChangeType(write, typeof(T));
                    int c = bytes.Length;
                    memory = new byte[c];

                    for (int i = 0; i < c; i++)
                    {
                        memory[i] = bytes[i];
                    }

                    size = bytes.Length;
                    break;
            }
//address + LoadIntCode(code, file)
            //Debug.Write("DEBUG: Writing bytes [TYPE:" + type + " ADDR:" + theCode + "] " + String.Join(",", memory) + Environment.NewLine);
            MemoryProtection oldMemProt = 0x00;
            if (removeWriteProtection)
                ChangePProtection(address, code, MemoryProtection.ExecuteReadWrite, out oldMemProt, file); // change protection
            bool writeProcMem = WriteProcessMemory(MProc.Handle, address + LoadIntCode(code, file), memory, (UIntPtr)size, IntPtr.Zero);
            if (removeWriteProtection)
                ChangePProtection(address, code, oldMemProt, out _, file); // restore
            
            return writeProcMem;
        }

        ///  <summary>
        ///  Write to address and move by moveQty. Good for byte arrays. See https://github.com/erfg12/memory.dll/wiki/Writing-a-Byte-Array for more information.
        ///  </summary>
        /// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
        ///  <param name="write">byte to write</param>
        ///  <param name="moveQty">quantity to move</param>
        ///  <param name="file">path and name of .ini file (OPTIONAL)</param>
        ///  <param name="slowDown">milliseconds to sleep between each byte</param>
        ///  <returns></returns>
        public bool WriteMove<T>(string code, T write, int moveQty, string file = "", int slowDown = 0)
        {
            byte[] memory = new byte[4];
            int size = 4;

            UIntPtr theCode = GetCode(code, file);

            //if (type == "float")
            //{
            //    memory = new byte[write.Length];
            //    memory = BitConverter.GetBytes(Convert.ToSingle(write));
            //    size = write.Length;
            //}
            //else if (type == "int")
            //{
            //    memory = BitConverter.GetBytes(Convert.ToInt32(write));
            //    size = 4;
            //}
            //else if (type == "double")
            //{
            //    memory = BitConverter.GetBytes(Convert.ToDouble(write));
            //    size = 8;
            //}
            //else if (type == "long")
            //{
            //    memory = BitConverter.GetBytes(Convert.ToInt64(write));
            //    size = 8;
            //}
            //else if (type == "byte")
            //{
            //    memory = new byte[1];
            //    memory[0] = Convert.ToByte(write, 16);
            //    size = 1;
            //}
            //else if (type == "string")
            //{
            //    memory = new byte[write.Length];
            //    memory = System.Text.Encoding.UTF8.GetBytes(write);
            //    size = write.Length;
            //}

            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Single:
                    memory = BitConverter.GetBytes(Convert.ToSingle(write));
                    break;
                case TypeCode.Int32:
                    memory = BitConverter.GetBytes(Convert.ToInt32(write));
                    break;
                case TypeCode.Double:
                    memory = BitConverter.GetBytes(Convert.ToDouble(write));
                    size = 8;
                    break;
                case TypeCode.Int64:
                    memory = BitConverter.GetBytes(Convert.ToInt64(write));
                    size = 8;
                    break;
                case TypeCode.Byte:
                    memory = new byte[1];
                    memory[0] = Convert.ToByte(write);
                    size = 1;
                    break;
                case TypeCode.String:
                    string writeString = Convert.ToString(write);
                    memory = Encoding.UTF8.GetBytes(writeString!);
                    size = writeString.Length;
                    break;
            }

            UIntPtr newCode = UIntPtr.Add(theCode, moveQty);

            //Debug.Write("DEBUG: Writing bytes [TYPE:" + type + " ADDR:[O]" + theCode + " [N]" + newCode + " MQTY:" + MoveQty + "] " + String.Join(",", memory) + Environment.NewLine);
            Thread.Sleep(slowDown);
            return WriteProcessMemory(MProc.Handle, newCode, memory, (UIntPtr)size, IntPtr.Zero);
        }

        /// <summary>
        /// Write byte array to addresses.
        /// </summary>
        /// <param name="code">address to write to</param>
        /// <param name="write">byte array to write</param>
        /// <param name="file">path and name of ini file. (OPTIONAL)</param>
        public void WriteBytes(string code, byte[] write, string file = "")
        {
            UIntPtr theCode = GetCode(code, file);
            WriteProcessMemory(MProc.Handle, theCode, write, (UIntPtr)write.Length, IntPtr.Zero);
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

            WriteProcessMemory(MProc.Handle, theCode, buf, (UIntPtr)1, IntPtr.Zero);
        }

        /// <summary>
        /// Write byte array to address
        /// </summary>
        /// <param name="address">Address to write to</param>
        /// <param name="write">Byte array to write to</param>
        public void WriteBytes(UIntPtr address, byte[] write)
        {
            WriteProcessMemory(MProc.Handle, address, write, (UIntPtr)write.Length, out IntPtr _);
        }
    }
}