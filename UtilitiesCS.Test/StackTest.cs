using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class StackTest
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
