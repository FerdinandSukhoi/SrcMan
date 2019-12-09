using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using MobileSuit;

namespace SrcMan
{
    [MobileSuitInfo("Source Manager")]
    public partial class SrcMan
    {
        private readonly string[] SrcExt = new string[] { ".mp4", ".wmv", ".avi", ".mkv", ".rmvb" };
        private Queue<FileInfo> SrcQueue { get; set; }
        [MobileSuitInfo("Configs")]
        public SrcManBase.Config ConfigData { get; set; }
        public string ConfigPath { get; set; }
        public DBEngine DB { get; set; }
        public FindEngine Find { get; set; }
        //public MTPEngine MTP { get; set; }
        public SrcMan()
        {
            if (File.Exists("SrcManConfig.json"))
            {
                ConfigPath = "SrcManConfig.json";
            }
            else
            {
                Console.WriteLine("InputPath of 'SrcManConfig.json', Leave empty to use current path");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Path of 'SrcManConfig.json'>");
                Console.ResetColor();
                ConfigPath = Path.Combine(Console.ReadLine(), "SrcManConfig.json");
            }
            
            
            if (!File.Exists(ConfigPath))
            {
                Console.WriteLine("File Not Found. Use 'init' or 'loadcfg' command to initialize.");
            }
            else
            {
                ConfigData = Newtonsoft.Json.JsonConvert.DeserializeObject<SrcManBase.Config>(File.ReadAllText(ConfigPath));
            }
            DB = new DBEngine(ConfigData);
            Load();
            Find = new FindEngine(DB);
            //MTP = new MTPEngine(DB, ConfigData);
        }
        public void Init()
        {
            ConfigData = new SrcManBase.Config();
            Config();
            DB.Build();
        }
        public void Config()
        {
            string buf;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("Enter Source Dirctory\n(Now={0})>",ConfigData.DataPath);
            Console.ResetColor();
            buf = Console.ReadLine();
            ConfigData.DataPath = buf == "" ? ConfigData.DataPath:buf;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("Enter ConfigFiles Dirctory\n(Now={0})>", ConfigData.ConfigPath);
            Console.ResetColor();
            buf = Console.ReadLine();
            ConfigData.ConfigPath = buf == "" ? ConfigData.ConfigPath : buf;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("Enter CacheFiles Dirctory\n(Now={0})>", ConfigData.CachePath);
            Console.ResetColor();
            buf = Console.ReadLine();
            ConfigData.CachePath = buf == "" ? ConfigData.CachePath : buf;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("Enter ConvertFiles Dirctory\n(Now={0})>", ConfigData.ConvertPath);
            Console.ResetColor();
            buf = Console.ReadLine();
            ConfigData.ConvertPath = buf == "" ? ConfigData.ConvertPath : buf;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("Enter Pull Path\n(Now={0})>", ConfigData.PullFilePath);
            Console.ResetColor();
            buf = Console.ReadLine();
            ConfigData.PullFilePath = buf == "" ? ConfigData.PullFilePath : buf;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("Enter MTP Device Name (May use 'MTP List' command to get)\n(Now={0})>", ConfigData.MTPDeviceName);
            Console.ResetColor();
            buf = Console.ReadLine();
            ConfigData.MTPDeviceName = buf == "" ? ConfigData.MTPDeviceName : buf;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("Enter MTP Device Directory Path (May use 'MTP BFSDir10' command to get some examples)\n(Now={0})>", ConfigData.MTPDirctoryPath);
            Console.ResetColor();
            buf = Console.ReadLine();
            ConfigData.MTPDirctoryPath = buf == "" ? ConfigData.MTPDirctoryPath : buf;

            Console.WriteLine();
            Console.WriteLine("Now:");
            Console.WriteLine($"DataFiles Directory={ConfigData.DataPath}");
            Console.WriteLine($"ConfigFiles Directory={ConfigData.ConfigPath}");
            Console.WriteLine($"CacheFiles Directory={ConfigData.CachePath}");
            Console.WriteLine($"ConvertFiles Directory={ConfigData.ConvertPath}");
            Console.WriteLine($"Pull Files Directory={ConfigData.PullFilePath}");
            Console.WriteLine($"MTP Device Name={ConfigData.MTPDeviceName}");
            Console.WriteLine($"MTP Device Directory Path={ConfigData.MTPDirctoryPath}");
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
            => DB.Load();
        public void Sav()
            => Save();
        public void Save()
            => DB.Save();
        public void Upd()
            => Update();
        public void Update()
            => DB.Format();
        public void EnQ() 
            => EnQueueAll();
        //public void Sync(string itemCode)
        //    => MTP.Sync(itemCode);
        //public void DSync(string itemCode)
        //    => MTP.DeSync(itemCode);
        //public void Push()
        //    => MTP.Push();
        public void Pull()
        {
            if (!DB.DbCheck()) return;
            Regex numbRgx = new Regex("[0-9]");
            foreach (var item in (new DirectoryInfo(ConfigData.PullFilePath)).GetFiles())
            {
                var itemInfoArr = item.FullName.Replace(item.Extension, "").Split("-");
                var itemInfo = new DBEngine.DBStore.SrcItem
                {
                    Name = itemInfoArr.Length > 4 && numbRgx.IsMatch(itemInfoArr[4])
                    ? $"{itemInfoArr[2].ToUpper()}-{itemInfoArr[3].ToUpper()}-{itemInfoArr[4]}"
                    : $"{itemInfoArr[2].ToUpper()}-{itemInfoArr[3].ToUpper()}",


                };
                DBEngine.DBStore.SrcActor actor;
                if (!DB.Store.Actors.Contains(itemInfoArr[1]))
                {
                    actor = new DBEngine.DBStore.SrcActor();
                    actor.Name = itemInfoArr[1];
                    actor.Index = DB.Store.Actors.Count;

                    DB.Store.Actors.Add(actor);
                }
                else
                {
                    actor = DB.Store.Actors[itemInfoArr[1]];
                }
                itemInfo.Index = 100 * actor.Index + actor.Items.Count;
                if (!actor.Items.Contains(itemInfo.Name))
                {
                    actor.Items.Add(itemInfo);
                    if (itemInfoArr.Length > 4)
                    {
                        foreach (var label in itemInfoArr[4..])
                        {
                            if (numbRgx.IsMatch(label)) continue;
                            itemInfo.Labels.Add(label.ToUpper());
                            if (!DB.Store.Labels.Contains(label.ToUpper()))
                            {
                                DB.Store.Labels.Add(new DBEngine.DBStore.SrcLabel() { Name = label.ToUpper() });
                            }
                            DB.Store.Labels[label.ToUpper()].Items.Add(itemInfo);
                        }
                    }
                    itemInfo.Stared = itemInfo.Labels.Contains("M");
                }
                else
                {
                    itemInfo = actor.Items[itemInfo.Name];
                }




                DB.Store.Init();
                var sb = new StringBuilder($"{ DBEngine.GetItemCode(itemInfo.Index) }-{actor.Name}-{itemInfo.Name}");
                sb.Append(item.Extension);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Pulled:");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{item.FullName}\n>>\n{sb.ToString()}");
                item.MoveTo(Path.Combine(item.DirectoryName, sb.ToString()));
                itemInfo.Path = Path.Combine(item.DirectoryName, sb.ToString());
                Console.ResetColor();
            }

            Update();
        }
        private bool QueueCheck()
        {
            if (SrcQueue?.Count==0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Cache Queue not loaded, use 'EnqueueAll'.");
                Console.ResetColor();
                return false;
            }
            return true;
        }
        private Regex ActorCodeRegex = new Regex("[A-Z][A-Z]");
        public void Peek()
        {
            if (!DB.DbCheck()) return;
            if (!QueueCheck()) return;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("Peek:");
            Console.ForegroundColor = ConsoleColor.Yellow;
            var rmf = SrcQueue.Peek();
            Console.WriteLine($"{rmf.FullName}");
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine($"Size:{(rmf.Length >> 20)}MB");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[{SrcQueue.Count}] Items Remaining.");
            Console.ResetColor();
        }
        public void Jump()
        {
            if (!DB.DbCheck()) return;
            if (!QueueCheck()) return;
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write("Jump:");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            var rmf = SrcQueue.Dequeue();
            Console.WriteLine($"{rmf.FullName}");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[{SrcQueue.Count}] Items Remaining.");
            Console.ResetColor();
        }
        public void Jmp() => Jump();
        public void Remove()
        {
            if (!DB.DbCheck()) return;
            if (!QueueCheck()) return;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Remove:");
            Console.ForegroundColor = ConsoleColor.Blue;
            var rmf = SrcQueue.Dequeue();
            Console.WriteLine($"{rmf.FullName}");
            rmf.Delete();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[{SrcQueue.Count}] Items Remaining.");
            Console.ResetColor();
        }
        public void Rm()
            => Remove();
        public void Add()
        {
            if (!DB.DbCheck()) return;
            if (!QueueCheck()) return;
            var itemInfoArr = DBEngine.SplitFn(SrcQueue.Peek());
            Regex numbRgx = new Regex("[0-9]");
            var itemInfo = new DBEngine.DBStore.SrcItem
            {
                Path = SrcQueue.Peek().FullName,
                Name = itemInfoArr.Length > 4 && numbRgx.IsMatch(itemInfoArr[4])
                ? $"{itemInfoArr[2].ToUpper()}-{itemInfoArr[3].ToUpper()}-{itemInfoArr[4]}"
                : $"{itemInfoArr[2].ToUpper()}-{itemInfoArr[3].ToUpper()}",
                
                
            };
            SrcMan.DBEngine.DBStore.SrcActor actor;
            if (!DB.Store.Actors.Contains(itemInfoArr[1]))
            {
                actor = new DBEngine.DBStore.SrcActor();
                actor.Name = itemInfoArr[1];
                actor.Index = DB.Store.Actors.Count;

                DB.Store.Actors.Add(actor);
            }
            else
            {
                actor = DB.Store.Actors[itemInfoArr[1]];
            }
            itemInfo.Index = 100 * actor.Index + actor.Items.Count;
            actor.Items.Add(itemInfo);
            if (itemInfoArr.Length > 4)
            {
                foreach (var label in itemInfoArr[4..])
                {
                    if (numbRgx.IsMatch(label)) continue;
                    itemInfo.Labels.Add(label.ToUpper());
                    if (!DB.Store.Labels.Contains(label.ToUpper()))
                    {
                        DB.Store.Labels.Add(new DBEngine.DBStore.SrcLabel() { Name = label.ToUpper() });
                    }
                    DB.Store.Labels[label.ToUpper()].Items.Add(itemInfo);
                }
            }
            itemInfo.Stared = itemInfo.Labels.Contains("M");

            DB.Store.Init();
            var sb = new StringBuilder($"{ DBEngine.GetItemCode(itemInfo.Index) }-{actor.Name}-{itemInfo.Name}");
            sb.Append(SrcQueue.Peek().Extension);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Add:");
            Console.ForegroundColor = ConsoleColor.Yellow;
            var fi = SrcQueue.Dequeue();
            Console.WriteLine($"{fi.FullName}\n>>\n{sb.ToString()}");
            fi.MoveTo(Path.Combine(fi.DirectoryName, sb.ToString()));
            itemInfo.Path = Path.Combine(fi.DirectoryName, sb.ToString());
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[{SrcQueue.Count}] Items Remaining.");
            Console.ResetColor();
        }
        public void Add(string actorCode, string name)
        {
            if(!DB.DbCheck())return;
            if (!QueueCheck()) return;
            actorCode = actorCode.ToUpper();
            DBEngine.DBStore.SrcActor actor;
            if (ActorCodeRegex.IsMatch(actorCode))
            {
                var index = 26 * (actorCode[0] - 'A') + actorCode[1] - 'A';
                actor = DB.Store.Actors.Where(a => a.Index == index).FirstOrDefault();
            }
            else
            {
                actor = DB.Store.Actors.Where(a => a.Name.Contains(actorCode)).FirstOrDefault();
            }
            if (actor == null)
            {
                actor = new DBEngine.DBStore.SrcActor();
                actor.Name = actorCode;
                actor.Index = DB.Store.Actors.Count;

                DB.Store.Actors.Add(actor);
            }
            var item = new DBEngine.DBStore.SrcItem();
            var fi = SrcQueue.Dequeue();

            item.Name = name;
            item.Index = 100 * actor.Index + actor.Items.Count;
            actor.Items.Add(item);
            DB.Store.Init();
            var sb = new StringBuilder($"{ DBEngine.GetItemCode(item.Index) }-{actor.Name}-{item.Name}");
            sb.Append(fi.Extension);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Add:");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{fi.FullName}\n>>\n{sb.ToString()}");
            fi.MoveTo(Path.Combine(fi.DirectoryName, sb.ToString()));
            item.Path = Path.Combine(fi.DirectoryName, sb.ToString());
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[{SrcQueue.Count}] Items Remaining.");
            Console.ResetColor();
        }
        public void Add(string actorCode, string name, string labels)
        {
            if (!DB.DbCheck()) return;
            if (!QueueCheck()) return;
            actorCode = actorCode.ToUpper();
            DBEngine.DBStore.SrcActor actor;
            if (ActorCodeRegex.IsMatch(actorCode))
            {
                var index = 26 * (actorCode[0] - 'A') + actorCode[1] - 'A';
                actor = DB.Store.Actors.Where(a => a.Index == index).FirstOrDefault();
            }
            else
            {
                actor = DB.Store.Actors.Where(a => a.Name.Contains(actorCode)).FirstOrDefault();
            }
            if (actor == null)
            {
                actor = new DBEngine.DBStore.SrcActor();
                actor.Name = actorCode;
                actor.Index = DB.Store.Actors.Count;

                DB.Store.Actors.Add(actor);
            }
            var item = new DBEngine.DBStore.SrcItem();
            var fi = SrcQueue.Dequeue();
            item.Path = fi.FullName;
            item.Name = name;
            item.Index = 100 * actor.Index + actor.Items.Count;
            var lbs = labels.ToUpper().Split("-");
            foreach (var lb in lbs)
            {
                item.Labels.Add(lb);
            }
            if (lbs.Contains("M"))
            {
                item.Stared = true;
                actor.Stared = true;
            }
            actor.Items.Add(item);
            DB.Store.Init();
            var sb = new StringBuilder($"{ DBEngine.GetItemCode(item.Index) }-{actor.Name}-{item.Name}");
            if (item.Labels.Count != 0)
            {
                foreach (var label in item.Labels)
                {
                    sb.Append($"-{label}");
                }
            }
            sb.Append(fi.Extension);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Add:");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{fi.FullName}\n>>\n{sb.ToString()}");
            fi.MoveTo(Path.Combine(fi.DirectoryName, sb.ToString()));
            item.Path = Path.Combine(fi.DirectoryName, sb.ToString());
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[{SrcQueue.Count}] Items Remaining.");
            Console.ResetColor();
        }
        public void AddC()
        {
            if (!DB.DbCheck()) return;
            if (!QueueCheck()) return;
            var itemInfoArr = DBEngine.SplitFn(SrcQueue.Peek());
            var numbRgx = new Regex("[0-9]");
            var itemInfo = new DBEngine.DBStore.SrcItem
            {
                Path = SrcQueue.Peek().FullName,
                Name = itemInfoArr.Length > 4 && numbRgx.IsMatch(itemInfoArr[4])
                ? $"{itemInfoArr[2].ToUpper()}-{itemInfoArr[3].ToUpper()}-{itemInfoArr[4]}"
                : $"{itemInfoArr[2].ToUpper()}-{itemInfoArr[3].ToUpper()}",


            };
            SrcMan.DBEngine.DBStore.SrcActor actor;
            if (!DB.Store.Actors.Contains(itemInfoArr[1]))
            {
                actor = new DBEngine.DBStore.SrcActor();
                actor.Name = itemInfoArr[1];
                actor.Index = DB.Store.Actors.Count;

                DB.Store.Actors.Add(actor);
            }
            else
            {
                actor = DB.Store.Actors[itemInfoArr[1]];
            }
            itemInfo.Index = 100 * actor.Index + actor.Items.Count;
            actor.Items.Add(itemInfo);
            if (itemInfoArr.Length > 4)
            {
                foreach (var label in itemInfoArr[4..])
                {
                    if (numbRgx.IsMatch(label)) continue;
                    itemInfo.Labels.Add(label.ToUpper());
                    if (!DB.Store.Labels.Contains(label.ToUpper()))
                    {
                        DB.Store.Labels.Add(new DBEngine.DBStore.SrcLabel() { Name = label.ToUpper() });
                    }
                    DB.Store.Labels[label.ToUpper()].Items.Add(itemInfo);
                }
            }
            itemInfo.Stared = itemInfo.Labels.Contains("M");

            DB.Store.Init();
            var sb = new StringBuilder($"{ DBEngine.GetItemCode(itemInfo.Index) }-{actor.Name}-{itemInfo.Name}");
            sb.Append(SrcQueue.Peek().Extension);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Add:");
            Console.ForegroundColor = ConsoleColor.Yellow;
            var fi = SrcQueue.Dequeue();
            Console.WriteLine($"{fi.FullName}\n>>\n{sb.ToString()}");

            itemInfo.Path = Path.Combine(fi.DirectoryName, sb.ToString());
            fi.MoveTo(Path.Combine(ConfigData.ConvertPath, sb.ToString()));
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[{SrcQueue.Count}] Items Remaining.");
            Console.ResetColor();
        }
        public void AddC(string actorCode, string name)
        {
            if (!DB.DbCheck()) return;
            if (!QueueCheck()) return;
            actorCode = actorCode.ToUpper();
            DBEngine.DBStore.SrcActor actor;
            if (ActorCodeRegex.IsMatch(actorCode))
            {
                var index = 26 * (actorCode[0] - 'A') + actorCode[1] - 'A';
                actor = DB.Store.Actors.Where(a => a.Index == index).FirstOrDefault();
            }
            else
            {
                actor = DB.Store.Actors.Where(a => a.Name.Contains(actorCode)).FirstOrDefault();
            }
            if (actor == null)
            {
                actor = new DBEngine.DBStore.SrcActor();
                actor.Name = actorCode;
                actor.Index = DB.Store.Actors.Count;

                DB.Store.Actors.Add(actor);
            }
            var item = new DBEngine.DBStore.SrcItem();
            var fi = SrcQueue.Dequeue();

            item.Name = name;
            item.Index = 100 * actor.Index + actor.Items.Count;
            actor.Items.Add(item);
            DB.Store.Init();
            var sb = new StringBuilder($"{ DBEngine.GetItemCode(item.Index) }-{actor.Name}-{item.Name}");
            sb.Append(fi.Extension);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Add:");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{fi.FullName}\n>>\n{sb.ToString()}");
            item.Path = Path.Combine(fi.DirectoryName, sb.ToString());
            fi.MoveTo(Path.Combine(ConfigData.ConvertPath, sb.ToString()));

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[{SrcQueue.Count}] Items Remaining.");
            Console.ResetColor();
        }
        public void AddC(string actorCode, string name, string labels)
        {
            if (!DB.DbCheck()) return;
            if (!QueueCheck()) return;
            actorCode = actorCode.ToUpper();
            DBEngine.DBStore.SrcActor actor;
            if (ActorCodeRegex.IsMatch(actorCode))
            {
                var index = 26 * (actorCode[0] - 'A') + actorCode[1] - 'A';
                actor = DB.Store.Actors.Where(a => a.Index == index).FirstOrDefault();
            }
            else
            {
                actor = DB.Store.Actors.Where(a => a.Name.Contains(actorCode)).FirstOrDefault();
            }
            if (actor == null)
            {
                actor = new DBEngine.DBStore.SrcActor();
                actor.Name = actorCode;
                actor.Index = DB.Store.Actors.Count;

                DB.Store.Actors.Add(actor);
            }
            var item = new DBEngine.DBStore.SrcItem();
            var fi = SrcQueue.Dequeue();
            item.Path = fi.FullName;
            item.Name = name;
            item.Index = 100 * actor.Index + actor.Items.Count;
            var lbs = labels.ToUpper().Split("-");
            foreach (var lb in lbs)
            {
                item.Labels.Add(lb);
            }
            if (lbs.Contains("M"))
            {
                item.Stared = true;
                actor.Stared = true;
            }
            actor.Items.Add(item);
            DB.Store.Init();
            var sb = new StringBuilder($"{ DBEngine.GetItemCode(item.Index) }-{actor.Name}-{item.Name}");
            if (item.Labels.Count != 0)
            {
                foreach (var label in item.Labels)
                {
                    sb.Append($"-{label}");
                }
            }
            sb.Append(fi.Extension);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Add:");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{fi.FullName}\n>>\n{sb.ToString()}");
            fi.MoveTo(Path.Combine(ConfigData.ConvertPath, sb.ToString()));
            item.Path = Path.Combine(fi.DirectoryName, sb.ToString());
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[{SrcQueue.Count}] Items Remaining.");
            Console.ResetColor();
        }

        public void Play()
        {
            if (!QueueCheck())
            {
                return;
            }
            var src = SrcQueue.Peek();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Play:");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{src.FullName}");
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine($"Size:{(src.Length>>20)}MB");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[{SrcQueue.Count-1}] Items Remaining.");
            Console.ResetColor();
            var player = new Process();
            player.StartInfo.FileName = @"C:\Program Files\DAUM\PotPlayer\PotPlayerMini64.exe";
            player.StartInfo.Arguments = src.FullName;
            player.Start();
        }
        public void EnQueueAll()
        {
            SrcQueue = new Queue<FileInfo>();
            Dpkg();
            foreach (var file in (new DirectoryInfo(ConfigData.CachePath)).GetFiles())
            {
                if (SrcExt.Contains(file.Extension.ToLower()))
                {
                    SrcQueue.Enqueue(file);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("Enqueued:");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(file.FullName);
                    Console.ResetColor();
                }
                else
                {
                    file.Delete();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Removed:");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine(file.FullName);
                    Console.ResetColor();
                }
            }
        }
        public void Dpkg()
            => DepackageFolders();
        public void DepackageFolders()
        {
            foreach (var dir in (new DirectoryInfo(ConfigData.CachePath)).GetDirectories())
            {

                foreach (var file in dir.GetFiles())
                {
                    if (SrcExt.Contains(file.Extension.ToLower()))
                    {
                        file.MoveTo(Path.Combine(ConfigData.CachePath,file.Name));
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write("Moved:");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"{file.FullName}");
                        Console.ResetColor();
                    }
                }
                dir.Delete(true);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Remove:");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(dir.FullName);
                Console.ResetColor();
            }
        }
        [MobileSuitInfo("MergeLabel <SrcLabel> <DesLabel>")]
        public void MergeLabel(string source, string destination)
        {
            if (!DB.Store.Labels.Contains(source.ToUpper()))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No Such Label!");
                Console.ResetColor();
                return;
            }
            foreach(var item in DB.Store.Labels[source.ToUpper()].Items)
            {
                item.Labels.Remove(source.ToUpper());
                item.Labels.Add(destination.ToUpper());
            }
            DB.Store.Init();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Label Merged:{source}>>{destination}, use 'DB Format' to format.");
            Console.ResetColor();
        }
    }
}
