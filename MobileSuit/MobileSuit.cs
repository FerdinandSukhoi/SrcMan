#nullable enable
#pragma warning disable CS8600 // 将 null 文本或可能的 null 值转换为非 null 类型。
#pragma warning disable CS8602 // 可能的 null 引用参数。
#pragma warning disable CS8604 // 可能的 null 引用参数。
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

namespace SrcMan
{
    public class MobileSuit
    {
        public ConsoleColor DefaultForeColor { get; set; }
        public enum TraceBack
        {

            OnExit=1,
            AllOk=0,
            InvalidCommand=-1,
            ObjectNotFound=-2

        }
        public Assembly Assembly { get; set; }
        public string? Prompt { get; set; }
        public object? WorkInstance { get; set; }
        public Type? WorkType { get; set; }
        public MobileSuit()
        {
            Assembly = Assembly.GetExecutingAssembly();
            DefaultForeColor = Console.ForegroundColor == ConsoleColor.Black ? ConsoleColor.White : Console.ForegroundColor;
        }
        public MobileSuit(Type type)
        {
            Assembly = Assembly.GetExecutingAssembly();
            DefaultForeColor = Console.ForegroundColor==ConsoleColor.Black? ConsoleColor.White:Console.ForegroundColor;
            WorkType = type;
            WorkInstance = Assembly.CreateInstance(type.FullName);
            
        }
        public bool UseTraceBack { get; set; } = true;
        public bool ShowDone { get; set; } = false;
        private void TBAllOk()
        {
            if (UseTraceBack && ShowDone) Console.WriteLine("Done.");
        }  
        private void ErrInvalidCommand()
        {
            if (UseTraceBack)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error! Invalid Command!");
                Console.Beep();
                Console.ForegroundColor = DefaultForeColor;
            }
            else
            {
                throw new Exception("Invalid Command!");
            }
        }
        private void ErrObjectNotFound()
        {
            if (UseTraceBack)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error! Object Noy Found!");
                Console.Beep();
                Console.ForegroundColor = DefaultForeColor;
            }
            else
            {
                throw new Exception("Object Not Found!");
            }
        }
        private void UpdatePrompt(string prompt)
        {
            if (prompt == "" && WorkInstance != null)
            {
                if (WorkInstance is IMobileSuitInfo)
                {
                    Prompt = ((IMobileSuitInfo)WorkInstance).Prompt;
                }
                else
                {
                    Prompt = ((MobileSuitInfo)WorkType.GetCustomAttribute(typeof(MobileSuitInfo))
                        ?? new MobileSuitInfo(WorkInstance.GetType().Name)).Prompt;
                }
            }
            else
            {
                Prompt = prompt;
            }
        }
        private TraceBack RunLocal(string cmd)
        {
            var cmdlist = cmd.ToLower().Split(' ');
            switch (cmdlist[0])
            {
                case "use":
                    WorkType = 
                        Assembly.GetType(cmdlist[1], false, true)??
                        Assembly.GetType(WorkType?.FullName + '.' + cmdlist[1], false, true)??
                        Assembly.GetType(Assembly.GetName().Name + '.' + cmdlist[1], false, true);
                    if (WorkType == null)
                    {
                        return TraceBack.ObjectNotFound;
                    }

                    WorkInstance = Assembly.CreateInstance(WorkType.FullName);
                    Prompt = ((MobileSuitInfo)WorkType.GetCustomAttribute(typeof(MobileSuitInfo))
                        ?? new MobileSuitInfo(cmdlist[1])).Prompt;
                    return TraceBack.AllOk;
                case "exit":
                    return TraceBack.OnExit;
                case "free":
                    WorkType = null;
                    WorkInstance = null;
                    Prompt = "";
                    return TraceBack.AllOk;
                case "this":
                    Console.WriteLine("Work Instance:{0}", WorkType.FullName);
                    return TraceBack.AllOk;
                //case "modify":

                default:
                    return TraceBack.InvalidCommand;
            }

        }
        //private TraceBack ModifyValue(ref string[] cmdlist, int readindex, object? instance) { }
        private TraceBack RunObject(ref string[] cmdlist, int readindex, object? instance)
        {
            if (readindex >= cmdlist.Length)
            {
                return TraceBack.ObjectNotFound;
            }
            if(instance != null)
            {
                var type = instance.GetType();
                MethodInfo mi;
                try
                {
                    mi = type.GetMethod(cmdlist[readindex], BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                }
                catch (AmbiguousMatchException)
                {

                    return TraceBack.InvalidCommand;
                }

                //if there's no such method
                if (mi == null)
                {

                    var nextobj = Assembly.GetType(type.FullName + "." + cmdlist[readindex]);
                    if (nextobj == null)
                    {
                        return TraceBack.ObjectNotFound;
                    }
                    else
                    {
                        return RunObject(ref cmdlist, readindex + 1,
                            Assembly.CreateInstance(nextobj.FullName));
                    }
                }
                else
                {
                    if (readindex + 1 == cmdlist.Length)
                    {
                        mi.Invoke(instance, null);
                    }
                    else
                    {
                        string[] args = new string[cmdlist.Length - readindex - 1];
                        cmdlist.CopyTo(args, 1);
                        mi.Invoke(instance, args);
                    }
                    return TraceBack.AllOk;
                }
            }
            else if (WorkInstance==null)
            {
                var nextobj = Assembly.GetType(cmdlist[readindex], false, true) ?? Assembly.GetType(Assembly.GetName().Name + '.' + cmdlist[readindex], false, true);
                if (nextobj == null)
                {
                    return TraceBack.ObjectNotFound;
                }
                else
                {
                    return RunObject(ref cmdlist, readindex + 1,
                        Assembly.CreateInstance(nextobj.FullName));
                }
            }
            //If Null Instance
            else
            {
                return TraceBack.ObjectNotFound;

            }
            
        }
        public int Run(string prompt)
        {
            UpdatePrompt(prompt);
            for (; ; )
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write(Prompt+'>');
                Console.ForegroundColor = DefaultForeColor;
                string cmd = Console.ReadLine();
                if (cmd == "")
                {
                    continue;
                }
                else if (cmd[0] == '@')
                {
                    switch (RunLocal(cmd.Remove(0, 1)))
                    {
                        case TraceBack.OnExit:
                            return 0;
                        case TraceBack.AllOk:
                            TBAllOk();
                            break;
                        case TraceBack.InvalidCommand:
                            ErrInvalidCommand();
                            break;
                        case TraceBack.ObjectNotFound:
                            ErrObjectNotFound();
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    string[] cmdlist = cmd.Split(' ');
                    switch (RunObject(ref cmdlist, 0, WorkInstance))
                    {
                        case TraceBack.OnExit:
                            return 0;
                        case TraceBack.AllOk:
                            TBAllOk();
                            break;
                        case TraceBack.InvalidCommand:
                            ErrInvalidCommand();
                            break;
                        case TraceBack.ObjectNotFound:
                            ErrObjectNotFound();
                            break;
                        default:
                            break;
                    }
                }
                UpdatePrompt(prompt);
            }
        }

        public int Run()
            => Run("");
    }
}
