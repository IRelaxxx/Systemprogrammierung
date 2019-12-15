using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Datalogger
{
    public class BMP280Sensor
    {
        [DllImport("libBMP280.so", EntryPoint = "bmp280lib_init", SetLastError = true)]
        internal static extern void bmp280lib_init();

        [DllImport("libBMP280.so", EntryPoint = "bmp280lib_get_temp", SetLastError = true)]
        internal extern static double bmp280lib_get_temp();

        [DllImport("libBMP280.so", EntryPoint = "bmp280lib_get_pres", SetLastError = true)]
        internal extern static double bmp280lib_get_pres();

        private FileStream tempFile;
        private FileStream presFile;

        public BMP280Sensor()
        {
            tempFile = File.OpenWrite("temperature.csv");
            presFile = File.OpenWrite("pressure.csv");
            bmp280lib_init();
        }

        ~BMP280Sensor()
        {
            tempFile.Close();
            presFile.Close();
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
            tempFile.Write(Encoding.ASCII.GetBytes("/n"));
            presFile.Write(Encoding.ASCII.GetBytes("/n"));
            tempFile.Flush();
            presFile.Flush();
            return status == GPIOStatus.Temperature ? temp : pres;
        }
    }
}