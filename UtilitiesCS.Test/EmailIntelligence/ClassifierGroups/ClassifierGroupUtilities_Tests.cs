using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.ClassifierGroups;

namespace UtilitiesCS.Test.EmailIntelligence.ClassifierGroups
{
    [TestClass]
    public class ClassifierGroupUtilities_Tests
    {
        [TestMethod]
        public async Task CreateClassifierGroupAsync_WithEmptyCollection_ShouldReturnGroupWithZeroCounts()
        {
            var globals = CreateGlobals();
            var utils = new ClassifierGroupUtilities(globals.Object);

            var result = await utils.CreateClassifierGroupAsync(
                System.Array.Empty<MinedMailInfo>()
            );

            result.Should().NotBeNull();
            result.TotalEmailCount.Should().Be(0);
            result.SharedTokenBase.Should().NotBeNull();
        }

        [TestMethod]
        public async Task CreateClassifierGroupAsync_WithTokens_ShouldBuildSharedTokenBase()
        {
            var globals = CreateGlobals();
            var utils = new ClassifierGroupUtilities(globals.Object);
            var collection = new[]
            {
                new MinedMailInfo { Tokens = new[] { "hello", "world", "hello" } },
                new MinedMailInfo { Tokens = new[] { "world", "test" } },
            };

            var result = await utils.CreateClassifierGroupAsync(collection);

            result.TotalEmailCount.Should().Be(2);
            result.SharedTokenBase.Should().NotBeNull();
        }

        [TestMethod]
        public async Task CreateClassifierGroupAsync_WithMinimumCountFilter_ShouldFilterTokens()
        {
            var globals = CreateGlobals();
            var utils = new ClassifierGroupUtilities(globals.Object);
            var collection = new[]
            {
                new MinedMailInfo { Tokens = new[] { "rare", "common", "common" } },
                new MinedMailInfo { Tokens = new[] { "common" } },
            };

            var result = await utils.CreateClassifierGroupAsync(
                collection,
                minimumCountPerToken: 3
            );

            result.TotalEmailCount.Should().Be(2);
            result.SharedTokenBase.Should().NotBeNull();
        }

        [TestMethod]
        public void Globals_ShouldReturnInjectedGlobals()
        {
            var globals = CreateGlobals();
            var utils = new ClassifierGroupUtilities(globals.Object);

            utils.Globals.Should().BeSameAs(globals.Object);
        }

        [TestMethod]
        public void Deserialize_WhenAppDataFolderNotConfigured_ShouldReturnDefault()
        {
            var globals = CreateGlobals();
            var fs = new Mock<IFileSystemFolderPaths>();
            var specialFolders = new ConcurrentDictionary<string, string>();
            fs.SetupGet(x => x.SpecialFolders).Returns(specialFolders);
            globals.SetupGet(x => x.FS).Returns(fs.Object);
            var utils = new ClassifierGroupUtilities(globals.Object);

            var result = utils.Deserialize<BayesianClassifierGroup>("test");

            result.Should().BeNull();
        }

        private static Mock<IApplicationGlobals> CreateGlobals()
        {
            return new Mock<IApplicationGlobals>();
        }
    }
}
