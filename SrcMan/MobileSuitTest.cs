﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SrcMan
{
    [MobileSuitInfo("Test")]
    public class MobileSuitTest : IMobileSuitInfo
    {
        public string Prompt { get; set; } = "Test";
        public TestC TestCC { get; set; } = new TestC();
        public TestC tc = new TestC();
        public void Test()
        {
            Console.WriteLine("Test!!!!");
        }
        [MobileSuitInfo("TestC")]
        public class TestC
        {
            public void T()
            {
                Console.WriteLine("t");
            }
        }
    }
}
