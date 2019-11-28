//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.IO;
//using MediaDevices;
//using System.Linq;
//using System.Text.RegularExpressions;

//namespace SrcMan
//{
//    partial class SrcMan
//    {
//        public class MTPEngine
//        {
//            List<DBEngine.DBStore.SrcItem> Items { get; set; }
//            SrcManBase.Config Config { get; set; }
//            private DBEngine DB { get; set; }
//            public MTPEngine(DBEngine db, SrcManBase.Config config)
//            {
//                DB = db;
//                Config = config;
//                Items = new List<DBEngine.DBStore.SrcItem>();
//            }
//            public void List()
//            {
//                var devs = MediaDevice.GetDevices();
//                Console.ForegroundColor = ConsoleColor.Blue;
//                Console.Write("Connected Devices:");
//                Console.ForegroundColor = ConsoleColor.Yellow;

//                Console.ResetColor();
//                foreach (var item in devs)
//                {
//                    Console.WriteLine($"\t{item.FriendlyName}");
//                }
//                Console.ResetColor();
//            }
//            private bool QueueCheck()
//            {
//                //if (SrcQueue?.Count == 0)
//                //{
//                //    Console.ForegroundColor = ConsoleColor.Red;
//                //    Console.WriteLine("Cache Queue not loaded, use 'EnqueueAll'.");
//                //    Console.ResetColor();
//                //    return false;
//                //}
//                return true;
//            }
//            public void Push()
//            {
//                var dev = MediaDevice.GetDevices().First(a=>a.FriendlyName==Config.MTPDeviceName);
//                if (Items?.Count==0)
//                {
//                    Console.ForegroundColor = ConsoleColor.Red;
//                    Console.WriteLine("No Item to push. Use 'sync <Item> to add.'");
//                    Console.ResetColor();
//                }
//                if (dev==null)
//                {
//                    Console.ForegroundColor = ConsoleColor.Red;
//                    Console.WriteLine("Device Not Found.");
//                    Console.ResetColor();
//                    return;
//                }
//                dev.Connect();

//                if (!dev.DirectoryExists(Config.MTPDirctoryPath))
//                {
//                    try
//                    {
//                        dev.CreateDirectory(Config.MTPDirctoryPath+"\\");
//                    }
//                    catch (Exception)
//                    {
//                        Console.ForegroundColor = ConsoleColor.Red;
//                        Console.WriteLine("Invalid Location.");
//                        Console.ResetColor();
//                        return;
//                    }
//                }
//                Console.WriteLine(dev.DirectoryExists(Config.MTPDirctoryPath));
//                foreach (var item in Items)
//                {
//                    var fi = new FileInfo(item.Path);
//                    var target = Path.Combine(Config.MTPDirctoryPath, fi.Name);
//                    if (dev.FileExists(target))
//                    {
//                        Console.ForegroundColor = ConsoleColor.Red;
//                        Console.WriteLine($"Skipped:{fi.Name}.");
//                        Console.ResetColor();
//                    }
//                    Console.ForegroundColor = ConsoleColor.Yellow;
//                    Console.WriteLine($"Uploading:{fi.Name}.");
//                    Console.ResetColor();
//                    dev.UploadFile(fi.OpenRead(), "\\"+Path.Combine(Config.MTPDirctoryPath, fi.Name));
//                    Console.ForegroundColor = ConsoleColor.Green;
//                    Console.WriteLine($"Uploaded:{fi.Name}.");
//                    Console.ResetColor();
//                }

//                dev.Disconnect();
//            }
//            public void BFSDir10()
//            {
//                var dev = MediaDevice.GetDevices().First(a => a.FriendlyName == Config.MTPDeviceName);
//                dev.Connect();
//                Queue<string> bfs = new Queue<string>();
//                int i = 0;
//                bfs.Enqueue(@"\");
//                while (bfs.Count != 0)
//                {
//                    foreach (var item in dev.GetDirectories(bfs.Dequeue()))
//                    {
//                        Console.WriteLine(item);
//                        bfs.Enqueue(item);
//                        i++;
//                    }
//                    if (i>=10)
//                    {
//                        break;
//                    }
//                }

//            }
//            private int GetItemIndex(string itemCode)
//            {
//                return 100 * (26 * (itemCode[0] - 'A') + itemCode[1] - 'A') + Convert.ToInt32(itemCode[2..]);
//            }
//            public void DeSync(string itemCode)
//            {
//                if (!DB.DBCheck()) return;
//                itemCode = itemCode.ToUpper();
//                var ICRegex = new Regex("[A-Z][A-Z][0-9][0-9]");
//                DBEngine.DBStore.SrcItem item;
//                if (ICRegex.IsMatch(itemCode))
//                {
//                    item = DB.Store.Items.Where(a => a.Index == GetItemIndex(itemCode)).FirstOrDefault();
//                }
//                else
//                {
//                    item = DB.Store.Items.Where(a => a.Name.Contains(itemCode)).FirstOrDefault();
//                }
//                if (item == null||!Items.Contains(item))
//                {
//                    Console.ForegroundColor = ConsoleColor.Red;
//                    Console.Write("Item Not Found:");
//                    Console.ForegroundColor = ConsoleColor.Yellow;
//                    Console.WriteLine(itemCode);
//                    Console.ResetColor();
//                    return;
//                }
//                Items.Remove(item);
//                Console.ForegroundColor = ConsoleColor.Blue;
//                Console.Write("DeSync:");
//                Console.ForegroundColor = ConsoleColor.Yellow;
//                Console.WriteLine($"{item.Name} by {item.Actress.Name}");
//                Console.ResetColor();
//            }
//            public void Sync(string itemCode)
//            {
//                if (!DB.DBCheck()) return;
//                itemCode = itemCode.ToUpper();
//                var ICRegex = new Regex("[A-Z][A-Z][0-9][0-9]");
//                DBEngine.DBStore.SrcItem item;
//                if (ICRegex.IsMatch(itemCode))
//                {
//                    item = DB.Store.Items.Where(a => a.Index == GetItemIndex(itemCode)).FirstOrDefault();
//                }
//                else
//                {
//                    item = DB.Store.Items.Where(a => a.Name.Contains(itemCode)).FirstOrDefault();
//                }
//                if (item==null)
//                {
//                    Console.ForegroundColor = ConsoleColor.Red;
//                    Console.Write("Item Not Found:");
//                    Console.ForegroundColor = ConsoleColor.Yellow;
//                    Console.WriteLine(itemCode);
//                    Console.ResetColor();
//                    return;
//                }
//                Items.Add(item);
//                Console.ForegroundColor = ConsoleColor.Green;
//                Console.Write("Sync:");
//                Console.ForegroundColor = ConsoleColor.Yellow;
//                Console.WriteLine($"{item.Name} by {item.Actress.Name}");
//                Console.ResetColor();

//            }
//        }
//    }
//}
