using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;

namespace UtilitiesCS.Test.EmailIntelligence.EmailParsingSorting
{
    [TestClass]
    public class EmailFiler_Tests
    {
        [TestMethod]
        public void DefaultConstructor_CreatesInstance()
        {
            // Act
            var filer = new EmailFiler();

            // Assert
            filer.Should().NotBeNull();
            filer.Config.Should().BeNull();
        }

        [TestMethod]
        public void Constructor_WithConfig_SetsConfig()
        {
            // Arrange
            var config = new EmailFilerConfig();

            // Act
            var filer = new EmailFiler(config);

            // Assert
            filer.Config.Should().BeSameAs(config);
        }

        [TestMethod]
        public void Config_SetAndGet_RoundTrips()
        {
            // Arrange
            var filer = new EmailFiler();
            var config = new EmailFilerConfig();

            // Act
            filer.Config = config;

            // Assert
            filer.Config.Should().BeSameAs(config);
        }

        [TestMethod]
        public void MailHelpers_SetAndGet_RoundTrips()
        {
            // Arrange
            var filer = new EmailFiler();
            var helpers = new System.Collections.Generic.List<MailItemHelper>();

            // Act
            filer.MailHelpers = helpers;

            // Assert
            filer.MailHelpers.Should().BeSameAs(helpers);
        }
    }

    [TestClass]
    public class EmailDataMiner_Tests
    {
        [TestMethod]
        public void Constructor_WithGlobals_CreatesInstance()
        {
            // Arrange
            var globals = new Mock<IApplicationGlobals>();

            // Act
            var miner = new EmailDataMiner(globals.Object);

            // Assert
            miner.Should().NotBeNull();
        }
    }
}
