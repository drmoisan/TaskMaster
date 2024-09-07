using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TaskMaster;
using UtilitiesCS;
using Moq;

namespace TaskMaster.Test
{
    [TestClass]
    public class UnitTest1
    {
        private MockRepository mockRepository;
        private Mock<IApplicationGlobals> mockGlobals;
        private Mock<Microsoft.Office.Interop.Outlook.Application> mockApplication;

        [TestInitialize]
        public void TestInitialize()
        {
            Console.SetOut(new DebugTextWriter());
            mockRepository = new MockRepository(MockBehavior.Strict);
            mockGlobals = mockRepository.Create<IApplicationGlobals>();
            mockApplication = mockRepository.Create<Microsoft.Office.Interop.Outlook.Application>();
        }

        [TestMethod]
        public void TestMethod1()
        {
            var appGlobals = new ApplicationGlobals(mockApplication.Object);
            //var af = new AppAutoFileObjects(mockGlobals.Object);
            var af = new AppAutoFileObjects(appGlobals);
            //af.LoadManagerConfig();
            var names = af.GetManifestResourceNames();
            foreach (var name in names)
            {
                Console.WriteLine(name);
            }

        }
    }
}
