﻿using System;
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
                    return GPIOStatus.Sens1;

                case 1:
                    return GPIOStatus.Sens2;

                default:
                    return GPIOStatus.Invalid;
            }
        }
    }
}