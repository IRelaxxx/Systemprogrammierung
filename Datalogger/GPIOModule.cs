using System;
using System.IO;
using System.Text;

namespace Datalogger
{
    public class GPIOModule
    {
        private const string drvPath = "/dev/sysprog";

        public GPIOStatus getStatus()
        {
            using (var dev = File.OpenRead(drvPath))
            {
                byte[] data = new byte[2];
                dev.Read(data, 0, 2);
                dev.Close();
                string s = Encoding.ASCII.GetString(data);
                return (int.Parse(s)) switch
                {
                    0 => GPIOStatus.Temperature,
                    1 => GPIOStatus.Pressure,
                    _ => throw new IOException(string.Format("Could not read /dev/boi output should be 0 or 1 (binary) but was {0}", data[0])),
                };
            }
        }

        public void setPWMValue(byte val)
        {
            try
            {
                using (var dev = File.OpenWrite(drvPath))
                {
                    dev.WriteByte(val);
                }
            } catch(IOException ex)
            {
                Console.WriteLine("Could not open(write) device driver");
            } 
        }
    }
}