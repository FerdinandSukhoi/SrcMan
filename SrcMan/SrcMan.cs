using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace SrcMan
{
    [MobileSuitInfo("Source Manager")]
    public partial class SrcMan
    {
        [MobileSuitInfo("Configs")]
        public SrcManBase.Config ConfigData { get; set; }
        public string ConfigPath { get; set; }
        public DBEngine DB { get; set; }
        public FindEngine Find { get; set; }
        public SrcMan()
        {
            Console.WriteLine("InputPath of 'SrcManConfig.json', Leave empty to use current path");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Path of 'SrcManConfig.json'>");
            Console.ResetColor();
            ConfigPath = Path.Combine(Console.ReadLine(), "SrcManConfig.json");
            
            if (!File.Exists(ConfigPath))
            {
                Console.WriteLine("File Not Found. Use 'init' or 'loadcfg' command to initialize.");
            }
            else
            {
                ConfigData = Newtonsoft.Json.JsonConvert.DeserializeObject<SrcManBase.Config>(File.ReadAllText(ConfigPath));
            }
            DB = new DBEngine(ConfigData);
        }
        public void Init()
        {
            ConfigData = new SrcManBase.Config();
            Config();
        }
        public void Config()
        {
            string buf;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("Enter Source Dirctory(Now={0})>",ConfigData.DataPath);
            Console.ResetColor();
            buf = Console.ReadLine();
            ConfigData.DataPath = buf == "" ? ConfigData.DataPath:buf;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("Enter ConfigFiles Dirctory(Now={0})>", ConfigData.ConfigPath);
            Console.ResetColor();
            buf = Console.ReadLine();
            ConfigData.ConfigPath = buf == "" ? ConfigData.DataPath : buf;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("Enter CacheFiles Dirctory(Now={0})>", ConfigData.CachePath);
            Console.ResetColor();
            buf = Console.ReadLine();
            ConfigData.CachePath = buf == "" ? ConfigData.DataPath : buf;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("Enter ConvertFiles Dirctory(Now={0})>", ConfigData.ConvertPath);
            Console.ResetColor();
            buf = Console.ReadLine();
            ConfigData.ConvertPath = buf == "" ? ConfigData.DataPath : buf;
            Console.WriteLine();
            Console.WriteLine("Now:");
            Console.WriteLine($"DataFiles Dirctory={ConfigData.DataPath}");
            Console.WriteLine($"ConfigFiles Dirctory={ConfigData.ConfigPath}");
            Console.WriteLine($"CacheFiles Dirctory={ConfigData.CachePath}");
            Console.WriteLine($"ConvertFiles Dirctory={ConfigData.ConvertPath}");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("All OK?(input y to save)>");
            Console.ResetColor();
            if (Console.ReadLine().ToLower()=="y")
            {
                File.WriteAllText(ConfigPath, Newtonsoft.Json.JsonConvert.SerializeObject(ConfigData));
            }
        }
        public void Load()
        {

        }
    }
}
