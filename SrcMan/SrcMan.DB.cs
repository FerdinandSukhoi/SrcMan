#nullable enable
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using MobileSuit;
using MobileSuit.IO;

namespace SrcMan
{
    partial class SrcMan
    {
        public class DbEngine:IIoInteractive
        {
            private IoInterface? Io { get; set; }
            public void SetIo(IoInterface io)
            {
                Io = io;
            }
            public class DbStore
            {
                public SrcActor.SrcActorCollection Actors { get; set; } = new SrcActor.SrcActorCollection();
                [JsonIgnore]
                public SrcItem.SrcItemCollection Items { get; set; } = new SrcItem.SrcItemCollection();
                [JsonIgnore]
                public SrcLabel.SrcLabelCollection Labels { get; set; } = new SrcLabel.SrcLabelCollection();
                public class SrcLabel
                {
                    public class SrcLabelCollection : KeyedCollection<string?, SrcLabel>
                    {
                        protected override string? GetKeyForItem(SrcLabel item)
                            => item?.Name;
                    }
                    public string? Name { get; set; }

                    [JsonIgnore]
                    public SrcItem.SrcItemCollection Items { get; set; } 
                        = new SrcItem.SrcItemCollection();
                }
                public class SrcActor
                {
                    public class SrcActorCollection : KeyedCollection<string?, SrcActor>
                    {
                        protected override string? GetKeyForItem(SrcActor item)
                            => item?.Name;
                    }
                    public int Index { get; set; }
                    public string? Name { get; set; }

                    public bool Stared { get; set; }
                    public SrcItem.SrcItemCollection Items { get; set; } = new SrcItem.SrcItemCollection();


                }
                public class SrcItem
                {
                    public class SrcItemCollection : KeyedCollection<string?, SrcItem>
                    {
                        protected override string? GetKeyForItem(SrcItem item)
                            => item?.Name;
                    }
                    public string? Path { get; set; }
                    public string? Name { get; set; }
                    public List<string> Labels { get; set; } = new List<string>();
                    public int Index { get; set; }
                    public bool Stared { get; set; }
                    [JsonIgnore]
                    public SrcActor? Actress { get; set; }
                }
                public DbStore() { }
                public void Init()
                {
                    Items.Clear();
                    Labels.Clear();
                    foreach (var actor in Actors)
                    {
                        foreach (var item in actor.Items)
                        {
                            Items.Add(item);
                            item.Actress = actor;
                            if (item.Labels.Count > 0)
                            {
                                foreach (var label in item.Labels)
                                {
                                    if (!Labels.Contains(label))
                                    {
                                        Labels.Add(new SrcLabel() { Name=label });
                                    }
                                    Labels[label].Items.Add(item);
                                }
                            }

                        }
                    }
                }
            }
            internal DbStore? Store { get; set; } 
    
            SrcManBase.Config Config { get; set; }
            
            public void Order()
            {
                if (!DbCheck()) return;
                var i = 0;
                if (Store is null) return;
                foreach (var actor in Store.Actors)
                {
                    actor.Index = i;
                    foreach (var item in actor.Items)
                    {
                        item.Index = item.Index % 100 + 100 * i;
                    }
                    i++;
                }
                ;
                Io?.WriteLine($"DB Ordered successfully.", IoInterface.OutputType.AllOk);
                
                Format();
            }
            public DbEngine(SrcManBase.Config config)
            {
                Config = config;
                
            }
            private bool ConfigCheck()
            {
                if (Config != null) return true;
                Io?.WriteLine("No Config Loaded!Use 'init' or 'loadcfg' command to initialize Config.",IoInterface.OutputType.Error);
                    
                return false;
            }
            internal bool DbCheck()
            {
                if (Store != null) return true;
                ;
                Io?.WriteLine("No DB Loaded/Built!Use 'db build' or 'db load' command to initialize DB.",IoInterface.OutputType.Error);
                
                return false;
            }
            internal static string[] SplitFn(FileInfo src)
            {
                return src.Name.Replace("_", "-").Replace(src.Extension, "").Split("-");
            }
            public void Load()
            {
                if (!ConfigCheck()) return;
                //,default, ConsoleColor.Blue;
                //Io?.Write("Enter Source Directory>");
                //
                var dataBasePath = Path.Combine(Config.ConfigPath, "SrcDB.json");
                if (!Directory.Exists(Config.ConfigPath)||!File.Exists(dataBasePath))
                {
                    Io?.Write("Directory/File Not Exist!", IoInterface.OutputType.Error);
                    
                    return;
                }
                Store = JsonConvert.DeserializeObject<DbStore>(File.ReadAllText(dataBasePath));
                Store.Init();
                MobileSuitHost.GeneralIo.WriteLine("DB Loaded Successfully.",
                    IoInterface.OutputType.AllOk);
            }
            public void Build()
            {
                Store = new DbStore();
                if (!ConfigCheck()) return;
                //,default, ConsoleColor.Blue;
                //Io?.Write("Enter Source Directory>");
                //
                if (!Directory.Exists(Config.DataPath))
                {
                    Io?.Write("Directory Not Exist!", IoInterface.OutputType.Error);
                    
                    return;
                }
                var numbRgx = new Regex("[0-9]");
                var di = new DirectoryInfo(Config.DataPath);
                foreach (var actorShell in di.GetDirectories())
                {
                    foreach (var actorDir in actorShell.GetDirectories())
                    {
                        var actorInfo = actorDir.Name.Split('-');

                        var actorName = actorInfo[1];
                        var actor = new DbStore.SrcActor
                        {
                            Index = 26 * (actorInfo[0][0] - 'A') + actorInfo[0][1] - 'A',
                            Name = actorName,
                            Stared = (actorInfo.Length == 3)
                        };
                        Store.Actors.Add(actor);
                        var itemIndex = 0;
                        foreach (var item in actorDir.GetFiles())
                        {
                            var itemInfoArr = SplitFn(item);
                            if (itemInfoArr.Length < 4)
                            {
                                Io?.WriteLine($"FORMAT ERROR:{itemInfoArr[0]}");
                                continue;
                            }
                            var itemInfo = new DbStore.SrcItem
                            {
                                Path = item.FullName,
                                Name = itemInfoArr.Length > 4 && numbRgx.IsMatch(itemInfoArr[4])
                                ? $"{itemInfoArr[2].ToUpper()}-{itemInfoArr[3].ToUpper()}-{itemInfoArr[4]}"
                                : $"{itemInfoArr[2].ToUpper()}-{itemInfoArr[3].ToUpper()}"
                            };

                            if (itemInfoArr.Length>4)
                            {
                                foreach (var label in itemInfoArr[4..])
                                {
                                    if (numbRgx.IsMatch(label)) continue;
                                    itemInfo.Labels.Add(label.ToUpper());
                                    if (!Store.Labels.Contains(label.ToUpper()))
                                    {
                                        Store.Labels.Add(new DbStore.SrcLabel() { Name = label.ToUpper() });
                                    }
                                    Store.Labels[label.ToUpper()].Items.Add(itemInfo);
                                }
                            }
                            itemInfo.Stared = itemInfo.Labels.Contains("M");
                            itemInfo.Index = actor.Index * 100 + itemIndex;
                            Store.Actors[actorName].Items.Add(itemInfo);
                            Store.Items.Add(itemInfo);
                            itemIndex++;
                        }
                    } 
                    

                }
                Io?.WriteLine("SrcMan DB Built Successfully. Use 'db save' to save db. Use 'db format' to format files.",IoInterface.OutputType.AllOk);
                
            }
            public void Save()
            {
                if (!ConfigCheck()) return;
                if (!DbCheck()) return;
                if (!Directory.Exists(Config.ConfigPath))
                {
                    Io?.Write("Directory Not Exist!", IoInterface.OutputType.Error);
                    
                    return;
                }
                File.WriteAllText(Path.Combine(Config.ConfigPath, "SrcDB.json"), JsonConvert.SerializeObject(Store));
                Io?.WriteLine("SrcMan DB Saved Successfully. Use 'db load' to load db. Use 'db format' to format files.",IoInterface.OutputType.AllOk);
                
            }
            internal static string GetActorCode(int index)
            {
                return new string(new char[] { (char)(index / 26 + 'A'), (char)(index % 26 + 'A') });
            }
            internal static string GetItemCode(int index)
            {
                return $"{GetActorCode(index / 100)}{index % 100:00}";
            }
            public void Format()
            {
                if (!ConfigCheck()) return;
                if (!DbCheck()) return;
                if (Store is null) return;
                
                if (Io?.ReadLine(
                        @"SrcMan will format your SrcFiles. This will be irreversible. Are you sure to continue?(default input y to continue)",
                        "y",true,ConsoleColor.Yellow)?.ToLower() != "y") return;
                foreach (var actor in Store.Actors)
                {
                    var actorPrefixDirectory = Path.Combine(Config.DataPath, ((char)(actor.Index / 26 + 'A')).ToString());
                    if (!Directory.Exists(actorPrefixDirectory))
                    {
                        
                        Io?.WriteLine($"MkDir {actorPrefixDirectory}", default, ConsoleColor.Green);
                        
                        Directory.CreateDirectory(actorPrefixDirectory);
                    }
                    var actorDir= Path.Combine(actorPrefixDirectory,
                        $"{GetActorCode(actor.Index)}-{actor.Name}{(actor.Stared?"-M":"")}");
                    if (!Directory.Exists(actorDir))
                    {
                        
                        Io?.WriteLine($"MkDir {actorDir}", default, ConsoleColor.Green);
                        
                        Directory.CreateDirectory(actorDir);
                    }
                    foreach (var item in actor.Items)
                    {
                        var itemPath= Path.Combine(actorDir,
                            $"{GetItemCode(item.Index)}-{actor.Name}-{item.Name}");
                        var sb = new StringBuilder(itemPath);
                        var fi = new FileInfo(item.Path);
                        if (item.Labels.Count != 0)
                        {
                            foreach (var label in item.Labels)
                            {
                                sb.Append($"-{label}");
                            }

                        }
                        sb.Append(fi.Extension);
                        itemPath = sb.ToString();
                        if (fi.FullName==itemPath)
                        {
                            continue;
                        }
                        
                        Io?.WriteLine($"Move {fi.FullName} \n >> {itemPath}", default, ConsoleColor.Blue);
                        
                        fi.MoveTo(itemPath,true);
                        item.Path = itemPath;
                    }
                }
                //BFS
                var q = new Queue<DirectoryInfo>();
                q.Enqueue(new DirectoryInfo(Config.DataPath));
                while (q.Count>0)
                {
                    var wkDir = q.Dequeue();
                    var dirs = wkDir.GetDirectories();
                    var files = wkDir.GetFiles();
                    if (dirs.Length!=0)
                    {
                        foreach (var subDir in dirs)
                        {
                            q.Enqueue(subDir);
                        }
                    }
                    else if(files.Length==0)
                    {
                        
                        Io?.WriteLine($"RmDir {wkDir.FullName}", default, ConsoleColor.Red);
                        
                        wkDir.Delete();

                    }
                }

                
                Io?.WriteLine($"DB formatted successfully.", default, ConsoleColor.Green);
                
                Save();
            }
            //public void Status(string arg)
            //{

            //    switch (arg)
            //    {
            //        case "-a":
            //            break;
            //        case "-v":

            //        case "-l":

            //        default:
            //            Io?.WriteLine("This is null");
            //            break;
            //    }
            //}
        }
    }
}
