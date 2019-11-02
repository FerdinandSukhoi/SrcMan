using System;
using System.Collections.Generic;
using System.Text;

namespace SrcMan
{
    [AttributeUsage(AttributeTargets.All,AllowMultiple = false)]
    public sealed class MobileSuitInfo: Attribute, IMobileSuitInfo
    {
        public string Prompt { get; private set; }
        public MobileSuitInfo(string prompt)
        {
            Prompt = prompt;
        }
    }
}
