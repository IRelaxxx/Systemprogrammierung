using System;
using System.Runtime.InteropServices;

namespace Datalogger
{
    internal class I2CDevice
    {
        private const int OPEN_READ_WRITE = 2;
        private const int I2C_SLAVE = 0x0703;

        [DllImport("libc.so.6", EntryPoint = "open", SetLastError = true)]
        internal static extern int Open(string fileName, int mode);

        [DllImport("libc.so.6", EntryPoint = "ioctl", SetLastError = true)]
        internal extern static int Ioctl(int fd, int request, int data);

        [DllImport("libc.so.6", EntryPoint = "read", SetLastError = true)]
        internal static extern int Read(int handle, byte[] data, int length);

        [DllImport("libc.so.6", EntryPoint = "write", SetLastError = true)]
        internal static extern int Write(int handle, byte[] data, int length);

        [DllImport("libc.so.6", EntryPoint = "close", SetLastError = true)]
        internal static extern int Close(int busHandle);

        private readonly byte address;

        public I2CDevice(byte address)
        {
            this.address = address;
        }

        public int i2c_reg_write(byte reg_addr, byte[] reg_data, UInt16 length)
        {
            /* init i2c */
            int fd;
            if ((fd = Open("/dev/i2c-1", OPEN_READ_WRITE)) < 0)
            {
                Console.WriteLine("Open failed errno: {0}\n", Marshal.GetLastWin32Error());
                return -1;
            }

            if (Ioctl(fd, I2C_SLAVE, address) < 0)
            {
                Console.WriteLine("ioctl failed errno: {0}\n", Marshal.GetLastWin32Error());
                return -1;
            }
            // Write reg address
            byte[] newData = new byte[length + 1];
            newData[0] = reg_addr;
            Array.Copy(reg_data, 0, newData, 1, length);
            if (Write(fd, newData, 1 + length) < 0)
            {
                Console.WriteLine("i2c_write: could not write reg addr errno {0}, a\n", Marshal.GetLastWin32Error());
                return -1;
            }
            if (Close(fd) < 0)
            {
                Console.WriteLine("close failed errno {0}\n", Marshal.GetLastWin32Error());
                return -1;
            }
            return 0;
        }

        public byte[] i2c_reg_read(byte reg_addr, UInt16 length)
        {
            /* Implement the I2C read routine according to the target machine. */
            int fd;
            if ((fd = Open("/dev/i2c-1", OPEN_READ_WRITE)) < 0)
            {
                Console.WriteLine("Open failed errno: {0}\n", Marshal.GetLastWin32Error());
                return null;
            }

            if (Ioctl(fd, I2C_SLAVE, address) < 0)
            {
                Console.WriteLine("ioctl failed errno: {0}\n", Marshal.GetLastWin32Error());
                return null;
            }
            // Write reg address
            if (reg_addr != -1)
            {
                if (Write(fd, new byte[] { reg_addr }, 1) != 1)
                {
                    Console.WriteLine("could not write reg addr errno {0}\n", Marshal.GetLastWin32Error());
                    return null;
                }
            }
            byte[] data = new byte[length];
            if (Read(fd, data, length) < 0)
            {
                Console.WriteLine("read failed. errno {0}\n", Marshal.GetLastWin32Error());
                return null;
            }
            if (Close(fd) < 0)
            {
                Console.WriteLine("close failed errno {0}\n", Marshal.GetLastWin32Error());
                return null;
            }
            return data;
        }
    }
}