using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static SrcMan.SrcMan.DBEngine.DBStore;

namespace SrcMan
{
    partial class SrcMan
    {
        public class FindEngine
        {
            DBEngine DB { get; set; }
            public FindEngine(DBEngine db)
            {
                DB = db;
            }
            public void A(string arg0) => Actor(arg0);
            public void Actor(string arg0)
            {
                if (!DB.DBCheck()) return;
                var acts = DB.Store.Actors.Where(a => a.Name.Contains(arg0));
                if (acts.Count()==0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No such Actor found.");
                    Console.ResetColor();
                }
                else
                {
                    foreach (var actr in  acts)
                    {
                        Console.WriteLine();
                        Console.Write("Actor ");
                        Console.ForegroundColor = actr.Stared?ConsoleColor.Green: ConsoleColor.DarkGreen;
                        Console.Write(actr.Name);
                        Console.ResetColor();
                        Console.Write(" Index ");
                        Console.ForegroundColor = actr.Stared ? ConsoleColor.Green : ConsoleColor.DarkGreen;
                        Console.Write(DBEngine.GetActorCode(actr.Index));
                        Console.ResetColor();
                        Console.Write(" with ");
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.Write(actr.Items.Count);
                        Console.ResetColor();
                        Console.Write(" Items:\n");
                        foreach (var item in actr.Items)
                        {
                            Console.ForegroundColor = item.Stared ?ConsoleColor.Yellow : ConsoleColor.DarkYellow;
                            Console.Write($"\t {item.Name}\t");
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write($"{DBEngine.GetItemCode(item.Index)}\t");
                            if (item.Labels.Count > 0)
                            {
                                Console.ForegroundColor = ConsoleColor.Blue;
                                foreach (var label in item.Labels)
                                {
                                    Console.Write($" {label}");
                                }
                            }
                            Console.Write("\n");
                            Console.ResetColor();

                        }
                    }
                }


            }
            public void A(string arg0, string arg1) => Actor(arg0, arg1);
            public void Actor(string arg0, string arg1)
            {
                if (!DB.DBCheck()) return;
                var acts = DB.Store.Actors.Where(a => a.Name.Contains(arg0) && a.Name.Contains(arg1));
                if (acts.Count() == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No such Actor found.");
                    Console.ResetColor();
                }
                else
                {
                    foreach (var actr in acts)
                    {
                        Console.WriteLine();
                        Console.Write("Actor ");
                        Console.ForegroundColor = actr.Stared ? ConsoleColor.Green : ConsoleColor.DarkGreen;
                        Console.Write(actr.Name);
                        Console.ResetColor();
                        Console.Write(" Index ");
                        Console.ForegroundColor = actr.Stared ? ConsoleColor.Green : ConsoleColor.DarkGreen;
                        Console.Write(DBEngine.GetActorCode(actr.Index));
                        Console.ResetColor();
                        Console.Write(" with ");
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.Write(actr.Items.Count);
                        Console.ResetColor();
                        Console.Write(" Items:\n");
                        foreach (var item in actr.Items)
                        {
                            Console.ForegroundColor = item.Stared ? ConsoleColor.Yellow : ConsoleColor.DarkYellow;
                            Console.Write($"\t {item.Name}\t");
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write($"{DBEngine.GetItemCode(item.Index)}\t");
                            if (item.Labels.Count > 0)
                            {
                                Console.ForegroundColor = ConsoleColor.Blue;
                                foreach (var label in item.Labels)
                                {
                                    Console.Write($" {label}");
                                }
                            }
                            Console.Write("\n");
                            Console.ResetColor();

                        }
                    }
                }


            }
            public void L(string arg0) => Label(arg0);
            public void Label(string arg0)
            {
                if (!DB.DBCheck()) return;
                var lbs = DB.Store.Labels.Where(a => a.Name.Contains(arg0.ToUpper()));

                if (lbs.Count() == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No such Item(s) found.");
                    Console.ResetColor();
                }
                else
                {
                    foreach (var lb in lbs)
                    {
                        Console.WriteLine();
                        Console.Write("Label ");
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.Write(lb.Name);
                        Console.ResetColor();
                        Console.Write(" with ");
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.Write(lb.Items.Count);
                        Console.ResetColor();
                        Console.Write(" Items:\n");
                        foreach (var item in lb.Items)
                        {
                            Console.ForegroundColor = item.Stared ? ConsoleColor.Yellow : ConsoleColor.DarkYellow;
                            Console.Write($"\t {item.Name}\t");
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write($"{DBEngine.GetItemCode(item.Index)}\t");
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.Write($"{item.Actress.Name}\t");
                            if (item.Labels.Count > 0)
                            {
                                Console.ForegroundColor = ConsoleColor.Blue;
                                foreach (var label in item.Labels)
                                {
                                    Console.Write($" {label}");
                                }
                            }
                            Console.Write("\n");
                            Console.ResetColor();

                        }
                    }
                }


            }
            public void L(string arg0, string arg1) => Label(arg0, arg1);
            public void Label(string arg0, string arg1)
            {
                if (!DB.DBCheck()) return;
                var itms = DB.Store.Items.Where(a => a.Labels.Contains(arg0.ToUpper()) && a.Labels.Contains(arg1.ToUpper()));

                if (itms.Count() == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No such Item(s) found.");
                    Console.ResetColor();
                }
                else
                {
                    foreach (var item in itms)
                    {
                        Console.ForegroundColor = item.Stared ? ConsoleColor.Yellow : ConsoleColor.DarkYellow;
                        Console.Write($"\t {item.Name}\t");
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write($"{DBEngine.GetItemCode(item.Index)}\t");
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write($"{item.Actress.Name}\t");
                        if (item.Labels.Count > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            foreach (var label in item.Labels)
                            {
                                Console.Write($" {label}");
                            }
                        }
                        Console.Write("\n");
                        Console.ResetColor();
                    }
                }


            }
            public void L(string arg0, string arg1, string arg2) => Label(arg0, arg1, arg2);
            public void Label(string arg0, string arg1, string arg2)
            {
                if (!DB.DBCheck()) return;
                var itms = DB.Store.Items.Where(a => a.Labels.Contains(arg0.ToUpper()) 
                && a.Labels.Contains(arg1.ToUpper()) && a.Labels.Contains(arg2.ToUpper()));

                if (itms.Count() == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No such Item(s) found.");
                    Console.ResetColor();
                }
                else
                {
                    foreach (var item in itms)
                    {
                        Console.ForegroundColor = item.Stared ? ConsoleColor.Yellow : ConsoleColor.DarkYellow;
                        Console.Write($"\t {item.Name}\t");
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write($"{DBEngine.GetItemCode(item.Index)}\t");
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write($"{item.Actress.Name}\t");
                        if (item.Labels.Count > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            foreach (var label in item.Labels)
                            {
                                Console.Write($" {label}");
                            }
                        }
                        Console.Write("\n");
                        Console.ResetColor();
                    }
                }


            }
            public void I(string arg0) => Item(arg0);
            public void Item(string arg0)
            {
                if (!DB.DBCheck()) return;
                var itms = DB.Store.Items.Where(a => a.Name.Contains(arg0.ToUpper()));

                if (itms.Count() == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No such Item(s) found.");
                    Console.ResetColor();
                }
                else
                {
                    foreach (var item in itms)
                    {
                        Console.ForegroundColor = item.Stared ? ConsoleColor.Yellow : ConsoleColor.DarkYellow;
                        Console.Write($"\t {item.Name}\t ");
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write($"{DBEngine.GetItemCode(item.Index)}\t");
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write($"{item.Actress.Name}\t");
                        if (item.Labels.Count > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            foreach (var label in item.Labels)
                            {
                                Console.Write($" {label}");
                            }
                        }


                        Console.Write("\n");
                        Console.ResetColor();
                    }
                }


            }
            public void I(string arg0, string arg1) => Item(arg0, arg1);
            public void Item(string arg0, string arg1)
            {
                if (!DB.DBCheck()) return;
                var itms = DB.Store.Items.Where(a => a.Name.Contains(arg0.ToUpper()) && a.Name.Contains(arg1.ToUpper()));

                if (itms.Count() == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No such Item(s) found.");
                    Console.ResetColor();
                }
                else
                {
                    foreach (var item in itms)
                    {
                        Console.ForegroundColor = item.Stared ? ConsoleColor.Yellow : ConsoleColor.DarkYellow;
                        Console.Write($"\t {item.Name}\t");
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write($"{DBEngine.GetItemCode(item.Index)}\t");
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write($"{item.Actress.Name}\t");
                        if (item.Labels.Count > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            foreach (var label in item.Labels)
                            {
                                Console.Write($" {label}");
                            }
                        }
                        Console.Write("\n");
                        Console.ResetColor();
                    }
                }

            }
        }
    }
}
