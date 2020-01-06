using System;
using System.Threading;

namespace Datalogger
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            I2CDisplay disp = new I2CDisplay(0x49);
            BMP280Sensor sens = new BMP280Sensor();
            while (true)
            {
                Console.WriteLine(sens.bmp280_get_temp());
                Thread.Sleep(1000);
            }
            GPIOModule mod = new GPIOModule(); // static instead of object?
            //disp.write("-1.1");
            //disp.writeLow("<C");
            //disp.flush();
            while (true)
            {
                GPIOStatus status = GPIOStatus.Temperature;//mod.getStatus();
                double data = sens.getData(status);
                switch (status)
                {
                    case GPIOStatus.Temperature:
                        disp.write(data.ToString()); // TODO: trim to 1 decimal point
                        disp.writeLow("<C");
                        break;

                    case GPIOStatus.Pressure:
                        disp.write(data.ToString());
                        disp.writeLow("pa");//TODO: display string
                        break;
                }
                disp.flush();
                Thread.Sleep(1000);
            }
        }
    }
}