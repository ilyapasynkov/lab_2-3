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
    public partial class KeyForm : Form
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

                for (int i = 384; i < Func.Pas.Length + 384; i++)
                {
                    txtPas.Invoke(new Action(() => { txtPas.Text += (char)mbrData[i]; }));

                }

            }
        }

        Func b;
        public KeyForm()
        {
            b = new Func();
            InitializeComponent();
        }

        private void btnKey_Click(object sender, EventArgs e)
        {            
            string EncodePas;
            string EnterPas; 

            if (txtKey.Text == "")  
            {
                MessageBox.Show("Поле ключа не может быть пустым!");
            }
            else
            {             
                EnterPas = b.Encode(Func.Pas, txtKey.Text);
                
                if (txtPas.Text == EnterPas) 
                {                   
                    Func.Key = b.GenKey();
                    
                    EncodePas = b.Encode(Func.Pas, Func.Key);
                   
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
                {                    
                    MessageBox.Show(String.Format("Неправильный пароль или ключ!\nОсталось попыток: {0}", Func.PopytkaNum));
                    Func.PopytkaNum--;

                    if (Func.PopytkaNum < 0)
                    {
                        MessageBox.Show("Попытки закончились!");
                        Application.Exit();
                    }

                    LogForm f = new LogForm();
                    
                    this.Hide();
                    f.ShowDialog();
                    this.Close();
                }
            }
        }

        private void KeyForm_Load(object sender, EventArgs e)
        {
            Thread potok = new Thread(readmbr);
            potok.Start();
        }
    }
}
