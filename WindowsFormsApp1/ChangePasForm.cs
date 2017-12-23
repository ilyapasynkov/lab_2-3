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
using System.Security.Cryptography;

namespace WindowsFormsApp1
{
    public partial class ChangePasForm : Form
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

        void readmbr()
        {
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
                this.Invoke(new Action(() => {
                    txtPas.Text = "";
                }));
                mbrData = new byte[512];

                disk.Read(mbrData, 0, 512);

                for (int i = 384; i < txtPasOld.Text.Length + 384; i++)
                {
                    txtPas.Invoke(new Action(() => { txtPas.Text += (char)mbrData[i]; }));

                }

            }
        }

        Func b;
        public ChangePasForm()
        {
            b = new Func();
            InitializeComponent();
        }

        private void btnReg_Click(object sender, EventArgs e)
        {            
            if (txtPasOld.Text != "" || txtPasNew.Text != "")
            {
                string PasOld; 
              
                PasOld = b.Encode(txtPasOld.Text, Func.Key);
               
                if(PasOld == txtPas.Text)
                {
                    string EncodePas; 
                    
                    Func.Key = b.GenKey();
                    
                    EncodePas = b.Encode(txtPasNew.Text, Func.Key);
                   
                    b.WriteInFile(EncodePas);
                    
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
                    LabelKeyForm1 f = new LabelKeyForm1();

                    this.Hide();
                    f.ShowDialog();
                    this.Close();
                }
                else
                    MessageBox.Show("Неправильный пароль!");
            }

            // (1).
            else if (txtPasOld.Text == "" || txtPasNew.Text == "")
            {
                MessageBox.Show("Одно (или оба) из полей для ввода пароля пустое!");
            }

            txtPasOld.Clear();
            txtPasNew.Clear();
        }

        private void txtPasNew_Click(object sender, EventArgs e)
        {
            if (txtPasOld.Text != "")
            {
                Thread potok = new Thread(readmbr);
                potok.Start();
            }
        }
    }
}
