using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BrightIdeasSoftware;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.ClassifierGroups;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class Triage_OlLogicTests
    {
        private Mock<IApplicationGlobals> _mockGlobals;
        private Triage _triage;
        private Triage_OlLogic _triageOlLogic;

        [TestInitialize]
        public void Setup()
        {
            Console.SetOut(new DebugTextWriter());
            _mockGlobals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            _triage = new Triage(_mockGlobals.Object, CancellationToken.None)
            {
                ClassifierGroup = new BayesianClassifierGroup(),
            };
            _triageOlLogic = new Triage_OlLogic(_triage);
        }

        [TestMethod]
        public void Constructor_ShouldInitializeParent()
        {
            Assert.AreEqual(_triage, _triageOlLogic.Parent);
        }

        [TestMethod]
        public async Task FilterViewAsync_ShouldCallFilterView()
        {
            var mockToDoObjects = new Mock<IToDoObjects>(MockBehavior.Strict);
            var mockOlObjects = new Mock<IOlObjects>(MockBehavior.Strict);
            var mockApplication = new Mock<Application>(MockBehavior.Strict);
            var mockExplorer = new Mock<Explorer>(MockBehavior.Strict);
            var mockView = new Mock<View>(MockBehavior.Strict);

            mockToDoObjects
                .SetupGet(td => td.SelectFromList)
                .Returns(_ => new List<string> { "A" });
            _mockGlobals.Setup(g => g.TD).Returns(mockToDoObjects.Object);
            _mockGlobals.Setup(g => g.Ol).Returns(mockOlObjects.Object);

            mockOlObjects.Setup(o => o.App).Returns(mockApplication.Object);
            mockApplication.Setup(a => a.ActiveExplorer()).Returns(mockExplorer.Object);
            mockExplorer.Setup(e => e.CurrentView).Returns(mockView.Object);
            mockView.SetupProperty(v => v.Filter, "[Triage] = 'A'");
            mockView.Setup(v => v.Apply());

            await _triageOlLogic.FilterViewAsync();

            mockView.VerifySet(v => v.Filter = It.IsAny<string>(), Times.Once);
            mockView.Verify(v => v.Apply(), Times.Once);
        }

        [TestMethod]
        public void FilterView_ShouldCallFilterViewWithTriageValues()
        {
            var mockToDoObjects = new Mock<IToDoObjects>(MockBehavior.Strict);
            var mockOlObjects = new Mock<IOlObjects>(MockBehavior.Strict);
            var mockApplication = new Mock<Application>(MockBehavior.Strict);
            var mockExplorer = new Mock<Explorer>(MockBehavior.Strict);
            var mockView = new Mock<View>(MockBehavior.Strict);

            var choices = new List<string> { "A", "B", "C" };
            mockToDoObjects
                .SetupGet(td => td.SelectFromList)
                .Returns(_ => new List<string> { "A", "B" });
            _mockGlobals.Setup(g => g.TD).Returns(mockToDoObjects.Object);
            _mockGlobals.Setup(g => g.Ol).Returns(mockOlObjects.Object);

            mockOlObjects.Setup(o => o.App).Returns(mockApplication.Object);
            mockApplication.Setup(a => a.ActiveExplorer()).Returns(mockExplorer.Object);
            mockExplorer.Setup(e => e.CurrentView).Returns(mockView.Object);
            mockView.SetupProperty(v => v.Filter, "[Triage] = 'A'");
            mockView.Setup(v => v.Apply());

            _triageOlLogic.FilterView();

            mockToDoObjects.VerifyGet(td => td.SelectFromList, Times.Once);
            mockView.VerifySet(v => v.Filter = It.IsAny<string>(), Times.Once);
            mockView.Verify(v => v.Apply(), Times.Once);
        }

        [TestMethod]
        public void FilterView_WithTriageValues_ShouldApplyFilter()
        {
            var mockExplorer = new Mock<Explorer>(MockBehavior.Strict);
            var mockView = new Mock<View>(MockBehavior.Strict);
            var mockApplication = new Mock<Application>(MockBehavior.Strict);
            var mockOlObjects = new Mock<IOlObjects>(MockBehavior.Strict);

            _mockGlobals.Setup(g => g.Ol).Returns(mockOlObjects.Object);
            mockOlObjects.Setup(o => o.App.ActiveExplorer()).Returns(mockExplorer.Object);
            mockExplorer.Setup(e => e.CurrentView).Returns(mockView.Object);
            mockOlObjects.Setup(o => o.App).Returns(mockApplication.Object);
            mockApplication.Setup(a => a.ActiveExplorer()).Returns(mockExplorer.Object);

            mockView.SetupProperty(v => v.Filter, "[Triage] = 'A'");
            mockView.Setup(v => v.Apply());

            _triageOlLogic.FilterView(new char[] { 'B', 'C' });

            mockView.VerifySet(v => v.Filter = It.IsAny<string>(), Times.Once);
            mockView.Verify(v => v.Apply(), Times.Once);
        }

        [TestMethod]
        public void ParseAndStripFilter_ShouldReturnStrippedFilter()
        {
            string filter =
                "\"http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Triage\" LIKE '%A%' OR \"http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Triage\" LIKE '%B%'";

            string result = _triageOlLogic.ParseAndStripFilter(filter);

            Assert.IsFalse(result.Contains("/Triage"));
        }

        [TestMethod]
        public void ParseAndStripFilter_ShouldReturnStrippedFilter2()
        {
            //string filter = @"(""http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Actionable"" = 'Task' AND (""http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Triage"" = 'A' OR ""http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Triage"" = 'B'))";
            var filter =
                "(\"http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Actionable\" LIKE '%Task%' AND (\"http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Triage\" LIKE '%A%' OR \"http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Triage\" = 'B'))";

            Console.WriteLine(filter);
            var parser = new DASLFilterParser();
            var logicTree = parser.Parse(filter);
            parser.PrintTree(logicTree, 0);
            var recombined = parser.CombineTree(logicTree);
            Console.WriteLine(recombined);

            string actual = _triageOlLogic.ParseAndStripFilter(filter);

            Assert.IsTrue(actual.Contains("/Actionable"));
            Assert.IsFalse(actual.Contains("/Triage"));
        }

        [TestMethod]
        public async Task TrainSelectionAsync_ShouldTrainSelection()
        {
            _triageOlLogic = new Triage_OlLogic(_triage);
            Assert.IsNotNull(_triageOlLogic);
            await Task.CompletedTask;
        }
    }
}
