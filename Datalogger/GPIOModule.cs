using System.IO;
using System.Text;

namespace Datalogger
{
    public class GPIOModule
    {
        public GPIOStatus getStatus()
        {
            var dev = File.OpenRead("/dev/myboi");
            byte[] data = new byte[2];
            dev.Read(data, 0, 2);
            dev.Close();
            string s = Encoding.ASCII.GetString(data);
            switch (int.Parse(s))
            {
                case 0:
                    return GPIOStatus.Temperature;

                case 1:
                    return GPIOStatus.Pressure;

                default:
                    throw new IOException(string.Format("Could not read /dev/boi output should be 0 or 1 (binary) but was {0}", data[0]));
            }
        }
    }
}