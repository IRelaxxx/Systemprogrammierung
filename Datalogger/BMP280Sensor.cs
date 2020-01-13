using System;
using System.IO;
using System.Text;

namespace Datalogger
{
    public class BMP280Sensor
    {
        internal struct bmp280_calib_data
        {
            public UInt16 dig_T1;
            public Int16 dig_T2;
            public Int16 dig_T3;
            public UInt16 dig_P1;
            public Int16 dig_P2;
            public Int16 dig_P3;
            public Int16 dig_P4;
            public Int16 dig_P5;
            public Int16 dig_P6;
            public Int16 dig_P7;
            public Int16 dig_P8;
            public Int16 dig_P9;

            public Int32 t_fine;
        }

        internal struct bmp280_uncomp_data
        {
            public Int32 temperature;
            public UInt32 pressure;
        }

        private FileStream tempFile;
        private FileStream presFile;
        private bmp280_calib_data calib;
        private I2CDevice i2c_sensor;
        private DateTime lastDay; // If today way your last Day ...

        public BMP280Sensor()
        {
            lastDay = DateTime.Today;
            openfiles();

            i2c_sensor = new I2CDevice(0x77);
            sensor_init();
        }

        ~BMP280Sensor()
        {
            tempFile.Close();
            presFile.Close();
        }

        private void openfiles()
        {
            if (tempFile != null) tempFile.Close();
            if (presFile != null) presFile.Close();
            tempFile = File.OpenWrite("temperature" + DateTime.Today.ToShortDateString().Replace('/', '-') + ".csv");
            presFile = File.OpenWrite("pressure" + DateTime.Today.ToShortDateString().Replace('/', '-') + ".csv");
        }

        private const byte BME280_REGISTER_DIG_T1 = 0x88; //temperature 1 address
        private const byte BME280_REGISTER_DIG_T2 = 0x8A; //temperature 2 address
        private const byte BME280_REGISTER_DIG_T3 = 0x8C; //temperature 3 address
        private const byte BMP280_TEMP_MSB_ADDR = 0xFA;
        private const byte BMP280_TEMP_LSB_ADDR = 0xFB;
        private const byte BMP280_TEMP_XLSB_ADDR = 0xFC;
        private const byte BMP280_CTRL_MEAS_ADDR = 0xF4;

        private void sensor_init()
        {
            calib = new bmp280_calib_data();
            calib.t_fine = 0;
            Span<byte> data = new byte[24];
            data = i2c_sensor.i2c_reg_read(BME280_REGISTER_DIG_T1, 24).AsSpan();
            calib.dig_T1 = BitConverter.ToUInt16(data.Slice(0, 2));
            calib.dig_T2 = BitConverter.ToInt16(data.Slice(2, 2));
            calib.dig_T3 = BitConverter.ToInt16(data.Slice(4, 2));
            calib.dig_P1 = BitConverter.ToUInt16(data.Slice(6, 2));
            calib.dig_P2 = BitConverter.ToInt16(data.Slice(8, 2));
            calib.dig_P3 = BitConverter.ToInt16(data.Slice(10, 2));
            calib.dig_P4 = BitConverter.ToInt16(data.Slice(12, 2));
            calib.dig_P5 = BitConverter.ToInt16(data.Slice(14, 2));
            calib.dig_P6 = BitConverter.ToInt16(data.Slice(16, 2));
            calib.dig_P7 = BitConverter.ToInt16(data.Slice(18, 2));
            calib.dig_P8 = BitConverter.ToInt16(data.Slice(20, 2));
            calib.dig_P9 = BitConverter.ToInt16(data.Slice(22, 2));
            i2c_sensor.i2c_reg_write(BMP280_CTRL_MEAS_ADDR, new byte[] { 0b01010111 }, 1); // Temp OS x2(010) Pres OS x 16(101) Normal mode(11)
            i2c_sensor.i2c_reg_write(0xF5/*config register*/, new byte[] { 0b10001000 }, 1); // 500 ms standby(100) Filter x4 (010)(best guess) padding(0) Spi off(0)
        }

        private bmp280_uncomp_data bmp280_get_uncomp_data()
        {
            bmp280_uncomp_data data = new bmp280_uncomp_data();
            byte[] temp = new byte[6];
            temp = i2c_sensor.i2c_reg_read(0xF7/*BMP280_PRES_MSB_ADDR*/, 6);

            data.pressure = (UInt32)((((UInt32)(temp[0])) << 12) | (((UInt32)(temp[1])) << 4) | ((UInt32)temp[2] >> 4));
            data.temperature = (Int32)((((Int32)(temp[3])) << 12) | (((Int32)(temp[4])) << 4) | (((Int32)(temp[5])) >> 4));

            return data;
        }

        private double bmp280_calc_temp_double(Int32 raw)
        {
            double var1 = (((double)raw) / 16384.0 - ((double)calib.dig_T1) / 1024.0) *
                   ((double)calib.dig_T2);
            double var2 =
                ((((double)raw) / 131072.0 - ((double)calib.dig_T1) / 8192.0) *
                 (((double)raw) / 131072.0 - ((double)calib.dig_T1) / 8192.0)) *
                ((double)calib.dig_T3);
            calib.t_fine = (Int32)(var1 + var2);
            return (var1 + var2) / 5120.0;
        }

        // 64 bit version
        private double bmp280_calc_pres_double(UInt32 raw)
        {
            double var1 = ((double)calib.t_fine / 2.0) - 64000.0;
            double var2 = var1 * var1 * ((double)calib.dig_P6) / 32768.0;
            var2 = var2 + var1 * ((double)calib.dig_P5) * 2.0;
            var2 = (var2 / 4.0) + (((double)calib.dig_P4) * 65536.0);
            var1 = (((double)calib.dig_P3) * var1 * var1 / 524288.0 + ((double)calib.dig_P2) * var1) /
                   524288.0;
            var1 = (1.0 + var1 / 32768.0) * ((double)calib.dig_P1);

            double pressure = 1048576.0 - (double)raw;
            if (var1 < 0 || var1 > 0)
            {
                pressure = (pressure - (var2 / 4096.0)) * 6250.0 / var1;
                var1 = ((double)calib.dig_P9) * (pressure) * (pressure) / 2147483648.0;
                var2 = (pressure) * ((double)calib.dig_P8) / 32768.0;
                pressure = pressure + (var1 + var2 + ((double)calib.dig_P7)) / 16.0;
                return pressure;
            }
            else
            {
                return 0;
            }
        }

        private double bmp280_get_temp()
        {
            bmp280_uncomp_data data = bmp280_get_uncomp_data();
            return bmp280_calc_temp_double((Int32)data.temperature);
        }

        private double bmp280_get_pres()
        {
            bmp280_uncomp_data data = bmp280_get_uncomp_data();
            return bmp280_calc_pres_double((UInt32)data.pressure);
        }

        public (double, double) getData()
        {
            // New day -> save to new files
            if (DateTime.Today > lastDay)
            {
                lastDay = DateTime.Today;
                openfiles();
            }
            bmp280_uncomp_data data = bmp280_get_uncomp_data();
            double temp = bmp280_calc_temp_double((Int32)data.temperature);
            double press = bmp280_calc_pres_double((UInt32)data.pressure);

            string date = DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss");
            tempFile.Write(Encoding.ASCII.GetBytes(date));
            presFile.Write(Encoding.ASCII.GetBytes(date));
            tempFile.Write(Encoding.ASCII.GetBytes(";"));
            presFile.Write(Encoding.ASCII.GetBytes(";"));
            tempFile.Write(Encoding.ASCII.GetBytes(temp.ToString()));
            presFile.Write(Encoding.ASCII.GetBytes(press.ToString()));
            tempFile.Write(Encoding.ASCII.GetBytes("\n"));
            presFile.Write(Encoding.ASCII.GetBytes("\n"));
            tempFile.Flush();
            presFile.Flush();
            return (temp, press);
        }
    }
}