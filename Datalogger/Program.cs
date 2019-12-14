using System;
using System.Collections.Generic;
using System.Threading;

namespace Datalogger
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            I2CDisplay disp = new I2CDisplay(0x49);
            BMP280Sensor sens = new BMP280Sensor();
            GPIOModule mod = new GPIOModule(); // static instead of object?
            while (true)
            {
                GPIOStatus status = mod.getStatus();
                double data = sens.getData(status);
                switch (status)
                {
                    case GPIOStatus.Temperature:
                        break;

                    case GPIOStatus.Pressure:
                        break;
                }
                disp.writeNumber(data % 10);
                disp.writeNumber(data / 10);
                disp.flush();
                Thread.Sleep(1000);
            }
        }
    }
}