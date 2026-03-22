using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class InputBox_Test
    {
        [TestMethod]
        [Ignore("Interactive popup dialog test; excluded from unattended test runs.")]
        public void ShowDialog_Test()
        {
            string result = InputBox.ShowDialog(
                "Test to see if this works",
                "Title",
                "Random text"
            );
            Assert.AreEqual("Random text47", result);
        }
    }
}
