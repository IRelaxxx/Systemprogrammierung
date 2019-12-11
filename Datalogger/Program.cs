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
            I2CSensor sens = new I2CSensor(1);
            GPIOModule mod = new GPIOModule(); // static instead of object?
            disp.writeNumber(1);
            disp.flush();
           /* while (true)
            {
                GPIOStatus status = mod.getStatus();
                int data = sens.getData();
                disp.writeNumber(data % 10);
                disp.writeNumber(data / 10);
                disp.flush();
                Thread.Sleep(1000);
            }*/
        }
    }
}