using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestApplication
{
    public partial class TrainerForm : Form
    {
        public bool ProcOpen = false;

        /// <summary>
        /// Process opening code. Generates a list of modules too.
        /// </summary>
        public void OpenTheProc()
        {
            if (String.Compare(ProcTypeBox.Text, "Name") == 0) // if combobox set to Name, use string
                ProcOpen = m.OpenProcess(ProcTextBox.Text);
            else // if combobox set to ID, use integer
                ProcOpen = m.OpenProcess(Convert.ToInt32(ProcTextBox.Text));

            if (ProcOpen) // if process opens successfully
            {
                foreach (KeyValuePair<string, IntPtr> kvp in m.modules) // iterate through process module list
                {
                    string[] arr = new string[4];
                    ListViewItem itm;
                    arr[0] = "0x" + kvp.Value.ToString("x8");
                    arr[1] = kvp.Key;
                    itm = new ListViewItem(arr);
                    ModuleList.Items.Add(itm);
                }
                ProcStatus.Text = "Open";
                ProcStatus.ForeColor = Color.Green;
            }
            else // on process open fail, show error message
            {
                MessageBox.Show("ERROR: Process open failed!");
            }
        }

        /// <summary>
        /// For the Read Address feature. Address goes in, value comes out.
        /// </summary>
        /// <param name="address">address to read from</param>
        /// <param name="type">type of value that should be returned</param>
        /// <returns></returns>
        public string ReadOutput(string address, string type)
        {
            string ReadOutput = "";

            switch (type)
            {
                case "string":
                    ReadOutput = m.ReadString(address);
                    break;
                case "int":
                    ReadOutput = m.ReadInt(address).ToString();
                    break;
                case "long":
                    ReadOutput = m.ReadLong(address).ToString();
                    break;
                case "byte":
                    ReadOutput = m.ReadByte(address).ToString();
                    break;
                case "double":
                    ReadOutput = m.ReadDouble(address).ToString();
                    break;
                case "float":
                    ReadOutput = m.ReadFloat(address).ToString();
                    break;
                case "UInt":
                    ReadOutput = m.ReadUInt(address).ToString();
                    break;
                case "2 byte":
                    ReadOutput = m.Read2Byte(address).ToString();
                    break;
                default:
                    ReadOutput = "";
                    break;
            }

            return ReadOutput;
        }

        // this function is async, which means it does not block other code
        public async void SampleAoBScan(string ScanPattern)
        {
            if (!ProcOpen)
                return;

            IEnumerable<long> AoBScanResults;

            // AoB scan and store it in AoBScanResults. We specify our start and end address regions to decrease scan time.
            if (String.Compare(StartAddrBox.Text, "") == 0 || String.Compare(EndAddrBox.Text, "") == 0)
            {
                AoBScanResults = await m.AoBScan(ScanPattern, false, true);
            }
            else
            {
                AoBScanResults = await m.AoBScan(Convert.ToInt64(StartAddrBox.Text), Convert.ToInt64(EndAddrBox.Text), ScanPattern, false, true);
            }
            
            // Ex: get the first found address, store it in the variable SingleAoBScanResult
            // long SingleAoBScanResult = AoBScanResults.FirstOrDefault();

            // iterate through each found address.
            foreach (long res in AoBScanResults)
            {
                string[] arr = new string[4];
                ListViewItem itm;
                arr[0] = res.ToString("x8");
                itm = new ListViewItem(arr);

                // because we run this in another thread, we need to invoke the UI thread elements
                AobScanList.Invoke((MethodInvoker) delegate
                {
                    AobScanList.Items.Add(itm);
                });
            }
        }
    }
}
