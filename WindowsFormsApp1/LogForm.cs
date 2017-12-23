using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO; 
using Functions;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;


namespace WindowsFormsApp1
{
    public partial class LogForm : Form
    {
        byte[] mbrData;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern SafeFileHandle CreateFile(
            string lpFileName,
            [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
            [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
            [MarshalAs(UnmanagedType.U4)] FileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        private Func b;
        public LogForm()
        {
            b = new Func();
            InitializeComponent();
            search_external_drives(comboBox1);
            comboBox1.SelectedIndex = 0;
        }

        
        private void search_external_drives(ComboBox input) 
        {
            string mydrive;
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady && (d.DriveType == DriveType.Removable))
                {
                    mydrive = d.Name;
                    input.Items.Add(mydrive);
                }
            }
            if (input.Items.Count == 0)
            {
                input.Items.Add("Внешние носители отсутствуют");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Func.FlashDrive = comboBox1.Text.Remove(2);
            Func.Pas = txtPas.Text;

            
            if (!(File.Exists("\\test.txt")))
            {               
                string EncodePas; 
                
                Func.Key = b.GenKey();                 
                
                EncodePas = b.Encode(txtPas.Text, Func.Key);
                
                b.WriteInFile(EncodePas);

                // запись на флешку
                SafeFileHandle handle = CreateFile(
                lpFileName: @"\\.\" + Func.FlashDrive,
                dwDesiredAccess: FileAccess.Read,
                dwShareMode: FileShare.ReadWrite,
                lpSecurityAttributes: IntPtr.Zero,                              
                dwCreationDisposition: System.IO.FileMode.OpenOrCreate,
                dwFlagsAndAttributes: FileAttributes.Normal,
                 hTemplateFile: IntPtr.Zero);

                using (FileStream disk = new FileStream(handle, FileAccess.Read))
                {
                    mbrData = new byte[512];
                    disk.Read(mbrData, 0, 512);
                }

                handle = CreateFile(
                lpFileName: @"\\.\" + Func.FlashDrive,
                dwDesiredAccess: FileAccess.Write,
                dwShareMode: FileShare.ReadWrite,
                lpSecurityAttributes: IntPtr.Zero,
                dwCreationDisposition: System.IO.FileMode.OpenOrCreate,
                dwFlagsAndAttributes: FileAttributes.Normal,
                hTemplateFile: IntPtr.Zero);

                using (FileStream disk = new FileStream(handle, FileAccess.Write))
                {

                    for (int i = 0; i < EncodePas.Length; i++)
                        mbrData[384 + i] = (byte)EncodePas[i];

                    disk.Write(mbrData, 0, 512);
                }
                   
                this.Hide();
                LabelKeyForm1 f = new LabelKeyForm1();
                f.ShowDialog();
                this.Close();
                
            }           
            else
            {                
                KeyForm f = new KeyForm();
                
                this.Hide();
                f.ShowDialog();
                this.Close();
            }
            
        }

    }
}
