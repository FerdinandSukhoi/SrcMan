#nullable enable
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;

namespace SrcMan
{
    partial class SrcMan
    {
        public class DBEngine
        {
            public class DBStore
            {
                public SrcActor.SrcActorCollection Actors { get; set; } = new SrcActor.SrcActorCollection();
                [JsonIgnore]
                public SrcItem.SrcItemCollection Items { get; set; } = new SrcItem.SrcItemCollection();
                [JsonIgnore]
                public SrcLabel.SrcLabelCollection Labels { get; set; } = new SrcLabel.SrcLabelCollection();
                public class SrcLabel
                {
                    public class SrcLabelCollection : KeyedCollection<string, SrcLabel>
                    {
                        protected override string GetKeyForItem(SrcLabel item)
                            => item?.Name;
                    }
                    public string? Name { get; set; }

                    [JsonIgnore]
                    public SrcItem.SrcItemCollection Items { get; set; } 
                        = new SrcItem.SrcItemCollection();
                }
                public class SrcActor
                {
                    public class SrcActorCollection : KeyedCollection<string, SrcActor>
                    {
                        protected override string GetKeyForItem(SrcActor item)
                            => item.Name;
                    }
                    public int Index { get; set; }
                    public string? Name { get; set; }

                    public bool Stared { get; set; }
                    public SrcItem.SrcItemCollection Items { get; set; } = new SrcItem.SrcItemCollection();

                    public void Count()
                    {
                        Console.WriteLine($"Actor {Name} Has {Items.Count} Item(s) In DB");
                    }

                }
                public class SrcItem
                {
                    public class SrcItemCollection : KeyedCollection<string, SrcItem>
                    {
                        protected override string GetKeyForItem(SrcItem item)
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
                public DBStore() { }
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
            internal DBStore? Store { get; set; } 
    
            SrcManBase.Config Config { get; set; }
            
            public void Order()
            {
                if (!DBCheck()) return;
                int i = 0;
                foreach (var actor in Store?.Actors)
                {
                    actor.Index = i;
                    foreach (var item in actor.Items)
                    {
                        item.Index = item.Index % 100 + 100 * i;
                    }
                    i++;
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"DB Ordered successfully.");
                Console.ResetColor();
                Format();
            }
            public DBEngine(SrcManBase.Config config)
            {
                Config = config;
                
            }
            private bool ConfigCheck()
            {
                if (Config==null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No Config Loaded!Use 'init' or 'loadcfg' command to initialize Config.");
                    Console.ResetColor();
                    return false;
                }
                return true;
            }
            internal bool DBCheck()
            {
                if (Store == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No DB Loaded/Built!Use 'db build' or 'db load' command to initialize DB.");
                    Console.ResetColor();
                    return false;
                }
                return true;
            }
            internal static string[] SplitFN(FileInfo src)
            {
                return src.Name.Replace("_", "-").Replace(src.Extension, "").Split("-");
            }
            public void Load()
            {
                if (!ConfigCheck()) return;
                //Console.ForegroundColor = ConsoleColor.Blue;
                //Console.Write("Enter Source Dirctory>");
                //Console.ResetColor();
                var dbpath = Path.Combine(Config.ConfigPath, "SrcDB.json");
                if (!Directory.Exists(Config.ConfigPath)||!File.Exists(dbpath))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Dirctory/File Not Exist!");
                    Console.ResetColor();
                    return;
                }
                Store = JsonConvert.DeserializeObject<DBStore>(File.ReadAllText(dbpath));
                Store.Init();
            }
            public void Build()
            {
                Store = new DBStore();
                if (!ConfigCheck()) return;
                //Console.ForegroundColor = ConsoleColor.Blue;
                //Console.Write("Enter Source Dirctory>");
                //Console.ResetColor();
                if (!Directory.Exists(Config.DataPath))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Dirctory Not Exist!");
                    Console.ResetColor();
                    return;
                }
                Regex numbRgx = new Regex("[0-9]");
                DirectoryInfo di = new DirectoryInfo(Config.DataPath);
                foreach (var actorShell in di.GetDirectories())
                {
                    foreach (var actorDir in actorShell.GetDirectories())
                    {
                        var actorInfo = actorDir.Name.Split('-');

                        var actorName = actorInfo[1];
                        var actor = new DBStore.SrcActor
                        {
                            Index = 26 * (actorInfo[0][0] - 'A') + actorInfo[0][1] - 'A',
                            Name = actorName,
                            Stared = (actorInfo.Length == 3)
                        };
                        Store.Actors.Add(actor);
                        int itemIndex = 0;
                        foreach (var item in actorDir.GetFiles())
                        {
                            var itemInfoArr = SplitFN(item);
                            var itemInfo = new DBStore.SrcItem
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
                                        Store.Labels.Add(new DBStore.SrcLabel() { Name = label.ToUpper() });
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
                        //foreach (var itemBox in actorDir.GetDirectories())
                        //{
                        //    int i = 1;
                        //    foreach (var item in itemBox.GetFiles())
                        //    {
                        //        var itemInfoArr = SplitFN(item);
                        //        var itemInfo = new DBStore.SrcItem();
                        //        itemInfo.Path = item.FullName;
                        //        itemInfo.Name = $"{itemInfoArr[2].ToUpper()}-{itemInfoArr[3].ToUpper()}-{i}";
                        //        if (itemInfoArr.Length > 4)
                        //        {
                        //            foreach (var label in itemInfoArr[4..])
                        //            {
                        //                if (numbRgx.IsMatch(label.ToUpper())) 
                        //                    continue;
                        //                itemInfo.Labels.Add(label.ToUpper());
                        //                if (!Store.Labels.Contains(label.ToUpper()))
                        //                {
                        //                    Store.Labels.Add(new DBStore.SrcLabel() { Name = label.ToUpper() });
                        //                }
                        //                Store.Labels[label.ToUpper()].Items.Add(itemInfo);


                        //            }
                        //        }
                        //        Store.Actors[actorName].Items.Add(itemInfo);
                        //        Store.Items.Add(itemInfo);
                        //        itemInfo.Stared = itemInfo.Labels.Contains("M");
                        //        itemInfo.Index = actor.Index * 100 + itemIndex;
                        //        itemIndex++;
                        //        i++;
                        //    }
                        //}
                    } 
                    

                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("SrcMan DB Built Successfully. Use 'db save' to save db. Use 'db format' to format files.");
                Console.ResetColor();
            }
            public void Save()
            {
                if (!ConfigCheck()) return;
                if (!DBCheck()) return;
                //Console.ForegroundColor = ConsoleColor.Blue;
                //Console.Write("Enter Source Dirctory>");
                //Console.ResetColor();
                if (!Directory.Exists(Config.ConfigPath))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Dirctory Not Exist!");
                    Console.ResetColor();
                    return;
                }
                File.WriteAllText(Path.Combine(Config.ConfigPath, "SrcDB.json"), JsonConvert.SerializeObject(Store));
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("SrcMan DB Saved Successfully. Use 'db load' to load db. Use 'db format' to format files.");
                Console.ResetColor();
            }
            internal static string GetActorCode(int index)
            {
                return new string(new char[] { (char)(index / 26 + 'A'), (char)(index % 26 + 'A') });
            }
            internal static string GetItemCode(int index)
            {
                return string.Format("{0}{1:00}", GetActorCode(index / 100), index % 100);
            }
            public void Format()
            {
                if (!ConfigCheck()) return;
                if (!DBCheck()) return;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("SrcMan will format your SrcFiles. This will be irreversible. Are you sure to continue?(input y to continue)>");
                Console.ResetColor();
                if (Console.ReadLine().ToLower() == "y")
                {
                    foreach (var actor in Store?.Actors)
                    {
                        var pactorHead = Path.Combine(Config.DataPath, ((char)(actor.Index / 26 + 'A')).ToString());
                        if (!Directory.Exists(pactorHead))
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"MkDir {pactorHead}");
                            Console.ResetColor();
                            Directory.CreateDirectory(pactorHead);
                        }
                        var actorDir= Path.Combine(pactorHead,
                            $"{GetActorCode(actor.Index)}-{actor.Name}{(actor.Stared?"-M":"")}");
                        if (!Directory.Exists(actorDir))
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"MkDir {actorDir}");
                            Console.ResetColor();
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
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"Move {fi.FullName} \n >> {itemPath}");
                            Console.ResetColor();
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
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"RmDir {wkDir.FullName}");
                            Console.ResetColor();
                            wkDir.Delete();

                        }
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"DB formatted successfully.");
                    Console.ResetColor();
                    Save();
                }
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
            //            Console.WriteLine("This is null");
            //            break;
            //    }
            //}
        }
    }
}
