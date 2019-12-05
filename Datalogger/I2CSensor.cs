using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Datalogger
{
    public class I2CSensor
    {
        private readonly I2CDevice dev;
        private FileStream file;

        public I2CSensor(int address)
        {
            dev = new I2CDevice(address);
            file = File.OpenWrite("i2c-dev-" + address + ".csv");
        }

        public int getData()
        {
            byte[] data = dev.readData(1);
            file.Write(Encoding.ASCII.GetBytes(DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss")));
            file.Write(Encoding.ASCII.GetBytes(";"));
            file.Write(data); // TODO: correct to readable ascii
            file.Write(Encoding.ASCII.GetBytes("/n"));
            file.Flush();
            return data[0];
        }
    }
}