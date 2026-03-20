using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.ClassifierGroups;

namespace UtilitiesCS.Test.EmailIntelligence.ClassifierGroups
{
    [TestClass]
    public class ActionableClassifierGroup_Tests
    {
        [TestMethod]
        public void DefaultConstructor_CreatesInstance()
        {
            var group = new ActionableClassifierGroup();
            group.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_WithGlobals_SetsProperties()
        {
            var mockGlobals = CreateMockGlobals();
            var group = new ActionableClassifierGroup(mockGlobals.Object);

            group.Should().NotBeNull();
            group.EngineName.Should().Be("Actionable");
        }

        private static Mock<IApplicationGlobals> CreateMockGlobals()
        {
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockOl = new Mock<IOlObjects>();
            var mockFs = new Mock<IFileSystemFolderPaths>();
            var mockAf = new Mock<IAppAutoFileObjects>();
            mockGlobals.Setup(g => g.Ol).Returns(mockOl.Object);
            mockGlobals.Setup(g => g.FS).Returns(mockFs.Object);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);
            return mockGlobals;
        }
    }

    [TestClass]
    public class CategoryClassifierGroup_Tests
    {
        [TestMethod]
        public void Constructor_WithGlobals_CreatesInstance()
        {
            var mockGlobals = CreateMockGlobals();
            var group = new UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories.CategoryClassifierGroup(
                mockGlobals.Object
            );
            group.Should().NotBeNull();
        }

        private static Mock<IApplicationGlobals> CreateMockGlobals()
        {
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockOl = new Mock<IOlObjects>();
            var mockFs = new Mock<IFileSystemFolderPaths>();
            var mockAf = new Mock<IAppAutoFileObjects>();
            mockGlobals.Setup(g => g.Ol).Returns(mockOl.Object);
            mockGlobals.Setup(g => g.FS).Returns(mockFs.Object);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);
            return mockGlobals;
        }
    }

    [TestClass]
    public class OlFolderClassifierGroup_Tests
    {
        [TestMethod]
        public void Constructor_WithGlobals_CreatesInstance()
        {
            var mockGlobals = CreateMockGlobals();
            var group = new UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder.OlFolderClassifierGroup(
                mockGlobals.Object
            );
            group.Should().NotBeNull();
            group.Globals.Should().BeSameAs(mockGlobals.Object);
        }

        private static Mock<IApplicationGlobals> CreateMockGlobals()
        {
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockOl = new Mock<IOlObjects>();
            var mockFs = new Mock<IFileSystemFolderPaths>();
            var mockAf = new Mock<IAppAutoFileObjects>();
            mockGlobals.Setup(g => g.Ol).Returns(mockOl.Object);
            mockGlobals.Setup(g => g.FS).Returns(mockFs.Object);
            mockGlobals.Setup(g => g.AF).Returns(mockAf.Object);
            return mockGlobals;
        }
    }
}
