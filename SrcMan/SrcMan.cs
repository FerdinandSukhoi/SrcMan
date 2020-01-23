#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PlasticMetal.MobileSuit.IO;
using PlasticMetal.MobileSuit.ObjectModel.Attributes;
using PlasticMetal.MobileSuit.ObjectModel.Interfaces;
using SrcManBase;

namespace SrcMan
{
    [MsInfo("Source Manager")]
    public partial class SrcMan : IIoInteractive
    {
        private static readonly string[] FindLabels =
        {
            "#a", "#l", "#i"
        };

        private readonly string[] SrcExt = {".mp4", ".wmv", ".avi", ".mkv", ".rmvb"};

        public SrcMan()
        {
            if (File.Exists("SrcManConfig.json"))
            {
                ConfigPath = "SrcManConfig.json";
            }
            else
            {
                Io?.WriteLine("");

                ConfigPath = Path.Combine(Io?.ReadLine(
                                              "InputPath of 'SrcManConfig.json', Leave empty to use current path",
                                              default, true, ConsoleColor.Yellow)
                                          ?? "", "SrcManConfig.json");
            }


            if (!File.Exists(ConfigPath))
                Io?.WriteLine("File Not Found. Use 'init' or 'loadcfg' command to initialize.",
                    OutputType.Error);
            else
                ConfigData =
                    JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));

            Db = new DbEngine(ConfigData ?? new Config());

            Load();
            FindHandler = new FindEngine(Db);
            //MTP = new MTPEngine(DB, ConfigData);
        }

        private Queue<FileInfo>? SrcQueue { get; set; }
        [MsInfo("Configs")] public Config? ConfigData { get; set; }
        public string ConfigPath { get; set; }
        public DbEngine? Db { get; set; }
        private FindEngine? FindHandler { get; }
        private Regex ActorCodeRegex { get; } = new Regex("[A-Z][A-Z]");

        private HashSet<DbEngine.DbStore.SrcItem>? ConvertFiles { set; get; }
        private IoServer? Io { get; set; }

        [MsIgnorable]
        public void SetIo(IoServer io)
        {
            Io = io;
            Db?.SetIo(Io);
            FindHandler?.SetIo(Io);
        }

        public void Init()
        {
            ConfigData = new Config();
            Config();
            Db?.Build();
        }

        public void Config()
        {
            if (ConfigData is null) ConfigData = new Config();
            ConfigData.DataPath = Io?.ReadLine($"Enter Source Directory\n(Now={ConfigData.DataPath})>",
                ConfigData.DataPath, true, ConsoleColor.Blue);
            ConfigData.ConfigPath = Io?.ReadLine($"Enter ConfigFiles Directory\n(Now={ConfigData.ConfigPath})",
                ConfigData.ConfigPath, true, ConsoleColor.Blue);
            ConfigData.CachePath = Io?.ReadLine($"Enter CacheFiles Directory\n(Now={ConfigData.CachePath})",
                ConfigData.CachePath, true, ConsoleColor.Blue);
            ConfigData.ConvertPath = Io?.ReadLine($"Enter ConvertFiles Directory\n(Now={ConfigData.ConvertPath})",
                ConfigData.ConvertPath, true, ConsoleColor.Blue);
            ConfigData.PullFilePath = Io?.ReadLine($"Enter Pull Path\n(Now={ConfigData.PullFilePath})",
                ConfigData.PullFilePath, true, ConsoleColor.Blue);
            ConfigData.MTPDeviceName =
                Io?.ReadLine(
                    $"Enter MTP Device Name (May use 'MTP List' command to get)\n(Now={ConfigData.MTPDeviceName})",
                    ConfigData.MTPDeviceName, true, ConsoleColor.Blue);
            ConfigData.MTPDirectoryPath =
                Io?.ReadLine(
                    $"Enter MTP Device Directory Path (May use 'MTP BFSDir10' command to get some examples)\n(Now={ConfigData.MTPDirectoryPath})",
                    ConfigData.MTPDirectoryPath, true, ConsoleColor.Blue);
            Io?.WriteLine("");
            Io?.WriteLine("Now:");
            Io?.WriteLine($"DataFiles Directory={ConfigData.DataPath}");
            Io?.WriteLine($"ConfigFiles Directory={ConfigData.ConfigPath}");
            Io?.WriteLine($"CacheFiles Directory={ConfigData.CachePath}");
            Io?.WriteLine($"ConvertFiles Directory={ConfigData.ConvertPath}");
            Io?.WriteLine($"Pull Files Directory={ConfigData.PullFilePath}");
            Io?.WriteLine($"MTP Device Name={ConfigData.MTPDeviceName}");
            Io?.WriteLine($"MTP Device Directory Path={ConfigData.MTPDirectoryPath}");
            Io?.WriteLine("");
            if (Io?.ReadLine("All OK?(default input y to save)", "y", true, ConsoleColor.Yellow)?.ToLower() == "y")
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(ConfigData));
        }

        public void Load()
        {
            Db?.Load();
            var cvtSetPath = Path.Combine(ConfigData?.ConfigPath ?? "", "CvtSet.json");
            ConvertFiles = File.Exists(cvtSetPath)
                ? JsonConvert
                    .DeserializeObject<HashSet<DbEngine.DbStore.SrcItem>>
                        (File.ReadAllText(cvtSetPath))
                : new HashSet<DbEngine.DbStore.SrcItem>();
        }

        [MsAlias("Sav")]
        public void Save()
        {
            Db?.Save();
            File.WriteAllText(Path.Combine(ConfigData?.ConfigPath ?? "", "CvtSet.json"),
                JsonConvert.SerializeObject(ConvertFiles));
        }

        [MsAlias("Upd")]
        public void Update()
        {
            if (ConvertCheck()) Db?.Format();
        }

        public string? Find(string[]? args = null)
        {
            if (FindHandler is null) return "'Find Engine' not initialized.";
            switch (args?.Length)
            {
                case 1:
                    Find(args[0]);
                    break;
                case 2:
                    if (FindLabels.Contains(args[0].ToLower()))
                    {
                        var find = args[0].ToLower() switch
                        {
                            "#a" => (Find1) FindHandler.A,
                            "#i" => (Find1) FindHandler.I,
                            "#l" => (Find1) FindHandler.L,
                            _ => null
                        };
                        find?.Invoke(args[1]);
                    }
                    else
                    {
                        Find(args[0], args[1]);
                    }

                    break;
                case 3:
                    if (FindLabels.Contains(args[0].ToLower()))
                    {
                        var find = args[0].ToLower() switch
                        {
                            "#a" => (Find2) FindHandler.A,
                            "#i" => (Find2) FindHandler.I,
                            "#l" => (Find2) FindHandler.L,
                            _ => null
                        };
                        find?.Invoke(args[1], args[2]);
                    }
                    else
                    {
                        Find(args[0], args[1], args[2]);
                    }

                    break;
                default:
                    return "Invalid Command.";
            }

            return null;
        }

        private void Find(string arg0)
        {
            if (FindHandler is null) return;
            FindHandler?.I(arg0);
            FindHandler?.A(arg0);
            FindHandler?.L(arg0);
        }

        private void Find(string arg0, string arg1)
        {
            FindHandler?.I(arg0, arg1);
            FindHandler?.A(arg0, arg1);
            FindHandler?.L(arg0, arg1);
        }

        private void Find(string arg0, string arg1, string arg2)
        {
            FindHandler?.L(arg0, arg1, arg2);
        }

        public void Play(string itemId)
        {
            if (!(Db?.DbCheck() ?? false)) return;
            Save();
            var regex = new Regex("[A-Z][A-Z][0-9][0-9]");
            itemId = itemId.ToUpper();
            var item = regex.IsMatch(itemId)
                ? Db?.Store?.Actors?
                    .FirstOrDefault(a => a.Index == 26 * (itemId[0] - 'A') + itemId[1] - 'A')?.Items
                    .FirstOrDefault(i =>
                        i.Index
                        == 100 * (26 * (itemId[0] - 'A') + itemId[1] - 'A') + 10 * (itemId[2] - '0') + itemId[3] - '0')
                : Db?.Store?.Items.FirstOrDefault(i => i.Name?.Contains(itemId) ?? false);
            if (item is null)
            {
                Io?.WriteLine("No Such Item.", OutputType.Error);
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
            if (!(Db?.DbCheck() ?? false)) return;
            var numbRgx = new Regex("[0-9]");
            foreach (var item in new DirectoryInfo(ConfigData?.PullFilePath ?? "").GetFiles())
            {
                var itemInfoArr = item.FullName.Replace(item.Extension, "").Split("-");
                var itemInfo = new DbEngine.DbStore.SrcItem
                {
                    Name = itemInfoArr.Length > 4 && numbRgx.IsMatch(itemInfoArr[4])
                        ? $"{itemInfoArr[2].ToUpper()}-{itemInfoArr[3].ToUpper()}-{itemInfoArr[4]}"
                        : $"{itemInfoArr[2].ToUpper()}-{itemInfoArr[3].ToUpper()}"
                };
                DbEngine.DbStore.SrcActor actor;
                if (!(Db?.Store?.Actors.Contains(itemInfoArr[1]) ?? false))
                {
                    actor = new DbEngine.DbStore.SrcActor {Name = itemInfoArr[1], Index = Db?.Store?.Actors.Count ?? 0};

                    Db?.Store?.Actors.Add(actor);
                }
                else
                {
                    actor = Db.Store.Actors[itemInfoArr[1]];
                }

                itemInfo.Index = 100 * actor.Index + actor.Items.Count;
                if (!actor.Items.Contains(itemInfo.Name))
                {
                    actor.Items.Add(itemInfo);
                    if (itemInfoArr.Length > 4)
                        foreach (var label in itemInfoArr[4..])
                        {
                            if (numbRgx.IsMatch(label)) continue;
                            itemInfo.Labels.Add(label.ToUpper());
                            if (!Db?.Store?.Labels.Contains(label.ToUpper()) ?? false)
                                Db?.Store?.Labels.Add(new DbEngine.DbStore.SrcLabel {Name = label.ToUpper()});
                            Db?.Store?.Labels[label.ToUpper()].Items.Add(itemInfo);
                        }

                    itemInfo.Stared = itemInfo.Labels.Contains("M");
                }
                else
                {
                    itemInfo = actor.Items[itemInfo.Name];
                }


                Db?.Store?.Init();
                var sb = new StringBuilder($"{DbEngine.GetItemCode(itemInfo.Index)}-{actor.Name}-{itemInfo.Name}");
                sb.Append(item.Extension);
                Io?.Write("Pulled:", default, ConsoleColor.Green);
                Io?.WriteLine($"{item.FullName}\n>>\n{sb}", default, ConsoleColor.Yellow);
                item.MoveTo(Path.Combine(item.DirectoryName, sb.ToString()));
                itemInfo.Path = Path.Combine(item.DirectoryName, sb.ToString());
            }

            Update();
        }

        private bool QueueCheck()
        {
            if (SrcQueue?.Count == 0)
            {
                Io?.WriteLine("Cache Queue not loaded, use 'EnqueueAll'.", OutputType.Error);

                return false;
            }

            return true;
        }

        public void Peek()
        {
            if (!(Db?.DbCheck() ?? false)) return;
            if (!QueueCheck()) return;
            Io?.Write("Peek:", default, ConsoleColor.Blue);
            var rmf = SrcQueue?.Peek();
            Io?.WriteLine($"{rmf?.FullName}", default, ConsoleColor.Yellow);
            Io?.WriteLine($"Size:{rmf?.Length >> 20}MB", default, ConsoleColor.DarkMagenta);
            Io?.WriteLine($"[{SrcQueue?.Count}] Items Remaining.", default, ConsoleColor.Magenta);
        }

        [MsAlias("Jmp")]
        public void Jump()
        {
            if (!(Db?.DbCheck() ?? false)) return;
            if (!QueueCheck()) return;
            Io?.Write("Jump:", default, ConsoleColor.DarkBlue);
            var rmf = SrcQueue?.Dequeue();
            Io?.WriteLine($"{rmf?.FullName}", default, ConsoleColor.DarkRed);
            Io?.WriteLine($"[{SrcQueue?.Count}] Items Remaining.", default, ConsoleColor.Magenta);
        }

        [MsAlias("Rm")]
        public void Remove()
        {
            if (!(Db?.DbCheck() ?? false)) return;
            if (!QueueCheck()) return;
            Io?.Write("Remove:", default, ConsoleColor.Red);
            var rmf = SrcQueue?.Dequeue();
            Io?.WriteLine($"{rmf?.FullName}", default, ConsoleColor.Blue);
            rmf?.Delete();
            Io?.WriteLine($"[{SrcQueue?.Count}] Items Remaining.", default, ConsoleColor.Magenta);
        }

        public void Add()
        {
            if (!(Db?.DbCheck() ?? false)) return;
            if (!QueueCheck()) return;
            if (SrcQueue is null) return;
            var itemInfoArr = DbEngine.SplitFn(SrcQueue.Peek());
            var numbRgx = new Regex("[0-9]");
            var itemInfo = new DbEngine.DbStore.SrcItem
            {
                Path = SrcQueue?.Peek().FullName,
                Name = itemInfoArr.Length > 4 && numbRgx.IsMatch(itemInfoArr[4])
                    ? $"{itemInfoArr[2].ToUpper()}-{itemInfoArr[3].ToUpper()}-{itemInfoArr[4]}"
                    : $"{itemInfoArr[2].ToUpper()}-{itemInfoArr[3].ToUpper()}"
            };
            DbEngine.DbStore.SrcActor actor;
            if (!(Db?.Store?.Actors.Contains(itemInfoArr[1]) ?? false))
            {
                actor = new DbEngine.DbStore.SrcActor
                {
                    Name = itemInfoArr[1],
                    Index = Db?.Store?.Actors.Count ?? 0
                };

                Db?.Store?.Actors.Add(actor);
            }
            else
            {
                actor = Db.Store.Actors[itemInfoArr[1]];
            }

            itemInfo.Index = 100 * actor.Index + actor.Items.Count;
            actor.Items.Add(itemInfo);
            if (itemInfoArr.Length > 4)
                foreach (var label in itemInfoArr[4..])
                {
                    if (numbRgx.IsMatch(label)) continue;
                    itemInfo.Labels.Add(label.ToUpper());
                    if (!(Db?.Store?.Labels.Contains(label.ToUpper()) ?? false))
                        Db?.Store?.Labels.Add(new DbEngine.DbStore.SrcLabel {Name = label.ToUpper()});
                    Db?.Store?.Labels[label.ToUpper()].Items.Add(itemInfo);
                }

            itemInfo.Stared = itemInfo.Labels.Contains("M");
            Db?.Store?.Init();
            var sb = new StringBuilder($"{DbEngine.GetItemCode(itemInfo.Index)}-{actor.Name}-{itemInfo.Name}");

            sb.Append(SrcQueue?.Peek().Extension);
            Io?.Write("Add:", default, ConsoleColor.Green);
            var fi = SrcQueue?.Dequeue();
            Io?.WriteLine($"{fi?.FullName}\n>>\n{sb}", default, ConsoleColor.Yellow);
            fi?.MoveTo(Path.Combine(fi.DirectoryName, sb.ToString()));
            itemInfo.Path = Path.Combine(fi?.DirectoryName ?? "", sb.ToString());
            Io?.WriteLine($"[{SrcQueue?.Count}] Items Remaining.", default, ConsoleColor.Magenta);
        }

        public void Add(string actorCode, string name)
        {
            if (!(Db?.DbCheck() ?? false)) return;
            if (!QueueCheck()) return;
            actorCode = actorCode.ToUpper();
            DbEngine.DbStore.SrcActor? actor;
            if (ActorCodeRegex.IsMatch(actorCode))
            {
                var index = 26 * (actorCode[0] - 'A') + actorCode[1] - 'A';
                actor = Db?.Store?.Actors?.FirstOrDefault(a => a?.Index == index);
            }
            else
            {
                actor = Db?.Store?.Actors?.Where(a => a?.Name?.Contains(actorCode) ?? false).FirstOrDefault();
            }

            if (actor == null)
            {
                actor = new DbEngine.DbStore.SrcActor {Name = actorCode, Index = Db?.Store?.Actors?.Count ?? 0};

                Db?.Store?.Actors?.Add(actor);
            }

            var item = new DbEngine.DbStore.SrcItem();
            var fi = SrcQueue?.Dequeue();

            item.Name = name;
            item.Index = 100 * actor.Index + actor.Items.Count;
            actor.Items.Add(item);
            Db?.Store?.Init();
            var sb = new StringBuilder($"{DbEngine.GetItemCode(item.Index)}-{actor.Name}-{item.Name}");
            sb.Append(fi?.Extension);
            Io?.Write("Add:", default, ConsoleColor.Green);
            Io?.WriteLine($"{fi?.FullName}\n>>\n{sb}", default, ConsoleColor.Yellow);
            fi?.MoveTo(Path.Combine(fi.DirectoryName, sb.ToString()));
            item.Path = Path.Combine(fi?.DirectoryName ?? "", sb.ToString());
            Io?.WriteLine($"[{SrcQueue?.Count}] Items Remaining.", default, ConsoleColor.Magenta);
        }

        public void Add(string actorCode, string name, string labels)
        {
            if (!(Db?.DbCheck() ?? false)) return;
            if (!QueueCheck()) return;
            actorCode = actorCode.ToUpper();
            DbEngine.DbStore.SrcActor? actor;
            if (ActorCodeRegex.IsMatch(actorCode))
            {
                var index = 26 * (actorCode[0] - 'A') + actorCode[1] - 'A';
                actor = Db?.Store?.Actors.FirstOrDefault(a => a.Index == index);
            }
            else
            {
                actor = Db?.Store?.Actors.FirstOrDefault(a => a?.Name?.Contains(actorCode) ?? false);
            }

            if (actor == null)
            {
                actor = new DbEngine.DbStore.SrcActor {Name = actorCode, Index = Db?.Store?.Actors.Count ?? 0};

                Db?.Store?.Actors.Add(actor);
            }

            var item = new DbEngine.DbStore.SrcItem();
            var fi = SrcQueue?.Dequeue();
            item.Path = fi?.FullName;
            item.Name = name;
            item.Index = 100 * actor.Index + actor.Items.Count;
            var lbs = labels.ToUpper().Split("-");
            foreach (var lb in lbs) item.Labels.Add(lb);
            if (lbs.Contains("M"))
            {
                item.Stared = true;
                actor.Stared = true;
            }

            actor.Items.Add(item);
            Db?.Store?.Init();
            var sb = new StringBuilder($"{DbEngine.GetItemCode(item.Index)}-{actor.Name}-{item.Name}");
            if (item.Labels.Count != 0)
                foreach (var label in item.Labels)
                    sb.Append($"-{label}");
            sb.Append(fi?.Extension);
            Io?.Write("Add:", default, ConsoleColor.Green);
            Io?.WriteLine($"{fi?.FullName}\n>>\n{sb}", default, ConsoleColor.Yellow);
            fi?.MoveTo(Path.Combine(fi.DirectoryName, sb.ToString()));
            item.Path = Path.Combine(fi?.DirectoryName ?? "", sb.ToString());
            Io?.WriteLine($"[{SrcQueue?.Count}] Items Remaining.", default, ConsoleColor.Magenta);
        }

        public void AddC()
        {
            if (!(Db?.DbCheck() ?? false)) return;
            if (!QueueCheck()) return;
            if (SrcQueue is null) return;
            var itemInfoArr = DbEngine.SplitFn(SrcQueue.Peek());
            var numbRgx = new Regex("[0-9]");
            var itemInfo = new DbEngine.DbStore.SrcItem
            {
                Path = SrcQueue.Peek().FullName,
                Name = itemInfoArr.Length > 4 && numbRgx.IsMatch(itemInfoArr[4])
                    ? $"{itemInfoArr[2].ToUpper()}-{itemInfoArr[3].ToUpper()}-{itemInfoArr[4]}"
                    : $"{itemInfoArr[2].ToUpper()}-{itemInfoArr[3].ToUpper()}"
            };
            DbEngine.DbStore.SrcActor actor;
            if (!(Db?.Store?.Actors.Contains(itemInfoArr[1]) ?? false))
            {
                actor = new DbEngine.DbStore.SrcActor {Name = itemInfoArr[1], Index = Db?.Store?.Actors.Count ?? 0};

                Db?.Store?.Actors.Add(actor);
            }
            else
            {
                actor = Db.Store.Actors[itemInfoArr[1]];
            }

            itemInfo.Index = 100 * actor.Index + actor.Items.Count;
            actor.Items.Add(itemInfo);
            if (itemInfoArr.Length > 4)
                foreach (var label in itemInfoArr[4..])
                {
                    if (numbRgx.IsMatch(label)) continue;
                    itemInfo.Labels.Add(label.ToUpper());
                    if (!(Db?.Store?.Labels.Contains(label.ToUpper()) ?? false))
                        Db?.Store?.Labels.Add(new DbEngine.DbStore.SrcLabel {Name = label.ToUpper()});
                    Db?.Store?.Labels[label.ToUpper()].Items.Add(itemInfo);
                }

            itemInfo.Stared = itemInfo.Labels.Contains("M");

            Db?.Store?.Init();
            var sb = new StringBuilder($"{DbEngine.GetItemCode(itemInfo.Index)}-{actor.Name}-{itemInfo.Name}");
            sb.Append(SrcQueue.Peek().Extension);
            Io?.Write("Add:", default, ConsoleColor.Green);
            var fi = SrcQueue.Dequeue();
            Io?.WriteLine($"{fi.FullName}\n>>\n{sb}", default, ConsoleColor.Yellow);

            itemInfo.Path = Path.Combine(fi.DirectoryName, sb.ToString());
            ConvertFiles?.Add(itemInfo);
            fi.MoveTo(Path.Combine(ConfigData?.ConvertPath ?? "", sb.ToString()));
            Io?.WriteLine($"[{SrcQueue.Count}] Items Remaining.", default, ConsoleColor.Magenta);
        }

        private bool ConvertCheck()
        {
            if (ConvertFiles is null) return true;
            var rmStk = new Stack<DbEngine.DbStore.SrcItem>();
            foreach (var convertFile in ConvertFiles)
                if (File.Exists(convertFile.Path))
                {
                    rmStk.Push(convertFile);
                }
                else
                {
                    Io?.WriteLine($"Converting File {convertFile.Name}", OutputType.Error);
                    return false;
                }

            while (rmStk.Any()) ConvertFiles.Remove(rmStk.Pop());
            File.WriteAllText(Path.Combine(ConfigData?.ConfigPath ?? "", "CvtSet.json"),
                JsonConvert.SerializeObject(ConvertFiles));
            return true;
        }

        public void AddC(string actorCode, string name)
        {
            if (!(Db?.DbCheck() ?? false)) return;
            if (!QueueCheck()) return;
            actorCode = actorCode.ToUpper();
            DbEngine.DbStore.SrcActor? actor;
            if (ActorCodeRegex.IsMatch(actorCode))
            {
                var index = 26 * (actorCode[0] - 'A') + actorCode[1] - 'A';
                actor = Db?.Store?.Actors.FirstOrDefault(a => a.Index == index);
            }
            else
            {
                actor = Db?.Store?.Actors.FirstOrDefault(a => a?.Name?.Contains(actorCode) ?? false);
            }

            if (actor == null)
            {
                actor = new DbEngine.DbStore.SrcActor {Name = actorCode, Index = Db?.Store?.Actors.Count ?? 0};

                Db?.Store?.Actors.Add(actor);
            }

            var item = new DbEngine.DbStore.SrcItem();
            if (SrcQueue is null) return;
            var fi = SrcQueue.Dequeue();

            item.Name = name;
            item.Index = 100 * actor.Index + actor.Items.Count;
            actor.Items.Add(item);
            Db?.Store?.Init();
            var sb = new StringBuilder($"{DbEngine.GetItemCode(item.Index)}-{actor.Name}-{item.Name}");
            sb.Append(fi.Extension);
            Io?.Write("Add:", default, ConsoleColor.Green);
            Io?.WriteLine($"{fi.FullName}\n>>\n{sb}", default, ConsoleColor.Yellow);
            item.Path = Path.Combine(fi.DirectoryName, sb.ToString());
            ConvertFiles?.Add(item);
            fi.MoveTo(Path.Combine(ConfigData?.ConvertPath ?? "", sb.ToString()));
            Io?.WriteLine($"[{SrcQueue.Count}] Items Remaining.", default, ConsoleColor.Magenta);
        }

        public void AddC(string actorCode, string name, string labels)
        {
            if (!(Db?.DbCheck() ?? false)) return;
            if (!QueueCheck()) return;
            actorCode = actorCode.ToUpper();
            DbEngine.DbStore.SrcActor? actor;
            if (ActorCodeRegex.IsMatch(actorCode))
            {
                var index = 26 * (actorCode[0] - 'A') + actorCode[1] - 'A';
                actor = Db?.Store?.Actors.FirstOrDefault(a => a.Index == index);
            }
            else
            {
                actor = Db?.Store?.Actors.FirstOrDefault(a => a?.Name?.Contains(actorCode) ?? false);
            }

            if (actor == null)
            {
                actor = new DbEngine.DbStore.SrcActor
                {
                    Name = actorCode,
                    Index = Db?.Store?.Actors.Count ?? 0
                };

                Db?.Store?.Actors.Add(actor);
            }

            var item = new DbEngine.DbStore.SrcItem();
            if (SrcQueue is null) return;
            var fi = SrcQueue.Dequeue();
            item.Path = fi.FullName;
            item.Name = name;
            item.Index = 100 * actor.Index + actor.Items.Count;
            var lbs = labels.ToUpper().Split("-");
            foreach (var lb in lbs) item.Labels.Add(lb);
            if (lbs.Contains("M"))
            {
                item.Stared = true;
                actor.Stared = true;
            }

            actor.Items.Add(item);
            Db?.Store?.Init();
            var sb = new StringBuilder($"{DbEngine.GetItemCode(item.Index)}-{actor.Name}-{item.Name}");
            if (item.Labels.Count != 0)
                foreach (var label in item.Labels)
                    sb.Append($"-{label}");
            sb.Append(fi.Extension);
            Io?.Write("Add:", default, ConsoleColor.Green);
            Io?.WriteLine($"{fi.FullName}\n>>\n{sb}", default, ConsoleColor.Yellow);
            ConvertFiles?.Add(item);
            item.Path = Path.Combine(fi.DirectoryName, sb.ToString());
            fi.MoveTo(Path.Combine(ConfigData?.ConvertPath ?? "", sb.ToString()));

            Io?.WriteLine($"[{SrcQueue.Count}] Items Remaining.", default, ConsoleColor.Magenta);
        }

        public void Play()
        {
            if (SrcQueue is null) return;
            if (!QueueCheck()) return;
            var src = SrcQueue.Peek();
            Io?.Write("Play:", default, ConsoleColor.Cyan);
            Io?.WriteLine($"{src.FullName}", default, ConsoleColor.Yellow);
            Io?.WriteLine($"Size:{src.Length >> 20}MB", default, ConsoleColor.DarkMagenta);
            Io?.WriteLine($"[{SrcQueue.Count - 1}] Items Remaining.", default, ConsoleColor.Magenta);

            var player = new Process
            {
                StartInfo =
                {
                    FileName = @"C:\Program Files\DAUM\PotPlayer\PotPlayerMini64.exe", Arguments = src.FullName
                }
            };
            player.Start();
        }

        [MsAlias("Enq")]
        public string EnQueueAll()
        {
            SrcQueue = new Queue<FileInfo>();
            DepackageFolders();
            foreach (var file in new DirectoryInfo(ConfigData?.CachePath).GetFiles())
                if (SrcExt.Contains(file.Extension.ToLower()))
                {
                    SrcQueue.Enqueue(file);
                    Io?.Write("Queued:", default, ConsoleColor.Green);
                    Io?.WriteLine(file.FullName, default, ConsoleColor.Yellow);
                }
                else
                {
                    file.Delete();
                    Io?.Write("Removed:", default, ConsoleColor.Red);
                    Io?.WriteLine(file.FullName, default, ConsoleColor.Blue);
                }

            return $"{SrcQueue.Count} Items are queued.";
        }

        private void DepackageFolders()
        {
            foreach (var dir in new DirectoryInfo(ConfigData?.CachePath ?? "").GetDirectories())
            {
                foreach (var file in dir.GetFiles())
                    if (SrcExt.Contains(file.Extension.ToLower()))
                    {
                        file.MoveTo(Path.Combine(ConfigData?.CachePath ?? "", file.Name));
                        Io?.Write("Moved:", default, ConsoleColor.Cyan);
                        Io?.WriteLine($"{file.FullName}", default, ConsoleColor.Yellow);
                    }

                dir.Delete(true);
                Io?.Write("Remove:", default, ConsoleColor.Red);
                Io?.WriteLine(dir.FullName, default, ConsoleColor.Blue);
            }
        }

        [MsInfo("MergeLabel <SrcLabel> <DesLabel>")]
        public void MergeLabel(string source, string destination)
        {
            if (!(Db?.Store?.Labels.Contains(source.ToUpper()) ?? false))
            {
                Io?.WriteLine("No Such Label!", default, ConsoleColor.Red);

                return;
            }

            foreach (var item in Db.Store.Labels[source.ToUpper()].Items)
            {
                item.Labels.Remove(source.ToUpper());
                item.Labels.Add(destination.ToUpper());
            }

            Db.Store.Init();
            Io?.WriteLine($"Label Merged:{source}>>{destination}, use 'DB Format' to format.", default,
                ConsoleColor.Green);
        }

        private delegate void Find1(string arg0);

        private delegate void Find2(string arg0, string arg1);

        private delegate void Find3(string arg0, string arg2, string arg3);
    }
}