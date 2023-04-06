using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class UnitTest2
    {
        [TestMethod]
        public void TestMethod1()
        {
            Form1 frm = new Form1();
            frm.ShowDialog();
        }
        [TestMethod]
        public void TestMethod2()
        {
            Form2 frm = new Form2();
            frm.ShowDialog();
        }
    }
}
