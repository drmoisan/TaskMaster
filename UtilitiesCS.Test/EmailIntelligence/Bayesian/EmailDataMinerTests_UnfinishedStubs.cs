using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS;
using System.Threading;

namespace Z.Unfinished.UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    [TestClass]
    public class EmailDataMinerTests_UnfinishedStubs
    {
        private MockRepository mockRepository;

        private Mock<IApplicationGlobals> mockApplicationGlobals;

        [TestInitialize]
        public void TestInitialize()
        {
            Console.SetOut(new DebugTextWriter());
            this.mockRepository = new MockRepository(MockBehavior.Loose);
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

        #region Unfinished Tests

        [TestMethod]
        public async Task Unfinished_MineEmails_StateUnderTest_ExpectedBehavior()
        {
            //// Arrange
            //var emailDataMiner = this.CreateEmailDataMiner();

            //// Act
            //await emailDataMiner.MineEmails();

            //// Assert
            //Assert.Fail();
            //this.mockRepository.VerifyAll();
            await Task.CompletedTask;
        }

        [TestMethod]
        public async Task Unfinished_LoadStaging_StateUnderTest_ExpectedBehavior()
        {
            //// Arrange
            //var emailDataMiner = this.CreateEmailDataMiner();

            //// Act
            //var result = await emailDataMiner.LoadStaging();

            //// Assert
            //Assert.Fail();
            //this.mockRepository.VerifyAll();
            await Task.CompletedTask;
        }

        [TestMethod]
        public async Task Unfinished_BuildClassifierAsync_StateUnderTest_ExpectedBehavior()
        {
            //// Arrange
            //var emailDataMiner = this.CreateEmailDataMiner();

            //// Act
            //await emailDataMiner.BuildClassifierAsync();

            // Assert
            //Assert.Fail();
            //this.mockRepository.VerifyAll();
            await Task.CompletedTask;
        }

        [TestMethod]
        public async Task Unfinished_BuildClassifierAsync1_StateUnderTest_ExpectedBehavior()
        {
            //// Arrange
            //var emailDataMiner = this.CreateEmailDataMiner();

            //// Act
            //await emailDataMiner.BuildClassifierAsync1();

            //// Assert
            //Assert.Fail();
            //this.mockRepository.VerifyAll();
            await Task.CompletedTask;
        }

        [TestMethod]
        public async Task Unfinished_BuildClassifierAsync2_StateUnderTest_ExpectedBehavior()
        {
            //// Arrange
            //var emailDataMiner = this.CreateEmailDataMiner();

            //// Act
            //await emailDataMiner.BuildClassifierAsync2();

            //// Assert
            //Assert.Fail();
            //this.mockRepository.VerifyAll();
            await Task.CompletedTask;
        }

        #endregion Unfinished Tests

        #region Disabled Tests

        // Uses an embedded interop type which creates compiler error
        [TestMethod]
        public async Task _Disabled_ScrapeEmails_StateUnderTest_ExpectedBehavior()
        {
            //// Arrange
            //var emailDataMiner = this.CreateEmailDataMiner();
            //CancellationTokenSource tokenSource = null;
            //ProgressTracker progress = null;

            //// Act
            //var result = await emailDataMiner.ScrapeEmails(
            //    tokenSource,
            //    progress);

            // Assert
            await Task.CompletedTask;
            //Assert.Fail();
            //this.mockRepository.VerifyAll();
        }

        #endregion Disabled Tests
    }
}
