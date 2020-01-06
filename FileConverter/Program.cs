using Newtonsoft.Json;
using System;
using System.IO;

namespace FileConverter
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string file = args.Length > 2 ? args[1] : string.Empty;
            if (string.IsNullOrEmpty(file))
            {
                Console.WriteLine("Usage: fileconverter FILEPATH");
            }

            string[] text = File.ReadAllLines(file);
            DateTime[] dateTimes = new DateTime[text.Length];
            string[] data = new string[text.Length];
            for (int j = 0; j < text.Length; j++)
            {
                string[] splits = text[j].Split(';');
                dateTimes[j] = DateTime.ParseExact(splits[0], "MM-dd-yyyy HH:mm:ss", null);
                data[j] = splits[1];
            }
            string json = JsonConvert.SerializeObject(dateTimes);
            string json2 = JsonConvert.SerializeObject(data);
            json = json.Substring(1);
            json2 = json2.Substring(1);
            int lastDirSep = file.LastIndexOf(Path.DirectorySeparatorChar);
            int length = file.Length - 5 - lastDirSep;
            json = "[\"\"," + json;
            json2 = "[\"" + file.Substring(lastDirSep + 1, length) + "\"," + json2;
            File.WriteAllText(file.Substring(0, file.Length - 4) + ".json", "{ \"dates\": " + json + ", \"vals\": " + json2 + "}");
        }
    }
}