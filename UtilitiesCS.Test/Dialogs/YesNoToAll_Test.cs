using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class YesNoToAll_Test
    {
        [TestMethod]
        public void TestMethod1()
        {
            YesNoToAllResponse response = YesNoToAll.ShowDialog("Test Message");
            Assert.AreEqual(YesNoToAllResponse.NoToAll, response);
        }
    }
}
