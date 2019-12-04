using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace FileConverter
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string[] files = Directory.GetFiles(args.Length > 2 ? args[1] : Directory.GetCurrentDirectory());

            for (int i = 0; i < files.Length; i++)
            {
                if (!files[i].Contains("i2c") || files[i].Contains(".json")) continue;
                string[] text = File.ReadAllLines(files[i]);
                DateTime[] dateTimes = new DateTime[text.Length];
                string[] data = new string[text.Length];
                for (int j = 0; j < text.Length; j++)
                {
                    string[] splits = text[j].Split(';');
                    dateTimes[j] = DateTime.Parse(splits[0]);
                    data[j] = splits[1];
                }
                string json = JsonConvert.SerializeObject(dateTimes);
                string json2 = JsonConvert.SerializeObject(data);
                File.WriteAllText(files[i] + "conv.json", "{ \"dates\": " + json + ", \"vals\": " + json2 + "}");
            }
        }
    }
}