using System.Threading;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.ClassifierGroups;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class Triage_OlLogic_Remediation_Tests
    {
        [TestMethod]
        public void FilterView_WithJetFilter_AppendsParenthesizedTriageClause()
        {
            var triageOlLogic = CreateTriageOlLogic(out var mockGlobals);
            var mockOlObjects = new Mock<IOlObjects>(MockBehavior.Strict);
            var mockApplication = new Mock<Application>(MockBehavior.Strict);
            var mockExplorer = new Mock<Explorer>(MockBehavior.Strict);
            var mockView = new Mock<View>(MockBehavior.Strict);
            mockGlobals.Setup(g => g.Ol).Returns(mockOlObjects.Object);
            mockOlObjects.Setup(o => o.App).Returns(mockApplication.Object);
            mockApplication.Setup(a => a.ActiveExplorer()).Returns(mockExplorer.Object);
            mockExplorer.Setup(e => e.CurrentView).Returns(mockView.Object);
            mockView.SetupProperty(v => v.Filter, "[Subject] = 'Roadmap'");
            mockView.Setup(v => v.Apply());

            triageOlLogic.FilterView(new[] { 'A' });

            mockView.Object.Filter.Should().Be("([Subject] = 'Roadmap') AND ([Triage] = 'A')");
            mockView.Verify(v => v.Apply(), Times.Once);
        }

        private static Triage_OlLogic CreateTriageOlLogic(out Mock<IApplicationGlobals> mockGlobals)
        {
            mockGlobals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            var triage = new Triage(mockGlobals.Object, CancellationToken.None)
            {
                ClassifierGroup = new BayesianClassifierGroup(),
            };
            return new Triage_OlLogic(triage);
        }
    }
}
