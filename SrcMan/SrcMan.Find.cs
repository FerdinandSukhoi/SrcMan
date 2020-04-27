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
        public class FindEngine:SuitClient
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
                    
                    IO.WriteLine("No such Actor found.", OutputType.Error);
                }
                else
                {
                    foreach (var actr in  acts)
                    {
                        IO.WriteLine("");
                        IO.Write("Actor ");
                        IO.Write(actr.Name,default, actr.Stared ? ConsoleColor.Green : ConsoleColor.DarkGreen);
                        IO.Write(" Index ");
                        IO.Write(DbEngine.GetActorCode(actr.Index),default, actr.Stared ? ConsoleColor.Green : ConsoleColor.DarkGreen);
                        IO.Write(" with ");
                        IO.Write(actr.Items.Count.ToString(),default, ConsoleColor.DarkGreen);
                        IO.Write(" Items:\n");
                        foreach (var item in actr.Items)
                        {
                            IO.Write($"\t {item.Name}\t",default, item.Stared ? ConsoleColor.Yellow : ConsoleColor.DarkYellow);
                            IO.Write($"{DbEngine.GetItemCode(item.Index)}\t",default, ConsoleColor.Cyan);
                            if (item.Labels.Count > 0)
                            {
                                foreach (var label in item.Labels)
                                {
                                    IO.Write($" {label}",default, ConsoleColor.Blue);
                                }
                            }
                            IO.Write("\n");

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
                    IO.WriteLine("No such Actor found.",OutputType.Error);
                }
                else
                {
                    foreach (var actr in acts)
                    {
                        IO.WriteLine("");
                        IO.Write("Actor ");
                        IO.Write(actr.Name,default, actr.Stared ? ConsoleColor.Green : ConsoleColor.DarkGreen);
                        IO.Write(" Index ");
                        IO.Write(DbEngine.GetActorCode(actr.Index), default, actr.Stared ? ConsoleColor.Green : ConsoleColor.DarkGreen);
                        IO.Write(" with ");
                        
                        IO.Write(actr.Items.Count.ToString(), default, ConsoleColor.DarkGreen);
                        
                        IO.Write(" Items:\n");
                        foreach (var item in actr.Items)
                        {
                            IO.Write($"\t {item.Name}\t", default, item.Stared ? ConsoleColor.Yellow : ConsoleColor.DarkYellow);
                            IO.Write($"{DbEngine.GetItemCode(item.Index)}\t", default, ConsoleColor.Cyan);
                            if (item.Labels.Count > 0)
                            {
                                
                                foreach (var label in item.Labels)
                                {
                                    IO.Write($" {label}", default, ConsoleColor.Blue);
                                }
                            }
                            IO.Write("\n");
                            

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
                    
                    IO.WriteLine("No such Item(s) found.", default, ConsoleColor.Red);
                    
                }
                else
                {
                    foreach (var lb in lbs)
                    {
                        IO.WriteLine("");
                        IO.Write("Label ");
                        
                        IO.Write(lb.Name, default, ConsoleColor.DarkGreen);
                        
                        IO.Write(" with ");
                        
                        IO.Write(lb.Items.Count.ToString(), default, ConsoleColor.DarkGreen);
                        
                        IO.Write(" Items:\n");
                        foreach (var item in lb.Items)
                        {
                            
                            IO.Write($"\t {item.Name}\t", default, item.Stared ? ConsoleColor.Yellow : ConsoleColor.DarkYellow);
                            
                            IO.Write($"{DbEngine.GetItemCode(item.Index)}\t", default, ConsoleColor.Cyan);
                            
                            IO.Write($"{item.Actress.Name}\t", default, ConsoleColor.Magenta);
                            if (item.Labels.Count > 0)
                            {
                                
                                foreach (var label in item.Labels)
                                {
                                    IO.Write($" {label}", default, ConsoleColor.Blue);
                                }
                            }
                            IO.Write("\n");
                            

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
                    
                    IO.WriteLine("No such Item(s) found.",OutputType.Error);
                    
                }
                else
                {
                    foreach (var item in itms)
                    {
                        
                        IO.Write($"\t {item.Name}\t", default, item.Stared ? ConsoleColor.Yellow : ConsoleColor.DarkYellow);
                        
                        IO.Write($"{DbEngine.GetItemCode(item.Index)}\t", default, ConsoleColor.Cyan);
                        
                        IO.Write($"{item.Actress.Name}\t", default, ConsoleColor.Magenta);
                        if (item.Labels.Count > 0)
                        {
                            
                            foreach (var label in item.Labels)
                            {
                                IO.Write($" {label}", default, ConsoleColor.Blue);
                            }
                        }
                        IO.Write("\n");
                        
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
                    
                    IO.WriteLine("No such Item(s) found.",OutputType.Error);
                    
                }
                else
                {
                    foreach (var item in itms)
                    {
                        
                        IO.Write($"\t {item.Name}\t", default, item.Stared ? ConsoleColor.Yellow : ConsoleColor.DarkYellow);
                        
                        IO.Write($"{DbEngine.GetItemCode(item.Index)}\t", default, ConsoleColor.Cyan);
                        
                        IO.Write($"{item.Actress.Name}\t", default, ConsoleColor.Magenta);
                        if (item.Labels.Count > 0)
                        {
                            ;
                            foreach (var label in item.Labels)
                            {
                                IO.Write($" {label}", default, ConsoleColor.Blue);
                            }
                        }
                        IO.Write("\n");
                        
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
                    IO.WriteLine("No such Item(s) found.", default, ConsoleColor.Red);
                    
                }
                else
                {
                    foreach (var item in itms)
                    {
                        ;
                        IO.Write($"\t {item.Name}\t ", default, item.Stared ? ConsoleColor.Yellow : ConsoleColor.DarkYellow);
                        ;
                        IO.Write($"{DbEngine.GetItemCode(item.Index)}\t", default, ConsoleColor.Cyan);
                        ;
                        IO.Write($"{item.Actress.Name}\t", default, ConsoleColor.Magenta);
                        if (item.Labels.Count > 0)
                        {
                            ;
                            foreach (var label in item.Labels)
                            {
                                IO.Write($" {label}", default, ConsoleColor.Blue);
                            }
                        }


                        IO.Write("\n");
                        
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
                    
                    IO.WriteLine("No such Item(s) found.",OutputType.Error);
                    
                }
                else
                {
                    foreach (var item in items)
                    {
                        IO.Write($"\t {item.Name}\t", default, item.Stared ? ConsoleColor.Yellow : ConsoleColor.DarkYellow);
                        IO.Write($"{DbEngine.GetItemCode(item.Index)}\t", default, ConsoleColor.Cyan);
                        IO.Write($"{item.Actress.Name}\t", default, ConsoleColor.Magenta);
                        if (item.Labels.Count > 0)
                        {
                            foreach (var label in item.Labels)
                            {
                                IO.Write($" {label}", default, ConsoleColor.Blue);
                            }
                        }
                        IO.Write("\n");
                        
                    }
                }

            }
        }
    }
}
