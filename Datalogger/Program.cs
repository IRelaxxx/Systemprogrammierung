using System;
using System.Collections.Generic;

namespace Datalogger
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            List<Pair<DateTime, int>> data = new List<Pair<DateTime, int>>();

            I2CDisplay disp = new I2CDisplay(48);
            I2CSensor sens = new I2CSensor(1);
            GPIOModule mod = new GPIOModule(); // static instead of object?
        }
    }
}