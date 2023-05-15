using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using UtilitiesCS.Dialogs;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class InputBox_Test
    {
        [TestMethod]
        public void ShowDialog_Test()
        {
            string result = InputBox.ShowDialog("Test to see if this works", "Title", "Random text");
            Assert.AreEqual("Random text47", result);
        }
    }
}
