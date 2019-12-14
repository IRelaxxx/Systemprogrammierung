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

        private readonly int address;

        public I2CDevice(int address)
        {
            this.address = address;
        }

        public byte[] readData(int length)
        {
            int fd = Open("/dev/i2c-1", OPEN_READ_WRITE);
            if (fd < 0) return null;
            var deviceReturncode = Ioctl(fd, I2C_SLAVE, address);
            if (deviceReturncode < 0) return null;
            byte[] data = new byte[length];
            var returnCode = Read(fd, data, length);
            if (returnCode < 0) return null;
            Close(fd);
            return data;
        }

        public int writeData(int length, byte[] data)
        {
            int fd = Open("/dev/i2c-1", OPEN_READ_WRITE);
            if (fd < 0)
            {
                int errno = Marshal.GetLastWin32Error();
                Console.WriteLine("Open errno: {0}", errno);
                return errno;
            }
            var deviceReturncode = Ioctl(fd, I2C_SLAVE, address);
            if (deviceReturncode < 0)
            {
                int errno = Marshal.GetLastWin32Error();
                Console.WriteLine("Icctl errno: {0}", errno);
                return errno;
            }
            var err = Write(fd, data, length);
            if (err < 0)
            {
                int errno = Marshal.GetLastWin32Error();
                Console.WriteLine("Write errno: {0}", errno);
                return errno;
            }
            Close(fd);
            return 0;
        }
    }
}