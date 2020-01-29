using System;
using System.Runtime.Loader;
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
            AssemblyLoadContext.GetLoadContext(typeof(Program).Assembly).Unloading += (cont) => { disp.clear(); mod.setPWMValue(0); };
            Console.CancelKeyPress += (o, e) => { disp.clear(); mod.setPWMValue(0); };
            double scaleBot = args.Length >= 2 ? double.Parse(args[0]) : 24;
            double scaleTop = args.Length >= 2 ? double.Parse(args[1]) : 26;
            Console.WriteLine(scaleBot);
            Console.WriteLine(scaleTop);
            /*while (true)
            {
                Console.WriteLine(sens.getData(mod.getStatus()));
                Thread.Sleep(1000);
            }*/
            //disp.write("-1.1");
            //disp.writeLow("<C");
            //disp.flush();
            Thread.Sleep(1000);
            while (true)
            {
                GPIOStatus status = mod.getStatus();
                (double temp, double press) = sens.getData();
                Console.WriteLine(temp);
                Console.WriteLine(press);
                switch (status)
                {
                    case GPIOStatus.Temperature:
                        disp.write(temp.ToString()); // TODO: trim to 1 decimal point
                        disp.writeLow("<C");
                        byte pwmValue = (byte)(((temp - scaleBot) / (scaleTop - scaleBot)) * 100);
                        pwmValue = Math.Min(Math.Max((byte)0, pwmValue), (byte)100);
                        Console.WriteLine(((temp - scaleBot) / (scaleTop - scaleBot)) * 100);
                        Console.WriteLine(pwmValue);
                        mod.setPWMValue(pwmValue);
                        break;

                    case GPIOStatus.Pressure:
                        disp.write((press / 100).ToString());
                        disp.writeLow("hPa");
                        break;
                }
                disp.flush();
                Thread.Sleep(1000);
            }
        }
    }
}