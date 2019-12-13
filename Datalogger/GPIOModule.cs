using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Datalogger
{
    public class GPIOModule
    {
        public GPIOStatus getStatus()
        {
            var dev = File.OpenRead("/dev/boi");
            byte[] data = new byte[1];
            dev.Read(data, 0, 1);
            dev.Close();
            switch (data[0])
            {
                case 0:
                    return GPIOStatus.Temperature;

                case 1:
                    return GPIOStatus.Pressure;

                default:
                    throw new IOException(String.Format("Could not read /dev/boi output should be 0 or 1 (binary) but was {0}", data[0]));
            }
        }
    }
}