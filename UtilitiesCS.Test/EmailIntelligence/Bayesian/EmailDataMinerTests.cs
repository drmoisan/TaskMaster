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
using System.Linq;

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

        [TestMethod]
        public void GetDedicated_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var emailDataMiner = this.CreateEmailDataMiner();
            var input = this.CreateCollection();
            List<DedicatedToken> expected =
            [
                new("token2", "folderPath1", 2),
                new("token3", "folderPath1", 1),
                new("token5", "folderPath1", 1),
                new("token6", "folderPath2", 2),
                new("token7", "folderPath2", 2),
                new("token8", "folderPath2", 2),
                new("token9", "folderPath3", 3),
                new("token10", "folderPath3", 1),
            ];

            // Act
            var actualDictionary = emailDataMiner.GetDedicated(input);
            var actual = actualDictionary.Values.ToList();

            Console.WriteLine(
                expected.Select(x => new[] { x.Token, x.FolderPath, x.Count.ToString() })
                .ToArray()
                .ToFormattedText(
                [ "Token", "FolderPath", "Count" ], 
                [Enums.Justification.Center, Enums.Justification.Left, Enums.Justification.Right],
                "Expected Dedicated Tokens"));

            Console.WriteLine(
                actual.Select(x => new[] { x.Token, x.FolderPath, x.Count.ToString() })
                .ToArray()
                .ToFormattedText(
                ["Token", "FolderPath", "Count"],
                [Enums.Justification.Center, Enums.Justification.Left, Enums.Justification.Right],
                "Expected Dedicated Tokens"));

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }
        
    }
}
