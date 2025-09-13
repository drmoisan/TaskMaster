using BrightIdeasSoftware;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.ClassifierGroups;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class Triage_OlLogicTests
    {
        private Mock<Triage> _mockTriage;
        private Triage_OlLogic _triageOlLogic;

        [TestInitialize]
        public void Setup()
        {
            Console.SetOut(new DebugTextWriter());
            _mockTriage = new Mock<Triage>(MockBehavior.Strict, null, CancellationToken.None);
            _triageOlLogic = new Triage_OlLogic(_mockTriage.Object);
        }

        [TestMethod]
        public void Constructor_ShouldInitializeParent()
        {
            Assert.AreEqual(_mockTriage.Object, _triageOlLogic.Parent);
        }

        [TestMethod]
        public async Task FilterViewAsync_ShouldCallFilterView()
        {
            var filterViewCalled = false;
            _triageOlLogic = new Triage_OlLogic(_mockTriage.Object);
            
            await _triageOlLogic.FilterViewAsync();

            Assert.IsTrue(filterViewCalled);
        }

        [TestMethod]
        public void FilterView_ShouldCallFilterViewWithTriageValues()
        {
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockToDoObjects = new Mock<IToDoObjects>();
            mockGlobals.Setup(g => g.TD).Returns(mockToDoObjects.Object);
            _mockTriage.Setup(t => t.Globals).Returns(mockGlobals.Object);

            var choices = new List<string> { "A", "B", "C" };
            mockToDoObjects.Setup(td => td.SelectFromList(choices)).Returns(new List<string> { "A", "B" });

            _triageOlLogic.FilterView();

            // Verify that FilterView(char[] triageValues) was called with the correct values
            // This can be done by setting up a mock or spy on the method
        }

        [TestMethod]
        public void FilterView_WithTriageValues_ShouldApplyFilter()
        {
            var mockExplorer = new Mock<Explorer>();
            var mockView = new Mock<View>();
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockOlObjects = new Mock<IOlObjects>();
            mockGlobals.Setup(g => g.Ol).Returns(mockOlObjects.Object);
            mockOlObjects.Setup(o => o.App.ActiveExplorer()).Returns(mockExplorer.Object);
            mockExplorer.Setup(e => e.CurrentView).Returns(mockView.Object);
            _mockTriage.Setup(t => t.Globals).Returns(mockGlobals.Object);

            mockView.Setup(v => v.Filter).Returns("[Triage] = 'A'");
            mockView.Setup(v => v.Apply());

            _triageOlLogic.FilterView(new char[] { 'B', 'C' });

            mockView.VerifySet(v => v.Filter = It.IsAny<string>(), Times.Once);
            mockView.Verify(v => v.Apply(), Times.Once);
        }

        [TestMethod]
        public void ParseAndStripFilter_ShouldReturnStrippedFilter()
        {
            string filter = "\"http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Triage\" LIKE '%A%' OR \"http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Triage\" LIKE '%B%'";

            string result = _triageOlLogic.ParseAndStripFilter(filter);

            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void ParseAndStripFilter_ShouldReturnStrippedFilter2()
        {
            //string filter = @"(""http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Actionable"" = 'Task' AND (""http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Triage"" = 'A' OR ""http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Triage"" = 'B'))";
            var filter = "(\"http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Actionable\" LIKE '%Task%' AND (\"http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Triage\" LIKE '%A%' OR \"http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Triage\" = 'B'))";
            string expected = @"""http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Actionable"" = 'Task'";

            Console.WriteLine(filter);
            var parser = new DASLFilterParser();
            var logicTree = parser.Parse(filter);
            parser.PrintTree(logicTree, 0);
            var recombined = parser.CombineTree(logicTree);
            Console.WriteLine(recombined);

            string actual = _triageOlLogic.ParseAndStripFilter(filter);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public async Task TrainSelectionAsync_ShouldTrainSelection()
        {
            var mockExplorer = new Mock<Explorer>();
            var mockSelection = new Mock<Selection>();
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockOlObjects = new Mock<IOlObjects>();
            mockGlobals.Setup(g => g.Ol).Returns(mockOlObjects.Object);
            mockOlObjects.Setup(o => o.App.ActiveExplorer()).Returns(mockExplorer.Object);
            mockExplorer.Setup(e => e.Selection).Returns(mockSelection.Object);
            _mockTriage.Setup(t => t.Globals).Returns(mockGlobals.Object);

            mockSelection.Setup(s => s.Cast<object>()).Returns(new List<object> { new Mock<MailItem>().Object }.Cast<object>());

            await _triageOlLogic.TrainSelectionAsync("TriageId");

            // Verify that TrainAsync was called with the correct values
            // This can be done by setting up a mock or spy on the method
        }
    }
}
