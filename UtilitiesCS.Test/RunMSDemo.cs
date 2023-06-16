using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using UtilitiesCS.Examples;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class RunMSDemo
    {
        [TestMethod]
        public void TestMethod1()
        {
            var OlApp = new Outlook.Application();
            Outlook.NameSpace session = OlApp.Session;
            Outlook.MailItem Item = (Outlook.MailItem)session.GetItemFromID("00000000DBBA8359AE186B459B8593980990086E07002ABB4C43D6CADA4EAF5B95F147DA9D9D00000000010C00003E29CCD1546FF744A2936EDCC51E7D580001DDA889FA0000");
            MSDemoConv.DemoConversation(Item);
        }
    }
}
