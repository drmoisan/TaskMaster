using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class YesNoToAll_Test
    {
        [TestMethod]
        [Ignore("Interactive popup dialog test; excluded from unattended test runs.")]
        public void TestMethod1()
        {
            YesNoToAllResponse response = YesNoToAll.ShowDialog("Test Message");
            Assert.AreEqual(YesNoToAllResponse.NoToAll, response);
        }
    }
}
