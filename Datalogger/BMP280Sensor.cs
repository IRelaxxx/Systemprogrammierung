using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Datalogger
{
    internal struct bmp280_calib_data_temp
    {
        public UInt16 dig_T1;
        public Int16 dig_T2;
        public Int16 dig_T3;
    }

    internal struct bmp280_temp_data
    {
        public byte tmsb;
        public byte tlsb;
        public byte txsb;

        public UInt32 temperature;
    }

    public class BMP280Sensor
    {
        [DllImport("libBMP280.so", EntryPoint = "bmp280lib_init", SetLastError = true)]
        internal static extern void bmp280lib_init();

        [DllImport("libBMP280.so", EntryPoint = "bmp280lib_get_temp", SetLastError = true)]
        internal extern static double bmp280lib_get_temp();

        [DllImport("libBMP280.so", EntryPoint = "bmp280lib_get_press", SetLastError = true)]
        internal extern static double bmp280lib_get_pres();

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

        private FileStream tempFile;
        private FileStream presFile;
        private bmp280_calib_data_temp calib_temp;

        public BMP280Sensor()
        {
            tempFile = File.OpenWrite("temperature.csv");
            presFile = File.OpenWrite("pressure.csv");
            //bmp280lib_init();
            sensor_init();
        }

        ~BMP280Sensor()
        {
            tempFile.Close();
            presFile.Close();
        }

        private const byte BME280_REGISTER_DIG_T1 = 0x88; //temperature 1 address
        private const byte BME280_REGISTER_DIG_T2 = 0x8A; //temperature 2 address
        private const byte BME280_REGISTER_DIG_T3 = 0x8C; //temperature 3 address
        private const byte BMP280_TEMP_MSB_ADDR = (0xFA);
        private const byte BMP280_TEMP_LSB_ADDR = (0xFB);
        private const byte BMP280_TEMP_XLSB_ADDR = (0xFC);
        private const byte BMP280_CTRL_MEAS_ADDR = (0xF4);

        private void sensor_init()
        {
            calib_temp = new bmp280_calib_data_temp();
            calib_temp.dig_T1 = BitConverter.ToUInt16(i2c_reg_read(BME280_REGISTER_DIG_T1, 2));
            calib_temp.dig_T2 = BitConverter.ToInt16(i2c_reg_read(BME280_REGISTER_DIG_T2, 2));
            calib_temp.dig_T3 = BitConverter.ToInt16(i2c_reg_read(BME280_REGISTER_DIG_T3, 2));
            i2c_reg_write(BMP280_CTRL_MEAS_ADDR, new byte[] { 0b11100011 }, 1);
        }

        private bmp280_temp_data getRawData()
        {
            bmp280_temp_data data = new bmp280_temp_data();
            data.tmsb = i2c_reg_read(BMP280_TEMP_MSB_ADDR, 1)[0];
            data.tlsb = i2c_reg_read(BMP280_TEMP_LSB_ADDR, 1)[0];
            data.txsb = i2c_reg_read(BMP280_TEMP_XLSB_ADDR, 1)[0];

            data.temperature = 0;
            data.temperature = (data.temperature | data.tmsb) << 8;
            data.temperature = (data.temperature | data.tlsb) << 8;
            data.temperature = (data.temperature | data.txsb) >> 4;

            return data;
        }

        private Int32 bmp280_calc_temp(bmp280_calib_data_temp cal, UInt32 raw)
        {
            Int32 var1 = (Int32)(((raw >> 3) - ((Int32)cal.dig_T1 << 1)) * cal.dig_T2) >> 11;
            Int32 var2 = (Int32)(((((raw >> 4) - ((Int32)cal.dig_T1)) * ((raw >> 4) - ((Int32)cal.dig_T1))) >> 12) * cal.dig_T3) >> 14;

            return var1 + var2;
        }

        private float bmp280_temp_comp(Int32 temp)
        {
            float T = (temp * 5 + 128) >> 8;
            return T / 100;
        }

        public float bmp280_get_temp()
        {
            bmp280_temp_data data = getRawData();
            int temp = bmp280_calc_temp(calib_temp, data.temperature);
            return bmp280_temp_comp(temp);
        }

        public double getData(GPIOStatus status)
        {
            double temp = bmp280lib_get_temp();
            double pres = bmp280lib_get_pres();
            string date = DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss");
            tempFile.Write(Encoding.ASCII.GetBytes(date));
            presFile.Write(Encoding.ASCII.GetBytes(date));
            tempFile.Write(Encoding.ASCII.GetBytes(";"));
            presFile.Write(Encoding.ASCII.GetBytes(";"));
            tempFile.Write(Encoding.ASCII.GetBytes(temp.ToString()));
            presFile.Write(Encoding.ASCII.GetBytes(pres.ToString()));
            tempFile.Write(Encoding.ASCII.GetBytes("\n"));
            presFile.Write(Encoding.ASCII.GetBytes("\n"));
            tempFile.Flush();
            presFile.Flush();
            return status == GPIOStatus.Temperature ? temp : pres;
        }

        private byte[] readDi2c_write(int length)
        {
            int fd = Open("/dev/i2c-1", OPEN_READ_WRITE);
            if (fd < 0) return null;
            var deviceReturncode = Ioctl(fd, I2C_SLAVE, 0x77);
            if (deviceReturncode < 0) return null;
            byte[] data = new byte[length];
            var returnCode = Read(fd, data, length);
            if (returnCode < 0) return null;
            Close(fd);
            return data;
        }

        private int i2c_reg_write(byte reg_addr, byte[] reg_data, UInt16 length)
        {
            /* init i2c */
            int fd;
            if ((fd = Open("/dev/i2c-1", OPEN_READ_WRITE)) < 0)
            {
                Console.WriteLine("Open failed errno: {0}\n", Marshal.GetLastWin32Error());
                return -1;
            }

            if (Ioctl(fd, I2C_SLAVE, 0x77) < 0)
            {
                Console.WriteLine("ioctl failed errno: {0}\n", Marshal.GetLastWin32Error());
                return -1;
            }
            // Write reg address
            byte[] newData = new byte[length + 1];
            newData[0] = reg_addr;
            Array.Copy(reg_data, 0, newData, 1, length);
            if (Write(fd, newData, 1 + length) != 1)
            {
                Console.WriteLine("could not write reg addr errno {0}\n", Marshal.GetLastWin32Error());
                return -1;
            }
            if (Close(fd) < 0)
            {
                Console.WriteLine("close failed errno {0}\n", Marshal.GetLastWin32Error());
                return -1;
            }
            return 0;
        }

        private byte[] i2c_reg_read(byte reg_addr, UInt16 length)
        {
            /* Implement the I2C read routine according to the target machine. */
            int fd;
            if ((fd = Open("/dev/i2c-1", OPEN_READ_WRITE)) < 0)
            {
                Console.WriteLine("Open failed errno: {0}\n", Marshal.GetLastWin32Error());
                return null;
            }

            if (Ioctl(fd, I2C_SLAVE, 0x77) < 0)
            {
                Console.WriteLine("ioctl failed errno: {0}\n", Marshal.GetLastWin32Error());
                return null;
            }
            // Write reg address
            if (Write(fd, new byte[] { reg_addr }, 1) != 1)
            {
                Console.WriteLine("could not write reg addr errno {0}\n", Marshal.GetLastWin32Error());
                return null;
            }
            byte[] data = new byte[length];
            if (Read(fd, data, length) != length)
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

        public int writeData(int length, byte[] data)
        {
            int fd = Open("/dev/i2c-1", OPEN_READ_WRITE);
            if (fd < 0)
            {
                int errno = Marshal.GetLastWin32Error();
                Console.WriteLine("Open errno: {0}", errno);
                return errno;
            }
            var deviceReturncode = Ioctl(fd, I2C_SLAVE, 0x77);
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