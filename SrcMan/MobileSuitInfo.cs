using System;
using PlasticMetal.MobileSuit.ObjectModel.Interfaces;
using System.Collections.Generic;
using System.Text;

namespace SrcMan
{
    [AttributeUsage(AttributeTargets.All,AllowMultiple = false)]
    public sealed class MobileSuitInfo: Attribute, IInfoProvider
    {
        public MobileSuitInfo(string prompt)
        {
            Text = prompt;
        }

        public string Text { get; }
    }
}
