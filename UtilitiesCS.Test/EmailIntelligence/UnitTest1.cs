using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using UtilitiesCS;

namespace UtilitiesCS
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var viewer = new FilterOlFoldersViewer();
            viewer.ShowDialog();
        }
    }
}
