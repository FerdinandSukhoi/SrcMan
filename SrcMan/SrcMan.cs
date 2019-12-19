using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using MobileSuit;
using MobileSuit.IO;
using Newtonsoft.Json;

namespace SrcMan
{
    [MobileSuitInfo("Source Manager")]
    public partial class SrcMan : IIoInteractive
    {
        private readonly string[] SrcExt = new string[] { ".mp4", ".wmv", ".avi", ".mkv", ".rmvb" };
        private Queue<FileInfo> SrcQueue { get; set; }
        [MobileSuitInfo("Configs")]
        public SrcManBase.Config ConfigData { get; set; }
        public string ConfigPath { get; set; }
        public DbEngine DB { get; set; }
        private FindEngine FindHandler { get; set; }

        //public MTPEngine MTP { get; set; }
        public SrcMan()
        {
            if (File.Exists("SrcManConfig.json"))
            {
                ConfigPath = "SrcManConfig.json";
            }
            else
            {
                Io.WriteLine("");

                ConfigPath = Path.Combine(Io.ReadLine("InputPath of 'SrcManConfig.json', Leave empty to use current path",
                    default, true, ConsoleColor.Yellow), "SrcManConfig.json");
            }


            if (!File.Exists(ConfigPath))
            {
                Io.WriteLine("File Not Found. Use 'init' or 'loadcfg' command to initialize.", IoInterface.OutputType.Error);
            }
            else
            {
                ConfigData = Newtonsoft.Json.JsonConvert.DeserializeObject<SrcManBase.Config>(File.ReadAllText(ConfigPath));
            }
            DB = new DbEngine(ConfigData);

            Load();
            FindHandler = new FindEngine(DB);
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
            ConfigData.DataPath = Io.ReadLine($"Enter Source Directory\n(Now={ConfigData.DataPath})>", ConfigData.DataPath, true, ConsoleColor.Blue);
            ConfigData.ConfigPath = Io.ReadLine($"Enter ConfigFiles Directory\n(Now={ConfigData.ConfigPath})", ConfigData.ConfigPath, true, ConsoleColor.Blue);
            ConfigData.CachePath = Io.ReadLine($"Enter CacheFiles Directory\n(Now={ConfigData.CachePath})", ConfigData.CachePath, true, ConsoleColor.Blue);
            ConfigData.ConvertPath = Io.ReadLine($"Enter ConvertFiles Directory\n(Now={ConfigData.ConvertPath})", ConfigData.ConvertPath, true, ConsoleColor.Blue);
            ConfigData.PullFilePath = Io.ReadLine($"Enter Pull Path\n(Now={ConfigData.PullFilePath})", ConfigData.PullFilePath, true, ConsoleColor.Blue);
            ConfigData.MTPDeviceName = Io.ReadLine($"Enter MTP Device Name (May use 'MTP List' command to get)\n(Now={ConfigData.MTPDeviceName})", ConfigData.MTPDeviceName, true, ConsoleColor.Blue);
            ConfigData.MTPDirectoryPath = Io.ReadLine($"Enter MTP Device Directory Path (May use 'MTP BFSDir10' command to get some examples)\n(Now={ConfigData.MTPDirectoryPath})", ConfigData.MTPDirectoryPath, true, ConsoleColor.Blue);
            Io.WriteLine("");
            Io.WriteLine("Now:");
            Io.WriteLine($"DataFiles Directory={ConfigData.DataPath}");
            Io.WriteLine($"ConfigFiles Directory={ConfigData.ConfigPath}");
            Io.WriteLine($"CacheFiles Directory={ConfigData.CachePath}");
            Io.WriteLine($"ConvertFiles Directory={ConfigData.ConvertPath}");
            Io.WriteLine($"Pull Files Directory={ConfigData.PullFilePath}");
            Io.WriteLine($"MTP Device Name={ConfigData.MTPDeviceName}");
            Io.WriteLine($"MTP Device Directory Path={ConfigData.MTPDirectoryPath}");
            Io.WriteLine("");
            if (Io.ReadLine("All OK?(default input y to save)", "y", true, ConsoleColor.Yellow).ToLower() == "y")
            {
                File.WriteAllText(ConfigPath, Newtonsoft.Json.JsonConvert.SerializeObject(ConfigData));
            }
        }
        public void Load()
        {
            DB.Load();
            var cvtSetPath = Path.Combine(ConfigData.ConfigPath, "CvtSet.json");
            ConvertFiles = File.Exists(cvtSetPath) ? 
                JsonConvert
                .DeserializeObject<HashSet<DbEngine.DbStore.SrcItem>>
                    (File.ReadAllText(cvtSetPath)) 
                : new HashSet<DbEngine.DbStore.SrcItem>();
        }

        public void Sav()
            => Save();

        public void Save()
        {
            DB.Save();
            File.WriteAllText(Path.Combine(ConfigData.ConfigPath, "CvtSet.json"),
                JsonConvert.SerializeObject(ConvertFiles));
        }

        public void Upd()
            => Update();
        public void Update()
        {
            if (ConvertCheck()) DB.Format();
        }
        public void EnQ()
            => EnQueueAll();
        //public void Sync(string itemCode)
        //    => MTP.Sync(itemCode);
        //public void DSync(string itemCode)
        //    => MTP.DeSync(itemCode);
        //public void Push()
        //    => MTP.Push();
        public void Find(string arg0)
        {
            FindHandler.I(arg0);
            FindHandler.A(arg0);
            FindHandler.L(arg0);
        }
        public void Find(string arg0, string arg1)
        {
            FindHandler.I(arg0, arg1);
            FindHandler.A(arg0, arg1);
            FindHandler.L(arg0, arg1);
        }
        public void Find(string arg0, string arg1, string arg2)
        {

            FindHandler.L(arg0, arg1, arg2);
        }

        public void Play(string itemId)
        {
            if (!DB.DbCheck()) return;
            Save();
            var regex = new Regex("[A-Z][A-Z][0-9][0-9]");
            itemId = itemId.ToUpper();
            var item = regex.IsMatch(itemId)
                ? DB.Store.Actors?
                    .FirstOrDefault(a => a.Index == 26 * (itemId[0] - 'A') + itemId[1] - 'A')?.Items
                    .FirstOrDefault(i =>
                        i.Index == 100 * (26 * (itemId[0] - 'A') + itemId[1] - 'A') + 10 * (itemId[2] - '0') +
                        itemId[3] - '0')
                : DB.Store.Items.FirstOrDefault(i => i.Name.Contains(itemId));
            if (item is null)
            {
                Io.WriteLine("No Such Item.", IoInterface.OutputType.Error);
                return;
            }
            var player = new Process
            {
                StartInfo =
                {
                    FileName = @"C:\Program Files\DAUM\PotPlayer\PotPlayerMini64.exe", Arguments = item.Path
                }
            };
            player.Start();

        }
        public void Pull()
        {
            if (!DB.DbCheck()) return;
            Regex numbRgx = new Regex("[0-9]");
            foreach (var item in (new DirectoryInfo(ConfigData.PullFilePath)).GetFiles())
            {
                var itemInfoArr = item.FullName.Replace(item.Extension, "").Split("-");
                var itemInfo = new DbEngine.DbStore.SrcItem
                {
                    Name = itemInfoArr.Length > 4 && numbRgx.IsMatch(itemInfoArr[4])
                    ? $"{itemInfoArr[2].ToUpper()}-{itemInfoArr[3].ToUpper()}-{itemInfoArr[4]}"
                    : $"{itemInfoArr[2].ToUpper()}-{itemInfoArr[3].ToUpper()}",


                };
                DbEngine.DbStore.SrcActor actor;
                if (!DB.Store.Actors.Contains(itemInfoArr[1]))
                {
                    actor = new DbEngine.DbStore.SrcActor();
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
                                DB.Store.Labels.Add(new DbEngine.DbStore.SrcLabel() { Name = label.ToUpper() });
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
                var sb = new StringBuilder($"{ DbEngine.GetItemCode(itemInfo.Index) }-{actor.Name}-{itemInfo.Name}");
                sb.Append(item.Extension);
                Io.Write("Pulled:", default, ConsoleColor.Green);
                Io.WriteLine($"{item.FullName}\n>>\n{sb.ToString()}", default, ConsoleColor.Yellow);
                item.MoveTo(Path.Combine(item.DirectoryName, sb.ToString()));
                itemInfo.Path = Path.Combine(item.DirectoryName, sb.ToString());

            }

            Update();
        }
        private bool QueueCheck()
        {
            if (SrcQueue?.Count == 0)
            {
                Io.WriteLine("Cache Queue not loaded, use 'EnqueueAll'.", IoInterface.OutputType.Error);

                return false;
            }
            return true;
        }
        private Regex ActorCodeRegex { get; } = new Regex("[A-Z][A-Z]");
        public void Peek()
        {
            if (!DB.DbCheck()) return;
            if (!QueueCheck()) return;
            Io.Write("Peek:", default, ConsoleColor.Blue);
            var rmf = SrcQueue.Peek();
            Io.WriteLine($"{rmf.FullName}", default, ConsoleColor.Yellow);
            Io.WriteLine($"Size:{(rmf.Length >> 20)}MB", default, ConsoleColor.DarkMagenta);
            Io.WriteLine($"[{SrcQueue.Count}] Items Remaining.", default, ConsoleColor.Magenta);

        }
        public void Jump()
        {
            if (!DB.DbCheck()) return;
            if (!QueueCheck()) return;
            Io.Write("Jump:", default, ConsoleColor.DarkBlue);
            var rmf = SrcQueue.Dequeue();
            Io.WriteLine($"{rmf.FullName}", default, ConsoleColor.DarkRed);
            Io.WriteLine($"[{SrcQueue.Count}] Items Remaining.", default, ConsoleColor.Magenta);

        }
        public void Jmp() => Jump();
        public void Remove()
        {
            if (!DB.DbCheck()) return;
            if (!QueueCheck()) return;
            Io.Write("Remove:", default, ConsoleColor.Red);
            var rmf = SrcQueue.Dequeue();
            Io.WriteLine($"{rmf.FullName}", default, ConsoleColor.Blue);
            rmf.Delete();
            Io.WriteLine($"[{SrcQueue.Count}] Items Remaining.", default, ConsoleColor.Magenta);

        }
        public void Rm()
            => Remove();
        public void Add()
        {
            if (!DB.DbCheck()) return;
            if (!QueueCheck()) return;
            var itemInfoArr = DbEngine.SplitFn(SrcQueue.Peek());
            Regex numbRgx = new Regex("[0-9]");
            var itemInfo = new DbEngine.DbStore.SrcItem
            {
                Path = SrcQueue.Peek().FullName,
                Name = itemInfoArr.Length > 4 && numbRgx.IsMatch(itemInfoArr[4])
                ? $"{itemInfoArr[2].ToUpper()}-{itemInfoArr[3].ToUpper()}-{itemInfoArr[4]}"
                : $"{itemInfoArr[2].ToUpper()}-{itemInfoArr[3].ToUpper()}",


            };
            SrcMan.DbEngine.DbStore.SrcActor actor;
            if (!DB.Store.Actors.Contains(itemInfoArr[1]))
            {
                actor = new DbEngine.DbStore.SrcActor();
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
                        DB.Store.Labels.Add(new DbEngine.DbStore.SrcLabel() { Name = label.ToUpper() });
                    }
                    DB.Store.Labels[label.ToUpper()].Items.Add(itemInfo);
                }
            }
            itemInfo.Stared = itemInfo.Labels.Contains("M");

            DB.Store.Init();
            var sb = new StringBuilder($"{ DbEngine.GetItemCode(itemInfo.Index) }-{actor.Name}-{itemInfo.Name}");
            sb.Append(SrcQueue.Peek().Extension);
            Io.Write("Add:", default, ConsoleColor.Green);
            var fi = SrcQueue.Dequeue();
            Io.WriteLine($"{fi.FullName}\n>>\n{sb.ToString()}", default, ConsoleColor.Yellow);
            fi.MoveTo(Path.Combine(fi.DirectoryName, sb.ToString()));
            itemInfo.Path = Path.Combine(fi.DirectoryName, sb.ToString());
            Io.WriteLine($"[{SrcQueue.Count}] Items Remaining.", default, ConsoleColor.Magenta);

        }
        public void Add(string actorCode, string name)
        {
            if (!DB.DbCheck()) return;
            if (!QueueCheck()) return;
            actorCode = actorCode.ToUpper();
            DbEngine.DbStore.SrcActor actor;
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
                actor = new DbEngine.DbStore.SrcActor();
                actor.Name = actorCode;
                actor.Index = DB.Store.Actors.Count;

                DB.Store.Actors.Add(actor);
            }
            var item = new DbEngine.DbStore.SrcItem();
            var fi = SrcQueue.Dequeue();

            item.Name = name;
            item.Index = 100 * actor.Index + actor.Items.Count;
            actor.Items.Add(item);
            DB.Store.Init();
            var sb = new StringBuilder($"{ DbEngine.GetItemCode(item.Index) }-{actor.Name}-{item.Name}");
            sb.Append(fi.Extension);
            Io.Write("Add:", default, ConsoleColor.Green);
            Io.WriteLine($"{fi.FullName}\n>>\n{sb.ToString()}", default, ConsoleColor.Yellow);
            fi.MoveTo(Path.Combine(fi.DirectoryName, sb.ToString()));
            item.Path = Path.Combine(fi.DirectoryName, sb.ToString());
            Io.WriteLine($"[{SrcQueue.Count}] Items Remaining.", default, ConsoleColor.Magenta);

        }

        private HashSet<DbEngine.DbStore.SrcItem> ConvertFiles { set; get; }

        public void Add(string actorCode, string name, string labels)
        {
            if (!DB.DbCheck()) return;
            if (!QueueCheck()) return;
            actorCode = actorCode.ToUpper();
            DbEngine.DbStore.SrcActor actor;
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
                actor = new DbEngine.DbStore.SrcActor();
                actor.Name = actorCode;
                actor.Index = DB.Store.Actors.Count;

                DB.Store.Actors.Add(actor);
            }
            var item = new DbEngine.DbStore.SrcItem();
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
            var sb = new StringBuilder($"{ DbEngine.GetItemCode(item.Index) }-{actor.Name}-{item.Name}");
            if (item.Labels.Count != 0)
            {
                foreach (var label in item.Labels)
                {
                    sb.Append($"-{label}");
                }
            }
            sb.Append(fi.Extension);
            Io.Write("Add:", default, ConsoleColor.Green);
            Io.WriteLine($"{fi.FullName}\n>>\n{sb.ToString()}", default, ConsoleColor.Yellow);
            fi.MoveTo(Path.Combine(fi.DirectoryName, sb.ToString()));
            item.Path = Path.Combine(fi.DirectoryName, sb.ToString());
            Io.WriteLine($"[{SrcQueue.Count}] Items Remaining.", default, ConsoleColor.Magenta);

        }
        public void AddC()
        {
            if (!DB.DbCheck()) return;
            if (!QueueCheck()) return;
            var itemInfoArr = DbEngine.SplitFn(SrcQueue.Peek());
            var numbRgx = new Regex("[0-9]");
            var itemInfo = new DbEngine.DbStore.SrcItem
            {
                Path = SrcQueue.Peek().FullName,
                Name = itemInfoArr.Length > 4 && numbRgx.IsMatch(itemInfoArr[4])
                ? $"{itemInfoArr[2].ToUpper()}-{itemInfoArr[3].ToUpper()}-{itemInfoArr[4]}"
                : $"{itemInfoArr[2].ToUpper()}-{itemInfoArr[3].ToUpper()}",


            };
            SrcMan.DbEngine.DbStore.SrcActor actor;
            if (!DB.Store.Actors.Contains(itemInfoArr[1]))
            {
                actor = new DbEngine.DbStore.SrcActor();
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
                        DB.Store.Labels.Add(new DbEngine.DbStore.SrcLabel() { Name = label.ToUpper() });
                    }
                    DB.Store.Labels[label.ToUpper()].Items.Add(itemInfo);
                }
            }
            itemInfo.Stared = itemInfo.Labels.Contains("M");

            DB.Store.Init();
            var sb = new StringBuilder($"{ DbEngine.GetItemCode(itemInfo.Index) }-{actor.Name}-{itemInfo.Name}");
            sb.Append(SrcQueue.Peek().Extension);
            Io.Write("Add:", default, ConsoleColor.Green);
            var fi = SrcQueue.Dequeue();
            Io.WriteLine($"{fi.FullName}\n>>\n{sb.ToString()}", default, ConsoleColor.Yellow);

            itemInfo.Path = Path.Combine(fi.DirectoryName, sb.ToString());
            ConvertFiles.Add(itemInfo);
            fi.MoveTo(Path.Combine(ConfigData.ConvertPath, sb.ToString()));
            Io.WriteLine($"[{SrcQueue.Count}] Items Remaining.", default, ConsoleColor.Magenta);

        }

        public bool ConvertCheck()
        {
            var rmStk = new Stack<DbEngine.DbStore.SrcItem>();
            foreach (var convertFile in ConvertFiles)
            {
                if (File.Exists(convertFile.Path))
                {
                    rmStk.Push(convertFile);

                }
                else
                {
                    Io.WriteLine($"Converting File {convertFile.Name}", IoInterface.OutputType.Error);
                    return false;
                }
            }

            while (rmStk.Any())
            {
                ConvertFiles.Remove(rmStk.Pop());
            }
            File.WriteAllText(Path.Combine(ConfigData.ConfigPath, "CvtSet.json"),
                JsonConvert.SerializeObject(ConvertFiles));
            return true;
        }
        public void AddC(string actorCode, string name)
        {
            if (!DB.DbCheck()) return;
            if (!QueueCheck()) return;
            actorCode = actorCode.ToUpper();
            DbEngine.DbStore.SrcActor actor;
            if (ActorCodeRegex.IsMatch(actorCode))
            {
                var index = 26 * (actorCode[0] - 'A') + actorCode[1] - 'A';
                actor = DB.Store.Actors.FirstOrDefault(a => a.Index == index);
            }
            else
            {
                actor = DB.Store.Actors.FirstOrDefault(a => a.Name.Contains(actorCode));
            }
            if (actor == null)
            {
                actor = new DbEngine.DbStore.SrcActor();
                actor.Name = actorCode;
                actor.Index = DB.Store.Actors.Count;

                DB.Store.Actors.Add(actor);
            }
            var item = new DbEngine.DbStore.SrcItem();
            var fi = SrcQueue.Dequeue();

            item.Name = name;
            item.Index = 100 * actor.Index + actor.Items.Count;
            actor.Items.Add(item);
            DB.Store.Init();
            var sb = new StringBuilder($"{ DbEngine.GetItemCode(item.Index) }-{actor.Name}-{item.Name}");
            sb.Append(fi.Extension);
            Io.Write("Add:", default, ConsoleColor.Green);
            Io.WriteLine($"{fi.FullName}\n>>\n{sb.ToString()}", default, ConsoleColor.Yellow);
            item.Path = Path.Combine(fi.DirectoryName, sb.ToString());
            ConvertFiles.Add(item);
            fi.MoveTo(Path.Combine(ConfigData.ConvertPath, sb.ToString()));
            Io.WriteLine($"[{SrcQueue.Count}] Items Remaining.", default, ConsoleColor.Magenta);
        }
        public void AddC(string actorCode, string name, string labels)
        {
            if (!DB.DbCheck()) return;
            if (!QueueCheck()) return;
            actorCode = actorCode.ToUpper();
            DbEngine.DbStore.SrcActor actor;
            if (ActorCodeRegex.IsMatch(actorCode))
            {
                var index = 26 * (actorCode[0] - 'A') + actorCode[1] - 'A';
                actor = DB.Store.Actors.FirstOrDefault(a => a.Index == index);
            }
            else
            {
                actor = DB.Store.Actors.FirstOrDefault(a => a.Name.Contains(actorCode));
            }
            if (actor == null)
            {
                actor = new DbEngine.DbStore.SrcActor
                {
                    Name = actorCode,
                    Index = DB.Store.Actors.Count
                };

                DB.Store.Actors.Add(actor);
            }
            var item = new DbEngine.DbStore.SrcItem();
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
            var sb = new StringBuilder($"{ DbEngine.GetItemCode(item.Index) }-{actor.Name}-{item.Name}");
            if (item.Labels.Count != 0)
            {
                foreach (var label in item.Labels)
                {
                    sb.Append($"-{label}");
                }
            }
            sb.Append(fi.Extension);
            Io.Write("Add:", default, ConsoleColor.Green);
            Io.WriteLine($"{fi.FullName}\n>>\n{sb.ToString()}", default, ConsoleColor.Yellow);
            ConvertFiles.Add(item);
            item.Path = Path.Combine(fi.DirectoryName, sb.ToString());
            fi.MoveTo(Path.Combine(ConfigData.ConvertPath, sb.ToString()));

            Io.WriteLine($"[{SrcQueue.Count}] Items Remaining.", default, ConsoleColor.Magenta);

        }

        public void Play()
        {
            if (!QueueCheck())
            {
                return;
            }
            var src = SrcQueue.Peek();
            Io.Write("Play:", default, ConsoleColor.Cyan);
            Io.WriteLine($"{src.FullName}", default, ConsoleColor.Yellow);
            Io.WriteLine($"Size:{(src.Length >> 20)}MB", default, ConsoleColor.DarkMagenta);
            Io.WriteLine($"[{SrcQueue.Count - 1}] Items Remaining.", default, ConsoleColor.Magenta);

            var player = new Process
            {
                StartInfo =
                {
                    FileName = @"C:\Program Files\DAUM\PotPlayer\PotPlayerMini64.exe", Arguments = src.FullName
                }
            };
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
                    Io.Write("Enqueued:", default, ConsoleColor.Green);
                    Io.WriteLine(file.FullName, default, ConsoleColor.Yellow);

                }
                else
                {
                    file.Delete();
                    Io.Write("Removed:", default, ConsoleColor.Red);
                    Io.WriteLine(file.FullName, default, ConsoleColor.Blue);

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
                        file.MoveTo(Path.Combine(ConfigData.CachePath, file.Name));
                        Io.Write("Moved:", default, ConsoleColor.Cyan);
                        Io.WriteLine($"{file.FullName}", default, ConsoleColor.Yellow);

                    }
                }
                dir.Delete(true);
                Io.Write("Remove:", default, ConsoleColor.Red);
                Io.WriteLine(dir.FullName, default, ConsoleColor.Blue);

            }
        }
        [MobileSuitInfo("MergeLabel <SrcLabel> <DesLabel>")]
        public void MergeLabel(string source, string destination)
        {
            if (!DB.Store.Labels.Contains(source.ToUpper()))
            {
                Io.WriteLine("No Such Label!", default, ConsoleColor.Red);

                return;
            }
            foreach (var item in DB.Store.Labels[source.ToUpper()].Items)
            {
                item.Labels.Remove(source.ToUpper());
                item.Labels.Add(destination.ToUpper());
            }
            DB.Store.Init();
            Io.WriteLine($"Label Merged:{source}>>{destination}, use 'DB Format' to format.", default, ConsoleColor.Green);

        }
        private IoInterface Io { get; set; }
        public void SetIo(IoInterface io)
        {
            Io = io;
            DB.SetIo(Io);
            FindHandler.SetIo(Io);

        }
    }
}
