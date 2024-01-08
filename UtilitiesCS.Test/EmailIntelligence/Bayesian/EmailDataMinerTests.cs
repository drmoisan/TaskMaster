using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.Bayesian;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Deedle.Internal;
using FluentAssertions;

namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    [TestClass]
    public class EmailDataMinerTests
    {
        private MockRepository mockRepository;

        private Mock<IApplicationGlobals> mockApplicationGlobals;

        [TestInitialize]
        public void TestInitialize()
        {
            Console.SetOut(new DebugTextWriter());
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockApplicationGlobals = this.mockRepository.Create<IApplicationGlobals>();
        }

        private EmailDataMiner CreateEmailDataMiner()
        {
            return new EmailDataMiner(
                this.mockApplicationGlobals.Object);
        }

        private ConcurrentBag<MinedMailInfo> CreateCollection()
        {
            var collection = new ConcurrentBag<MinedMailInfo>
            {
                new("folderPath1", ["token1", "token2", "token3", "token2", "token4", "token5", "token1"]),
                new("folderPath2", ["token6", "token6", "token7", "token7", "token8", "token8", "token1"]),
                new("folderPath3", ["token9", "token9","token4","token9","token10"])
            };

            return collection;
        }

        [TestMethod]
        public void GetDedicated_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var emailDataMiner = this.CreateEmailDataMiner();
            var input = this.CreateCollection();
            var expected = new List<DedicatedToken>()
            {
                new("token2", "folderPath1", 2),
                new("token3", "folderPath1", 1),
                new("token5", "folderPath1", 1),
                new("token6", "folderPath2", 2),
                new("token7", "folderPath2", 2),
                new("token8", "folderPath2", 2),
                new("token9", "folderPath3", 3),
                new("token10", "folderPath3", 1),
            };

            // Act
            var actual = emailDataMiner.GetDedicated(input);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        //[TestMethod]
        //public async Task ScrapeEmails_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var emailDataMiner = this.CreateEmailDataMiner();
        //    CancellationTokenSource tokenSource = null;
        //    ProgressTracker progress = null;

        //    // Act
        //    var result = await emailDataMiner.ScrapeEmails(
        //        tokenSource,
        //        progress);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        [TestMethod]
        public async Task MineEmails_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var emailDataMiner = this.CreateEmailDataMiner();

            // Act
            await emailDataMiner.MineEmails();

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public async Task LoadStaging_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var emailDataMiner = this.CreateEmailDataMiner();

            // Act
            var result = await emailDataMiner.LoadStaging();

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        

        [TestMethod]
        public async Task BuildClassifierAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var emailDataMiner = this.CreateEmailDataMiner();

            // Act
            await emailDataMiner.BuildClassifierAsync();

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public async Task BuildClassifierAsync1_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var emailDataMiner = this.CreateEmailDataMiner();

            // Act
            await emailDataMiner.BuildClassifierAsync1();

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public async Task BuildClassifierAsync2_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var emailDataMiner = this.CreateEmailDataMiner();

            // Act
            await emailDataMiner.BuildClassifierAsync2();

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }
    }
}
