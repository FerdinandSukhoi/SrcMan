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
        public Stack<object> InstanceRef { get; set; } = new Stack<object>();
        public List<string> InstanceNameStk { get; set; } = new List<string>();
        public bool ShowRef { get; set; } = true;
        public const BindingFlags Flags = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;
        public ConsoleColor DefaultForeColor { get; set; }
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
            DefaultForeColor = Console.ForegroundColor == ConsoleColor.Black ? ConsoleColor.White : Console.ForegroundColor;
        }
        public MobileSuit(Type type):this()
        {
            //Assembly = Assembly.GetExecutingAssembly();
            //DefaultForeColor = Console.ForegroundColor==ConsoleColor.Black? ConsoleColor.White:Console.ForegroundColor;
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
                Console.WriteLine("Error! Object Not Found!");
                Console.Beep();
                Console.ForegroundColor = DefaultForeColor;
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
                Console.ForegroundColor = DefaultForeColor;
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
            var fi = WorkType.GetFields(Flags);
            var pi = WorkType.GetProperties(Flags);
            List<string> members = new List<string>(fi.Length + pi.Length);
            if (!(fi?.Length == 0))
            {
                foreach (var item in fi)
                {
                    members.Add(item.Name);
                }
            }
            if (!(pi?.Length == 0))
            {
                foreach (var item in pi)
                {
                    if (item.Name != "Prompt")
                    {
                        members.Add(item.Name);
                    }

                }
            }
            members.Sort();
            if (members.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Members:");
                Console.ForegroundColor = DefaultForeColor;
                foreach (var item in members)
                {
                    Console.WriteLine($"\t{item}");
                }
            }
            members.Clear();
            var mi = WorkType.GetMethods(Flags);
            if (!(mi?.Length == 0))
            {

                foreach (var item in mi)
                {
                    var iname = item.Name;
                    if (iname != "ToString"
                        && iname != "GetHashCode"
                        && iname != "GetType"
                        && iname != "Equals"
                        && (iname.Length <= 4 || iname.Substring(1, 3) != "et_"))
                    {
                        members.Add(iname);
                    }
                }
            }
            members.Sort();
            if (members.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Methods:");
                Console.ForegroundColor = DefaultForeColor;
                foreach (var item in members)
                {
                    Console.WriteLine($"\t{item}");
                }
            }
            return TraceBack.AllOk;
        }
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
        private TraceBack RunLocal(string cmd)
        {

            var cmdlist = cmd.ToLower().Split(' ');
            switch (cmdlist[0])
            {
                case "nw":
                case "new":
                    WorkType = 
                        Assembly.GetType(cmdlist[1], false, true)??
                        Assembly.GetType(WorkType?.FullName + '.' + cmdlist[1], false, true)??
                        Assembly.GetType(Assembly.GetName().Name + '.' + cmdlist[1], false, true);
                    if (WorkType == null|| WorkType.GetCustomAttribute(typeof(MobileSuitItem)) == null)
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
                    var nextobj = WorkType.GetProperty(cmdlist[1], Flags)?.GetValue(WorkInstance) ??
                        WorkType.GetField(cmdlist[1], Flags)?.GetValue(WorkInstance);
                    if (nextobj?.GetType().GetCustomAttribute(typeof(MobileSuitItem)) == null)
                    {
                        return TraceBack.MemberNotFound;
                    }
                    else
                    {
                        InstanceRef.Push(WorkInstance);
                        InstanceNameStk.Add(WorkType.GetProperty(cmdlist[1], Flags)?.Name ??
                            WorkType.GetField(cmdlist[1], Flags)?.Name);
                        WorkInstance = nextobj;
                        WorkType = nextobj.GetType();
                        return TraceBack.AllOk;
                    }

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
                    mi = type.GetMethod(cmdlist[readindex], Flags);
                }
                catch (AmbiguousMatchException)
                {

                    return TraceBack.InvalidCommand;
                }

                //if there's no such method
                if (mi == null)
                {
                    var nextobj = type.GetProperty(cmdlist[readindex], Flags)?.GetValue(instance)??
                        type.GetField(cmdlist[readindex], Flags)?.GetValue(instance)??
                        type.Assembly.CreateInstance(type.FullName + "." + cmdlist[readindex]);
                    if (nextobj?.GetType().GetCustomAttribute(typeof(MobileSuitItem)) == null)
                    {
                        return TraceBack.ObjectNotFound;
                    }
                    return RunObject(ref cmdlist, readindex + 1, nextobj);
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
                if (nextobj == null|| nextobj.GetCustomAttribute(typeof(MobileSuitItem)) == null)
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
                    tb= RunObject(ref cmdlist, 0, WorkInstance);
                    
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
