using System;
using System.Collections.Generic;
using System.Text;

namespace SrcMan
{
    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public sealed class MobileSuitDataConverter: Attribute
    {
        public Converter<string,object> Converter { get; private set; }
        public MobileSuitDataConverter(Converter<string, object> converter)
        {
            Converter = converter;
        }


    }
}
