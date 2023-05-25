using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class UnitTest3
    {
        [TestMethod]
        public void TestMethod1()
        {
            Stack<string> stack = new Stack<string>();
            stack.Push("Bottom");
            stack.Push("Middle");
            stack.Push("Top");
            foreach (string item in stack) 
            { 
                Debug.WriteLine(item);
            }
        }
    }
}
