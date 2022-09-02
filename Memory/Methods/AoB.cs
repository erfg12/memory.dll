using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static Memory.Imps;

namespace Memory
{
    public partial class Mem
    {
        /// <summary>
        /// Array of byte scan.
        /// </summary>
        /// <param name="search">array of bytes to search for, OR your ini code label.</param>
        /// <param name="writable">Include writable addresses in scan</param>
        /// <param name="executable">Include executable addresses in scan</param>
        /// <param name="file">ini file (OPTIONAL)</param>
        /// <returns>IEnumerable of all addresses found.</returns>
        public Task<IEnumerable<long>> AoBScan(string search, bool writable = false, bool executable = true, string file = "")
        {
            return AoBScan(0, long.MaxValue, search, writable, executable, false, file);
        }

        /// <summary>
        /// Array of byte scan.
        /// </summary>
        /// <param name="search">array of bytes to search for, OR your ini code label.</param>
        /// <param name="readable">Include readable addresses in scan</param>
        /// <param name="writable">Include writable addresses in scan</param>
        /// <param name="executable">Include executable addresses in scan</param>
        /// <param name="file">ini file (OPTIONAL)</param>
        /// <returns>IEnumerable of all addresses found.</returns>
        public Task<IEnumerable<long>> AoBScan(string search, bool readable, bool writable, bool executable, string file = "")
        {
            return AoBScan(0, long.MaxValue, search, readable, writable, executable, false, file);
        }


        /// <summary>
        /// Array of Byte scan.
        /// </summary>
        /// <param name="start">Your starting address.</param>
        /// <param name="end">ending address</param>
        /// <param name="search">array of bytes to search for, OR your ini code label.</param>
        /// <param name="file">ini file (OPTIONAL)</param>
        /// <param name="writable">Include writable addresses in scan</param>
        /// <param name="executable">Include executable addresses in scan</param>
        /// <param name="mapped">Include mapped addresses in scan</param>
        /// <returns>IEnumerable of all addresses found.</returns>
        public Task<IEnumerable<long>> AoBScan(long start, long end, string search, bool writable = false, bool executable = true, bool mapped = false, string file = "")
        {
            // Not including read only memory was scan behavior prior.
            return AoBScan(start, end, search, false, writable, executable, mapped, file);
        }

        /// <summary>
        /// Array of Byte scan.
        /// </summary>
        /// <param name="start">Your starting address.</param>
        /// <param name="end">ending address</param>
        /// <param name="search">array of bytes to search for, OR your ini code label.</param>
        /// <param name="file">ini file (OPTIONAL)</param>
        /// <param name="readable">Include readable addresses in scan</param>
        /// <param name="writable">Include writable addresses in scan</param>
        /// <param name="executable">Include executable addresses in scan</param>
        /// <param name="mapped">Include mapped addresses in scan</param>
        /// <returns>IEnumerable of all addresses found.</returns>
        public Task<IEnumerable<long>> AoBScan(long start, long end, string search, bool readable, bool writable, bool executable, bool mapped, string file = "")
        {
            return Task.Run(() =>
            {
                var memRegionList = new List<MemoryRegionResult>();

                string memCode = LoadCode(search, file);

                string[] stringByteArray = memCode.Split(' ');

                byte[] aobPattern = new byte[stringByteArray.Length];
                byte[] mask = new byte[stringByteArray.Length];

                for (int i = 0; i < stringByteArray.Length; i++)
                {
                    string ba = stringByteArray[i];

                    if (ba == "??" || (ba.Length == 1 && ba == "?"))
                    {
                        mask[i] = 0x00;
                        stringByteArray[i] = "0x00";
                    }
                    else if (char.IsLetterOrDigit(ba[0]) && ba[1] == '?')
                    {
                        mask[i] = 0xF0;
                        stringByteArray[i] = ba[0] + "0";
                    }
                    else if (char.IsLetterOrDigit(ba[1]) && ba[0] == '?')
                    {
                        mask[i] = 0x0F;
                        stringByteArray[i] = "0" + ba[1];
                    }
                    else
                        mask[i] = 0xFF;
                }


                for (int i = 0; i < stringByteArray.Length; i++)
                    aobPattern[i] = (byte)(Convert.ToByte(stringByteArray[i], 16) & mask[i]);

                GetSystemInfo(out SYSTEM_INFO sysInfo);

                UIntPtr procMinAddress = sysInfo.MinimumApplicationAddress;
                UIntPtr procMaxAddress = sysInfo.MaximumApplicationAddress;

                if (start < (long)procMinAddress.ToUInt64())
                    start = (long)procMinAddress.ToUInt64();

                if (end > (long)procMaxAddress.ToUInt64())
                    end = (long)procMaxAddress.ToUInt64();

                Debug.WriteLine("[DEBUG] memory scan starting... (start:0x" + start.ToString(MSize()) + " end:0x" + end.ToString(MSize()) + " time:" + DateTime.Now.ToString("h:mm:ss tt") + ")");
                UIntPtr currentBaseAddress = new UIntPtr((ulong)start);

                //Debug.WriteLine("[DEBUG] start:0x" + start.ToString("X8") + " curBase:0x" + currentBaseAddress.ToUInt64().ToString("X8") + " end:0x" + end.ToString("X8") + " size:0x" + memInfo.RegionSize.ToString("X8") + " vAloc:" + VirtualQueryEx(mProc.Handle, currentBaseAddress, out memInfo).ToUInt64().ToString());

            while (VirtualQueryEx(MProc.Handle, currentBaseAddress, out MEMORY_BASIC_INFORMATION memInfo).ToUInt64() != 0 &&
                       currentBaseAddress.ToUInt64() < (ulong)end &&
                       currentBaseAddress.ToUInt64() + (ulong)memInfo.RegionSize >
                       currentBaseAddress.ToUInt64())
                {
                    bool isValid = memInfo.State == MemCommit;
                    isValid &= memInfo.BaseAddress.ToUInt64() < procMaxAddress.ToUInt64();
                    isValid &= ((memInfo.Protect & Guard) == 0);
                    isValid &= ((memInfo.Protect & Noaccess) == 0);
                    isValid &= (memInfo.Type == MemPrivate) || (memInfo.Type == MemImage);
                    if (mapped)
                        isValid &= (memInfo.Type == MemMapped);

                    if (isValid)
                    {
                        bool isReadable = (memInfo.Protect & Readonly) > 0;

                        bool isWritable = ((memInfo.Protect & Readwrite) > 0) ||
                                          ((memInfo.Protect & Writecopy) > 0) ||
                                          ((memInfo.Protect & ExecuteReadwrite) > 0) ||
                                          ((memInfo.Protect & ExecuteWritecopy) > 0);

                        bool isExecutable = ((memInfo.Protect & Execute) > 0) ||
                                            ((memInfo.Protect & ExecuteRead) > 0) ||
                                            ((memInfo.Protect & ExecuteReadwrite) > 0) ||
                                            ((memInfo.Protect & ExecuteWritecopy) > 0);

                        isReadable &= readable;
                        isWritable &= writable;
                        isExecutable &= executable;

                        isValid &= isReadable || isWritable || isExecutable;
                    }

                    if (!isValid)
                    {
                        currentBaseAddress = new UIntPtr(memInfo.BaseAddress.ToUInt64() + (ulong)memInfo.RegionSize);
                        continue;
                    }

                    MemoryRegionResult memRegion = new MemoryRegionResult
                    {
                        CurrentBaseAddress = currentBaseAddress,
                        RegionSize = memInfo.RegionSize,
                        RegionBase = memInfo.BaseAddress
                    };

                    currentBaseAddress = new UIntPtr(memInfo.BaseAddress.ToUInt64() + (ulong)memInfo.RegionSize);

                //Console.WriteLine("SCAN start:" + memRegion.RegionBase.ToString() + " end:" + currentBaseAddress.ToString());

                if (memRegionList.Count > 0)
                    {
                        MemoryRegionResult previousRegion = memRegionList[^1];

                        if ((long)previousRegion.RegionBase + previousRegion.RegionSize == (long)memInfo.BaseAddress)
                        {
                            memRegionList[^1] = new MemoryRegionResult
                            {
                                CurrentBaseAddress = previousRegion.CurrentBaseAddress,
                                RegionBase = previousRegion.RegionBase,
                                RegionSize = previousRegion.RegionSize + memInfo.RegionSize
                            };

                            continue;
                        }
                    }

                    memRegionList.Add(memRegion);
                }

                ConcurrentBag<long> bagResult = new();

                Parallel.ForEach(memRegionList,
                                 (item, _, _) =>
                                 {
                                     long[] compareResults = CompareScan(item, aobPattern, mask);

                                     foreach (long result in compareResults)
                                         bagResult.Add(result);
                                 });

                Debug.WriteLine("[DEBUG] memory scan completed. (time:" + DateTime.Now.ToString("h:mm:ss tt") + ")");

                return bagResult.ToList().OrderBy(c => c).AsEnumerable();
            });
        }

        /// <summary>
        /// Array of bytes scan
        /// </summary>
        /// <param name="code">Starting address or ini label</param>
        /// <param name="end">ending address</param>
        /// <param name="search">array of bytes to search for or your ini code label</param>
        /// <param name="file">ini file</param>
        /// <returns>First address found</returns>
        public async Task<long> AoBScan(string code, long end, string search, string file = "")
        {
            long start = (long)GetCode(code, file).ToUInt64();

            return (await AoBScan(start, end, search, true, true, true, false, file)).FirstOrDefault();
        }

        private long[] CompareScan(MemoryRegionResult item, byte[] aobPattern, byte[] mask)
        {
            if (mask.Length != aobPattern.Length)
                throw new ArgumentException($"{nameof(aobPattern)}.Length != {nameof(mask)}.Length");

            IntPtr buffer = Marshal.AllocHGlobal((int)item.RegionSize);

            ReadProcessMemory(MProc.Handle, item.CurrentBaseAddress, buffer, (UIntPtr)item.RegionSize, out ulong bytesRead);

            int result = 0 - aobPattern.Length;
            List<long> ret = new List<long>();
            unsafe
            {
                do
                {

                    result = FindPattern((byte*)buffer.ToPointer(), (int)bytesRead, aobPattern, mask, result + aobPattern.Length);

                    if (result >= 0)
                        ret.Add((long)item.CurrentBaseAddress + result);

                } while (result != -1);
            }

            Marshal.FreeHGlobal(buffer);

            return ret.ToArray();
        }

        private int FindPattern(IReadOnlyList<byte> body, IReadOnlyList<byte> pattern, IReadOnlyList<byte> masks, int start = 0)
        {
            int foundIndex = -1;

            if (body.Count <= 0 || pattern.Count <= 0 || start > body.Count - pattern.Count ||
                pattern.Count > body.Count) return foundIndex;

            for (int index = start; index <= body.Count - pattern.Count; index++)
            {
                if ((body[index] & masks[0]) != (pattern[0] & masks[0])) continue;
                bool match = true;
                for (int index2 = 1; index2 <= pattern.Count - 1; index2++)
                {
                    if ((body[index + index2] & masks[index2]) == (pattern[index2] & masks[index2])) continue;
                    match = false;
                    break;

                }

                if (!match) continue;

                foundIndex = index;
                break;
            }

            return foundIndex;
        }

        private unsafe int FindPattern(byte* body, int bodyLength, IReadOnlyList<byte> pattern, IReadOnlyList<byte> masks, int start = 0)
        {
            int foundIndex = -1;

            if (bodyLength <= 0 || pattern.Count <= 0 || start > bodyLength - pattern.Count ||
                pattern.Count > bodyLength) return foundIndex;

            for (int index = start; index <= bodyLength - pattern.Count; index++)
            {
                if ((body[index] & masks[0]) != (pattern[0] & masks[0])) continue;
                bool match = true;
                for (int index2 = pattern.Count - 1; index2 >= 1; index2--)
                {
                    if ((body[index + index2] & masks[index2]) == (pattern[index2] & masks[index2])) continue;
                    match = false;
                    break;

                }

                if (!match) continue;

                foundIndex = index;
                break;
            }

            return foundIndex;
        }
    }
}