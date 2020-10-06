using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Memory;

// HOW TO MAKE YOUR OWN TRAINER
// 1. Add reference to compiled Memory.dll file.
// 2. Uncheck prefer 32bit in project build properties OR change compiler from `Any CPU` to `x64`.
// 3. Add a new app.manifest file, chanage requestedExecutionLevel level to requireAdministrator.
// Visit https://github.com/erfg12/memory.dll/wiki for full documentation.

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
            OpenTheProc();
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
    }
}
