using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Memory;

// HOW TO MAKE YOUR OWN TRAINER
// 1. Add reference to compiled Memory.dll file.
// 2. Uncheck prefer 32bit in project build properties OR change compiler from `Any CPU` to `x64`.
// 3. Add a new app.manifest file, chanage requestedExecutionLevel level to requireAdministrator.
// Visit https://github.com/erfg12/memory.dll/wiki for full documentation.

// NOTE: To view 'Classes/Processing.cs' file properly, you must use View Code. 

namespace TestApplication
{
    public partial class TrainerForm : Form
    {
        public TrainerForm()
        {
            InitializeComponent();
        }

        Mem m = new Mem(); // Declare m as our Memory.dll function reference variable.

        private void TrainerForm_Shown(object sender, EventArgs e)
        {
            // set our combobox defaults after the trainer is shown (visible)
            ProcTypeBox.SelectedIndex = 0;
            ReadTypeBox.SelectedIndex = 0;
            WriteTypeBox.SelectedIndex = 2;
        }

        private void OpenProcessBtn_Click(object sender, EventArgs e)
        {
            BackgroundWork.RunWorkerAsync();
        }

        private void WriteButton_Click(object sender, EventArgs e)
        {
            if (ProcOpen)
                m.WriteMemory(WriteTextBox.Text, WriteTypeBox.Text, WriteValueBox.Text);
        }

        private void ReadButton_Click(object sender, EventArgs e)
        {
            if (ProcOpen)
                ReadTextBox.Text = ReadOutput(ReadValueBox.Text, ReadTypeBox.Text);
        }

        private void ScanButton_Click(object sender, EventArgs e)
        {
            Task taskA = new Task(() => SampleAoBScan(AobScanTextBox.Text));
            taskA.Start();
        }

        private void BrowseDLLButton_Click(object sender, EventArgs e)
        {
            if (DLLOpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                DLLTextBox.Text = DLLOpenFileDialog.FileName;
            }
        }

        private void InjectButton_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(DLLTextBox.Text))
            {
                if (m.InjectDll(DLLTextBox.Text))
                {
                    MessageBox.Show("DLL Injected Successfully!");
                } else
                {
                    MessageBox.Show("DLL Failed To Inject!");
                }
            }
        }

        private void SendPipeButton_Click(object sender, EventArgs e)
        {
            Thread ClientThread = new Thread(() => NewThreadStartClient(PipeMsgTextBox.Text, PipeNameTextBox.Text));
            ClientThread.Start();
        }

        private void BackgroundWork_DoWork(object sender, DoWorkEventArgs e)
        {
            // infinite loop that checks if the process is available and open, if not, modify the UI.
            while (true)
            {
                OpenTheProc();
                System.Threading.Thread.Sleep(1000);
            }
        }

        private void UpdateModulesButton_Click(object sender, EventArgs e)
        {
            if (ProcOpen)
                GetModuleList();
        }
    }
}
