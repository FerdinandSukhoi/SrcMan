using System;
using System.Collections.Generic;
using System.Text;

namespace SrcMan
{
    [AttributeUsage(AttributeTargets.Class |
                       AttributeTargets.Struct,
                       AllowMultiple = true)]
    public class MobileSuitItem: Attribute
    {

    }
}
