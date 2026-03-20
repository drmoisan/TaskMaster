using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.Bayesian.Performance;

namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    [TestClass]
    public class BayesianPerformanceMeasurement_Tests
    {
        [TestMethod]
        public void Constructor_SetsGlobals()
        {
            // Arrange
            var mockRepo = new MockRepository(MockBehavior.Loose);
            var mockApp = mockRepo.Create<Microsoft.Office.Interop.Outlook.Application>();
            var globals = new TaskMaster.ApplicationGlobals(mockApp.Object, true);

            // Act
            var sut = new BayesianPerformanceMeasurement(globals);

            // Assert
            sut.Globals.Should().BeSameAs(globals);
        }

        [TestMethod]
        public void SaveWip_DefaultsToTrue()
        {
            // Arrange
            var mockRepo = new MockRepository(MockBehavior.Loose);
            var mockApp = mockRepo.Create<Microsoft.Office.Interop.Outlook.Application>();
            var globals = new TaskMaster.ApplicationGlobals(mockApp.Object, true);

            // Act
            var sut = new BayesianPerformanceMeasurement(globals);

            // Assert
            sut.SaveWip.Should().BeTrue();
        }

        [TestMethod]
        public void SaveWip_SetAndGet_Works()
        {
            // Arrange
            var mockRepo = new MockRepository(MockBehavior.Loose);
            var mockApp = mockRepo.Create<Microsoft.Office.Interop.Outlook.Application>();
            var globals = new TaskMaster.ApplicationGlobals(mockApp.Object, true);
            var sut = new BayesianPerformanceMeasurement(globals);

            // Act
            sut.SaveWip = false;

            // Assert
            sut.SaveWip.Should().BeFalse();
        }

        [TestMethod]
        public void Serialization_IsSetInConstructor()
        {
            // Arrange
            var mockRepo = new MockRepository(MockBehavior.Loose);
            var mockApp = mockRepo.Create<Microsoft.Office.Interop.Outlook.Application>();
            var globals = new TaskMaster.ApplicationGlobals(mockApp.Object, true);

            // Act
            var sut = new BayesianPerformanceMeasurement(globals);

            // Assert
            sut.Serialization.Should().NotBeNull();
            sut.Serialization.Globals.Should().BeSameAs(globals);
        }
    }

    [TestClass]
    public class BayesianSerializationHelper_Tests
    {
        [TestMethod]
        public void Constructor_SetsGlobals()
        {
            // Arrange
            var mockRepo = new MockRepository(MockBehavior.Loose);
            var mockApp = mockRepo.Create<Microsoft.Office.Interop.Outlook.Application>();
            var globals = new TaskMaster.ApplicationGlobals(mockApp.Object, true);

            // Act
            var sut = new BayesianSerializationHelper(globals);

            // Assert
            sut.Globals.Should().BeSameAs(globals);
        }
    }
}
