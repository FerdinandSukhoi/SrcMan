using System;
using System.Collections.Generic;
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
            public void Actor(string[] args)
            {
                var stk = new Stack<SrcActor>();
                foreach (var actor in DB.Store.Actors)
                {
                    bool flag = true;
                    foreach (var item in args)
                    {
                        if (!actor.Name.Contains(item))
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag) { 
                        stk.Push(actor); 
                    }
                }
            }
        }
    }
}
