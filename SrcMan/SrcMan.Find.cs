using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlasticMetal.MobileSuit;
using PlasticMetal.MobileSuit.ObjectModel;
using PlasticMetal.MobileSuit.IO;
using static SrcMan.SrcMan.DbEngine.DbStore;

namespace SrcMan
{
    partial class SrcMan
    {
        public class FindEngine:MsClient
        {

            DbEngine DB { get; set; }
            public FindEngine(DbEngine db)
            {
                DB = db;
            }
            public void A(string arg0) => Actor(arg0);
            public void Actor(string arg0)
            {
                if (!DB.DbCheck()) return;
                var acts = DB.Store.Actors.Where(a => a.Name.Contains(arg0)).ToList();
                if (!acts?.Any()??false)
                {
                    Io.WriteLine("No such Actor found.", OutputType.Error);
                }
                else
                {
                    foreach (var actr in  acts)
                    {
                        Io.WriteLine("");
                        Io.Write("Actor ");
                        Io.Write(actr.Name,default, actr.Stared ? ConsoleColor.Green : ConsoleColor.DarkGreen);
                        Io.Write(" Index ");
                        Io.Write(DbEngine.GetActorCode(actr.Index),default, actr.Stared ? ConsoleColor.Green : ConsoleColor.DarkGreen);
                        Io.Write(" with ");
                        Io.Write(actr.Items.Count.ToString(),default, ConsoleColor.DarkGreen);
                        Io.Write(" Items:\n");
                        foreach (var item in actr.Items)
                        {
                            Io.Write($"\t {item.Name}\t",default, item.Stared ? ConsoleColor.Yellow : ConsoleColor.DarkYellow);
                            Io.Write($"{DbEngine.GetItemCode(item.Index)}\t",default, ConsoleColor.Cyan);
                            if (item.Labels.Count > 0)
                            {
                                foreach (var label in item.Labels)
                                {
                                    Io.Write($" {label}",default, ConsoleColor.Blue);
                                }
                            }
                            Io.Write("\n");

                        }
                    }
                }


            }
            public void A(string arg0, string arg1) => Actor(arg0, arg1);
            public void Actor(string arg0, string arg1)
            {
                if (!DB.DbCheck()) return;
                var acts = DB.Store.Actors.Where(a => a.Name.Contains(arg0) && a.Name.Contains(arg1)).ToList();
                if (!acts.Any())
                {
                    Io.WriteLine("No such Actor found.",OutputType.Error);
                }
                else
                {
                    foreach (var actr in acts)
                    {
                        Io.WriteLine("");
                        Io.Write("Actor ");
                        Io.Write(actr.Name,default, actr.Stared ? ConsoleColor.Green : ConsoleColor.DarkGreen);
                        Io.Write(" Index ");
                        Io.Write(DbEngine.GetActorCode(actr.Index), default, actr.Stared ? ConsoleColor.Green : ConsoleColor.DarkGreen);
                        Io.Write(" with ");
                        
                        Io.Write(actr.Items.Count.ToString(), default, ConsoleColor.DarkGreen);
                        
                        Io.Write(" Items:\n");
                        foreach (var item in actr.Items)
                        {
                            Io.Write($"\t {item.Name}\t", default, item.Stared ? ConsoleColor.Yellow : ConsoleColor.DarkYellow);
                            Io.Write($"{DbEngine.GetItemCode(item.Index)}\t", default, ConsoleColor.Cyan);
                            if (item.Labels.Count > 0)
                            {
                                
                                foreach (var label in item.Labels)
                                {
                                    Io.Write($" {label}", default, ConsoleColor.Blue);
                                }
                            }
                            Io.Write("\n");
                            

                        }
                    }
                }


            }
            public void L(string arg0) => Label(arg0);
            public void Label(string arg0)
            {
                if (!DB.DbCheck()) return;
                var lbs = DB.Store.Labels.Where(a => a.Name.Contains(arg0.ToUpper()));

                if (lbs.Count() == 0)
                {
                    
                    Io.WriteLine("No such Item(s) found.", default, ConsoleColor.Red);
                    
                }
                else
                {
                    foreach (var lb in lbs)
                    {
                        Io.WriteLine("");
                        Io.Write("Label ");
                        
                        Io.Write(lb.Name, default, ConsoleColor.DarkGreen);
                        
                        Io.Write(" with ");
                        
                        Io.Write(lb.Items.Count.ToString(), default, ConsoleColor.DarkGreen);
                        
                        Io.Write(" Items:\n");
                        foreach (var item in lb.Items)
                        {
                            
                            Io.Write($"\t {item.Name}\t", default, item.Stared ? ConsoleColor.Yellow : ConsoleColor.DarkYellow);
                            
                            Io.Write($"{DbEngine.GetItemCode(item.Index)}\t", default, ConsoleColor.Cyan);
                            
                            Io.Write($"{item.Actress.Name}\t", default, ConsoleColor.Magenta);
                            if (item.Labels.Count > 0)
                            {
                                
                                foreach (var label in item.Labels)
                                {
                                    Io.Write($" {label}", default, ConsoleColor.Blue);
                                }
                            }
                            Io.Write("\n");
                            

                        }
                    }
                }


            }
            public void L(string arg0, string arg1) => Label(arg0, arg1);
            public void Label(string arg0, string arg1)
            {
                if (!DB.DbCheck()) return;
                var itms = DB.Store.Items.Where(a => a.Labels.Contains(arg0.ToUpper()) && a.Labels.Contains(arg1.ToUpper()));

                if (itms.Count() == 0)
                {
                    
                    Io.WriteLine("No such Item(s) found.",OutputType.Error);
                    
                }
                else
                {
                    foreach (var item in itms)
                    {
                        
                        Io.Write($"\t {item.Name}\t", default, item.Stared ? ConsoleColor.Yellow : ConsoleColor.DarkYellow);
                        
                        Io.Write($"{DbEngine.GetItemCode(item.Index)}\t", default, ConsoleColor.Cyan);
                        
                        Io.Write($"{item.Actress.Name}\t", default, ConsoleColor.Magenta);
                        if (item.Labels.Count > 0)
                        {
                            
                            foreach (var label in item.Labels)
                            {
                                Io.Write($" {label}", default, ConsoleColor.Blue);
                            }
                        }
                        Io.Write("\n");
                        
                    }
                }


            }
            public void L(string arg0, string arg1, string arg2) => Label(arg0, arg1, arg2);
            public void Label(string arg0, string arg1, string arg2)
            {
                if (!DB.DbCheck()) return;
                var itms = DB.Store.Items.Where(a => a.Labels.Contains(arg0.ToUpper()) 
                && a.Labels.Contains(arg1.ToUpper()) && a.Labels.Contains(arg2.ToUpper())).ToList();

                if (!itms.Any())
                {
                    
                    Io.WriteLine("No such Item(s) found.",OutputType.Error);
                    
                }
                else
                {
                    foreach (var item in itms)
                    {
                        
                        Io.Write($"\t {item.Name}\t", default, item.Stared ? ConsoleColor.Yellow : ConsoleColor.DarkYellow);
                        
                        Io.Write($"{DbEngine.GetItemCode(item.Index)}\t", default, ConsoleColor.Cyan);
                        
                        Io.Write($"{item.Actress.Name}\t", default, ConsoleColor.Magenta);
                        if (item.Labels.Count > 0)
                        {
                            ;
                            foreach (var label in item.Labels)
                            {
                                Io.Write($" {label}", default, ConsoleColor.Blue);
                            }
                        }
                        Io.Write("\n");
                        
                    }
                }


            }
            public void I(string arg0) => Item(arg0);
            public void Item(string arg0)
            {
                if (!DB.DbCheck()) return;
                var itms = DB.Store.Items.Where(a => a.Name.Contains(arg0.ToUpper())).ToList();

                if (!itms.Any())
                {
                    Io.WriteLine("No such Item(s) found.", default, ConsoleColor.Red);
                    
                }
                else
                {
                    foreach (var item in itms)
                    {
                        ;
                        Io.Write($"\t {item.Name}\t ", default, item.Stared ? ConsoleColor.Yellow : ConsoleColor.DarkYellow);
                        ;
                        Io.Write($"{DbEngine.GetItemCode(item.Index)}\t", default, ConsoleColor.Cyan);
                        ;
                        Io.Write($"{item.Actress.Name}\t", default, ConsoleColor.Magenta);
                        if (item.Labels.Count > 0)
                        {
                            ;
                            foreach (var label in item.Labels)
                            {
                                Io.Write($" {label}", default, ConsoleColor.Blue);
                            }
                        }


                        Io.Write("\n");
                        
                    }
                }


            }
            public void I(string arg0, string arg1) => Item(arg0, arg1);
            public void Item(string arg0, string arg1)
            {
                if (!DB.DbCheck()) return;
                var items = DB.Store.Items.Where(a => a.Name.Contains(arg0.ToUpper()) && a.Name.Contains(arg1.ToUpper())).ToList();

                if (!items.Any())
                {
                    
                    Io.WriteLine("No such Item(s) found.",OutputType.Error);
                    
                }
                else
                {
                    foreach (var item in items)
                    {
                        Io.Write($"\t {item.Name}\t", default, item.Stared ? ConsoleColor.Yellow : ConsoleColor.DarkYellow);
                        Io.Write($"{DbEngine.GetItemCode(item.Index)}\t", default, ConsoleColor.Cyan);
                        Io.Write($"{item.Actress.Name}\t", default, ConsoleColor.Magenta);
                        if (item.Labels.Count > 0)
                        {
                            foreach (var label in item.Labels)
                            {
                                Io.Write($" {label}", default, ConsoleColor.Blue);
                            }
                        }
                        Io.Write("\n");
                        
                    }
                }

            }
        }
    }
}
