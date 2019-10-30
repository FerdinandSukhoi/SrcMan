using System;
using System.Collections.Generic;
using System.Text;

namespace SrcMan
{
    [AttributeUsage(AttributeTargets.Class |
                       AttributeTargets.Struct,
                       AllowMultiple = true)]
    public class MobileSuitInfo: Attribute, IMobileSuitInfo
    {
        public string Prompt { get; set; }
        public MobileSuitInfo(string prompt)
        {
            Prompt = prompt;
        }
    }
}
