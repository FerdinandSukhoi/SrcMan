#nullable enable
#pragma warning disable CS8600 // 将 null 文本或可能的 null 值转换为非 null 类型。
#pragma warning disable CS8602 // 可能的 null 引用参数。
#pragma warning disable CS8604 // 可能的 null 引用参数。
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Linq;

namespace SrcMan
{
    public class MobileSuit
    {
        public Stack<object> InstanceRef { get; set; } = new Stack<object>();
        public List<string> InstanceNameStk { get; set; } = new List<string>();
        public bool ShowRef { get; set; } = true;
        public const BindingFlags Flags = BindingFlags.IgnoreCase |BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;

        public enum TraceBack
        {

            OnExit=1,
            AllOk=0,
            InvalidCommand=-1,
            ObjectNotFound=-2,
            MemberNotFound=-3

        }
        public Assembly Assembly { get; set; }
        public string? Prompt { get; set; }
        public object? WorkInstance { get; set; }
        public Type? WorkType { get; set; }
        public MobileSuit()
        {
            Assembly = Assembly.GetCallingAssembly();
        }
        public MobileSuit(Type type):this()
        {
            
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
                Console.ResetColor();
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
                Console.WriteLine("Error! Object Not Found!");
                Console.Beep();
                Console.ResetColor();
            }
            else
            {
                throw new Exception("Object Not Found!");
            }
        }
        private void ErrMemberNotFound()
        {
            if (UseTraceBack)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error! Member Not Found!");
                Console.Beep();
                Console.ResetColor();
            }
            else
            {
                throw new Exception("Member Not Found!");
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
            if (ShowRef && InstanceNameStk.Count>0)
            {

                StringBuilder SB = new StringBuilder();
                SB.Append(Prompt);
                SB.Append('[');
                SB.Append(InstanceNameStk[0]);
                if (InstanceNameStk.Count>1)
                {
                    for (int i = 1; i < InstanceNameStk.Count; i++)
                    {
                        SB.Append($".{InstanceNameStk[i]}");
                    }
                }
                SB.Append(']');
                Prompt = SB.ToString();
            }
        }
        private TraceBack ListMembers()
        {
            var fi = from i in (from f in WorkType.GetFields(Flags)
                                select (MemberInfo)f).Union
                               (from p in WorkType.GetProperties(Flags)
                                select (MemberInfo)p)
                     orderby i.Name
                     select i;


            if (fi?.Count() > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Members:");
                Console.ResetColor();
                foreach (var item in fi)
                {
                    var info = (MobileSuitInfo)item.GetCustomAttribute(typeof(MobileSuitInfo));
                    var exInfo = info == null
                                ? ""
                                : $"{info.Prompt}]";
                    Console.Write($"\t{item.Name}");
                    Console.ForegroundColor = info == null
                                              ? ConsoleColor.DarkBlue
                                              : ConsoleColor.DarkCyan;
                    Console.WriteLine(exInfo);
                    Console.ResetColor();
                }
            }
            var mi = from m in WorkType.GetMethods(Flags)
                      where
                            !(from p in WorkType.GetProperties(Flags)
                              select $"get_{p.Name}").Contains(m.Name)
                         && !(from p in WorkType.GetProperties(Flags)
                              select $"set_{p.Name}").Contains(m.Name)
                      select m;
                        

            if (mi?.Count() > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Methods:");
                Console.ResetColor();
                foreach (var item in mi)
                {
                    var info = (MobileSuitInfo)item.GetCustomAttribute(typeof(MobileSuitInfo));
                    var exInfo = info == null
                                ? $"({ item.GetParameters().Length} Parameters)"
                                : $"[{info.Prompt}]";
                    Console.Write($"\t{item.Name}");
                    Console.ForegroundColor = info == null
                                              ? ConsoleColor.DarkBlue
                                              : ConsoleColor.DarkCyan;
                    Console.WriteLine(exInfo);
                    Console.ResetColor();
                }
            }
            return TraceBack.AllOk;
        }
        private delegate void set_prop(object? obj, object? arg);
        private TraceBack SwitchOption(string optionName)
        {
            switch (optionName)
            {
                case "sr":
                case "ShowRef":
                    ShowRef = !ShowRef;
                    return TraceBack.AllOk;
                case "sd":
                case "ShowDone":
                    ShowDone = !ShowDone;
                    return TraceBack.AllOk;

                case "utb":
                case "UseTraceBack":
                    UseTraceBack = !UseTraceBack;
                    return TraceBack.AllOk;
                default:
                    return TraceBack.InvalidCommand;
            }
        }
        private TraceBack ModifyMember(string[] args)
        {
            if (WorkType == null) return TraceBack.ObjectNotFound;
            var obj = (MemberInfo)WorkType.GetProperty(args[0], Flags) ?? WorkType.GetField(args[0], Flags);
            if (obj == null) return TraceBack.MemberNotFound;
            var obj_set = (set_prop)WorkType.GetProperty(args[0], Flags).SetValue ?? WorkType.GetField(args[0], Flags).SetValue;
            var cvt = ((MobileSuitDataConverter)obj.GetCustomAttribute(typeof(MobileSuitDataConverter)))?.Converter;
            try
            {
                obj_set(WorkInstance, cvt != null ? cvt(args[1]) : args[1]);
                return TraceBack.AllOk;
            }
            catch
            {
                return TraceBack.InvalidCommand;
            }
        }
        private TraceBack RunLocal(string cmd)
        {

            var cmdlist = cmd.ToLower().Split(' ');
            switch (cmdlist[0])
            {
                case "vw":
                case "view":
                    if (cmdlist.Length == 1) return TraceBack.InvalidCommand;
                    var obj = WorkType.GetProperty(cmdlist[1], Flags)?.GetValue(WorkInstance) ??
                                  WorkType.GetField(cmdlist[1], Flags)?.GetValue(WorkInstance);
                    if (obj==null)
                    {
                        return TraceBack.ObjectNotFound;
                    }
                    Console.WriteLine(obj.ToString());
                    return TraceBack.AllOk;
                case "nw":
                case "new":
                    if (cmdlist.Length == 1) return TraceBack.InvalidCommand;
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
                    InstanceRef.Clear();
                    InstanceNameStk.Clear();
                    InstanceNameStk.Add($"(new {WorkType.Name})");
                    return TraceBack.AllOk;
                case "md":
                case "modify":
                    if (cmdlist.Length == 1) return TraceBack.InvalidCommand;
                    else return ModifyMember(cmdlist[1..]);
                case "lv":
                case "leave":
                    if (InstanceRef.Count == 0)
                        return TraceBack.InvalidCommand;
                    WorkInstance = InstanceRef.Pop();
                    InstanceNameStk.RemoveAt(InstanceNameStk.Count - 1);//PopBack
                    WorkType = WorkInstance.GetType();
                    return TraceBack.AllOk;
                case "et":
                case "enter":
                    if (cmdlist.Length == 1) return TraceBack.InvalidCommand;
                    var nextobj = WorkType.GetProperty(cmdlist[1], Flags)?.GetValue(WorkInstance) ??
                        WorkType.GetField(cmdlist[1], Flags)?.GetValue(WorkInstance);
                    InstanceRef.Push(WorkInstance);
                    InstanceNameStk.Add(WorkType.GetProperty(cmdlist[1], Flags)?.Name ??
                        WorkType.GetField(cmdlist[1], Flags)?.Name);
                    WorkInstance = nextobj;
                    WorkType = nextobj.GetType();
                    return TraceBack.AllOk;
                    

                case "exit":
                    return TraceBack.OnExit;
                case "fr":
                case "free":
                    WorkType = null;
                    WorkInstance = null;
                    Prompt = "";
                    InstanceRef.Clear();
                    InstanceNameStk.Clear();
                    return TraceBack.AllOk;
                case "ls":
                case "list":
                    return ListMembers();
                case "me":
                case "this":
                    Console.WriteLine("Work Instance:{0}", WorkType.FullName);
                    return TraceBack.AllOk;
                //case "modify":
                case "sw":
                case "switch":
                    return SwitchOption(cmdlist[1]);
                default:
                    return TraceBack.InvalidCommand;
            }

        }
        //private TraceBack ModifyValue(ref string[] cmdlist, int readindex, object? instance) { }
        private TraceBack RunObject(string[] cmdlist, object? instance)
        {
            if (0 == cmdlist.Length)
            {
                return TraceBack.ObjectNotFound;
            }
            if(instance != null)
            {
                var type = instance.GetType();
                MethodInfo mi;
                try
                {
                    mi = type.GetMethods(Flags)
                        .Where(m => m.Name.ToLower() == cmdlist[0].ToLower())
                        .Where(m => m.GetParameters().Length == cmdlist.Length - 1).FirstOrDefault();
                }
                catch (AmbiguousMatchException)
                {

                    return TraceBack.InvalidCommand;
                }

                //if there's no such method
                if (mi == null)
                {
                    var nextobj = 
                        type.GetProperty(cmdlist[0], Flags)?.GetValue(instance)??
                        type.GetField(cmdlist[0], Flags)?.GetValue(instance)??
                        type.Assembly.CreateInstance(type.FullName + "." + cmdlist[0]);
                    return RunObject(cmdlist[1..], nextobj);
                }
                else
                {
                    mi.Invoke(instance, cmdlist[1..]);
                    return TraceBack.AllOk;
                }
            }
            else if (WorkInstance==null)
            {
                var nextobj = Assembly.GetType(cmdlist[0], false, true) ?? 
                    Assembly.GetType(Assembly.GetName().Name + '.' + cmdlist[0], false, true);
                if (nextobj == null)
                {
                    return TraceBack.ObjectNotFound;
                }
                else
                {
                    return RunObject(cmdlist[1..],
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
                Console.ResetColor();
                string cmd = Console.ReadLine();
                TraceBack tb;
                if (cmd == "")
                {
                    continue;
                }
                else if (cmd[0] == '@')
                {
                    tb = RunLocal(cmd.Remove(0, 1));
                }
                else
                {
                    string[] cmdlist = cmd.Split(' ');
                    tb= RunObject(cmdlist, WorkInstance);
                    
                }
                switch (tb)
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
                    case TraceBack.MemberNotFound:
                        ErrMemberNotFound();
                        break;
                    default:
                        break;
                }
                UpdatePrompt(prompt);
            }
        }

        public int Run()
            => Run("");
    }
}
