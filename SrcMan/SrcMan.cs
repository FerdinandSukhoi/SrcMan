using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace SrcMan
{
    [MobileSuitInfo("Source Manager"), MobileSuitItem]
    public class SrcMan
    {
        public SrcManBase.Config Config { get; set; }
        public string ConfigPath { get; set; }
        public SrcMan()
        {
            Console.WriteLine("InputPath of 'SrcManConfig.json', Leave empty to use current path");
            var configpath= Path.Combine(Console.ReadLine(), "SrcManConfig.json");

            if (!File.Exists(configpath))
            {
                Console.WriteLine("File Not Found. Use 'init' command to initialize.");
            }
            else
            {
                Config = Newtonsoft.Json.JsonConvert.DeserializeObject<SrcManBase.Config>(File.ReadAllText(configpath));
            }
        }
        public void Init(string[] args)
        {

        }
        public void Load(string[] args)
        {

        }
    }
}
