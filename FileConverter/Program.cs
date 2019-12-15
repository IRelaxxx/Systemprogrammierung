using Newtonsoft.Json;
using System;
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
                if (!files[i].Contains("temperature") || files[i].Contains("pressure")) continue;
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
                json = json.Substring(1);
                json2 = json2.Substring(1);
                int lastDirSep = files[i].LastIndexOf(Path.DirectorySeparatorChar);
                int length = files[i].Length - 5 - lastDirSep;
                json = "[\"\"," + json;
                json2 = "[\"" + files[i].Substring(lastDirSep + 1, length) + "\"," + json2;
                File.WriteAllText(files[i].Substring(0, files[i].Length - 4) + ".json", "{ \"dates\": " + json + ", \"vals\": " + json2 + "}");
            }
        }
    }
}